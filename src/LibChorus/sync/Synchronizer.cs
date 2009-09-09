using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private DoWorkEventArgs _backgroundWorkerArguments;
		private BackgroundWorker _backgroundWorker;

		private string _localRepositoryPath;
		private ProjectFolderConfiguration _project;
		private IProgress _progress;

		public List<RepositoryAddress> ExtraRepositorySources { get; private set; }


		public Synchronizer(string localRepositoryPath, ProjectFolderConfiguration project)
		{
			_project = project;
			_localRepositoryPath = localRepositoryPath;
			ExtraRepositorySources = new List<RepositoryAddress>();
			ExtraRepositorySources.Add(RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "USB flash drive", false));
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

		private bool ShouldCancel(ref SyncResults results)
		{
			if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
			{
				_progress.WriteWarning("User cancelled operation.");
				_progress.WriteStatus("Cancelled.");
				results.Succeeded = false;//enhance: switch to success/cancelled/errors or something
				_backgroundWorkerArguments.Cancel = true;
				return true;
			}
			return false;
		}

		public SyncResults SyncNow(BackgroundWorker backgroundWorker, DoWorkEventArgs args, SyncOptions options, IProgress progress)
		{
			_backgroundWorker = backgroundWorker;
			_backgroundWorkerArguments = args;
			return SyncNow(options, progress);
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

			progress.WriteStatus("Storing changes in local repository...");

			try
			{
				repo.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, options.CheckinDescription);
			}
			catch (Exception error)
			{
				progress.WriteError(error.Message);
				results.Succeeded = false;
				results.DidGetChangesFromOthers = false;
				results.ErrorEncountered = error;
				return results;
			}

			var workingRevBeforeSync = repo.GetRevisionWorkingSetIsBasedOn();
			List<RepositoryAddress> sourcesToTry = options.RepositorySourcesToTry;

			//if the client didn't specify any, try them all
//            no, don't do that.  It's reasonable to just be doing a local checkin
//            if(repositoriesToTry==null || repositoriesToTry.Count == 0)
//                repositoriesToTry = ExtraRepositorySources;

			//this just saves us from trying to connect twice to the same repo that is, for example, no there.
			Dictionary<RepositoryAddress, bool> didConnect= new Dictionary<RepositoryAddress, bool>();

			if (options.DoPullFromOthers)
			{
				foreach (RepositoryAddress source in sourcesToTry)
				{
					if (ShouldCancel(ref results)){return results;}

					string resolvedUri = source.GetPotentialRepoUri(RepoProjectName, progress);

					if (source is UsbKeyRepositorySource)
					{
						progress.WriteStatus("Looking for USB flash drives...");
						var potential = source.GetPotentialRepoUri(RepoProjectName, progress);
						if (null ==potential)
						{
							progress.WriteWarning("None found");
						}
						else if (string.Empty == potential)
						{
							progress.WriteMessage("Did not find existing project on any USB key.");
						}
					}
					else
					{
						progress.WriteStatus("Connecting to {0}...", source.Name);
					}
					var canConnect = source.CanConnect(repo, RepoProjectName, progress);
					if (!didConnect.ContainsKey(source))
					{
						didConnect.Add(source, canConnect);
					}
					if (canConnect)
					{
						if (repo.TryToPull(source.Name,  resolvedUri))
						{
							results.DidGetChangesFromOthers = true; //nb, don't set it to false just because one source didn't have anything new
						}
					}
					else
					{
						if (source is UsbKeyRepositorySource)
						{
						   //already informed them, above
						}
						else
						{
							progress.WriteWarning("Could not connect to {0} at {1} for pulling", source.Name, resolvedUri);
						}
					}
				}
			}

			if (options.DoMergeWithOthers)
			{
				try
				{
					MergeHeads(progress, results);
				}
				catch (Exception error)
				{
					_progress.WriteError(error.Message);
					_progress.WriteError("Unable to complete the send/receive.  You can try restarting the computer, but you may need expert help to fix this problem.");

					//rollback
					UpdateToTheDescendantRevision(repo, workingRevBeforeSync);

					results.Succeeded = false;
					return results;
				}
			}

			if(options.DoPushToLocalSources)
			{
				foreach (RepositoryAddress address in sourcesToTry)
				{
					if (ShouldCancel(ref results)) { return results; }

					if (!address.ReadOnly)
					{
						string resolvedUri = address.GetPotentialRepoUri(RepoProjectName, progress);
						bool canConnect;
						if (didConnect.ContainsKey(address))
						{
							canConnect = didConnect[address];
						}
						else
						{
							canConnect = address.CanConnect(repo, RepoProjectName, progress);
							didConnect.Add(address, canConnect);
						}
						if (canConnect)
							{
								repo.Push(address, resolvedUri, progress);
							}
							else if (address is DirectoryRepositorySource || address is UsbKeyRepositorySource)
							{
								TryToMakeCloneForSource(progress, address);
								//nb: no need to push if we just made a clone
							}
					}
				}
			}
			try
			{
				UpdateToTheDescendantRevision(repo, workingRevBeforeSync);
			}
			catch (Exception error)
			{
				progress.WriteError("The command timed out.  Details: " + error.Message);
				results.Succeeded = false;
				results.ErrorEncountered = error;
				results.DidGetChangesFromOthers = false;
			}
			progress.WriteStatus("Done");
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
			//if (heads.Any(h => h.Number.Hash == parent.Number.Hash))
			//{
				//return; // our revision is still a head, so nothing to do
			//}

			//TODO: I think this "direct descendant" limitation won't be enough
			//  when there are more than 2 people merging and there's a failure
			foreach (var head in heads)
			{
				if (parent.Number.Hash == head.Number.Hash || head.IsDirectDescendantOf(parent))
				{
					repository.RollbackWorkingDirectoryToRevision(head.Number.LocalRevisionNumber);
					return;
				}
			}
			//don't know if this would ever happen, but it's better than stayin in limbo
			_progress.WriteError("Unexpected drop back to previous-tip");
		}

		private string GetMergeCommitSummary(string personMergedWith, HgRepository repository)
		{
			var message  = "Merged with "+ personMergedWith;

			if (repository.GetChangedFiles().Any(s => s.EndsWith(".conflicts")))
			{
				message = message + " (conflicts)";
			}
			return message;

		}

		/// <summary>
		/// used for local sources (usb, sd media, etc)
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
						progress.WriteStatus("Copying repository to {0}...", repoDescriptor.GetFullName(uri));
						progress.WriteVerbose("({0})", uri);
						MakeClone(uri, true, progress);
						return uri;
					}
					catch (Exception error)
					{
						 progress.WriteError("Could not create repository on {0}: {1}", uri, error.Message);
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
				throw new ArgumentException(String.Format("The directory must not already exist ({0})", newDirectory));
			}
			string parent = Directory.GetParent(newDirectory).FullName;
			if (!Directory.Exists(parent))
			{
				throw new ArgumentException(String.Format("The parent of the given directory must already exist ({0})", parent));
			}
			HgRepository local = new HgRepository(_localRepositoryPath, progress);

			if (!local.RemoveOldLocks())
			{
				progress.WriteError("Chorus could not create the clone at this time.  Try again after restarting the computer.");
				return string.Empty;
			}

			using (new ConsoleProgress("Creating repository clone at {0}", newDirectory))
			{
				local.CloneLocal(newDirectory);
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
		/// <returns>A list of people that actually needed merging with.  Throws exception if there is an error.</returns>
		private List<string> MergeHeads(IProgress progress, SyncResults results)
		{
			List<string> peopleWeMergedWith = new List<string>();

			List<Revision> heads = Repository.GetHeads();
			Revision myHead = Repository.GetRevisionWorkingSetIsBasedOn();
			if (myHead == default(Revision))
				return peopleWeMergedWith;

			foreach (Revision head in heads)
			{
				//this is for posterity, on other people's machines, so use the hashes instead of local numbers
				MergeSituation.PushRevisionsToEnvironmentVariables(myHead.UserId, myHead.Number.Hash, head.UserId, head.Number.Hash);

				MergeOrder.PushToEnvironmentVariables(_localRepositoryPath);
				if (head.Number.LocalRevisionNumber != myHead.Number.LocalRevisionNumber)
				{
					progress.WriteStatus("Merging with {0}...", head.UserId);
					RemoveMergeObstacles(myHead, head, progress);
					bool didMerge = MergeTwoChangeSets(myHead, head);
					if (didMerge)
					{
						peopleWeMergedWith.Add(head.UserId);
						//that merge may have generated conflict files, and we want these merged
						//version + updated/created conflict files to go right back into the repository
						Repository.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns, GetMergeCommitSummary(head.UserId, Repository));

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
			/* this has proved a bit hard to get right.
			 * when a file is in a recently brought in changeset, and also local (but untracked), status --rev ___ lists the file twice:
			 *
			 * >hg status --rev 14
			 * R test.txt
			 * ? test.txt
			 *
			 */

			//todo: push down to hgrepository
			var files = Repository.GetFilesInRevisionFromQuery(rev1 /*this param is bogus*/, "status -ru --rev " + rev2.Number.LocalRevisionNumber);

			foreach (var file in files)
			{
				if (file.ActionThatHappened == FileInRevision.Action.Deleted)// listed with 'R'
				{
					//is it also listed as unknown?
					if (files.Any(f => f.FullPath == file.FullPath && f.ActionThatHappened == FileInRevision.Action.Unknown))
					{
						try
						{
							var newPath = file.FullPath + "-" + Path.GetRandomFileName() + ".chorusRescue";

							progress.WriteWarning(
								"Renamed {0} to {1} because it is not part of {2}'s repository but it is part of {3}'s, and this would otherwise prevent a merge.",
								file.FullPath, Path.GetFileName(newPath), rev1.UserId, rev2.UserId);

							if (!File.Exists(file.FullPath))
							{
								progress.WriteError("The file marked for rescuing didn't actually exist.  Please report this bug in Chorus.");
								continue;
							}
							File.Move(file.FullPath, newPath);
						}
						catch (Exception error)
						{
							progress.WriteError("Could not move the file. Error was: {0}", error.Message);
							throw;
						}
					}
				}
			}
		}


		/// <returns>false if nothing needed to be merged, true if the merge was done. Throws exception if there is an error.</returns>
		private bool MergeTwoChangeSets(Revision head, Revision theirHead)
		{
#if MONO
			string chorusMergeFilePath = Path.Combine(Other.DirectoryOfExecutingAssembly, "chorusmerge");
#else
			string chorusMergeFilePath = Path.Combine(Other.DirectoryOfExecutingAssembly, "ChorusMerge.exe");
#endif
			using (new ShortTermEnvironmentalVariable("HGMERGE", chorusMergeFilePath))
			{
				using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.TheyWin.ToString()))
				{
					return Repository.Merge(_localRepositoryPath, theirHead.Number.LocalRevisionNumber);
				}
			}
		}

		public void SetIsOneOfDefaultSyncAddresses(RepositoryAddress address, bool enabled)
		{
			Repository.SetIsOneDefaultSyncAddresses(address, enabled);
		}
	}


	public class SyncResults
	{
		public bool Succeeded { get; set; }

		/// <summary>
		/// If if this is true, the client app needs to restart or read in the new stuff
		/// </summary>
		public bool DidGetChangesFromOthers { get; set; }

		public Exception ErrorEncountered
		{
			get; set;
		}

		public SyncResults()
		{
			Succeeded = true;
			DidGetChangesFromOthers = false;
		}
	}
}