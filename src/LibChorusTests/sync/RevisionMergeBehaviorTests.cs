using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace LibChorus.Tests.sync
{
	/// <summary>
	/// These tests are not about the internal merging files, but about the Synchronizer's revision
	/// merging behavior.
	/// </summary>
	[TestFixture]
	public class RevisionMergeBehaviorTests
	{
		[Test]
		public void SynchNow_OnDefaultBranchAndAnotherBranchExists_DoesNotMergeWithIt()
		{
			using (var repo = new RepositorySetup("bob"))
			{
				repo.AddAndCheckinFile("test.txt", "hello");
				repo.ChangeFileOnNamedBranchAndComeBack("test.txt", "blah", "mybranch");
				repo.AssertHeadCount(1);
				repo.ChangeFileAndCommit("test.txt", "hello there", "second");
				repo.AssertHeadCount(2);
				repo.CheckinAndPullAndMerge();
				repo.AssertHeadCount(2);
			}
		}

		[Test]
		public void SynchNow_OnNamedBranchAndDefaultBranchExists_DoesNotMergeWithIt()
		{
			using (var repo = new RepositorySetup("bob"))
			{
				repo.AddAndCheckinFile("test.txt", "apple");
				var afterFirstCheckin = repo.CreateBookmarkHere();
				repo.ChangeFileAndCommit("test.txt", "pear", "second on default");

				afterFirstCheckin.Go();
				repo.Repository.Branch("animals");
				repo.ChangeFileAndCommit("test.txt", "dog", "first on animals");
				var animalHead = repo.CreateBookmarkHere();

				repo.AssertHeadCount(2);
				repo.CheckinAndPullAndMerge();
				repo.AssertHeadCount(2);
				animalHead.AssertRepoIsAtThisPoint();
			}
		}

	}
}
