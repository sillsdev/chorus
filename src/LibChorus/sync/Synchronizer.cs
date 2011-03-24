using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using Palaso.Progress.LogBox;

namespace Chorus.sync
{
	/// <summary>
	/// Provides for synchronizing chorus repositories
	/// </summary>
	public class Synchronizer
	{
		#region Fields
		private DoWorkEventArgs _backgroundWorkerArguments;
		private BackgroundWorker _backgroundWorker;
		private string _localRepositoryPath;
		private ProjectFolderConfiguration _project;
		private IProgress _progress;
		private ChorusFileTypeHandlerCollection _handlers;
		public static readonly string RejectTagSubstring = "[reject]";
		//hack to prevent making change to custer repose when diagnosing problems... activated by -noPush commandline arg.
		public static bool s_testingDoNotPush;
		#endregion

		#region Properties

		public HgRepository Repository
		{
			get { return new HgRepository(_localRepositoryPath, _progress); }
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
		public List<RepositoryAddress> ExtraRepositorySources { get; private set; }

		#endregion

		#region Construction
	   public Synchronizer(string localRepositoryPath, ProjectFolderConfiguration project, IProgress progress)
		{
			_progress = progress;
			_project = project;
			_localRepositoryPath = localRepositoryPath;
			_handlers = ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers();
			ExtraRepositorySources = new List<RepositoryAddress>();
			ExtraRepositorySources.Add(RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "USB flash drive", false));
		}

		public static Synchronizer FromProjectConfiguration(ProjectFolderConfiguration project, IProgress progress)
		{
			var hg = HgRepository.CreateOrLocate(project.FolderPath, progress);
			return new Synchronizer(hg.PathToRepo, project, progress);

		}

		#endregion

		#region Public Methods


		public SyncResults SyncNow(SyncOptions options)
		{
			SyncResults results = new SyncResults();
			List<RepositoryAddress> sourcesToTry = options.RepositorySourcesToTry;
			//this just saves us from trying to connect twice to the same repo that is, for example, no there.
			Dictionary<RepositoryAddress, bool> connectionAttempts = new Dictionary<RepositoryAddress, bool>();

			try
			{
				HgRepository repo = new HgRepository(_localRepositoryPath, _progress);

				RemoveLocks(repo);
				repo.RecoverFromInterruptedTransactionIfNeeded();
				UpdateHgrc(repo);
				Commit(options);

				var workingRevBeforeSync = repo.GetRevisionWorkingSetIsBasedOn();

				CreateRepositoryOnLocalAreaNetworkFolderIfNeededThrowIfFails(repo, sourcesToTry);

				if (options.DoPullFromOthers)
				{
					results.DidGetChangesFromOthers = PullFromOthers(repo, sourcesToTry, connectionAttempts);
				}

				if (options.DoMergeWithOthers)
				{
					MergeHeadsOrRollbackAndThrow(repo, workingRevBeforeSync);
				}

				if (options.DoSendToOthers)
				{
					SendToOthers(repo, sourcesToTry, connectionAttempts);
				}

				UpdateToTheDescendantRevision(repo, workingRevBeforeSync);

				results.Succeeded = true;
			   _progress.WriteStatus("Done");
			}
			catch (SynchronizationException error)
			{
				error.DoNotifications(Repository, _progress);
				results.Succeeded = false;
				results.ErrorEncountered = error;
			}
			catch (UserCancelledException error)
			{
				results.Succeeded = false;
				results.Cancelled = true;
				results.ErrorEncountered = null;
			}
			catch (Exception error)
			{
				if (error.InnerException != null)
				{
					_progress.WriteVerbose("inner exception:");
					_progress.WriteError(error.InnerException.Message);
					_progress.WriteVerbose(error.InnerException.StackTrace);
				}

				_progress.WriteError(error.Message);
				_progress.WriteVerbose(error.StackTrace);

				results.Succeeded = false;
				results.ErrorEncountered = error;
			}
			return results;
		}

		private void CreateRepositoryOnLocalAreaNetworkFolderIfNeededThrowIfFails(HgRepository repo, List<RepositoryAddress> sourcesToTry)
		{
			RepositoryAddress directorySource = sourcesToTry.FirstOrDefault(s=>s is DirectoryRepositorySource);
			if(directorySource!=null)
			{
				if(!Directory.Exists(Path.Combine(directorySource.URI, ".hg")))
				{
					_progress.WriteMessage("Creating new repository at "+directorySource.URI);
					repo.CloneToRemoteDirectoryWithoutCheckout(directorySource.URI);
				}
			}
		}

		/// <summary>
		/// This version is used by the CHorus UI, which wants to do the sync in the background
		/// </summary>

		public SyncResults SyncNow(BackgroundWorker backgroundWorker, DoWorkEventArgs args, SyncOptions options)
		{
			_backgroundWorker = backgroundWorker;
			_backgroundWorkerArguments = args;
			var r=SyncNow(options);
			args.Result = r;
			return r;
		}

		public List<RepositoryAddress> GetPotentialSynchronizationSources()
		{
			try
			{
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
			catch (Exception error) // we've see an exception here when the hgrc was open by someone else
			{
				_progress.WriteException(error);
				_progress.WriteVerbose(error.ToString());
				return new List<RepositoryAddress>();
			}
		}




	   public void SetIsOneOfDefaultSyncAddresses(RepositoryAddress address, bool enabled)
		{
			Repository.SetIsOneDefaultSyncAddresses(address, enabled);
		}
		#endregion

	   #region Private Methods

	   private void SendToOthers(HgRepository repo, List<RepositoryAddress> sourcesToTry, Dictionary<RepositoryAddress, bool> connectionAttempt)
		{
			foreach (RepositoryAddress address in sourcesToTry)
			{
				ThrowIfCancelPending();

				if (!address.ReadOnly)
				{
					SendToOneOther(address, connectionAttempt, repo);
				}
			}
			ThrowIfCancelPending();
		}

		private void ThrowIfCancelPending()
		{
			if (_backgroundWorker != null && _backgroundWorker.CancellationPending)
			{
				_progress.WriteWarning("User cancelled operation.");
				_progress.WriteStatus("Cancelled.");
				_backgroundWorkerArguments.Cancel = true;
				throw new UserCancelledException();
			}
		}

		private void SendToOneOther(RepositoryAddress address, Dictionary<RepositoryAddress, bool> connectionAttempt, HgRepository repo)
		{
			try
			{
				string resolvedUri = address.GetPotentialRepoUri(RepoProjectName, _progress);

				bool canConnect;
				if (connectionAttempt.ContainsKey(address))
				{
					canConnect = connectionAttempt[address];
				}
				else
				{
					canConnect = address.CanConnect(repo, RepoProjectName, _progress);
					connectionAttempt.Add(address, canConnect);
				}
				if (canConnect)
				{
					if(s_testingDoNotPush)
					{
						_progress.WriteWarning("**Skipping push because s_testingDoNotPush is true");
					}
					else
					{
						repo.Push(address, resolvedUri, _progress);
					}

					// For usb, it's safe and desireable to do an update (bring into the directory
					// the latest files from the repo) for LAN, it could be... for now we assume it is.
					// For me (RandyR) including the shared network folder
					// failed to do the update and killed the process, which left a 'wlock' file
					// in the shared folder's '.hg' folder. No more S/Rs could then be done,
					// because the repo was locked.
					// For now, at least, it is not a requirement to do the update on the shared folder.
					// JDH Oct 2010: added this back in if it doesn't look like a shared folder
					if (address is UsbKeyRepositorySource  ||
					(address is DirectoryRepositorySource && ((DirectoryRepositorySource)address).LooksLikeLocalDirectory))
					{
						var otherRepo = new HgRepository(resolvedUri, _progress);
						otherRepo.Update();
					}
				}
				else if (address is DirectoryRepositorySource || address is UsbKeyRepositorySource)
				{
					TryToMakeCloneForSource(address);
					//nb: no need to push if we just made a clone
				}
			}
			catch (Exception error)
			{
				ExplainAndThrow(error, "Could to send to {0}({1}).", address.Name, address.URI);
			}
		}

		/// <returns>true if there were successful pulls</returns>
		private bool PullFromOthers(HgRepository repo,  List<RepositoryAddress> sourcesToTry, Dictionary<RepositoryAddress, bool> connectionAttempt)
		{
			bool didGetFromAtLeastOneSource = false;
			foreach (RepositoryAddress source in sourcesToTry)
			{
				ThrowIfCancelPending();

				if(PullFromOneSource(repo, source, connectionAttempt))
					didGetFromAtLeastOneSource = true;
				ThrowIfCancelPending();
			}
			return didGetFromAtLeastOneSource;
		}

		private void RemoveLocks(HgRepository repo)
		{
			ThrowIfCancelPending();
			if (!repo.RemoveOldLocks())
			{
				throw new SynchronizationException(null, WhatToDo.SuggestRestart, "Synchronization abandoned for now because of file or directory locks.");
			}
		}


		private void Commit(SyncOptions options)
		{
			ThrowIfCancelPending();
			_progress.WriteStatus("Storing changes in local repository...");

			using (var commitCop = new CommitCop(Repository, _handlers, _progress))
			{
				AddAndCommitFiles(options.CheckinDescription);

				if (!string.IsNullOrEmpty(commitCop.ValidationResult))
				{
					throw new ApplicationException( "The changed data did not pass validation tests. Your project will be moved back to the last Send/Receive before this problem occurred, so that you can keep working.  Please notify whoever provides you with computer support. Error was: " + commitCop.ValidationResult);
				}
			}

		}

		/// <returns>true if there was a successful pull</returns>
		private bool PullFromOneSource(HgRepository repo, RepositoryAddress source, Dictionary<RepositoryAddress, bool> connectionAttempt)
		{
			string resolvedUri = source.GetPotentialRepoUri(RepoProjectName, _progress);

			if (source is UsbKeyRepositorySource)
			{
				_progress.WriteStatus("Looking for USB flash drives...");
				var potential = source.GetPotentialRepoUri(RepoProjectName, _progress);
				if (null ==potential)
				{
					_progress.WriteWarning("No USB flash drive found");
				}
				else if (string.Empty == potential)
				{
					_progress.WriteMessage("Did not find existing project on any USB flash drive.");
				}
			}
			else
			{
				_progress.WriteStatus("Connecting to {0}...", source.Name);
			}
			var canConnect = source.CanConnect(repo, RepoProjectName, _progress);
			if (!connectionAttempt.ContainsKey(source))
			{
				connectionAttempt.Add(source, canConnect);
			}
			if (canConnect)
			{
				try
				{
					ThrowIfCancelPending();
				}
				catch(Exception error)
				{
					throw new SynchronizationException(error, WhatToDo.CheckSettings, "Error while pulling {0} at {1}", source.Name, resolvedUri);
				}
				//NB: this returns false if there was nothing to get.
				return repo.TryToPull(source.Name, resolvedUri);
			}
			else
			{
				if (source is UsbKeyRepositorySource)
				{
					//already informed them, above
					 return false;
				}
				else
				{
					return false;// this may be fine, even expected
					//throw new SynchronizationException(null, WhatToDo.CheckAddressAndConnection, "Could not connect to {0} at {1} for pulling", source.Name, resolvedUri);
				}
			}
		}

		/// <summary>
		/// put anything in the hgrc that chorus requires
		/// todo: kinda lame to do it every time, but when is better?
		/// </summary>
		private void UpdateHgrc(HgRepository repository)
		{
			ThrowIfCancelPending();
			try
			{
				//TODO: I think these can be removed now, since we have our own mercurial.ini
				string[] names = new string[]
									 {
										 "hgext.win32text", //for converting line endings on windows machines
										 "hgext.graphlog", //for more easily readable diagnostic logs
										 "convert" //for catastrophic repair in case of repo corruption
									 };
				repository.EnsureTheseExtensionAreEnabled(names);

//                List<string> extensions = new List<string>();
//
//                foreach (var handler in _handlers.Handers)
//                {
//                    extensions.AddRange(handler.GetExtensionsOfKnownTextFileTypes());
//                }
			}
			catch (Exception error)
			{
				ExplainAndThrow(error, "Could not prepare the mercurial settings");
			}

		}

		private void ExplainAndThrow(Exception exception, string explanation, params object[] args)
		{
			throw new ApplicationException(string.Format(explanation, args), exception);
		}

		private void ExplainAndThrow(Exception exception, WhatToDo whatToDo, string explanation, params object[] args)
		{
			throw new SynchronizationException(exception, whatToDo, string.Format(explanation, args));
		}

		[Flags]
		private enum WhatToDo
		{
			Nothing = 0,
			SuggestRestart = 1,
			VerifyIntegrity = 2,
			NeedExpertHelp = 4,
			CheckAddressAndConnection = 8,
			CheckSettings = 16
		}

		private class SynchronizationException : ApplicationException
		{
			public  WhatToDo WhatToDo { get; set; }

			public SynchronizationException(Exception exception, WhatToDo whatToDo, string explanation, params object[] args)
				:base(string.Format(explanation, args), exception)
			{
				WhatToDo = whatToDo;
			}

			public void DoNotifications(HgRepository repository, IProgress progress)
			{
				if(progress.CancelRequested)
				{
					progress.WriteWarning("Cancelled.");
					return;
				}
				if (InnerException != null)
				{
					progress.WriteVerbose("inner exception:");
					progress.WriteError(Message);
				}

				progress.WriteError(Message);
				progress.WriteVerbose(StackTrace);


				if ((WhatToDo & WhatToDo.CheckAddressAndConnection) > 0)
				{
					//todo: seems we could do some of this ourselves, like pinging the destination
					progress.WriteError("Check your network connection and server address, or try again later.");
				}

				if ((WhatToDo & WhatToDo.CheckSettings) > 0)
				{
					progress.WriteError("Check your server settings, such as project name, user name, and password.");
				}

				if ((WhatToDo & WhatToDo.VerifyIntegrity) > 0)
				{
					if (HgRepository.IntegrityResults.Bad == repository.CheckIntegrity(progress))
					{
						MessageBox.Show(
							"Bad news: The mecurial repository is damaged.  You will need to seek expert help to resolve this problem.", "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;//don't suggest anything else
					}
				}

				if ((WhatToDo & WhatToDo.SuggestRestart) > 0)
				{
					progress.WriteError("The problem might be helped by restarting your computer.");
				}
				if ((WhatToDo & WhatToDo.NeedExpertHelp) > 0)
				{
					progress.WriteError("You may need expert help.");
				}
			}
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
			try
			{
				var heads = repository.GetHeads();
				if (heads.Count == 1)
				{
					repository.Update(); //update to the tip
					return;
				}
				if (heads.Count == 0)
				{
					return;//nothing has been checked in, so we're done! (this happens during some UI tests)
				}

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

				_progress.WriteWarning("Staying at previous-tip (unusual)");
			}
			catch (Exception error)
			{
				  ExplainAndThrow(error, "Could not update.");
			}
		}

		private string GetMergeCommitSummary(string personMergedWith, HgRepository repository)
		{
			return "Merged with "+ personMergedWith;
		}

		/// <summary>
		/// used for local sources (usb, sd media, etc)
		/// </summary>
		/// <returns>the uri of a successful clone</returns>
		private string TryToMakeCloneForSource(RepositoryAddress repoDescriptor)
		{
			List<string> possibleRepoCloneUris = repoDescriptor.GetPossibleCloneUris(RepoProjectName, _progress);
			if (possibleRepoCloneUris == null)
			{
				_progress.WriteMessage("No Uris available for cloning to {0}",
									  repoDescriptor.Name);
				return null;
			}
			else
			{
				foreach (string uri in possibleRepoCloneUris)
				{
					try
					{
						_progress.WriteStatus("Copying repository to {0}...", repoDescriptor.GetFullName(uri));
						_progress.WriteVerbose("({0})", uri);
						HgHighLevel.MakeCloneFromLocalToLocal(_localRepositoryPath, uri, true, _progress);
						return uri;
					}
					catch (Exception error)
					{
						_progress.WriteError("Could not create repository on {0}. Error follow:", uri);
						_progress.WriteException(error);
						continue;
					}
				}
			}
			return null;
		}


		#region Merging
		private void MergeHeadsOrRollbackAndThrow(HgRepository repo, Revision workingRevBeforeSync)
		{
			try
			{
				MergeHeads();
			}
			catch (Exception error)
			{
				_progress.WriteException(error);
				_progress.WriteError("Rolling back...");
				UpdateToTheDescendantRevision(repo, workingRevBeforeSync); //rollback
				throw;
			}
		}

		private void MergeHeads()
		  {
			  try
			  {
				  List<string> peopleWeMergedWith = new List<string>();

				  List<Revision> heads = Repository.GetHeads();
				  Revision myHead = Repository.GetRevisionWorkingSetIsBasedOn();
				  if (myHead == default(Revision))
					  return;

				  foreach (Revision head in heads)
				  {
					  if (head.Number.LocalRevisionNumber == myHead.Number.LocalRevisionNumber)
						  continue;

					  if (head.Tag.Contains(RejectTagSubstring))
						  continue;

					  //note: what we're checking here is actualy the *name* of the branch... obviously
					  //they are different branches, or merge would not be needed.
					  if (head.Branch != myHead.Branch) //Chorus policy is to only auto-merge on branches with same name
						  continue;

					  //this is for posterity, on other people's machines, so use the hashes instead of local numbers
					  MergeSituation.PushRevisionsToEnvironmentVariables(myHead.UserId, myHead.Number.Hash, head.UserId,
																		 head.Number.Hash);

					  MergeOrder.PushToEnvironmentVariables(_localRepositoryPath);
					  _progress.WriteStatus("Merging with {0}...", head.UserId);
					  RemoveMergeObstacles(myHead, head);
					  bool didMerge = MergeTwoChangeSets(myHead, head);
					  if (didMerge)
					  {
						  peopleWeMergedWith.Add(head.UserId);

						  //that merge may have generated notes files where they didn't exist before,
						  //and we want these merged
						  //version + updated/created notes files to go right back into the repository

						  //  args.Append(" -X " + SurroundWithQuotes(Path.Combine(_pathToRepository, "**.ChorusRescuedFile")));


						  AddAndCommitFiles(GetMergeCommitSummary(head.UserId, Repository));
					  }
				  }
			  }
			  catch (Exception error)
			  {
				  ExplainAndThrow(error,WhatToDo.NeedExpertHelp, "Unable to complete the send/receive.");
			  }
		  }


		/// <returns>false if nothing needed to be merged, true if the merge was done. Throws exception if there is an error.</returns>
		private bool MergeTwoChangeSets(Revision head, Revision theirHead)
		{
#if MONO
			string chorusMergeFilePath = Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly, "chorusmerge");
#else
			string chorusMergeFilePath = Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly, "ChorusMerge.exe");
#endif
			using (new ShortTermEnvironmentalVariable("HGMERGE", '"' + chorusMergeFilePath + '"'))
			{
				using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.TheyWin.ToString()))
				{
					return Repository.Merge(_localRepositoryPath, theirHead.Number.LocalRevisionNumber);
				}
			}
		}


#endregion

		private void AddAndCommitFiles(string summary)
		{
			List<string> includePatterns = _project.IncludePatterns;
			includePatterns.Add("**.ChorusNotes");
			List<string> excludePatterns = _project.ExcludePatterns;
			includePatterns.Add("**.ChorusRescuedFile");
			Repository.AddAndCheckinFiles(includePatterns, excludePatterns,
										  summary);
		}

		/// <summary>
		/// There may be more, but for now: take care of the case where one guy has a file not
		/// modified (and not checked in), and the other guy is going to hammer it (with a remove
		/// or change).
		/// </summary>
		private void RemoveMergeObstacles(Revision rev1, Revision rev2)
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
							var newPath = file.FullPath + "-" + Path.GetRandomFileName() + ".ChorusRescuedFile";

							_progress.WriteWarning(
								"Renamed {0} to {1} because it is not part of {2}'s repository but it is part of {3}'s, and this would otherwise prevent a merge.",
								file.FullPath, Path.GetFileName(newPath), rev1.UserId, rev2.UserId);

							if (!File.Exists(file.FullPath))
							{
								_progress.WriteError("The file marked for rescuing didn't actually exist.  Please report this bug in Chorus.");
								continue;
							}
							File.Move(file.FullPath, newPath);
						}
						catch (Exception error)
						{
							_progress.WriteError("Could not move the file. Error follows.");
							_progress.WriteException(error);
							throw;
						}
					}
				}
			}
		}

	   #endregion

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

		public bool Cancelled { get; set; }

		public SyncResults()
		{
			Succeeded = true;
			DidGetChangesFromOthers = false;
		}
	}
}
