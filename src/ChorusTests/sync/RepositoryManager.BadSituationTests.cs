using System;
using System.IO;
using Chorus.merge;
using Chorus.sync;
using Chorus.Tests.merge;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	/// <summary>
	/// I don't know what to call this.... it's about what happens when things go bad, want to make
	/// sure nothing is lost.
	/// </summary>
	[TestFixture]
	public class RepositoryManagerBadSituationTests
	{

		[Test]//regression
		public void RepoProjectName_SourceHasDotInName_IsNotLost()
		{
			using (TempFolder f = new TempFolder("SourceHasDotInName_IsNotLost.x.y"))
			{
				RepositoryManager m = new RepositoryManager(f.Path, new ProjectFolderConfiguration("blah"));

				Assert.AreEqual("SourceHasDotInName_IsNotLost.x.y", m.RepoProjectName);
			}
		}


		[Test]
		public void Sync_ExceptionInMergeCode_LeftWith2HeadsAndErrorOutputToProgress()
		{
			using (RepositoryWithFilesSetup bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					bob.ReplaceSomething("bobWasHere");
					bob.Checkin();
					sally.ReplaceSomething("sallyWasHere");
					using (new ShortTermEnvironmentalVariable("InduceChorusFailure", "LiftMerger.FindEntry"))
					{
						sally.CheckinAndPullAndMerge(bob);
					}
					Assert.IsTrue(sally.ProgressString.Contains("InduceChorusFailure"));

				   sally.AssertHeadCount(2);
					//ok, Bob's the tip, but...
					Assert.AreEqual("bob", sally.Repository.GetTip().UserId);
					//make sure we didn't move up to that tip, because we weren't able to merge with it
					var currentRevision = sally.GetRepository().GetRevisionWorkingSetIsBasedOn();
					Assert.AreEqual("sally",  sally.GetRepository().GetRevision(currentRevision.Number.Hash).UserId);
					Assert.IsTrue(File.ReadAllText(sally.UserFile.Path).Contains("sallyWasHere"));

					//and over at Bob's house, it's as if Sally had never connected

					bob.AssertHeadCount(1);
					Assert.AreEqual("bob", bob.Repository.GetTip().UserId);
					Assert.IsTrue(File.ReadAllText(bob.UserFile.Path).Contains("bobWasHere"));
				}
			}
		}
		[Test]
		public void Sync_BothChangedBinaryFile_FailureReportedOneChosenSingleHead()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", "test.a9a", "original"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					bob.ReplaceSomething("bobWasHere");
					bob.Checkin();
					sally.ReplaceSomething("sallyWasHere");

					//now we have a merge of a file type that don't know how to merge
					sally.CheckinAndPullAndMerge(bob);

					sally.AssertSingleHead();
					bob.AssertSingleHead();

					//sally.AssertSingleConflict(c => c.GetType == typeof (UnmergableFileTypeConflict));
					sally.AssertSingleConflictType<UnmergableFileTypeConflict>();

					//nb: this is bob becuase the conflict handling mode is (at the time of this test
					//writing) set to TheyWin.
					Assert.IsTrue(File.ReadAllText(sally.UserFile.Path).Contains("bobWasHere"));
				}

			}
		}
	}
}