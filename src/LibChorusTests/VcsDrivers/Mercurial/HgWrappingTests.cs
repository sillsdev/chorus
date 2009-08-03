using System;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.VcsDrivers.Mercurial
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
			_progress = new ConsoleProgress();
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
			using (var setup = new HgSetup())
			using (setup.GetWLock())
			{
				setup.Repository.AddAndCheckinFile(setup.Root.GetNewTempFile(true).Path);
			}
		}


		[Test, ExpectedException(typeof(TimeoutException))]
		public void Commit_WLockExists_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgSetup())
			using (setup.GetWLock())
			{
				setup.Repository.Commit(false, "test");
			}
		}

		[Test, ExpectedException(typeof(TimeoutException))]
		public void Pull_FileIsLocked_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgSetup())
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
			using (var setup = new HgSetup())
			{
				setup.Repository.SetUserNameInIni("me", new NullProgress());
				using (new StreamWriter(setup.Root.Combine(".hg", "hgrc")))
				{
					setup.Repository.SetUserNameInIni("otherme", new NullProgress());
				}
			}
		}

		[Test, Ignore("I'm too lazy at the moment to set up the test conditions")]
		public void GetCommonAncestorOfRevisions_Have3rdAsCommon_Get3rd()
		{

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
	}

	public class HgSetup  :IDisposable
	{
		public TempFolder Root;
		public HgRepository Repository;
		private ConsoleProgress _progress;

		[SetUp]
		public void Setup()
		{
			_progress = new ConsoleProgress();
		}
		public HgSetup()
		{
			Root = new TempFolder("ChorusHgWrappingTest");
			HgRepository.CreateRepositoryInExistingDir(Root.Path,_progress);
			Repository = new HgRepository(Root.Path, new NullProgress());
		}

		public void Dispose()
		{
			Root.Dispose();

		}

		public IDisposable GetWLock()
		{
			return TempFile.CreateAt(Root.Combine(".hg", "wlock"), "blah");
		}
		public IDisposable GetLock()
		{
			return TempFile.CreateAt(Root.Combine(".hg", "store", "lock"), "blah");
		}

	}
}