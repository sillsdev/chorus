using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Chorus.Utilities;
using Chorus.VcsDrivers;
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
			// Setup
			var branchingHelper = new HgModelVersionBranch(_repoWithFilesSetup.Repository, stestUser);

			// SUT
			var result = branchingHelper.CreateNewBranch("FLEx70000059");

			// Verification
			Assert.IsNotNull(result, "This should be a new branch revision.");
		}
	}
}
