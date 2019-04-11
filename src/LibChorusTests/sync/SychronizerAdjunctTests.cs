using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHandlers.lift;
using Chorus.Utilities;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

namespace LibChorus.Tests.sync
{
	[TestFixture]
	public class SychronizerAdjunctTests
	{
		[Test]
		public void CommitWithMergeDoesNotThrowWithDefaultSychronizerAdjunct()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;

				Assert.DoesNotThrow(() => sally.SyncWithOptions(options, synchronizer));
			}
		}

		[Test]
		public void SettingSychronizerAdjunctToNullEndsWithDoNothingDefaultInterfaceImplementation()
		{
			using (var bob = new RepositorySetup("bob", true))
			{
				var synchronizer = bob.CreateSynchronizer();
				Assert.IsNotNull(synchronizer.SynchronizerAdjunct);
				Assert.IsInstanceOf<DefaultSychronizerAdjunct>(synchronizer.SynchronizerAdjunct);

				synchronizer.SynchronizerAdjunct = null;
				Assert.IsNotNull(synchronizer.SynchronizerAdjunct);
				Assert.IsInstanceOf<DefaultSychronizerAdjunct>(synchronizer.SynchronizerAdjunct);
			}
		}

		[Test]
		public void CommitWithMergeDoesNotThrowTryingNullSychronizerAdjunct()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = null;

				Assert.DoesNotThrow(() => sally.SyncWithOptions(options, synchronizer));
			}
		}

		/// <summary>
		/// This test will do the first (local) commit, but does no merge.
		/// As such, the test CommitPathname will be created, but the test MergePathname will *not* be created.
		/// The presence or absence of the two files tells us whether the Synchronizer class called the new interface methods.
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux")]
		public void BasicCommitHasCommitFileButNotMergeFile()
		{
			using (var bob = new RepositorySetup("bob", true))
			{
				var syncAdjunct = new FileWriterSychronizerAdjunct(bob.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = false,
					DoPullFromOthers = false,
					DoSendToOthers = false
				};
				var synchronizer = bob.CreateSynchronizer();
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = bob.SyncWithOptions(options, synchronizer);
				Assert.IsFalse(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, false, false, true, false);
			}
		}

		/// <summary>
		/// This test will do the first (local) commit, but does no merge.
		/// Then it does a Sync which should find no changes and result in no pull (or pull file)
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux")]
		public void SendReceiveWithNoRemoteChangesGetsNoFiles()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			{
				var syncAdjunct = new FileWriterSychronizerAdjunct(bob.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);
				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				var synchronizer = bob.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;
				var syncResults = bob.SyncWithOptions(options, synchronizer);
				Assert.IsFalse(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, false, false, true, true);
			}
		}

		/// <summary>
		/// This test has bob and sally making changes to different data which should cause no merge collision.
		/// This should result in a SimpleUpdate call, sally made a change so it should have a commmit file and a pull file and a merge file
		/// </summary>
		[Test]
		[Platform(Exclude = "Linux")]
		public void SendReceiveWithTrivialMergeCallsSimpleUpdate()
		{
			using (var alistair = RepositoryWithFilesSetup.CreateWithLiftFile("alistair"))
			using (var susanna = RepositoryWithFilesSetup.CreateByCloning("suzy", alistair))
			{
				var syncAdjunct = new FileWriterSychronizerAdjunct(susanna.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);
				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				var bobOptions = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true,
					RepositorySourcesToTry = {alistair.RepoPath}
				};
				var synchronizer = susanna.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;
				alistair.ReplaceSomething("nice.");
				alistair.SyncWithOptions(options);
				susanna.ReplaceSomethingElse("no problems.");
				var syncResults = susanna.SyncWithOptions(bobOptions, synchronizer);
				Assert.IsTrue(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, true, true, true);
			}
		}

		/// <summary>
		/// This test will do the first (local) commit and does a merge.
		/// As such, the CommitPathname and MergePathname test files will *both* be created.
		/// The presence or absence of the two files tells us whether the Synchronizer class called the new interface methods.
		/// </summary>
		[Test]
		public void CommitWithMergeHasCommitFileAndMergeFile()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");

				var syncAdjunct = new FileWriterSychronizerAdjunct(sally.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = sally.SyncWithOptions(options, synchronizer);
				Assert.IsTrue(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, true, true, true);
			}
		}

		[Test]
		public void EachOneChangedOrAddedFileButNotSameFile_HasCommitAndPullAndMergeFilesOnly()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();

				sally.ProjectConfiguration.IncludePatterns.Add("sally.txt");

				var newFileForSally = TempFile.WithFilename(Path.Combine(sally.ProjectFolder.Path, "sally.txt"));
				File.WriteAllText(newFileForSally.Path, "Sally's new text.");

				var syncAdjunct = new FileWriterSychronizerAdjunct(sally.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = sally.SyncWithOptions(options, synchronizer);
				Assert.IsTrue(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, true, true, true);
			}
		}

		[Test]
		public void TheyMadeChanges_WeDidNothing_Fires_SimpleUpdate_WithFalse()
		{
			// 1. Simple pull got new stuff, while we changed nothing
			//		UpdateToTheDescendantRevision repository.Update(); //update to the tip (line 556)
			//		Expected files to exist: CommitPathname and PullPathname, but not RollbackPathname or MergePathname.
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();

				var syncAdjunct = new FileWriterSychronizerAdjunct(sally.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = sally.SyncWithOptions(options, synchronizer);
				Assert.IsTrue(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, false, true, true);
			}
		}

		[Test]
		public void BothMadeChanges_MergeFailure_Fires_SimpleUpdate_WithTrue()
		{
			// 2. Rollback on merge failure, when we changed stuff.
			//		UpdateToTheDescendantRevision repository.RollbackWorkingDirectoryToRevision(head.Number.LocalRevisionNumber); (line 570)
			//		Expected files to exist: CommitPathname and RollbackPathname, but not PullPathname or MergePathname.
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");

				var syncAdjunct = new FileWriterSychronizerAdjunct(sally.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				using (new FailureSimulator("SychronizerAdjunct"))
				{
					var syncResults = sally.SyncWithOptions(options, synchronizer);
					Assert.IsTrue(syncResults.DidGetChangesFromOthers);
					Assert.IsFalse(syncResults.Cancelled);
					Assert.IsFalse(syncResults.Succeeded);
					CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, true, false, true, false);
				}
			}
		}

		[Test]
		public void CheckBranchesGetsRightNumberOfBranches()
		{
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				bob.ReplaceSomething("bobWasHere");
				bob.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct("NOTDEFAULT");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");

				var syncAdjunct = new FileWriterSychronizerAdjunct(sally.RootFolder.Path);

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = sally.SyncWithOptions(options, synchronizer);
				Assert.IsTrue(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, false, true, true);
				var lines = File.ReadAllLines(syncAdjunct.CheckRepoBranchesPathName);
				Assert.AreEqual(lines.Length, 2, "Wrong number of branches on CheckBranches call");
			}
		}

		[Test]
		public void OurCommitOnlyFailsCommitCopCheck()
		{
			// 3. Backout after CommitCop bailout.
			//		UpdateToTheDescendantRevision repository.RollbackWorkingDirectoryToRevision(head.Number.LocalRevisionNumber); (line 570)
			//		Expected files to exist: CommitPathname, and RollbackPathname, but not PullPathname or MergePathname.
			using (var bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			{
				File.WriteAllText(bob.UserFile.Path, "New contents");

				var syncAdjunct = new FileWriterSychronizerAdjunct(bob.RootFolder.Path);
				CheckNoFilesExist(syncAdjunct);

				var options = new SyncOptions
				{
					DoMergeWithOthers = false,
					DoPullFromOthers = false,
					DoSendToOthers = false
				};
				var synchronizer = bob.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				var syncResults = bob.SyncWithOptions(options, synchronizer);
				Assert.IsFalse(syncResults.Cancelled);
				Assert.IsFalse(syncResults.DidGetChangesFromOthers);
				Assert.IsFalse(syncResults.Succeeded);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, true, false, true, false);
			}
		}

		[Test]
		public void SynchronizerWithOnlyCurrentBranchRevision_ReportsNothing()
		{
			var rev1 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something");
			var revs = new[] { rev1 };
			string savedSettings = "";
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "default", ref savedSettings), Is.Null);
			Assert.That(savedSettings,Is.EqualTo(""));

			// Even if we have remembered a previous revision on our own branch, we don't report problems on the current branch.
			var rev2 = new Revision(null, "default", "Fred", "1235", "hash1234", "change something else");
			revs = new[] { rev2 };
			savedSettings = "default";
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "default", ref savedSettings), Is.Null);
			Assert.That(savedSettings, Is.EqualTo("")); // still no revs on other branches to save.
		}

		[Test]
		public void Synchronizer_ReportsNewChangeOnOtherBranch()
		{
			string savedSettings = "";
			var rev1 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something");
			// The first revision we see on another branch doesn't produce a warning...it might be something old everyone has upgraded from.
			var revs = new[] { rev1 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.Null);
			//Assert.That(savedSettings, Is.EqualTo("7.2.1:1234")); // Don't really care what is here as long as it works

			var rev2 = new Revision(null, "default", "Fred", "1235", "hash1235", "change something else");
			revs = new[] { rev2 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.StringContaining("Fred"));
		}

		[Test]
		public void Synchronizer_DoesNotReportOldChangeOnOtherBranch()
		{
			string savedSettings = "";
			var rev1 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something");
			// The first revision we see on another branch doesn't produce a warning...it might be something old everyone has upgraded from.
			var revs = new[] { rev1 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.Null);
			//Assert.That(savedSettings, Is.EqualTo("default:1234")); // Don't really care what is here as long as it works

			var rev2 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something else");
			revs = new[] { rev2 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.Null);
		}

		[Test]
		public void Synchronizer_HandlesMultipleBranches()
		{
			string savedSettings = "";
			var rev1 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something");
			// The first revision we see on another branch doesn't produce a warning...it might be something old everyone has upgraded from.
			var revs = new[] { rev1 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.Null);

			var rev2 = new Revision(null, "7.2.0", "Joe", "1235", "hash1235", "change something else");
			// To get the right result this time, the list of revisions must include both branches we are pretending are in the repo.
			revs = new[] { rev1, rev2 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.Null); // first change we've seen on this branch

			var rev3 = new Revision(null, "default", "Fred", "1236", "hash1236", "Fred's second change");
			revs = new[] { rev2, rev3 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.EqualTo("Fred"));

			var rev4 = new Revision(null, "7.2.0", "Joe", "1236", "hash1237", "Joe's second change");
			revs = new[] { rev3, rev4 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "7.2.1", ref savedSettings), Is.EqualTo("Joe"));
		}

		[Test]
		public void Synchronizer_HandlesBothDefaultBranchOptions()
		{
			// Revisions can come in with both default or empty string on the default branch depending on OS
			string savedSettings = "";
			var rev1 = new Revision(null, "default", "Fred", "1234", "hash1234", "change something");
			// The first revision we see on another branch doesn't produce a warning...it might be something old everyone has upgraded from.
			var revs = new[] { rev1 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "", ref savedSettings), Is.Null);

			var rev2 = new Revision(null, "", "Joe", "1235", "hash1235", "change something else");
			// To get the right result this time, the list of revisions must include both branches we are pretending are in the repo.
			revs = new[] { rev1, rev2 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "default", ref savedSettings), Is.Null); // first change we've seen on this branch

			var rev3 = new Revision(null, "default", "Fred", "1236", "hash1236", "Fred's second change");
			revs = new[] { rev2, rev3 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "default", ref savedSettings), Is.Null);

			var rev4 = new Revision(null, "", "Joe", "1236", "hash1237", "Joe's second change");
			revs = new[] { rev3, rev4 };
			Assert.That(LiftSynchronizerAdjunct.GetRepositoryBranchCheckData(revs, "", ref savedSettings), Is.Null);
		}

		private static void CheckExistanceOfAdjunctFiles(FileWriterSychronizerAdjunct syncAdjunct, bool commitFileShouldExist,
														 bool pullFileShouldExist, bool rollbackFileShouldExist,
														 bool mergeFileShouldExist, bool branchNameFileShouldExist,
														 bool branchesFileShouldExist)
		{
			if (commitFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.CommitPathname), "CommitFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.CommitPathname), "CommitFile should not exist.");

			if (pullFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.PullPathname), "PullFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.PullPathname), "PullFile should not exist.");

			if (rollbackFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.RollbackPathname), "RollbackFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.RollbackPathname), "RollbackFile shouldn't exist.");

			if (mergeFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.MergePathname), "MergeFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.MergePathname), "MergeFile shouldn't exist.");

			if (branchNameFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.BranchNamePathName), "BranchNameFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.BranchNamePathName), "BranchNameFile shouldn't exist.");

			if (branchesFileShouldExist)
				Assert.IsTrue(File.Exists(syncAdjunct.CheckRepoBranchesPathName), "CheckRepoBranchesFile should exist.");
			else
				Assert.IsFalse(File.Exists(syncAdjunct.CheckRepoBranchesPathName), "CheckRepoBranchesFile shouldn't exist.");
		}

		private static void CheckNoFilesExist(FileWriterSychronizerAdjunct syncAdjunct)
		{
			Assert.IsFalse(File.Exists(syncAdjunct.CommitPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.PullPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.RollbackPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.MergePathname));
			Assert.IsFalse(File.Exists(syncAdjunct.BranchNamePathName));
			Assert.IsFalse(File.Exists(syncAdjunct.CheckRepoBranchesPathName));
		}

		private class FileWriterSychronizerAdjunct : ISychronizerAdjunct
		{
			private readonly string _pathToRepository;

			internal FileWriterSychronizerAdjunct(string pathToRepository)
			{
				_pathToRepository = pathToRepository;
			}

			internal string CommitPathname
			{
				get { return Path.Combine(_pathToRepository, "Commit.txt"); }
			}

			internal string PullPathname
			{
				get { return Path.Combine(_pathToRepository, "Pull.txt"); }
			}

			internal string RollbackPathname
			{
				get { return Path.Combine(_pathToRepository, "Rollback.txt"); }
			}

			internal string MergePathname
			{
				get { return Path.Combine(_pathToRepository, "Merge.txt"); }
			}

			internal string BranchNamePathName
			{
				get { return Path.Combine(_pathToRepository, "BranchName.txt"); }
			}

			internal string CheckRepoBranchesPathName
			{
				get { return Path.Combine(_pathToRepository, "CheckRepoBranches.txt"); }
			}

			#region Implementation of ISychronizerAdjunct

			/// <summary>
			/// Allow the client to do something right before the initial local commit.
			/// </summary>
			public void PrepareForInitialCommit(IProgress progress)
			{
				File.WriteAllText(CommitPathname, "Committed");
			}

			/// <summary>
			/// Allow the client to do something in one of two cases:
			///		1. User A had no new changes, but User B (from afar) did have them. No merge was done.
			///		2. There was a merge failure, so a rollback is being done.
			/// In both cases, the client may need to do something.
			/// </summary>
			///<param name="progress">A progress mechanism.</param>
			/// <param name="isRollback">"True" if there was a merge failure, and the repo is being rolled back to an earlier state. Otherwise "False".</param>
			public void SimpleUpdate(IProgress progress, bool isRollback)
			{
				WasUpdated = true;
				if (isRollback)
					File.WriteAllText(RollbackPathname, "Rollback");
				else
					File.WriteAllText(PullPathname, "Simple pull");
			}

			/// <summary>
			/// Allow the client to do something right after a merge, but before the merge is committed.
			/// </summary>
			/// <remarks>This method not be called at all, if there was no merging.</remarks>
			public void PrepareForPostMergeCommit(IProgress progress)
			{
				WasUpdated = true;
				File.WriteAllText(MergePathname, "Merged");
			}

			/// <summary>
			/// Get the branch name the client wants to use. This might be (for example) a current version label
			/// of the client's data model. Used to create a version branch in the repository
			/// (for these tests always the default branch).
			/// </summary>
			public string BranchName
			{
				get
				{
					File.WriteAllText(BranchNamePathName, "(default)");
					return "default"; // Hg 'default' branch is empty string.
				}
			}

			public bool WasUpdated { get; private set; }

			/// <summary>
			/// During a Send/Receive when Chorus has completed a pull and there is more than one branch on the repository
			/// it will pass the revision of the head of each branch to the client.
			/// The client can use this to display messages to the users when other branches are active other than their own.
			/// i.e. "Someone else has a new version you should update"
			/// or "Your colleague needs to update, you won't see their changes until they do."
			/// </summary>
			/// <param name="branches">A list (IEnumerable really) of all the open branches in this repo.</param>
			public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
			{
				foreach (var revision in branches)
				{
					File.AppendAllText(CheckRepoBranchesPathName, revision.Branch + Environment.NewLine);
				}
			}

			#endregion
		}
	}
}