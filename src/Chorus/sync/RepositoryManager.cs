using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.sync
{
	public class RepositoryManager
	{
		private string _localRepositoryPath;
		private ProjectFolderConfiguration _project;

		public List<RepositoryPath> ExtraRepositorySources { get; private set; }



		public List<RepositoryPath> GetPotentialSources(IProgress progress)
		{
			var list = new List<RepositoryPath>();
			list.AddRange(ExtraRepositorySources);
			var repo = GetRepository(progress);
			list.AddRange(repo.GetKnownPeerRepositories());
			return list;
		}



		public string RepoProjectName
		{
		   //get { return Path.GetFileNameWithoutExtension(_localRepositoryPath); }
			get { return Path.GetFileNameWithoutExtension(_localRepositoryPath)+Path.GetExtension(_localRepositoryPath); }
		}

		public RepositoryPath UsbPath
		{
			get
			{
				foreach (var source in ExtraRepositorySources)
				{
					if(source as UsbKeyRepositorySource !=null)
						return source;
				}
				return null;
			}
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
					//but in any case, this is just a default until they set the name explicity
					var hg = new HgRepository(newRepositoryPath, new NullProgress());
					hg.SetUserNameInIni(System.Environment.UserName, new NullProgress());
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
			var hg = new HgRepository(newRepositoryPath, new NullProgress());
			hg.SetUserNameInIni(userId, new NullProgress());
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

			var tipBeforeSync = repo.GetTip();
			List<RepositoryPath> sourcesToTry = options.RepositorySourcesToTry;

			//if the client didn't specify any, try them all
//            no, don't do that.  It's reasonable to just be doing a local checkin
//            if(repositoriesToTry==null || repositoriesToTry.Count == 0)
//                repositoriesToTry = ExtraRepositorySources;

			if (options.DoPullFromOthers)
			{
				foreach (RepositoryPath source in sourcesToTry)
				{
					string resolvedUri = source.PotentialRepoUri(RepoProjectName, progress);
					if (source.CanConnect(RepoProjectName, progress))
					{
						progress.WriteStatus("Trying to Pull from {0}({1})...", source.Name, source.URI);
						repo.TryToPull(resolvedUri);
					}
					else
					{
						progress.WriteMessage("Could not connect to {0} at {1} for pulling", source.Name, resolvedUri);
					}
				}
			}

			if (options.DoMergeWithOthers)
			{
				IList<string> peopleWeMergedWith = repo.MergeHeads(progress, results);

				//that merge may have generated conflict files, and we want these merged
				//version + updated/created conflict files to go right back into the repository
				if (peopleWeMergedWith.Count > 0)
				{
					repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, GetMergeCommitSummary(peopleWeMergedWith, repo));
				}
			}

			if(options.DoPushToLocalSources)
			{
				foreach (RepositoryPath repoDescriptor in sourcesToTry)
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
			UpdateToTheDescendantRevision(repo, tipBeforeSync);
			progress.WriteStatus("Done.");
			return results;
		}

		/// <summary>
		/// If everything got merged, then this is trivial. But in case of a merge failure,
		/// the "tip" might be the other guy's unmergable data (mabye because he has a newer
		/// version of some application than we do) We don't want to switch to that!
		///
		/// So if there are more than one head out there, we update to the one that is a descendant
		/// of our latest checkin (which in the simple merge failure case is the the checkin itself,
		/// but in a 3-or-more source scenario could be the result of a merge with a more cooperative
		/// revision).
		/// </summary>
		private void UpdateToTheDescendantRevision(HgRepository repository, Revision parent)
		{
			var heads = repository.GetHeads();
			if (heads.Count == 1)
			{
				repository.Update(); //update to the tip
				return;
			}
			if (heads.Any(h => h.Number.Hash == parent.Number.Hash))
			{
				return; // our revision is still a head, so nothing to do
			}

			//TODO: I think this "direct descendant" limitation won't be enough
			//  when there are more than 2 people merging and there's a failure
			foreach (var head in heads)
			{
				if (head.IsDirectDescendantOf(parent))
				{
					repository.Update(head.Number.Hash);
					return;
				}
			}
			throw new ApplicationException("Could not find a head to update to.");
		}

		private string GetMergeCommitSummary(IList<string> peopleWeMergedWith, HgRepository repository)
		{
			var message  = "Merged with ";
			foreach (string id in peopleWeMergedWith)
			{
				message += id + ", ";
			}
			message= message.Remove(message.Length - 2); //chop off the trailing comma

			if (repository.GetChangedFiles().Any(s => s.EndsWith(".conflicts")))
			{
				message = message + " (conflicts)";
			}
			return message;

		}

		/// <summary>
		/// used for usb sources
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="repoDescriptor"></param>
		/// <returns>the uri of a successful clone</returns>
		private string TryToMakeCloneForSource(IProgress progress, RepositoryPath repoDescriptor)
		{
			List<string> possibleRepoCloneUris = repoDescriptor.GetPossibleCloneUris(RepoProjectName, progress);
			if (possibleRepoCloneUris == null)
			{
				progress.WriteMessage("No Uris available for cloning to {0}",
									  repoDescriptor.Name);
				return null;
			}
			else
			{
				foreach (string uri in possibleRepoCloneUris)
				{
					try
					{
						progress.WriteStatus("Making repository on {0} at {1}...", repoDescriptor.Name, uri);
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
			ExtraRepositorySources = new List<RepositoryPath>();
			ExtraRepositorySources.Add(RepositoryPath.Create(RepositoryPath.HardWiredSources.UsbKey, "UsbKey", false));
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

		public List<Revision> GetAllRevisions(IProgress progress)
		{
			HgRepository local = new HgRepository(_localRepositoryPath, progress);

			var revs= local.GetAllRevisions();
			return revs;
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
			GetRepository(new NullProgress()).SetUserNameInIni(userId, new NullProgress());
//           HgRepository.SetUserId(_localRepositoryPath, userId);
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

		public HgRepository GetRepository(IProgress progress)
		{
			return new HgRepository(_localRepositoryPath, progress);
		}


	}


	public class SyncResults
	{

	}
}