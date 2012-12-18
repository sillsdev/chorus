using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;

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
				repo.AssertHeadCount(1);
				repo.ChangeFileOnNamedBranchAndComeBack("test.txt", "blah", "mybranch");
			   //NB: this used to pass prior to hg 1.5, but, well, it shouldn't!
				//	Shouldn't there be two heads after the branch, above? (jh, April 2010)
				//			repo.AssertHeadCount(1);
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
				repo.Repository.BranchingHelper.Branch(new NullProgress(), "animals");
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
