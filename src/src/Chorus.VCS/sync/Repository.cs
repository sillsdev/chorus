using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.sync
{
	public class RepositoryManager
	{
		private string _localRepositoryPath;
		private readonly ApplicationSyncContext _appContext;


		private List<RepositoryDescriptor> _knownRepositories=new List<RepositoryDescriptor>();

		public List<RepositoryDescriptor> KnownRepositories
		{
			get { return _knownRepositories; }
			set { _knownRepositories = value; }
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>if null, the user canceled</returns>
		public static RepositoryManager FromAppContext(ApplicationSyncContext syncContext)
		{
			if (!Directory.Exists(syncContext.Project.TopPath) && !File.Exists(syncContext.Project.TopPath))
			{
				throw new ArgumentException("File or directory wasn't found", syncContext.Project.TopPath);
			}
			string startingPath = syncContext.Project.TopPath;
			if (!Directory.Exists(startingPath)) // if it's a file... we need a directory
			{
				startingPath = Path.GetDirectoryName(startingPath);
			}

			string root = HgRepository.GetRepositoryRoot(startingPath);
			if (!string.IsNullOrEmpty(root))
			{
				return new RepositoryManager(root, syncContext);
			}
			else
			{
				string newRepositoryPath = AskUserForNewRepositoryPath(startingPath);
				if (!string.IsNullOrEmpty(startingPath) && Directory.Exists(newRepositoryPath))
				{
					HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
					return new RepositoryManager(newRepositoryPath, syncContext);
				}
				else
				{
					return null;//user canceled
				}
			}
		}

		internal static void MakeRepositoryForTest(string newRepositoryPath)
		{
			HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
		}


		public SyncResults SyncNow(SyncOptions options, IProgress progress)
		{
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,progress, _appContext.User.Id);

			progress.WriteStatus(_appContext.User.Id + " Checking In...");
			repo.AddAndCheckinFiles(_appContext.Project.IncludePatterns, _appContext.Project.ExcludePatterns, options.CheckinDescription);

			if (options.DoPullFromOthers)
			{
				progress.WriteStatus("Pulling...");
				foreach (RepositoryDescriptor otherRepo in KnownRepositories)
				{
					repo.TryToPull(otherRepo, progress, results);
				}
			}

			if (options.DoMergeWithOthers)
			{
				progress.WriteStatus("Merging...");
				repo.MergeHeads(progress, results);

				foreach (RepositoryDescriptor otherRepo in KnownRepositories)
				{
					if (!otherRepo.ReadOnly)
					{
						repo.Push(otherRepo, progress, results);
					}
				}
			}
			repo.Update();// REVIEW

			return results;
		}



		private static string AskUserForNewRepositoryPath(string pathToDirectory)
		{
			System.Windows.Forms.FolderBrowserDialog dlg = new FolderBrowserDialog();
			dlg.SelectedPath =pathToDirectory;
			dlg.ShowNewFolderButton = false;
			dlg.Description = "Select the folder to be the parent of the Chorus repository.";
			if(dlg.ShowDialog() != DialogResult.OK)
				return null;
			//todo: make sure the folder they chose is a parent of this
			return dlg.SelectedPath;
		}



		public RepositoryManager(string localRepositoryPath,ApplicationSyncContext appContext)
		{
			_localRepositoryPath = localRepositoryPath;
			_appContext = appContext;
		}


		public void MakeClone(string path, IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress, _appContext.User.Id);
			using (new ConsoleProgress("Creating repository clone to {0}", path))
			{
				local.Clone(path);
			}
		}
	}

}