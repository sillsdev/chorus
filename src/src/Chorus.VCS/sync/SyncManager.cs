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
	public class SyncManager
	{
		private string _localRepositoryPath;
		private string _userName;
		private List<RepositorySource> _knownRepositories=new List<RepositorySource>();
		static internal string LocationToMakeRepositoryDuringTest=null;//enchance: introduce resolver delegate
		private IProgress _progress;

		public List<RepositorySource> KnownRepositories
		{
			get { return _knownRepositories; }
			set { _knownRepositories = value; }
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="startingPath"></param>
		/// <returns>if null, the user canceled</returns>
		public static SyncManager FromChildPath(string startingPath, IProgress progress, string userName)
		{
			if (!Directory.Exists(startingPath) && !File.Exists(startingPath))
			{
				throw new ArgumentException("File or directory wasn't found", startingPath);
			}
			if (!Directory.Exists(startingPath)) // if it's a file... we need a directory
			{
				startingPath = Path.GetDirectoryName(startingPath);
			}

			string root = HgRepository.GetRepositoryRoot(startingPath);
			if (!string.IsNullOrEmpty(root))
			{
				return new SyncManager(root, progress, userName);
			}
			else
			{
				string newRepositoryPath = LocationToMakeRepositoryDuringTest;
				if (string.IsNullOrEmpty(newRepositoryPath))
				{
					newRepositoryPath = AskUserForNewRepositoryPath(startingPath);
				}
				if (!string.IsNullOrEmpty(startingPath) && Directory.Exists(newRepositoryPath))
				{
					HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
					return new SyncManager(newRepositoryPath, progress, userName);
				}
				else
				{
					return null;//user canceled
				}
			}
		}

		/*       private static string IsPartOfRepository(string startingPath, out bool foundRepository)
		{
			string dirPath = startingPath;
			foundRepository = false;
			while (!string.IsNullOrEmpty(dirPath))
			{
				foundRepository = IsReposistoryParent(dirPath);
				if (foundRepository)
					break;
				string parentDirPath = Directory.GetParent(dirPath).FullName;
				if (parentDirPath == dirPath)
				{
					break;
				}
				dirPath = parentDirPath;
			}
			return dirPath;
		}*/
		public string RepoProjectName
		{
			get { return Path.GetFileNameWithoutExtension(_localRepositoryPath); }
		}

		public SyncResults SyncNow(ProjectFolderConfiguration projectFolderConfiguration, SyncOptions options)
		{
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,_progress, _userName);

			_progress.WriteStatus(_userName + " Checking In...");
			repo.AddAndCheckinFiles(projectFolderConfiguration.IncludePatterns, projectFolderConfiguration.ExcludePatterns, options.CheckinDescription);

			if (options.DoPullFromOthers)
			{
				_progress.WriteStatus("Pulling...");
				foreach (RepositorySource repoDescriptor in KnownRepositories)
				{
					repo.TryToPull(repoDescriptor.ResolveUri(RepoProjectName, _progress), repoDescriptor.SourceName, _progress, results);
				}
			}

			if (options.DoMergeWithOthers)
			{
				_progress.WriteStatus("Merging...");
				repo.MergeHeads(_progress, results);

				foreach (RepositorySource otherRepo in KnownRepositories)
				{
					if (!otherRepo.ReadOnly)
					{
						repo.Push(otherRepo.ResolveUri(RepoProjectName, _progress), _progress, results);

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



		public SyncManager(string localRepositoryPath, IProgress progress, string userName)
		{
			_localRepositoryPath = localRepositoryPath;
			_progress = progress;
			_userName = userName;
		}


		public void MakeClone(string path)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, _progress, _userName);
			using (new ConsoleProgress("Creating repository clone to {0}", path))
			{
				local.Clone(path);
			}
		}
	}

	/// <summary>
	/// This is what the calling application knows; it doesn't know the full picture of this user's repository
	/// (like what other apps there are, what other projects theres are), but it knows about one project, and
	/// perhaps what the user's name is.
	/// </summary>
//    public class ApplicationSyncContext
//    {
//        public ProjectFolderConfiguration Project=new ProjectFolderConfiguration();
//        public UserDescriptor User=new UserDescriptor("unknown");
//    }

	/// <summary>
	/// Right now this is just a name, but it could grow to have either more info about the user or
	/// parameters relevant to syncing as this user.
	/// </summary>
	public class UserDescriptor
	{
		public string Id;

		public UserDescriptor(string id)
		{
			Id = id;
		}
	}



	public class SyncResults
	{
	}
}