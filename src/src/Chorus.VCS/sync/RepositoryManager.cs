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


		private List<RepositorySource> _knownRepositories=new List<RepositorySource>();

		public List<RepositorySource> KnownRepositories
		{
			get { return _knownRepositories; }
			set { _knownRepositories = value; }
		}

		public string RepoProjectName
		{
			get { return Path.GetFileNameWithoutExtension(_localRepositoryPath); }
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>if null, the user canceled</returns>
		public static RepositoryManager FromContext(ApplicationSyncContext syncContext)
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

			List<RepositorySource> repositoriesToTry = options.RepositoriesToTry;

			//if the client didn't specify any, try them all
			if(repositoriesToTry==null || repositoriesToTry.Count == 0)
				repositoriesToTry = KnownRepositories;

			if (options.DoPullFromOthers)
			{
				progress.WriteStatus("Pulling...");
				foreach (RepositorySource repoDescriptor in repositoriesToTry)
				{
					if (repoDescriptor.CanConnect(RepoProjectName, progress))
					{
						repo.TryToPull(repoDescriptor.ResolveUri(RepoProjectName, progress), repoDescriptor.SourceName, progress, results);
					}
					else
					{
						progress.WriteMessage("Could not connect to {0} at {1} for pulling", repoDescriptor.SourceName, repoDescriptor.URI);
					}
				}
			}

			if (options.DoMergeWithOthers)
			{
				progress.WriteStatus("Merging...");
				repo.MergeHeads(progress, results);

				foreach (RepositorySource repoDescriptor in repositoriesToTry)
				{
					if (!repoDescriptor.ReadOnly)
					{
						string resolvedUri;
						if (repoDescriptor.ShouldCreateClone(RepoProjectName, progress, out resolvedUri))
						{
							MakeClone(resolvedUri, true, progress);
						}

						if (repoDescriptor.CanConnect(RepoProjectName, progress))
						{
							repo.Push(repoDescriptor, progress, results);
						}
						else
						{
							progress.WriteMessage("Could not connect to {0} at {1} for pushing", repoDescriptor.SourceName, repoDescriptor.URI);
						}
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

			KnownRepositories.Add(RepositorySource.Create("UsbKey", "UsbKey", false));
		}


		public void MakeClone(string path, bool alsoDoCheckout, IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress, _appContext.User.Id);
			using (new ConsoleProgress("Creating repository clone to {0}", path))
			{
				local.Clone(path);
				if(alsoDoCheckout)
				{
					HgRepository clone = new HgRepository(path, progress, _appContext.User.Id);
					clone.Update();
				}
			}
		}
	}

}