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
		private ProjectFolderConfiguration _project;

		private List<RepositorySource> _knownRepositorySources=new List<RepositorySource>();

		public List<RepositorySource> KnownRepositorySources
		{
			get { return _knownRepositorySources; }
			set { _knownRepositorySources = value; }
		}

		public string RepoProjectName
		{
		   //get { return Path.GetFileNameWithoutExtension(_localRepositoryPath); }
			get { return Path.GetFileNameWithoutExtension(_localRepositoryPath)+Path.GetExtension(_localRepositoryPath); }
		}

		public RepositorySource UsbSource
		{
			get { return KnownRepositorySources[0] as UsbKeyRepositorySource; }
		}


		/// <summary>
		///
		/// </summary>
		public static RepositoryManager FromRootOrChildFolder(ProjectFolderConfiguration project)
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
				return new RepositoryManager(root, project);
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
					//review: Machine name would be more accurate, but most people have, like "Compaq" as their machine name
					HgRepository.SetUserId(newRepositoryPath, System.Environment.UserName);
					return new RepositoryManager(newRepositoryPath, project);
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

		public static IDisposable CreateDvcsMissingSimulation()
		{
			return new Chorus.VcsDrivers.Mercurial.HgMissingSimulation();
		}



		public SyncResults SyncNow(SyncOptions options, IProgress progress)
		{
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,progress);

			progress.WriteStatus("Checking In...");
			repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, options.CheckinDescription);

			List<RepositorySource> sourcesToTry = options.RepositorySourcesToTry;

			//if the client didn't specify any, try them all
//            no, don't do that.  It's reasonable to just be doing a local checkin
//            if(repositoriesToTry==null || repositoriesToTry.Count == 0)
//                repositoriesToTry = KnownRepositorySources;

			if (options.DoPullFromOthers)
			{
				progress.WriteStatus("Pulling...");
				foreach (RepositorySource source in sourcesToTry)
				{
					string resolvedUri = source.PotentialRepoUri(RepoProjectName, progress);
					if (source.CanConnect(RepoProjectName, progress))
					{
						repo.TryToPull(resolvedUri);
					}
					else
					{
						progress.WriteMessage("Could not connect to {0} at {1} for pulling", source.SourceLabel, resolvedUri);
					}
				}
			}

			if (options.DoMergeWithOthers)
			{
				progress.WriteStatus("Merging...");
				IList<string> peopleWeMergedWith = repo.MergeHeads(progress, results);//this may generate conflict files
				// in case of a merge, we want these merged version + updated/created conflict files to go right back into
				// the repository
				if (peopleWeMergedWith.Count > 0)
				{
					string message = "Merged with ";
					foreach (string id in peopleWeMergedWith)
					{
						message += id + ", ";
					}
					message = message.Remove(message.Length - 2); //chop off the trailing comma
					repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, message);
				}
			}

			if(options.DoPushToLocalSources)
			{
				foreach (RepositorySource repoDescriptor in sourcesToTry)
				{
					if (!repoDescriptor.ReadOnly)
					{
						string resolvedUri = repoDescriptor.PotentialRepoUri(RepoProjectName, progress);
						if (repoDescriptor.CanConnect(RepoProjectName, progress))
						{
							progress.WriteMessage("Pushing local repository to {0} at {1}", RepoProjectName, resolvedUri);
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
									  repoDescriptor.SourceLabel);
				return null;
			}
			else
			{
				foreach (string uri in possibleRepoCloneUris)
				{
					try
					{
						progress.WriteStatus("Making repository on {0} at {1}...", repoDescriptor.SourceLabel, uri);
						MakeClone(uri, true, progress);
						progress.WriteStatus("Done.");
						return uri;
					}
					catch (Exception error)
					{
						progress.WriteMessage("Could not create clone at {0}: {1}", uri, error.Message);
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
			: this(localRepositoryPath, project, System.Environment.UserName)
		{
		}

		public RepositoryManager(string localRepositoryPath, ProjectFolderConfiguration project, string userId)
		{
			_project = project;
			_localRepositoryPath = localRepositoryPath;

			KnownRepositorySources.Add(RepositorySource.Create(RepositorySource.HardWiredSources.UsbKey, "UsbKey", false));
		}


		/// <summary>
		///
		/// </summary>
		 /// <returns>path to clone</returns>
		public string MakeClone(string newDirectory, bool alsoDoCheckout, IProgress progress)
		{
			if (Directory.Exists(newDirectory))
			{
				throw new ArgumentException(String.Format("The newDirectory must not already exist ({0})", newDirectory));
			}
			string parent = Directory.GetParent(newDirectory).FullName;
			if (!Directory.Exists(parent))
			{
				throw new ArgumentException(String.Format("The parent of the given newDirectory must already exist ({0})", parent));
			}
			HgRepository local = new HgRepository(_localRepositoryPath, progress);
			using (new ConsoleProgress("Creating repository clone at {0}", newDirectory))
			{
				local.Clone(newDirectory);
				if(alsoDoCheckout)
				{
				   // string userIdForCLone = string.Empty; /* don't assume it's this user... a repo on a usb key probably shouldn't have a user default */
					HgRepository clone = new HgRepository(newDirectory, progress);
					clone.Update();
				}
				return newDirectory;
			}
		}

		public List<RevisionDescriptor> GetHistoryItems(IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress);

			return local.GetHistoryItems();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="messageLanguageId"></param>
		/// <returns>false if the environment is not set up correctly</returns>
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

		public void SetUserId(string userId)
		{
		   HgRepository.SetUserId(_localRepositoryPath, userId);
		}

		public bool GetFileExistsInRepo(string fullPath)
		{
			if (fullPath.IndexOf(_localRepositoryPath) < 0)
			{
				throw new ArgumentException(
					string.Format("GetFileExistsInRepo() requies the argument {0} be a child of the root {1}",
					fullPath,
					_localRepositoryPath));

			}
			HgRepository local = new HgRepository(_localRepositoryPath, new NullProgress());
			string subPath= fullPath.Replace(_localRepositoryPath, "");
			if (subPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
			{
				subPath = subPath.Remove(0,1);
			}
			return local.GetFileExistsInRepo(subPath);
		}
	}


	public class SyncResults
	{

	}
}