using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.merge;
using Chorus.Utilities;


namespace Chorus.VcsDrivers.Mercurial
{
	public class HgPartialMerge : HgRepository
	{
		public HgPartialMerge(string pathToRepository)
			: base(pathToRepository, new ConsoleProgress())
		{
		}

		protected void UpdateFake()
		{
			//hack to force a changeset
			string fake = Path.Combine(_pathToRepository, _userName + "_fake");
			//hack
			File.WriteAllText(fake, DateTime.Now.Ticks.ToString().Substring(14));
		}


		public static HgPartialMerge CreateNewDirectoryAndRepository(string parentDirOfNewRepository, string userName)
		{
			//   string repositoryPath = MakeDirectoryForUser(parentDirOfNewRepository, userName);
			string repositoryPath = Path.Combine(parentDirOfNewRepository, userName);

			using (new ConsoleProgress("Creating {0} from scratch", userName))
			{
				Execute("init", null, SurroundWithQuotes(repositoryPath));
				SetupPerson(repositoryPath, userName);

				HgPartialMerge repo = new HgPartialMerge(repositoryPath);
#pragma warning disable 612,618
				HgRepository.SetUserId(repositoryPath, userName);
#pragma warning restore 612,618
					 repo.AddFake();
				 return repo;
			}
		}

		private void AddFake()
		{
			//hack to force a changeset
			string fake = Path.Combine(_pathToRepository, _userName + "_fake");
			//hack
			File.WriteAllText(fake, DateTime.Now.Ticks.ToString().Substring(14));
			AddAndCheckinFile(fake);
		}

		public override void Commit(bool forceCreationOfChangeSet, string message, params object[] args)
		{
			if (forceCreationOfChangeSet)
			{
				UpdateFake();
			}

			//enhance: this is normally going to be redundant, as we always use the same branch.
			//but it does it set the first time, and handles the case where the user's account changes (either
			//because they've logged in as a different user, or changed the name of a their account.

			Branch(_userName);

			message = string.Format(message, args);
			using (new ConsoleProgress("{0} committing with comment: {1}", _userName, message))
			{
				ExecutionResult result = Execute("ci", _pathToRepository, "-m " + SurroundWithQuotes(_userName + ": " + message));
				_progress.WriteMessage(result.StandardOutput);
				if (forceCreationOfChangeSet && result.StandardOutput.Contains("nothing changed"))
				{
					throw new ApplicationException("Did not get the commit we needed.");
				}

				//nothing changed
				if (!string.IsNullOrEmpty(result.StandardError))
					_progress.WriteMessage(result.StandardError);
			}
		}

		public static HgPartialMerge CreateNewByCloning(HgPartialMerge sourceRepo, string parentDirOfNewRepository, string newPersonName)
		{
			string repositoryPath = Path.Combine(parentDirOfNewRepository, newPersonName);
			using (new ConsoleProgress("Creating {0} from {1}", newPersonName, sourceRepo.UserName))
			{
				Execute("clone", null, sourceRepo.PathWithQuotes + " " + SurroundWithQuotes(repositoryPath));
				SetupPerson(repositoryPath, newPersonName);
				HgPartialMerge repository = new HgPartialMerge(repositoryPath);
#pragma warning disable 612,618
				HgRepository.SetUserId(repositoryPath, newPersonName);
#pragma warning restore 612,618
				repository.AddFake();
				return repository;
			}
		}

		private static string MakeDirectoryForUser(string parentDirOfNewRepository, string userName)
		{
			string repositoryPath = Path.Combine(parentDirOfNewRepository, userName);
			System.IO.Directory.CreateDirectory(repositoryPath);
			return repositoryPath;
		}


		public void Sync(HgPartialMerge otherRepository)
		{
			Environment.SetEnvironmentVariable("HGMERGE", Path.Combine(Other.DirectoryOfExecutingAssembly, "ChorusMerge.exe"));

			using (new ConsoleProgress(_userName + " syncing with " + otherRepository.UserName))
			{
				Commit(true, "checking in " + _userName + "'s work at beginning of sync");
				PullFromRepository(otherRepository, true);

				bool didMerge = false;

				didMerge = MergeStubs();

				didMerge |= MergeUserHeads();

				if (!didMerge)
				{
					Update();
				}

				_progress.WriteMessage("Tree at end of sync:");
				_progress.WriteMessage(GetTextFromQuery(_pathToRepository, "glog"));
			}
		}

		private bool MergeStubs()
		{
			bool didMerge = false;
			foreach (RevisionStubPair pair in GetOutstandingStubPairs())
			{
				didMerge |= MergeUserHeadWithStub(pair._primary,pair._stub);
			}
			return didMerge;
		}

		private bool MergeUserHeads()
		{
			bool didMerge = false;
			List<Revision> heads = GetHeads();
			Revision myHead = GetMyHead();
			foreach (Revision theirHead in heads)
			{
				if (theirHead.Number.LocalRevisionNumber != myHead.Number.LocalRevisionNumber && !HeadIsAStub(theirHead))
				{
					didMerge |= SyncWithChangeSet(myHead, theirHead);
				}
			}
			return didMerge;
		}

		class RevisionStubPair
		{
			public Revision _primary;
			public Revision _stub;

			public RevisionStubPair(Revision primary, Revision stub)
			{
				_primary = primary;
				_stub = stub;
			}
		}

		private List<RevisionStubPair> GetOutstandingStubPairs()
		{
			List<RevisionStubPair> pairs = new List<RevisionStubPair>();
			List<Revision> heads = GetHeads();
			foreach (Revision primary in heads)
			{
				if (!HeadIsAStub(primary))
				{
					foreach (Revision possibleStub in heads)
					{
						if (primary.IsMatchingStub(possibleStub))
						{
							pairs.Add(new RevisionStubPair(primary, possibleStub));
						}
					}
				}
			}
			return pairs;
		}




		private bool MergeUserHeadWithStub(Revision head, Revision stub)
		{
			using (new ConsoleProgress("MergeUserHeadWithStub of {0} with the changeset: {1} {2}", head.Number.LocalRevisionNumber, stub.UserId, stub.Number.LocalRevisionNumber))
			{
				using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.WeWin.ToString()))
				{
					Update(head.Number.LocalRevisionNumber);
					ExecutionResult result =
						Execute(true, "merge", _pathToRepository, "-r", stub.Number.LocalRevisionNumber);

					if (result.ExitCode != 0)
					{
						throw new ApplicationException(result.StandardError);
					}
					else
					{
						Commit(false, "Checking in after merge with stub {0}, {1}.", stub.Number.LocalRevisionNumber, stub.Summary);
						return true;
					}
				}
			}
		}

		private bool SyncWithChangeSet(Revision myHead, Revision theirChangeSet)
		{
			using (new ConsoleProgress("SyncWithChangeSet of {0} with the changeset: {1} {2}", _userName, theirChangeSet.UserId, theirChangeSet.Number.LocalRevisionNumber))
			{

				try
				{
					File.Delete(PathToMergeFilePathsFile);

					/*  a.	Create an O’ = LCD(A, B, O) (non-conflicting changes merge) and commit it to the B branch
						  b.	Create a B’ = PartialMerge(A, B, O) and commit it to the B branch
						  c.	Merge A (our head) with O’, and commit that to the A branch.
					 */
					try
					{
						ExecutionResult result;
						using (new ShortTermEnvironmentalVariable(MergeOrder.kConflictHandlingModeEnvVarName, MergeOrder.ConflictHandlingModeChoices.LcdPlusPartials.ToString()))
						{
							result =
								Execute(true, "merge", _pathToRepository, "-r", theirChangeSet.Number.LocalRevisionNumber);
						}
						if (result.ExitCode != 0)
						{
							if (result.StandardError.Contains("nothing to merge"))
							{
								_progress.WriteMessage("Nothing to merge, updating instead to revision {0}.", theirChangeSet.Number.LocalRevisionNumber);
								Update(theirChangeSet.Number.LocalRevisionNumber);//REVIEW
								Commit(false, "!!! not expected to get in {0}:{1}.", theirChangeSet.UserId,
									   theirChangeSet.Number.LocalRevisionNumber);
								return false;
							}
							else
							{
								throw new ApplicationException(result.StandardError);
							}
						}
						else
						{
							if (!File.Exists(PathToMergeFilePathsFile))
							{
								//in this scenario, chorusMerge was never called.
								//it seems that hg knows it doesn't need to have a real
								//merger program, it can just apply the changes.

								_progress.WriteMessage("Did trivial merge.");
								Commit(false, "Checking in after no-conflicts merge with rev {0}, {1}.", theirChangeSet.Number.LocalRevisionNumber, theirChangeSet.Summary );
								return true;
							}
						}
					}
					catch (Exception expected)
					{
						if (expected.Message.Contains("nothing to merge"))
						{
							Commit(false, "*** not expected to get in {0}:{1}.", theirChangeSet.UserId, theirChangeSet.Number.LocalRevisionNumber);
							return false;
						}

						else
						{
							throw expected;
						}
					}

					_progress.WriteMessage("Tree just before partial-merge stuff:");
					_progress.WriteMessage(GetTextFromQuery(_pathToRepository, "glog"));

					Commit(false, "LCD Merge between {0} and {1}", _userName, theirChangeSet.UserId);

					//we get this far when our chorusMerge was really called
					Debug.Assert(File.Exists(PathToMergeFilePathsFile));
					DoPartialMerge(theirChangeSet, GetMyHead() /*note, this is the new, lcd-containing head*/);
					return true;
				}
				finally
				{
					File.Delete(PathToMergeFilePathsFile);
				}
			}
		}

		private void DoPartialMerge(Revision theirChangeSet, Revision myHead)
		{

			foreach (string fileSet in File.ReadAllLines(PathToMergeFilePathsFile))
			{
				string[] parts = fileSet.Split(',');
				Debug.Assert(parts.Length == 3);
				AddPartialMergeFiles(myHead, theirChangeSet, parts[0], parts[1], parts[2]);
			}
		}

		/// <summary>
		/// This is called immediately after a chorus-merge; which should have returned the LCD;
		/// Now our task is to add the two partial merges in; ours to our branch, theirs to a
		/// special "stub" or "partial" branch for them, which is unique to us (a team's
		/// repository is likely to have one stub branch for each pairing of team members).
		/// </summary>
		private void AddPartialMergeFiles(Revision myHead, Revision theirChangeSet,
										  string targetPath, string ourPartial, string theirPartial)
		{
			using (new ConsoleProgress("Adding their partial merge to their branch"))
			{
				Branch(theirChangeSet.UserId + " stub from " + _userName);//review: need "-f" to force?
				File.Copy(theirPartial, Path.Combine(_pathToRepository, targetPath), true);
				File.Delete(theirPartial);
				Commit(false, "({0} partial from {1})", theirChangeSet.UserId, _userName);
			}
			using (new ConsoleProgress("Going back to our head"))
			{
				Update(myHead.Number.LocalRevisionNumber);
			}

			//NOTE: THIS must not be allowed to be ignored as a "nothing"; hg wants to do that
			using (new ConsoleProgress("Adding our partial merge to our branch"))
			{
				File.Copy(ourPartial, targetPath, true);
				File.Delete(ourPartial);
				Commit(true, "(new {0} after partial-merg with {1})", _userName, theirChangeSet.UserId);
			}
		}

		private string PathToMergeFilePathsFile
		{
			get { return Path.Combine(Path.GetTempPath(), "chorusMergePaths.txt"); }
		}

		/// <summary>
		/// Tell us if this head is a the "stub"/"parital" change created by some other user
		/// for us to merge with when we see it, before doing any other merging.
		/// </summary>
		private bool HeadIsAStub(Revision theirHead)
		{
			return theirHead.Summary.Contains("(" + _userName + " partial");
		}
	}
}