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
		private string _userId="anonymous";
		private ProjectFolderConfiguration _project;

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

		public static RepositoryManager FromRootOrChildFolder(ProjectFolderConfiguration project)
		{
			return FromRootOrChildFolder(project, null);
		}

		/// <summary>
		///
		/// </summary>
		public static RepositoryManager FromRootOrChildFolder(ProjectFolderConfiguration project, string userId)
		{

			if (!Directory.Exists(project.FolderPath) && !File.Exists(project.FolderPath))
			{
				throw new ArgumentException("File or directory wasn't found", project.FolderPath);
			}
			string startingPath = project.FolderPath;
			if (!Directory.Exists(startingPath)) // if it's a file... we need a directory
			{
				startingPath = Path.GetDirectoryName(startingPath);
			}

			string root = HgRepository.GetRepositoryRoot(startingPath);
			if (!string.IsNullOrEmpty(root))
			{
				return new RepositoryManager(root, project, userId);
			}
			else
			{
				/*
				 I'm leaning away from this intervention at the moment.
					string newRepositoryPath = AskUserForNewRepositoryPath(startingPath);

				 Let's see how far we can get by just silently creating it, and leave it to the future
				 or user documentation/training to know to set up a repository at the level they want.
				*/
				string newRepositoryPath = project.FolderPath;

				if (!string.IsNullOrEmpty(startingPath) && Directory.Exists(newRepositoryPath))
				{
					HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
					return new RepositoryManager(newRepositoryPath, project, userId);
				}
				else
				{
					return null;
				}
			}
		}

		internal static void MakeRepositoryForTest(string newRepositoryPath, string userId)
		{
			HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
			HgRepository.SetUserId(newRepositoryPath, userId);
		}

		public static string GetEnvironmentReadinessMessage(string messageLanguageId)
		{
			return HgRepository.GetEnvironmentReadinessMessage(messageLanguageId);
		}


		public SyncResults SyncNow(SyncOptions options, IProgress progress)
		{
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,progress, _userId);

			progress.WriteStatus(_userId+ " Checking In...");
			repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, options.CheckinDescription);

			List<RepositorySource> repositoriesToTry = options.RepositoriesToTry;

			//if the client didn't specify any, try them all
//            no, don't do that.  It's reasonable to just be doing a local checkin
//            if(repositoriesToTry==null || repositoriesToTry.Count == 0)
//                repositoriesToTry = KnownRepositories;

			if (options.DoPullFromOthers)
			{
				progress.WriteStatus("Pulling...");
				foreach (RepositorySource repoDescriptor in repositoriesToTry)
				{
					string resolvedUri = repoDescriptor.ResolveUri(RepoProjectName, progress);
					if (repoDescriptor.CanConnect(RepoProjectName, progress))
					{
						repo.TryToPull(resolvedUri, repoDescriptor.SourceName, progress, results);
					}
					else
					{
						progress.WriteMessage("Could not connect to {0} at {1} for pulling", repoDescriptor.SourceName, resolvedUri);
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
						string resolvedUri = repoDescriptor.ResolveUri(RepoProjectName, progress);
						if (repoDescriptor.CanConnect(RepoProjectName, progress))
						{
							progress.WriteMessage("Pushing local repository to {0}", repoDescriptor.SourceName);
							repo.Push(resolvedUri, progress, results);
						}
						else
						{
							TryToMakeCloneForSource(progress, repoDescriptor);
							//nb: no need to push if we just made a clone
						}
					}
				}
			}
			repo.Update();// REVIEW
			progress.WriteStatus("Done.");
			return results;
		}

		/// <summary>
		/// used for usb sources
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="repoDescriptor"></param>
		/// <returns>the uri of a successful clone</returns>
		private string TryToMakeCloneForSource(IProgress progress, RepositorySource repoDescriptor)
		{
			List<string> possibleRepoCloneUris = repoDescriptor.GetPossibleCloneUris(RepoProjectName, progress);
			if (possibleRepoCloneUris == null)
			{
				progress.WriteMessage("No Uris available for cloning to {0}",
									  repoDescriptor.SourceName);
				return null;
			}
			else
			{
				foreach (string uri in possibleRepoCloneUris)
				{
					try
					{
						progress.WriteStatus("Making repository on {0} at {1}...", repoDescriptor.SourceName, uri);
						MakeClone(uri, true, progress);
						progress.WriteStatus("Done.");
						return uri;
					}
					catch (Exception)
					{
						progress.WriteMessage("Could not create clone at {1}", uri);
						continue;
					}
				}
			}
			return null;
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


		public RepositoryManager(string localRepositoryPath, ProjectFolderConfiguration project)
			: this(localRepositoryPath, project, null)
		{
		}

		public RepositoryManager(string localRepositoryPath, ProjectFolderConfiguration project, string userId)
		{
			_userId = userId;
			_project = project;
			_localRepositoryPath = localRepositoryPath;

			KnownRepositories.Add(RepositorySource.Create("UsbKey", "UsbKey", false));
		}


		public void MakeClone(string path, bool alsoDoCheckout, IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress,_userId);
			using (new ConsoleProgress("Creating repository clone at {0}", path))
			{
				local.Clone(path);
				if(alsoDoCheckout)
				{
					HgRepository clone = new HgRepository(path, progress, _userId);
					clone.Update();
				}
			}
		}

		public List<RevisionDescriptor> GetHistoryItems(IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress, _userId);

			return local.GetHistoryItems();
		}

		public static bool CheckEnvironmentAndShowMessageIfAppropriate(string messageLanguageId)
		{
			string s = RepositoryManager.GetEnvironmentReadinessMessage(messageLanguageId);
			if (!string.IsNullOrEmpty(s))
			{
					MessageBox.Show(s, "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return false;
			 }
			return true;
		}
	}


	public class SyncResults
	{
	}
}