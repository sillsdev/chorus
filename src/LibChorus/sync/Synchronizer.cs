using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using Chorus.Model;
using SIL.Progress;
using SIL.Reporting;
using SIL.Xml;

namespace Chorus.sync
{
	/// <summary>
	/// Provides for synchronizing chorus repositories
	/// </summary>
	public class Synchronizer
	{
		#region Fields

		private ISychronizerAdjunct _sychronizerAdjunct = new DefaultSychronizerAdjunct();
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
			// REVIEW (Hasso) 2020.10: should this be cached?
			get { return new HgRepository(_localRepositoryPath, _progress); }
		}

		public string RepoProjectName => Path.GetFileNameWithoutExtension(_localRepositoryPath) + Path.GetExtension(_localRepositoryPath);

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

		/// <summary>
		/// Sets the SychronizerAdjunct property to the given ISychronizerAdjunct instance.
		/// </summary>
		/// <remarks>
		/// Setting the property to null will result in the default, do-nothing, interface implementation.
		///
		/// </remarks>
		public ISychronizerAdjunct SynchronizerAdjunct
		{
			internal get { return _sychronizerAdjunct; } // For testing.
			set
			{
				_sychronizerAdjunct = value ?? new DefaultSychronizerAdjunct();
			}
		}

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
			var hg = HgRepository.CreateOrUseExisting(project.FolderPath, progress);
			return new Synchronizer(hg.PathToRepo, project, progress);

		}

		#endregion

		#region Public Methods


		public SyncResults SyncNow(SyncOptions options)
		{
			SyncResults results = new SyncResults();
			List<RepositoryAddress> sourcesToTry = options.RepositorySourcesToTry;
			// this saves us from trying to connect twice to the same repo that is, for example, not there.
			var connectionAttempts = new Dictionary<RepositoryAddress, bool>();

			try
			{
				_progress.ProgressIndicator?.IndicateUnknownProgress();
				var repo = new HgRepository(_localRepositoryPath, _progress);
				repo.LogBasicInfo(sourcesToTry);

				RemoveLocks(repo);
				repo.RecoverFromInterruptedTransactionIfNeeded();
				repo.FixUnicodeAudio();
				string branchName = _sychronizerAdjunct.BranchName;
				ChangeBranchIfNecessary(branchName);
				Commit(options);

				var workingRevBeforeSync = repo.GetRevisionWorkingSetIsBasedOn();

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

				//If we did pull any data or a trivial merge succeeded we should call UpdateToTheDescendantRevision
				if (results.DidGetChangesFromOthers || //we pulled something
					(workingRevBeforeSync!=null //will be null if this is the 1st checkin ever, but no files were added so there was no actual rev created
					&& !repo.GetRevisionWorkingSetIsBasedOn().Number.Hash.Equals(workingRevBeforeSync.Number.Hash))) //a merge happened
				{
					UpdateToTheDescendantRevision(repo, workingRevBeforeSync);
				}
				_sychronizerAdjunct.CheckRepositoryBranches(repo.BranchingHelper.GetBranches(), _progress);

				results.Succeeded = true;
				_progress.WriteMessage("Done");
			}
			catch (SynchronizationException error)
			{
				error.DoNotifications(Repository, _progress);
				results.Succeeded = false;
				results.ErrorEncountered = error;
			}
			catch (UserCancelledException)
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

				_progress.WriteException(error);//this preserves the whole exception for later retrieval by the client
				_progress.WriteError(error.Message);//review still needed if we have this new WriteException?
				_progress.WriteVerbose(error.StackTrace);//review still needed if we have this new WriteException?

				results.Succeeded = false;
				results.ErrorEncountered = error;
			}
			finally
			{
				_progress.WriteVerbose($"Finished at {DateTime.UtcNow:u}");
			}
			return results;
		}

		private void ChangeBranchIfNecessary(string branchName)
		{
			if (Repository.GetRevisionWorkingSetIsBasedOn() == null ||
				Repository.GetRevisionWorkingSetIsBasedOn().Branch != branchName)
			{
				Repository.BranchingHelper.Branch(_progress, branchName);
			}
		}

		/// <summary>
		/// This version is used by the Chorus UI, which wants to do the sync in the background
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
			catch (Exception error) // we've seen an exception here when the hgrc was open by someone else
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

				if (!address.IsReadOnly)
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
				_progress.WriteMessage("Operation cancelled.");
				_backgroundWorkerArguments.Cancel = true;
				throw new UserCancelledException();
			}
		}

		private void SendToOneOther(RepositoryAddress address, Dictionary<RepositoryAddress, bool> connectionAttempt, HgRepository repo)
		{
			try
			{
				var resolvedUri = address.GetPotentialRepoUri(Repository.Identifier, RepoProjectName, _progress);

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
						repo.Push(address, resolvedUri);
					}

					// For USB, we do not wish to do an update, since it can cause problems if the working
					// files are available to the user.
					// The update is only done for tests, since only tests now use "DirectoryRepositorySource".
					if (address is DirectoryRepositorySource && ((DirectoryRepositorySource) address).LooksLikeLocalDirectory)
					{
						// passes false to avoid updating the hgrc on a send to preserve backward compatibility
						var otherRepo = new HgRepository(resolvedUri, false, _progress);
						otherRepo.Update();
					}
				}
				else if (address is UsbKeyRepositorySource || address is DirectoryRepositorySource)
				{
					// If we cannot connect to a USB or Directory source (the repository doesn't exist),
					// try to clone our repository onto the source
					TryToMakeCloneForSource(address);
					//nb: no need to push if we just made a clone
				}
			}
			catch (UserCancelledException)
			{
				throw;
			}
			catch (Exception error)
			{
				ExplainAndThrow(error, "Failed to send to {0} ({1}).", address.Name, address.URI);
			}
		}

		/// <returns>true if there was at least one successful pull</returns>
		private bool PullFromOthers(HgRepository repo,  List<RepositoryAddress> sourcesToTry, Dictionary<RepositoryAddress, bool> connectionAttempt)
		{
			bool didGetFromAtLeastOneSource = false;
			foreach (RepositoryAddress source in new List<RepositoryAddress>(sourcesToTry)) // LT-18276: apparently possible to modify sourcesToTry
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
			_progress.WriteMessage("Storing changes in local repository...");

			_sychronizerAdjunct.PrepareForInitialCommit(_progress);

			// Must be done, before "AddAndCommitFiles" call.
			// It could be here, or first thing inside the 'using' for CommitCop.
			string tooLargeFilesMessage = LargeFileFilter.FilterFiles(Repository, _project, _handlers);
			if (!string.IsNullOrEmpty(tooLargeFilesMessage))
			{
				var msg = "We're sorry, but the Send/Receive system can't handle large files. The following files won't be stored or shared by this system until you can shrink them down below the maximum: "+Environment.NewLine;
				msg+= tooLargeFilesMessage;
				_progress.WriteWarning(msg);
			}

			var commitCopValidationResult = "";
			using (var commitCop = new CommitCop(Repository, _handlers, _progress))
			{
				AddAndCommitFiles(options.CheckinDescription);
				// The validation checking must come after the commit, otherwise files being added to the repository will not be validated
				commitCopValidationResult = commitCop.ValidationResult;
			}
			if (string.IsNullOrEmpty(commitCopValidationResult))
				return;

			// Commit cop reported a validation failure, but deal with it here, rather than inside the 'using', as the rollback won't have happened,
			// until Dispose, and that is way too early for the "SimpleUpdate" call.
			_sychronizerAdjunct.SimpleUpdate(_progress, true);
			throw new ApplicationException(
					"The changed data did not pass validation tests. Your project will be moved back to the last Send/Receive before this problem occurred, so that you can keep working.  Please notify whoever provides you with computer support. Error was: " +
					commitCopValidationResult);
		}

		/// <returns>true if there was a successful pull</returns>
		private bool PullFromOneSource(HgRepository repo, RepositoryAddress source, Dictionary<RepositoryAddress, bool> connectionAttempt)
		{
			var resolvedUri = source.GetPotentialRepoUri(repo.Identifier, RepoProjectName, _progress);

			if (source is UsbKeyRepositorySource)
			{
				_progress.WriteMessage("Looking for USB flash drives...");
				var potential = source.GetPotentialRepoUri(repo.Identifier, RepoProjectName, _progress);
				if (null == potential)
				{
					_progress.WriteWarning("No USB flash drive found");
				}
				else if (string.Empty == potential)
				{
					_progress.WriteMessage("Did not find this project on any USB flash drive.");
				}
			}
			else
			{
				_progress.WriteMessage("Connecting to {0}...", source.Name);
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
					throw new SynchronizationException(error, WhatToDo.CheckSettings,
						"Error while pulling {0} at {1}", source.Name, ServerSettingsModel.RemovePasswordForLog(resolvedUri));
				}
				//NB: this returns false if there was nothing to get.
				try
				{
					return repo.Pull(source, resolvedUri);
				}
				catch (HgCommonException err)
				{
					// These kinds of errors are worth an immediate dialog, to make sure we get the user's attention.
					ErrorReport.NotifyUserOfProblem(err.Message);
					// The main sync routine will catch the exception, abort any other parts of the Send/Receive,
					// and log the problem.
					throw;
				}
				// Any other kind of exception will be caught and logged at a higher level.
			}

			if (source is UsbKeyRepositorySource)
			{
				//already informed them, above
				return false;
			}

			_progress.WriteError("Could not connect to {0} at {1}", source.Name, ServerSettingsModel.RemovePasswordForLog(resolvedUri));
			return false;
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
						throw new ApplicationException(
							"Bad news: The mecurial repository is damaged.  You will need to seek expert help to resolve this problem."
						);
						// Removing windows forms dependency CP 2012-08
						//MessageBox.Show(
						//    "Bad news: The mecurial repository is damaged.  You will need to seek expert help to resolve this problem.", "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Error);
						//return;//don't suggest anything else
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
		/// the "tip" might be the other guy's unmergable data (maybe because he has a newer
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
				//if there is only one head for the branch we started from just update
				if (heads.Count(rev => rev.Branch == parent.Branch) == 1)
				{
					repository.Update(); //update to the tip
					_sychronizerAdjunct.SimpleUpdate(_progress, false);
					return;
				}
				if (heads.Count == 0)
				{
					return;//nothing has been checked in, so we're done! (this happens during some UI tests)
				}

				//  when there are more than 2 people merging and there's a failure or a no-op merge happened
				foreach (var head in heads)
				{
					if (parent.Number.Hash == head.Number.Hash || (head.Branch == parent.Branch && head.IsDirectDescendantOf(parent)))
					{
						repository.RollbackWorkingDirectoryToRevision(head.Number.LocalRevisionNumber);
						_sychronizerAdjunct.SimpleUpdate(_progress, true);
						return;
					}
				}

				_progress.WriteWarning("Staying at previous-tip (unusual). Other users recent changes will not be visible.");
			}
			catch (UserCancelledException)
			{
				throw;
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
		private void TryToMakeCloneForSource(RepositoryAddress repoDescriptor)
		{
			var possibleRepoCloneUris = repoDescriptor.GetPossibleCloneUris(Repository.Identifier, RepoProjectName, _progress);
			if (possibleRepoCloneUris == null)
			{
				_progress.WriteMessage("No Uris available for cloning to {0}",
									  repoDescriptor.Name);
				return;
			}

			foreach (var uri in possibleRepoCloneUris)
			{
				// target may be uri, or some other folder.
				var target = HgRepository.GetUniqueFolderPath(
					_progress,
					//"Folder at {0} already exists, so it can't be used. Creating clone in {1}, instead.",
					RepositoryAddress.DuplicateWarningMessage.Replace(RepositoryAddress.MediumVariable, "USB flash drive"),
					uri);
				try
				{
					_progress.WriteMessage("Copying repository to {0}...", repoDescriptor.GetFullName(target));
					_progress.WriteVerbose("({0})", target);
					HgHighLevel.MakeCloneFromLocalToUsb(_localRepositoryPath, target, _progress);
					return;
				}
				catch (Exception error)
				{
					_progress.WriteError("Could not create repository on {0}. Error follow:", target);
					_progress.WriteException(error);
					// keep looping
				}
			}
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
				foreach (var chorusMergeProcess in Process.GetProcessesByName("ChorusMerge"))
				{
					_progress.WriteWarning(string.Format("Killing ChorusMerge Process: '{0}'...", chorusMergeProcess.Id));
					chorusMergeProcess.Kill();
				}
				_progress.WriteException(error);
				_progress.WriteError("Rolling back...");
				UpdateToTheDescendantRevision(repo, workingRevBeforeSync); //rollback
				throw;
			}
		}

		/// <summary>
		/// Sets up everything necessary for a call out to the ChorusMerge executable
		/// </summary>
		/// <param name="targetHead"></param>
		/// <param name="sourceHead"></param>
		private void PrepareForMergeAttempt(Revision targetHead, Revision sourceHead)
		{
			//this is for posterity, on other people's machines, so use the hashes instead of local numbers
			MergeSituation.PushRevisionsToEnvironmentVariables(targetHead.UserId, targetHead.Number.Hash,
															   sourceHead.UserId, sourceHead.Number.Hash);

			MergeOrder.PushToEnvironmentVariables(_localRepositoryPath);
			_progress.WriteMessage("Merging {0} and {1}...", targetHead.UserId, sourceHead.UserId);
			_progress.WriteVerbose("   Revisions {0}:{1} with {2}:{3}...", targetHead.Number.LocalRevisionNumber, targetHead.Number.Hash,
								   sourceHead.Number.LocalRevisionNumber, sourceHead.Number.Hash);
			RemoveMergeObstacles(targetHead, sourceHead);
		}

		/// <summary>
		/// This method handles post merge tasks including the commit after the merge
		/// </summary>
		private void DoPostMergeCommit(Revision head)
		{
			//that merge may have generated notes files where they didn't exist before,
			//and we want these merged
			//version + updated/created notes files to go right back into the repository

			//  args.Append(" -X " + SurroundWithQuotes(Path.Combine(_pathToRepository, "**.ChorusRescuedFile")));

			AppendAnyNewNotes(_localRepositoryPath);

			_sychronizerAdjunct.PrepareForPostMergeCommit(_progress);

			AddAndCommitFiles(GetMergeCommitSummary(head.UserId, Repository));
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

				var skippedHeads = (heads.Where(
					head => head.Number.LocalRevisionNumber == myHead.Number.LocalRevisionNumber
						|| head.Tag.Contains(RejectTagSubstring)
						|| head.Branch != myHead.Branch
						|| CheckAndWarnIfNoCommonAncestor(myHead, head))).ToArray();
				foreach (var skippedHead in skippedHeads)
					heads.Remove(skippedHead);

				foreach (Revision head in heads)
				{
					PrepareForMergeAttempt(myHead, head);

					if (!MergeTwoChangeSets(myHead, head))
						continue; // Nothing to merge.

					peopleWeMergedWith.Add(head.UserId);
					DoPostMergeCommit(head);
				}
			}
			catch (UserCancelledException)
			{
				throw;
			}
			catch (Exception error)
			{
				ExplainAndThrow(error,WhatToDo.NeedExpertHelp, "Unable to complete the send/receive.");
			}
		}


		/// <summary>
		/// Find any .NewChorusNotes files which were created by the MergeChorus.exe and either rename them to .ChorusNotes
		/// or add any annotations found in them to the existing .ChorusNotes file.
		/// </summary>
		private static void AppendAnyNewNotes(string localRepositoryPath)
		{
			var allNewNotes = Directory.GetFiles(localRepositoryPath, "*.NewChorusNotes", SearchOption.AllDirectories);
			foreach (var newNote in allNewNotes)
			{
				var oldNotesFile = newNote.Replace("NewChorusNotes", "ChorusNotes");
				if (File.Exists(oldNotesFile))
				{
					// Add new annotations to the end of any which were in the repo
					var oldDoc = new XmlDocument();
					oldDoc.Load(oldNotesFile);
					var oldNotesNode = oldDoc.SelectSingleNode("/notes");
					var newDoc = new XmlDocument();
					newDoc.Load(newNote);
					var newAnnotations = newDoc.SelectNodes("/notes/annotation");
					foreach (XmlNode node in newAnnotations)
					{
						var newOldNode = oldDoc.ImportNode(node, true);
						oldNotesNode.AppendChild(newOldNode);
					}
					using (var fileWriter = XmlWriter.Create(oldNotesFile, CanonicalXmlSettings.CreateXmlWriterSettings()))
					{
						oldDoc.Save(fileWriter);
					}
					File.Delete(newNote);
				}
				else
				{
					// There was no former ChorusNotes file, so just rename
					File.Move(newNote, oldNotesFile);
				}
			}
		}

		private bool CheckAndWarnIfNoCommonAncestor(Revision a, Revision b )
		{
			if (null ==Repository.GetCommonAncestorOfRevisions(a.Number.Hash,b.Number.Hash))
			{
				_progress.WriteWarning(
					"This repository has an anomaly:  the two heads we want to merge have no common ancestor.  You should get help from the developers of this application.");
				_progress.WriteWarning("1) \"{0}\" on {1} by {2} ({3}). ", a.GetHashCode(), a.Summary, a.DateString, a.UserId);
				_progress.WriteWarning("2) \"{0}\" on {1} by {2} ({3}). ", b.GetHashCode(), b.Summary, b.DateString, b.UserId);
				return true;
			}
			return false;
		}

		/// <returns>false if nothing needed to be merged, true if the merge was done. Throws exception if there is an error.</returns>
		private bool MergeTwoChangeSets(Revision head, Revision theirHead)
		{
			// Theory has it that is a tossup on who ought to win, unless there is some more principled way to decide.
			// If 'they' end up being the right answer, or if it ends up being more exotic,
			// then be sure to change the alpha and beta info in the MergeSituation class.
			//using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.TheyWin.ToString()))
			// Go with 'WeWin', since that is the default and that is how the alpha and beta data of MergeSituation is set, right before this method is called.
			using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.WeWin.ToString()))
			{
				var didMerge = Repository.Merge(_localRepositoryPath, theirHead.Number.LocalRevisionNumber);
				FailureSimulator.IfTestRequestsItThrowNow("SychronizerAdjunct");
				return didMerge;
			}
		}

		#endregion

		private void AddAndCommitFiles(string summary)
		{
			ProjectFolderConfiguration.EnsureCommonPatternsArePresent(_project);
			_project.IncludePatterns.Add("**.ChorusRescuedFile");
			Repository.AddAndCheckinFiles(_project.IncludePatterns, _project.ExcludePatterns,
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

			var files = Repository.GetFilesInRevisionFromQuery(rev1, "status -u");
			files.AddRange(Repository.GetFilesInRevisionFromQuery(rev1 /*this param is bogus*/, "status -ru --rev " + rev2.Number.LocalRevisionNumber));

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
