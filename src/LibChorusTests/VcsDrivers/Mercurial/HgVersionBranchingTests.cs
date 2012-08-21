using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;

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
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void CreateBranchesTest()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count,
								"Setup problem in test, should be starting with one branch.");
				const string newBranchName = "FLEx70000059";
				var oldversion = branchingHelper.ClientVersion;

				// SUT
				branchingHelper.CreateNewBranch(newBranchName);
				repoWithFiles.ReplaceSomething("nottheoriginal");
				repoWithFiles.SyncWithOptions(new SyncOptions
					{
						DoPullFromOthers = false,
						CheckinDescription = "new local branch",
						DoSendToOthers = false
					});

				// Verification
				var revs = branchingHelper.GetBranches();
				Assert.AreEqual(2, revs.Count, "Should be 2 branches now.");
				Assert.AreEqual(newBranchName, revs[0].Branch, "Should be a branch with this name.");
				var localRevNum = revs[0].Number.LocalRevisionNumber;
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
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count,
								"Setup problem in test, should be starting with one branch.");
				// Make a new branch (should technically be on the remote with a different user...)
				const string newBranchName = "New Branch";
				branchingHelper.CreateNewBranch(newBranchName);
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
			}
		}

		[Test]
		public void DoesNewBranchExist_No()
		{
			// Setup
			using (var repoWithFiles = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser))
			{
				var branchingHelper = repoWithFiles.Repository.BranchingHelper;
				Assert.AreEqual(1, branchingHelper.GetBranches().Count,
								"Setup problem in test, should be starting with one branch.");

				const string myVersion = ""; // Equivalent to 'default'

				// SUT
				string revNum;
				bool result = branchingHelper.IsLatestBranchDifferent(myVersion, out revNum);

				// Verification
				Assert.IsFalse(result, "The only branch should be default.");
			}
		}
	}
}
