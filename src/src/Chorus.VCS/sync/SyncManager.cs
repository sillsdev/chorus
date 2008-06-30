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
		private List<RepositoryDescriptor> _knownRepositories=new List<RepositoryDescriptor>();
		static internal string LocationToMakeRepositoryDuringTest=null;//enchance: introduce resolver delegate
		private IProgress _progress;

		public List<RepositoryDescriptor> KnownRepositories
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

		public SyncResults SyncNow(ProjectDescriptor projectDescriptor, SyncOptions options)
		{
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,_progress, _userName);

			_progress.WriteStatus(_userName + " Checking In...");
			repo.AddAndCheckinFiles(projectDescriptor.IncludePatterns, projectDescriptor.ExcludePatterns, options.CheckinDescription);

			if (options.DoPullFromOthers)
			{
				_progress.WriteStatus("Pulling...");
				foreach (RepositoryDescriptor otherRepo in KnownRepositories)
				{
					repo.TryToPull(otherRepo, _progress, results);
				}
			}

			if (options.DoMergeWithOthers)
			{
				_progress.WriteStatus("Merging...");
				repo.MergeHeads(_progress, results);

				foreach (RepositoryDescriptor otherRepo in KnownRepositories)
				{
					if (!otherRepo.ReadOnly)
					{
						repo.Push(otherRepo, _progress, results);
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

	public class ProjectDescriptor
	{
		private List<string> _includePatterns=new List<string>();
		private List<string> _excludePatterns=new List<string>();
		public string TopPath;

		/// <summary>
		/// File Patterns to Add to the repository, unless excluded by ExcludePatterns
		/// </summary>
		/// <example>"LP/*.*"  include all files under the lp directory</example>
		/// <example>"**/*.Lift"  include all lift files, whereever they are found</example>
		public List<string> IncludePatterns
		{
			get { return _includePatterns; }
		}

		/// <summary>
		/// If includePatterns are also specified, these are applied after them.
		/// </summary>
		/// <example>"**/*.bak" </example>
		/// <example>"**/cache" any directory named 'cache'</example>
		public List<string> ExcludePatterns
		{
			get { return _excludePatterns; }
			set { _excludePatterns = value; }
		}
	}

	public class SyncOptions
	{
		private bool _doPullFromOthers;
		private bool _doMergeWithOthers;
		private string _checkinDescription;

		public SyncOptions()
		{
			_doPullFromOthers = true;
			_doMergeWithOthers = true;
			_checkinDescription = "missing checking description";
		}

		public bool DoPullFromOthers
		{
			get { return _doPullFromOthers; }
			set { _doPullFromOthers = value; }
		}

		public bool DoMergeWithOthers
		{
			get { return _doMergeWithOthers; }
			set { _doMergeWithOthers = value; }
		}

		public string CheckinDescription
		{
			get { return _checkinDescription; }
			set { _checkinDescription = value; }
		}
	}

	/// <summary>
	/// This is what the calling application knows; it doesn't know the full picture of this user's repository
	/// (like what other apps there are, what other projects theres are), but it knows about one project, and
	/// perhaps what the user's name is.
	/// </summary>
	public class ApplicationSyncContext
	{
		public ProjectDescriptor Project=new ProjectDescriptor();
		public UserDescriptor User=new UserDescriptor("unknown");
	}

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

	public class RepositoryDescriptor
	{
		private string _uri;
		private string _repoName;

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		private bool _readOnly;

		public RepositoryDescriptor(string uri, string userName, bool readOnly)
		{
			URI = uri;
			_repoName = userName;
			ReadOnly = readOnly;
		}

		public string URI
		{
			get { return _uri; }
			set { _uri = value; }
		}


		/// <summary>
		/// In the case of a repo sitting on the user's machine, this will be a person's name.
		/// It might also be the name of the web-based repo, or the name of the USB key.
		/// </summary>
		public string RepoName
		{
			get { return _repoName; }
		}

		/// <summary>
		/// THis will be false for, say, usb-keys or shared internet repos
		/// but true for other people on LANs (maybe?)
		/// </summary>
		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}
	}

	public class SyncResults
	{
	}
}