using System;
using System.IO;
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
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob"))
			{
				using (RepositoryWithFilesSetup sally = new RepositoryWithFilesSetup("sally", bob))
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
					Assert.IsTrue(File.ReadAllText(sally._liftFile.Path).Contains("sallyWasHere"));
				}
			}
		}
	}
}