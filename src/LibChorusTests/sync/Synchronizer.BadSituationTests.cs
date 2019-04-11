using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHandlers.test;
using Chorus.merge;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.sync
{
	/// <summary>
	/// I don't know what to call this.... it's about what happens when things go bad, want to make
	/// sure nothing is lost.
	/// </summary>
	[TestFixture]
	[Category("Sync")]
	public class SynchronizerBadSituationTests
	{
		[SetUp]
		public void Setup()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 10000;//reset it in between tests
		}

		[Test]//regression
		public void RepoProjectName_SourceHasDotInName_IsNotLost()
		{
			using (var f = new TemporaryFolder("SourceHasDotInName_IsNotLost.x.y"))
			{
				Synchronizer m = new Synchronizer(f.Path, new ProjectFolderConfiguration("blah"), new ConsoleProgress());

				Assert.AreEqual("SourceHasDotInName_IsNotLost.x.y", m.RepoProjectName);
			}
		}

		/// <summary>
		/// regression of WS-15036
		/// </summary>
		[Test]
		public void Sync_HgrcInUseByOther_FailsGracefully()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new RepositorySetup("bob"))
			{
				using (new StreamWriter(setup.ProjectFolder.Combine(".hg", "hgrc")))
				{
					var results = setup.CheckinAndPullAndMerge();
					Assert.IsFalse(results.Succeeded);
				}
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
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sallyWasHere");
					using (new FailureSimulator("LiftMerger.FindEntryById"))
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
			File.Delete(Path.Combine(Path.GetTempPath(), "LiftMerger.FindEntryById"));
		}

		[Test]
		public void Sync_MergeFailure_LeavesNoChorusMergeProcessAlive()
		{
			using (RepositoryWithFilesSetup bob = RepositoryWithFilesSetup.CreateWithLiftFile("bob"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					bob.ReplaceSomething("bobWasHere");
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sallyWasHere");
					using (new FailureSimulator("LiftMerger.FindEntryById"))
					{
						sally.CheckinAndPullAndMerge(bob);
					}
					Assert.AreEqual(0, Process.GetProcessesByName("ChorusMerge").Length);
				}
			}
			File.Delete(Path.Combine(Path.GetTempPath(), "LiftMerger.FindEntryById"));
		}

		[Test]
		[Category("SkipOnTeamCityRandomTestFailure")]
		public void Sync_MergeTimeoutExceeded_LeavesNoChorusMergeProcessAlive()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var fred = RepositoryWithFilesSetup.CreateWithLiftFile("fred"))
			{
				using (var betty = RepositoryWithFilesSetup.CreateByCloning("betty", fred))
				{
					fred.ReplaceSomething("fredWasHere");
					fred.AddAndCheckIn();
					betty.ReplaceSomething("bettyWasHere");
					betty.CheckinAndPullAndMerge(fred);
					Assert.AreEqual(0, Process.GetProcessesByName("ChorusMerge").Length);
				}
			}
		}

		[Test]
		public void Sync_MergeFailure_NoneOfTheOtherGuysFilesMakeItIntoWorkingDirectory()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				bob.ProjectFolderConfig.IncludePatterns.Add("*.txt");
				bob.AddAndCheckinFile("aaa.txt", "apple");
				bob.AddAndCheckinFile("bbb.txt", "bread");
				bob.AddAndCheckinFile("zzz.txt", "zoo");
				using (var sally = new RepositorySetup("sally", bob))
				{
					bob.AddAndCheckinFile("aaa.txt", "bob-apple");
					bob.AddAndCheckinFile("bbb.txt", "bob-bread");
					bob.AddAndCheckinFile("zzz.txt", "bob-zoo");
				   using (new FailureSimulator("TextMerger-bbb.txt"))
					{
						sally.AddAndCheckinFile("aaa.txt", "sally-apple");
						sally.AddAndCheckinFile("bbb.txt", "sally-bread");
						sally.AddAndCheckinFile("zzz.txt", "sally-zipper");
						Assert.IsFalse(sally.CheckinAndPullAndMerge(bob).Succeeded);

					   //make sure we ended up on Sally's revision, even though Bob's are newer
						var currentRevision = sally.Repository.GetRevisionWorkingSetIsBasedOn();
						Assert.AreEqual("sally", sally.Repository.GetRevision(currentRevision.Number.Hash).UserId);

					   //sally should see no changes, because it should all be rolled back
						sally.AssertFileContents("aaa.txt", "sally-apple");
						sally.AssertFileContents("bbb.txt", "sally-bread");
						sally.AssertFileContents("zzz.txt", "sally-zipper");

//                        sally.ShowInTortoise();
					   sally.AssertHeadCount(2);
						Assert.IsFalse(sally.GetProgressString().Contains("creates new remote heads"));
					}
				}
			}
			File.Delete(Path.Combine(Path.GetTempPath(), "TextMerger-bbb.txt"));
		}

		//Regression test: used to fail based on looking at the revision history and finding it null
		[Test]
		public void Sync_FirstCheckInButNoFilesAdded_NoProblem()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var result = bob.CheckinAndPullAndMerge();
				Assert.IsTrue(result.Succeeded, result.ErrorEncountered==null?"":result.ErrorEncountered.Message);
			}
		}

		/// <summary>
		/// regression test: there was a bug (found before we released) where on rollback
		/// we were going to the tip, which if this was the *second* attempt, could be the other guy's work!
		/// </summary>
		[Test]
		public void Sync_RepeatedMergeFailure_WeAreLeftOnOurOwnWorkingDefault()
		{
			using (var bob = new RepositoryWithFilesSetup("bob", "test.txt", "hello"))
			using (var sally = RepositoryWithFilesSetup.CreateByCloning("sally",bob))
			using (new FailureSimulator("TextMerger-test.txt"))
			{
				bob.WriteNewContentsToTestFile("bobWasHere");
				bob.AddAndCheckIn();
				sally.WriteNewContentsToTestFile("sallyWasHere");
				var result = sally.CheckinAndPullAndMerge(bob);
				Assert.IsFalse(result.Succeeded);

				//make sure we ended up on Sally's revision, even though Bob's are newer
				var currentRevision = sally.Repository.GetRevisionWorkingSetIsBasedOn();
				Assert.AreEqual("sally", sally.Repository.GetRevision(currentRevision.Number.Hash).UserId);

				//Now do it again

				bob.WriteNewContentsToTestFile("bobWasHere2");
				bob.AddAndCheckIn();
				Assert.AreEqual("bob", sally.Repository.GetTip().UserId,"if bob's not the tip, we're not testing the right situation");

				result = sally.CheckinAndPullAndMerge(bob);
				Assert.IsFalse(result.Succeeded);
				result = sally.CheckinAndPullAndMerge(bob);

				Assert.AreEqual("sally",sally.Repository.GetRevisionWorkingSetIsBasedOn().UserId);


				//sally.ShowInTortoise();

			}
			File.Delete(Path.Combine(Path.GetTempPath(), "TextMerger-test.txt"));
		}

		[Test]
		[Platform(Exclude = "Linux",
			Reason = "I (CP) can't get MONO to get an exclusive lock for write. See RepositorySetup::GetFileLockForWriting")]
		[Category("SkipOnTeamCityRandomTestFailure")]
		public void Sync_FileLockedForWritingDuringUpdate_GetUpdatedFileOnceLockIsGone()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 3;

			using (var bob = new RepositorySetup("bob"))
			{
				bob.ProjectFolderConfig.IncludePatterns.Add("*.txt");
				bob.AddAndCheckinFile("one.txt", "hello");
				using (var sally = new RepositorySetup("sally", bob))
				{
					bob.AddAndCheckinFile("one.txt", "hello-bob");
					using (sally.GetFileLockForWriting("one.txt"))
					{
						// Note: Mono succeeds here
						Assert.IsFalse(sally.CheckinAndPullAndMerge(bob).Succeeded, "CheckinAndPullAndMerge should have failed");
						sally.AssertFileContents("one.txt", "hello");
					}
					sally.AssertSingleHead();

					//ok, now whatever was holding that file is done with it, and we try again

					Assert.IsTrue(sally.CheckinAndPullAndMerge(bob).Succeeded, "ChecinAndPullAndMerge(bob) should have succeeded");
					sally.AssertFileContents("one.txt", "hello-bob");
				}
			}
		}

		[Test]
		[Category("SkipOnTeamCityRandomTestFailure")]
		public void Sync_FileLockedForReadingDuringMerge_LeftWithMultipleHeads()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 3;
			using (var bob = new RepositorySetup("bob"))
			{
				bob.ProjectFolderConfig.IncludePatterns.Add("*.txt");
				bob.AddAndCheckinFile("one.txt", "hello");
				using (var sally = new RepositorySetup("sally", bob))
				{
					bob.AddAndCheckinFile("one.txt", "hello-bob");
					using (sally.GetFileLockForReading("one.txt"))
					{
						sally.CheckinAndPullAndMerge(bob);
					}
					sally.AssertHeadCount(2);
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
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sallyWasHere");

					//now we have a merge of a file type that don't know how to merge
					sally.CheckinAndPullAndMerge(bob);

					sally.AssertSingleHead();
					bob.AssertSingleHead();

					//sally.AssertSingleConflict(c => c.GetType == typeof (UnmergableFileTypeConflict));
					sally.AssertSingleConflictType<UnmergableFileTypeConflict>();

					// nb: this is sally because the conflict handling mode is (at the time of this test
					// writing) set to WeWin.
					Assert.IsTrue(File.ReadAllText(sally.UserFile.Path).Contains("sallyWasHere"));
				}

			}
		}



		[Test]
		public void Sync_TheyHaveAFileWhichWeAlsoEditedButHavenotCheckedIn_OursIsRenamedToSafetyAndWeGetTheirs()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", "test.a9a", "original"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					File.WriteAllText(bob.ProjectFolder.Combine("problem.txt"), "bobs problem");
					bob.ProjectConfiguration.IncludePatterns.Add("problem.txt");
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sallyWasHere");
					File.WriteAllText(sally.ProjectFolder.Combine("problem.txt"), "sally's problem");
					//notice, we don't alter the include patter on sally, so this doesn't get checked in
					// on her side

					sally.CheckinAndPullAndMerge(bob);

					sally.AssertNoErrorsReported();

					var rescueFiles = Directory.GetFiles(sally.ProjectFolder.Path, "*.ChorusRescuedFile");
					Assert.AreEqual(1, rescueFiles.Length);
					Assert.AreEqual("sally's problem", File.ReadAllText(rescueFiles[0]));
					sally.AssertFileContents("problem.txt", "bobs problem");
				}

			}
		}

		/// <summary>
		/// regression test, for situation where RemoveMergeObstacles was over-zealou
		/// </summary>
		[Test]
		public void Sync_WeHaveUntrackedFile_NotRenamed()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", "test.a9a", "original"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					File.WriteAllText(bob.ProjectFolder.Combine("somethingNew.txt"), "blah");
					bob.ProjectConfiguration.IncludePatterns.Add("somethingNew.txt");
					bob.AddAndCheckIn();
					sally.ReplaceSomething("sallyWasHere");
					File.WriteAllText(sally.ProjectFolder.Combine("untracked.txt"), "foo");
					sally.CheckinAndPullAndMerge(bob);

					sally.AssertNoErrorsReported();

					var rescueFiles = Directory.GetFiles(sally.ProjectFolder.Path, "*.ChorusRescuedFile");
					Assert.AreEqual(0, rescueFiles.Length);
				}
			}
		}

		/// <summary>
		/// regression test (WS-14964), where the user had actually acquired 6 heads that needed to be merged.
		/// </summary>
		[Test]
		public void Sync_MergeWhenThereIsMoreThanOneHeadToMergeWith_MergesBoth()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", "test.a9a", "original"))
			using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
			{
				var tip = sally.Repository.GetTip();
				sally.ReplaceSomething("forbranch1");
				sally.AddAndCheckIn();
				 sally.Repository.Update(tip.Number.Hash);

				sally.ReplaceSomething("forbranch1");
				sally.AddAndCheckIn();
				 sally.Repository.Update(tip.Number.Hash);

				sally.ReplaceSomething("forbranch2");
				sally.AddAndCheckIn();
				sally.Repository.Update(tip.Number.Hash);

				sally.ReplaceSomething("forbranch3");
				sally.AddAndCheckIn();
				sally.Repository.Update(tip.Number.Hash);

				sally.AssertHeadCount(4);

				bob.ReplaceSomething("bobWasHere");
				bob.AddAndCheckIn();
				sally.ReplaceSomething("sallyWasHere");
				sally.CheckinAndPullAndMerge(bob);

				sally.AssertNoErrorsReported();

				var rescueFiles = Directory.GetFiles(sally.ProjectFolder.Path, "*.ChorusRescuedFile");
				Assert.AreEqual(0, rescueFiles.Length);

				sally.AssertHeadCount(1);
			}

		}

		/// <summary>
		/// The scenario here, as of 2 Nov 09, is that someone has manually tagged a branch as bad.
		/// </summary>
		[Test]
		public void Sync_ExistingRejectChangeSet_NotMergedIn()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				bob.AddAndCheckinFile("test.txt", "original");

				bob.CreateRejectForkAndComeBack();

				bob.ChangeFileAndCommit("test.txt", "ok", "goodGuy"); //move on so we have two distinct branches
				bob.AssertHeadCount(2);

				bob.CheckinAndPullAndMerge(null);

				Assert.AreEqual("goodGuy", bob.Repository.GetRevisionWorkingSetIsBasedOn().Summary);
				bob.AssertLocalRevisionNumber(3);
				bob.AssertHeadCount(2);
			}
		}


		[Test]
		public void Sync_ModifiedFileIsInvalid_CheckedInButThenBackedOut()
		{
			/*
				@  changeset:   2
				|  summary:     [Backout due to validation Failure]
				|
				o  changeset:   1
				|  summary:     missing checkin description
				|
				o  changeset:   0
				summary:     Add test.chorusTest
			 */
			using (var bob = new RepositorySetup("bob"))
			{
				bob.AddAndCheckinFile("test.chorusTest", "original");
				bob.AssertLocalRevisionNumber(0);
				bob.ChangeFile("test.chorusTest", ChorusTestFileHandler.GetInvalidContents());
				bob.CheckinAndPullAndMerge();
				bob.AssertLocalRevisionNumber(2);
				bob.AssertHeadCount(1);
				bob.AssertLocalRevisionNumber(int.Parse(bob.Repository.GetTip().Number.LocalRevisionNumber));
				Debug.WriteLine(bob.Repository.GetLog(-1));

			}
		}


		/// <summary>
		/// the diff here with the previous test is that while sally is still the one who is the driver
		/// (she dose the merge and push to bob), this time we follow up with bob doing a sync, which
		/// is essentially just a pull and update, to make sure that at that point the system renames
		/// his offending file (which Sally's chorus would have no way of knowing about, since it's
		/// not in his repository).
		/// </summary>
		[Test]
		public void Sync_WeHaveAFileWhichTheyAlsoEditedButHavenotCheckedIn_TheirsIsRenamedToSafetyAndTheyGetOurs()
		{
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", "test.a9a", "original"))
			{
				using (RepositoryWithFilesSetup sally = RepositoryWithFilesSetup.CreateByCloning("sally", bob))
				{
					File.WriteAllText(bob.ProjectFolder.Combine("problem.txt"), "bob's problem");
					//notice, we don't alter the include pattern on bob, so this doesn't get checked in
					// on his side
					bob.AddAndCheckIn();

					sally.ReplaceSomething("sallyWasHere");
					File.WriteAllText(sally.ProjectFolder.Combine("problem.txt"), "sally's problem");
					sally.ProjectConfiguration.IncludePatterns.Add("problem.txt");

					sally.CheckinAndPullAndMerge(bob);
					sally.AssertNoErrorsReported();

					//ok, so the problem is now lurking in bob's repo, but it doesn't hit him until
					//he does at least an update

					bob.CheckinAndPullAndMerge(sally);

					var rescueFiles = Directory.GetFiles(bob.ProjectFolder.Path, "*.ChorusRescuedFile");
					Assert.AreEqual(1, rescueFiles.Length);
					Assert.AreEqual("bob's problem", File.ReadAllText(rescueFiles[0]));
					sally.AssertFileContents("problem.txt", "sally's problem");
				}

			}
		}

//        [Test, Ignore("by hand only")]
//        public void TryingToReproduceNullsAtEndOfFile()
//        {
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                memoryStream.Write(new byte[]{60},0,1 );
//                string xmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
//                Assert.IsFalse(xmlString.Contains("\0"));
//
//                xmlString = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
//                Assert.IsFalse(xmlString.Contains("\0"));
//
//                xmlString = Encoding.UTF8.GetString(memoryStream.GetBuffer());
//                Assert.IsFalse(xmlString.Contains("\0"));
//            }
//        }

		/// <summary>
		/// Regression test: WS-34181
		/// </summary
		[Test]
		public void Sync_NewFileWithNonAsciCharacters_FileAdded()
		{
			string name = "ŭburux.txt";
			using (RepositoryWithFilesSetup bob = new RepositoryWithFilesSetup("bob", name, "original"))
			{
					   bob.AddAndCheckIn();
					   bob.AssertNoErrorsReported();
			}
		}

	}
}