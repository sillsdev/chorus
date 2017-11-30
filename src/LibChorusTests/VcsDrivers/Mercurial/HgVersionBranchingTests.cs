using System.Collections.Generic;
using System.Linq;
using Chorus.sync;
using LibChorus.TestUtilities;
using LibChorus.Tests.sync;
using NUnit.Framework;
using SIL.Progress;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgVersionBranchingTests
	{
		private const string stestUser = "Doofus";

		[Test]
		public void GetBranchesTest()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;

				// SUT
				var result = branchingHelper.GetBranches();

				// Verification
				Assert.AreEqual(1, result.Count());
			}
		}

		[Test]
		public void CreateBranchesTest()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				repoWithFiles.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct("default");
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count(),
								"Setup problem in test, should be starting with one branch.");
				const string newBranchName = "FLEx70000059";
				var oldversion = branchingHelper.ClientVersion;

				// SUT
				branchingHelper.Branch(new ConsoleProgress(), newBranchName);
				repoWithFiles.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct(newBranchName);
				repoWithFiles.ReplaceSomething("nottheoriginal");
				repoWithFiles.SyncWithOptions(new SyncOptions
					{
						DoPullFromOthers = false,
						CheckinDescription = "new local branch",
						DoSendToOthers = false
					});

				// Verification
				var revs = branchingHelper.GetBranches();
				Assert.AreEqual(2, revs.Count(), "Should be 2 branches now.");
				Assert.AreEqual(newBranchName, revs.First().Branch, "Should be a branch with this name.");
				var localRevNum = revs.First().Number.LocalRevisionNumber;
				var lastRev = repoWithFiles.Repository.GetRevision(localRevNum);
				Assert.AreEqual(stestUser, lastRev.UserId, "User name should be set.");
				Assert.AreNotEqual(oldversion, branchingHelper.ClientVersion, "Should have updated ClientVersion");
			}
		}

		[Test]
		public void DoesNewBranchExist_Yes()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				repoWithFiles.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct("default");
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count(),
								"Setup problem in test, should be starting with one branch.");
				// Make a new branch (should technically be on the remote with a different user...)
				const string newBranchName = "New Branch";
				branchingHelper.Branch(new NullProgress(), newBranchName);
				repoWithFiles.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct(newBranchName);
				repoWithFiles.ReplaceSomething("nottheoriginal");
				repoWithFiles.SyncWithOptions(new SyncOptions
				{
					DoPullFromOthers = false,
					CheckinDescription = "new local branch",
					DoSendToOthers = false
				});

				const string myVersion = ""; // Hg default branch name

				// SUT
				string revNum;
				bool result = branchingHelper.IsLatestBranchDifferent(myVersion, out revNum);

				// Verification
				Assert.IsTrue(result, "The only branch should be default.");
				var revision = repoWithFiles.Repository.GetRevision(revNum);
				Assert.AreEqual(newBranchName, revision.Branch, "Wrong branch name in new branch.");
				var revisions = repoWithFiles.Repository.GetAllRevisions();
				var branches = new HashSet<string>();
				foreach (var rev in revisions)
				{
					branches.Add(rev.Branch);
				}
				Assert.AreEqual(branches.Count, 2, "Branches not properly reported in revisions.");
			}
		}

		[Test]
		public void DoesNewBranchExist_No()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count(),
								"Setup problem in test, should be starting with one branch.");

				const string myVersion = ""; // Equivalent to 'default'

				// SUT
				string revNum;
				bool result = branchingHelper.IsLatestBranchDifferent(myVersion, out revNum);

				// Verification
				Assert.IsFalse(result, "The only branch should be default.");
			}
		}

		[Test]
		public void CanCreateVersionNumberBranch_BackwardCompatibilityTest()
		{
			// Setup (creates repo with default branch)
			using(var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count(),
								"Setup problem in test, should be starting with one branch.");
				// Make a new branch with an integer name
				var integerBranchName = "70000068";
				repoWithFiles.Synchronizer.SynchronizerAdjunct = new ProgrammableSynchronizerAdjunct(integerBranchName);
				repoWithFiles.ReplaceSomething("nottheoriginal");
				repoWithFiles.SyncWithOptions(new SyncOptions
				{
					DoPullFromOthers = false,
					CheckinDescription = "version number branch",
					DoSendToOthers = false
				});

				var revisions = repoWithFiles.Repository.GetAllRevisions();
				var branches = new HashSet<string>();
				foreach(var rev in revisions)
				{
					branches.Add(rev.Branch);
				}
				Assert.AreEqual(branches.Count, 2, "Should be 2 branches, default and " + integerBranchName);
				CollectionAssert.Contains(branches, integerBranchName, "The integer branch name was not created.");
			}
		}
	}
}
