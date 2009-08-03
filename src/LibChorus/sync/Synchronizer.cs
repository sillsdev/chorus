using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.sync
{
	/// <summary>
	/// Provides for synchronizing chorus repositories
	/// </summary>
	public class Synchronizer
	{
		private string _localRepositoryPath;
		private ProjectFolderConfiguration _project;
		private IProgress _progress;

		public List<RepositoryAddress> ExtraRepositorySources { get; private set; }


		public Synchronizer(string localRepositoryPath, ProjectFolderConfiguration project)
		{
			_project = project;
			_localRepositoryPath = localRepositoryPath;
			ExtraRepositorySources = new List<RepositoryAddress>();
			ExtraRepositorySources.Add(RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "UsbKey", false));
		}



		public static Synchronizer FromProjectConfiguration(ProjectFolderConfiguration project, IProgress progress)
		{
			var hg = HgRepository.CreateOrLocate(project.FolderPath, progress);
			return new Synchronizer(hg.PathToRepo, project);

		}

		public List<RepositoryAddress> GetPotentialSynchronizationSources(IProgress progress)
		{
			_progress = progress;
			var list = new List<RepositoryAddress>();
			list.AddRange(ExtraRepositorySources);
			var repo = Repository;
			list.AddRange(repo.GetRepositoryPathsInHgrc());
			var defaultSyncAliases = repo.GetDefaultSyncAliases();
			foreach (var path in list)
			{
				path.Enabled = defaultSyncAliases.Contains(path.Name);
			}

			return list;
		}


		public string RepoProjectName
		{
			get { return Path.GetFileNameWithoutExtension(_localRepositoryPath)+Path.GetExtension(_localRepositoryPath); }
		}

		public RepositoryAddress UsbPath
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


		public SyncResults SyncNow(SyncOptions options, IProgress progress)
		{
			_progress = progress;
			SyncResults results = new SyncResults();

			HgRepository repo = new HgRepository(_localRepositoryPath,progress);

			repo.RecoverIfNeeded();
			if (!repo.RemoveOldLocks())
			{
				progress.WriteError("Synchronization abandoned for now.  Try again after restarting the computer.");
				results.Succeeded = false;
				return results;
			}

			progress.WriteStatus("Checking In...");
			repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, options.CheckinDescription);

			var tipBeforeSync = repo.GetTip();
			List<RepositoryAddress> sourcesToTry = options.RepositorySourcesToTry;

			//if the client didn't specify any, try them all
//            no, don't do that.  It's reasonable to just be doing a local checkin
//            if(repositoriesToTry==null || repositoriesToTry.Count == 0)
//                repositoriesToTry = ExtraRepositorySources;

			if (options.DoPullFromOthers)
			{
				foreach (RepositoryAddress source in sourcesToTry)
				{
					string resolvedUri = source.GetPotentialRepoUri(RepoProjectName, progress);
					if (source.CanConnect(repo, RepoProjectName, progress))
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
				IList<string> peopleWeMergedWith = MergeHeads(progress, results);

				//that merge may have generated conflict files, and we want these merged
				//version + updated/created conflict files to go right back into the repository
				if (peopleWeMergedWith.Count > 0)
				{
					repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, GetMergeCommitSummary(peopleWeMergedWith, repo));
				}
			}

			if(options.DoPushToLocalSources)
			{
				foreach (RepositoryAddress repoDescriptor in sourcesToTry)
				{
					if (!repoDescriptor.ReadOnly)
					{
						string resolvedUri = repoDescriptor.GetPotentialRepoUri(RepoProjectName, progress);
						if (repoDescriptor.CanConnect(repo, RepoProjectName, progress))
						{
							progress.WriteMessage("Pushing local repository to {0} at {1}", RepoProjectName, resolvedUri);
							repo.Push(resolvedUri, progress);
						}
						else if(repoDescriptor is DirectoryRepositorySource || repoDescriptor is UsbKeyRepositorySource)
						{
							TryToMakeCloneForSource(progress, repoDescriptor);
							//nb: no need to push if we just made a clone
						}
					}
				}
			}
			try
			{
				UpdateToTheDescendantRevision(repo, tipBeforeSync);
			}
			catch (Exception error)
			{
				progress.WriteError("The command timed out.  Details: " + error.Message);
				results.Succeeded = false;
			}
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
		private string TryToMakeCloneForSource(IProgress progress, RepositoryAddress repoDescriptor)
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



		/// <summary>
		///
		/// </summary>
		 /// <returns>path to clone, or empty if it failed</returns>
		public string MakeClone(string newDirectory, bool alsoDoCheckout, IProgress progress)
		{
			_progress = progress;
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

			if (!local.RemoveOldLocks())
			{
				progress.WriteError("Chorus could not create the clone at this time.  Try again after restarting the computer.");
				return string.Empty;
			}

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

		public HgRepository Repository
		{
			get { return new HgRepository(_localRepositoryPath, _progress); }
		}



		/// <summary>
		/// note: intentionally does not commit afterwards
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="results"></param>
		private IList<string> MergeHeads(IProgress progress, SyncResults results)
		{
			List<string> peopleWeMergedWith = new List<string>();

			List<Revision> heads = Repository.GetHeads();
			Revision myHead = Repository.GetRevisionWorkingSetIsBasedOn();
			foreach (Revision head in heads)
			{
				MergeSituation.PushRevisionsToEnvironmentVariables(myHead.UserId, myHead.Number.LocalRevisionNumber, head.UserId, head.Number.LocalRevisionNumber);

				MergeOrder.PushToEnvironmentVariables(_localRepositoryPath);
				if (head.Number.LocalRevisionNumber != myHead.Number.LocalRevisionNumber)
				{
					progress.WriteStatus("Merging with {0}...", head.UserId);
					RemoveMergeObstacles(myHead, head, progress);
					bool didMerge = MergeTwoChangeSets(myHead, head);
					if (didMerge)
					{
						peopleWeMergedWith.Add(head.UserId);
					}
				}
			}

			return peopleWeMergedWith;
		}

		/// <summary>
		/// There may be more, but for now: take care of the case where one guy has a file not
		/// modified (and not checked in), and the other guy is going to hammer it (with a remove
		/// or change).
		/// </summary>
		private void RemoveMergeObstacles(Revision rev1, Revision rev2, IProgress progress)
		{
			//todo: push down to hgrepository
			var files = Repository.GetFilesInRevisionFromQuery(rev1 /*this param is bogus*/, "status -ru --rev " + rev2.Number.LocalRevisionNumber);

			foreach (var file in files)
			{
				if (file.ActionThatHappened == FileInRevision.Action.Unknown)
				{
					if (files.Any(f => f.FullPath == file.FullPath))
					{
						 var newPath = file.FullPath + "-" + Path.GetRandomFileName() + ".chorusRescue";
						File.Move(file.FullPath, newPath);
						progress.WriteWarning("Renamed {0} to {1} because it is not part of {2}'s repository but it is part of {3}'s, and this would otherwise prevent a merge.", file.FullPath, Path.GetFileName(newPath), rev1.UserId, rev2.UserId);
					}
				}
			}
		}



		private bool MergeTwoChangeSets(Revision head, Revision theirHead)
		{
			using (new ShortTermEnvironmentalVariable("HGMERGE", Path.Combine(Other.DirectoryOfExecutingAssembly, "ChorusMerge.exe")))
			{
				using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.TheyWin.ToString()))
				{
					return Repository.Merge(_localRepositoryPath, theirHead.Number.LocalRevisionNumber);
				}
			}
		}

		public void SetIsOneDefaultSyncAddresses(RepositoryAddress address, bool enabled)
		{
			Repository.SetIsOneDefaultSyncAddresses(address, enabled);
		}
	}


	public class SyncResults
	{
		public bool Succeeded { get; set; }

		public SyncResults()
		{
			Succeeded = true;
		}
	}
}