using System.IO;
using Chorus.sync;
using LibChorus.TestUtilities;
using NUnit.Framework;
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

				Assert.DoesNotThrow(() => sally.SyncWithOtions(options, synchronizer));
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

				Assert.DoesNotThrow(() => sally.SyncWithOtions(options, synchronizer));
			}
		}

		[Test]
		public void BasicCommitHasCommitFileButNotMergeFile()
		{
			using (var bob = new RepositorySetup("bob", true))
			{
				var syncAdjunct = new FileWriterSychronizerAdjunct(bob.RootFolder.Path);

				Assert.IsFalse(File.Exists(syncAdjunct.CommitPathname));
				Assert.IsFalse(File.Exists(syncAdjunct.MergePathname));

				var options = new SyncOptions
				{
					DoMergeWithOthers = false,
					DoPullFromOthers = false,
					DoSendToOthers = false
				};
				var synchronizer = bob.CreateSynchronizer();
				synchronizer.SynchronizerAdjunct = syncAdjunct;

				bob.SyncWithOtions(options, synchronizer);

				Assert.IsTrue(File.Exists(syncAdjunct.CommitPathname));
				Assert.IsFalse(File.Exists(syncAdjunct.MergePathname));
			}
		}

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

				Assert.IsFalse(File.Exists(syncAdjunct.CommitPathname));
				Assert.IsFalse(File.Exists(syncAdjunct.MergePathname));

				var options = new SyncOptions
				{
					DoMergeWithOthers = true,
					DoPullFromOthers = true,
					DoSendToOthers = true
				};
				options.RepositorySourcesToTry.Add(bob.RepoPath);
				var synchronizer = sally.Synchronizer;
				synchronizer.SynchronizerAdjunct = syncAdjunct;


				sally.SyncWithOtions(options, synchronizer);

				Assert.IsTrue(File.Exists(syncAdjunct.CommitPathname));
				Assert.IsTrue(File.Exists(syncAdjunct.MergePathname));
			}
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
			/// Allow the client to do something right after a merge, but before the merges are committed.
			/// </summary>
			/// <remarks>This method not be called at all, if there was no merging.</remarks>
			public void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge)
			{
				File.WriteAllText(MergePathname, "Merged");
			}

			#endregion
		}
	}
}