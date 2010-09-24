using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Chorus;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	/// <summary>
	/// This is for testing the simple wrapping functions, where the methods
	/// are just doing command-line queries and returning nicely packaged results
	/// </summary>
	[TestFixture]
	public class HgWrappingTests
	{
		private ConsoleProgress _progress;

		[SetUp]
		public void Setup()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 10000;//reset it in between tests
			_progress = new ConsoleProgress();
		}



		[Test]
		public void RemoveOldLocks_NoLocks_ReturnsTrue()
		{
			using (var setup = new HgTestSetup())
			{
				Assert.IsTrue(setup.Repository.RemoveOldLocks());
			}
		}

		[Test]
		public void RemoveOldLocks_WLockButNotRunningHg_LockRemoved()
		{
			using (var setup = new HgTestSetup())
			{
				var file = TempFile.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah");
				Assert.IsTrue(setup.Repository.RemoveOldLocks());
				Assert.IsFalse(File.Exists(file.Path));
			}
		}

		[Test]
		public void RemoveOldLocks_WLockAndLockButNotRunningHg_BothLocksRemoved()
		{
			using (var setup = new HgTestSetup())
			{
				var file1 = TempFile.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah");
				var file2 = TempFile.CreateAt(setup.Root.Combine(".hg", "store","lock"), "blah");
				Assert.IsTrue(setup.Repository.RemoveOldLocks());
				Assert.IsFalse(File.Exists(file1.Path));
				Assert.IsFalse(File.Exists(file2.Path));
			}
		}

		[Test]
		public void RemoveOldLocks_WLockAndHgIsRunning_ReturnsFalse()
		{
			using (var setup = new HgTestSetup())
			{
				//we have to pretent to be hg
				var ourName =System.Diagnostics.Process.GetCurrentProcess().ProcessName;

				var file = TempFile.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah");

				Assert.IsFalse(setup.Repository.RemoveOldLocks(ourName, true));
				file.Dispose();
			}
		}

		[Test]
		public void RemoveOldLocks_LockAndHgIsRunning_ReturnsFalse()
		{
			using (var setup = new HgTestSetup())
			{
				//we have to pretent to be hg
				var ourName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

				var file = TempFile.CreateAt(setup.Root.Combine(".hg", "store", "lock"), "blah");

				Assert.IsFalse(setup.Repository.RemoveOldLocks(ourName, true));
				file.Dispose();
			}
		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_NoCheckinsYet_GivesNull()
		{
			using (var testRoot = new TempFolder("ChorusHgWrappingTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				var rev = repo.GetRevisionWorkingSetIsBasedOn();
				Assert.IsNull(rev);
			}
		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_OneCheckin_Gives0()
		{
			using (var testRoot = new TempFolder("ChorusHgWrappingTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				using(var f = testRoot.GetNewTempFile(true))
				{
					repo.AddAndCheckinFile(f.Path);
					var rev = repo.GetRevisionWorkingSetIsBasedOn();
					Assert.AreEqual("0", rev.Number.LocalRevisionNumber);
					Assert.AreEqual(12, rev.Number.Hash.Length);
				}
			}
		}

		[Test]
		public void GetRevision_RevisionDoesntExist_GivesNull()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				Assert.IsNull(setup.Repository.GetRevision("1"));
			}
		}
		[Test]
		public void GetRevision_RevisionDoesExist_Ok()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.AddAndCheckinFile("test.txt", "hello");
				Assert.IsNotNull(setup.Repository.GetRevision("0"));
			}
		}

		[Test, ExpectedException(typeof(TimeoutException))]
		public void AddAndCheckinFile_WLockExists_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			using (setup.GetWLock())
			{
				setup.Repository.AddAndCheckinFile(setup.Root.GetNewTempFile(true).Path);
			}
		}


		[Test, ExpectedException(typeof(TimeoutException))]
		public void Commit_WLockExists_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			using (setup.GetWLock())
			{
				setup.Repository.Commit(false, "test");
			}
		}

		[Test, ExpectedException(typeof(TimeoutException))]
		public void Pull_FileIsLocked_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			using (setup.GetWLock())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				using (new StreamWriter(path))
				{
					setup.Repository.Update();
				}
			}
		}

		[Test, ExpectedException(typeof(TimeoutException))]
		public void SetUserNameInIni_HgrcIsOpenFromAnotherProcess_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			{
				setup.Repository.SetUserNameInIni("me", new NullProgress());
				using (new StreamWriter(setup.Root.Combine(".hg", "hgrc")))
				{
					setup.Repository.SetUserNameInIni("otherme", new NullProgress());
				}
			}
		}

		[Test, Ignore("I'm too lazy at the moment to figure out how to set up the test conditions")]
		public void GetCommonAncestorOfRevisions_Have3rdAsCommon_Get3rd()
		{

		}

		/// <summary>
		/// This is a special boundary case because hg backout fails with "cannot backout a change with no parents"
		/// </summary>
		[Test, ExpectedException(typeof(ApplicationException))]
		public void BackoutHead_FirstChangeSetInTheRepo_Throws()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				setup.Repository.BackoutHead("0", "testing");
			}
		}

		[Test]
		public void BackoutHead_UsesCommitMessage()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				File.WriteAllText(path,"2");
				setup.Repository.AddAndCheckinFile(path);
				setup.Repository.BackoutHead("1", "testing");
				setup.AssertLocalNumberOfTip("2");
				setup.AssertHeadOfWorkingDirNumber("2");
				setup.AssertHeadCount(1);
				setup.AssertCommitMessageOfRevision("2","testing");
			}
		}

		/// <summary>
		/// The thing here is that its easy to get an hg error if we are currently on a different branch,
		/// but this is something backout can take care of. Also, the behavior of this method is specified
		/// to take us back to the branch we were on.
		/// </summary>
		[Test]
		public void BackoutHead_CurrentlyOnAnotherBranch_LeaveUsWhereWeWere()
		{
			/*
			o  changeset:   3:61688974b0c3
			|  tag:         tip
			|  summary:     backout
			|
			| @  changeset:   2:79a705ba0bbd
			| |  summary:     daeao4yo.zb2-->ok
			| |
			o |  changeset:   1:0be0d43dd824
			|/    summary:     daeao4yo.zb2-->bad
			|
			o  changeset:   0:534055cd5da5
				summary:     Add daeao4yo.zb2
			*/
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				File.WriteAllText(path, "original");
				setup.Repository.AddAndCheckinFile(path);
				setup.ChangeAndCheckinFile(path, "bad");
				setup.AssertHeadOfWorkingDirNumber("1");
				setup.Repository.Update("0");//go back to start a new branch
				setup.ChangeAndCheckinFile(path, "ok");
				setup.AssertHeadCount(2);

				string backoutRev = setup.Repository.BackoutHead("1", "backout");
				Assert.AreEqual("3",backoutRev);
				setup.AssertHeadCount(2);

				setup.AssertHeadOfWorkingDirNumber("2");//expect to be left on the branch we were on, not the backed out one
				Assert.AreEqual("0", setup.Repository.GetRevisionWorkingSetIsBasedOn().Parents[0].LocalRevisionNumber);
			}
		}

		[Test]
		public void BackoutHead_BackingOutTheCurrentHead_LeaveUsOnTheNewHead()
		{
			/*
			@  changeset:   2:0ced43559525
			|  tag:         tip
			|  summary:     backout
			|
			o  changeset:   1:ddc098603f64
			|  summary:     tuetcscw.u0v-->bad
			|
			o  changeset:   0:999cb7368d7a
			   summary:     Add tuetcscw.u0v
			*/
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				File.WriteAllText(path, "original");
				setup.Repository.AddAndCheckinFile(path);
				setup.ChangeAndCheckinFile(path, "bad");
				setup.AssertHeadOfWorkingDirNumber("1");
				setup.AssertHeadCount(1);

				string backoutRev = setup.Repository.BackoutHead("1", "backout");
				Assert.AreEqual("2", backoutRev);
				setup.AssertHeadCount(1);

				setup.AssertHeadOfWorkingDirNumber("2");
			}
		}
//
//        [Test]
//        public void GetTip_NoRepository_GivesError()
//        {
//            var progress = new StringBuilderProgress();
//            var hg = new HgRepository(Path.GetTempPath(), progress);
//            hg.GetTip();
//            Assert.IsTrue(progress.Text.Contains("Error"));
//        }

		[Test, ExpectedException(typeof(FileNotFoundException))]
		public void PathToMercurialFolder_DirOKButHgMissing_Throws()
		{
			using (var folder = new TempFolder("HgWrappingTest"))
			using(new ShortTermMercurialPathSetting(folder.Path))
			{
				MercurialLocation.PathToMercurialFolder = folder.Path;
			}
		}

		/// <summary>
		/// this tests that it's really using the hg we say to use
		/// </summary>
		[Test]
		//[Category("SkipOnBuildServer")]//I (jh) don't know why it fails on the server, but oh well.
		public void Run_IndicatedHgExecutableIsBogus_Throws()
		{
			using(var folder = new TempFolder("HgWrappingTest"))
			{
				//make a bogus exe
				File.WriteAllText(folder.Combine("hg.exe"),@"hello");
				using (new ShortTermMercurialPathSetting(folder.Path))
				{
					//NB: we're using this approach rather than the normal nunit
					//one becuase different exceptions will come from different OS's
					//all we care is that something is thrown.
					try
					{
						HgRunner.Run("version", Environment.CurrentDirectory, 2, new NullProgress());
					}
					catch(Exception)
					{
						return;
					}
					Assert.Fail("Didn't get an exception");
				}
			}
		}

	}


}