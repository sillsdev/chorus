using System.IO;
using Chorus.Utilities;
using Chorus.sync;
using Chorus.VcsDrivers;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;

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
#if MONO
		[Ignore]
#endif
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, false, false);
			}
		}

		/// <summary>
		/// This test will do the first (local) commit, but does no merge.
		/// Then it does a Sync which should find no changes and result in no pull (or pull file)
		/// </summary>
		[Test]
#if MONO
		[Ignore]
#endif
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, false, false);
			}
		}

		/// <summary>
		/// This test will do the first (local) commit, but does no merge.
		/// Then it does a Sync which should find no changes and result in no pull (or pull file)
		/// </summary>
		[Test]
#if MONO
		[Ignore]
#endif
		public void SendReceiveWithTrivialMergeCallsSimpleUpdate()
		{
			using (var sally = RepositoryWithFilesSetup.CreateWithLiftFile("sally"))
			using (var bob = RepositoryWithFilesSetup.CreateByCloning("bob", sally))
			{
				var syncAdjunct = new FileWriterSychronizerAdjunct(bob.RootFolder.Path);
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
										RepositorySourcesToTry = {sally.RepoPath}
				};
				var synchronizer = bob.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;
				sally.ReplaceSomething("nice.");
				sally.SyncWithOptions(options);
				bob.ReplaceSomethingElse("no problems.");
				var syncResults = bob.SyncWithOptions(bobOptions, synchronizer);
				Assert.IsFalse(syncResults.DidGetChangesFromOthers);
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, false);
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, true);
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, true);
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, true, false, false);
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
					CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, true, false);
				}
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
				CheckExistanceOfAdjunctFiles(syncAdjunct, true, false, true, false);
			}
		}

		private static void CheckExistanceOfAdjunctFiles(FileWriterSychronizerAdjunct syncAdjunct, bool commitFileShouldExist, bool pullFileShouldExist, bool rollbackFileShouldExist, bool mergeFileShouldExist)
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
		}

		private static void CheckNoFilesExist(FileWriterSychronizerAdjunct syncAdjunct)
		{
			Assert.IsFalse(File.Exists(syncAdjunct.CommitPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.PullPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.RollbackPathname));
			Assert.IsFalse(File.Exists(syncAdjunct.MergePathname));
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
				File.WriteAllText(MergePathname, "Merged");
			}

			#endregion
		}
	}
}