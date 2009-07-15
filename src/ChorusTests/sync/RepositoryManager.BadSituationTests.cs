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
		public void Sync_ExceptionInMergeCode_GetExceptionAndMergeDoesntHappen()
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
						Exception goterror=null;
						try
						{
							sally.CheckinAndPullAndMerge(bob);
						}
						catch (Exception error)
						{
							goterror = error;
						}
						Assert.IsNotNull(goterror);
						Assert.IsTrue(goterror.Message.Contains("InduceChorusFailure"));
					}
					Assert.IsTrue(File.ReadAllText(sally.UserFile.Path).Contains("sallyWasHere"));

					sally.AssertSingleHead();
					bob.AssertSingleHead();

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

					//now we have a merge of a file type that don't know hoow to merge
					sally.CheckinAndPullAndMerge(bob);

					sally.AssertSingleHead();
					bob.AssertSingleHead();

					//sally.AssertSingleConflict(c => c.GetType == typeof (UnmergableFileTypeConflict));
					sally.AssertSingleConflictType<UnmergableFileTypeConflict>();

					Assert.IsTrue(File.ReadAllText(sally.UserFile.Path).Contains("sallyWasHere"));
				}

			}
		}
	}
}