
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using Palaso.Progress.LogBox;

using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgVersionBranchingTests
	{
		private const string stestUser = "Doofus";
		private RepositoryWithFilesSetup _repoWithFilesSetup = RepositoryWithFilesSetup.CreateWithLiftFile(stestUser);

		[Test]
		public void GetBranchesTest()
		{
			// Setup
			var branchingHelper = new HgModelVersionBranch(_repoWithFilesSetup.Repository, stestUser);

			// SUT
			var result = branchingHelper.GetBranches(new NullProgress());

			// Verification
			Assert.AreEqual(1, result.Count);
		}

		[Test]
		public void CreateBranchesTest()
		{
			var branchingHelper = new HgModelVersionBranch(_repoWithFilesSetup.Repository, stestUser);
			Assert.AreEqual(1, branchingHelper.GetBranches(new NullProgress()).Count, "Setup problem in test, should be starting with one branch.");
			const string newBranchName = "FLEx70000059";
			// SUT
			branchingHelper.CreateNewBranch(newBranchName);
			_repoWithFilesSetup.ReplaceSomething("nottheoriginal");
			_repoWithFilesSetup.SyncWithOptions(new SyncOptions
			{
				DoPullFromOthers = false,
				CheckinDescription = "new local branch",
				DoSendToOthers = false
			});
			// Verification
			var revs = branchingHelper.GetBranches(new NullProgress());
			Assert.AreEqual(2, revs.Count, "Should be 2 branches now.");
			Assert.AreEqual(newBranchName, revs[0].Branch, "Should be a branch with this name.");
		}
	}
}
