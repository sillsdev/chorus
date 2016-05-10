using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

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
		public void GetEnvironmentReadinessMessageIsNull()
		{
			var s = HgRepository.GetEnvironmentReadinessMessage("en");
			Assert.IsNullOrEmpty(s);
		}

		[Test, Ignore("By Hand only")]
		public void Test_GetProxyAndCredentials()
		{
			using (var setup = new HgTestSetup())
			{
				var result =setup.Repository.GetProxyConfigParameterString("http://proxycheck.palaso.org/");
			}
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
				var file = TempFileFromFolder.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah");
				Assert.IsTrue(setup.Repository.RemoveOldLocks());
				Assert.IsFalse(File.Exists(file.Path));
			}
		}

		[Test]
		public void RemoveOldLocks_WLockAndLockButNotRunningHg_BothLocksRemoved()
		{
			using (var setup = new HgTestSetup())
			{
				var file1 = TempFileFromFolder.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah");
				var file2 = TempFileFromFolder.CreateAt(setup.Root.Combine(".hg", "store", "lock"), "blah");
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

				using(var file = TempFileFromFolder.CreateAt(setup.Root.Combine(".hg", "wlock"), "blah"))
				{
					Assert.IsFalse(setup.Repository.RemoveOldLocks(ourName, true));
				}
			}
		}

		[Test]
		public void RemoveOldLocks_LockAndHgIsRunning_ReturnsFalse()
		{
			using (var setup = new HgTestSetup())
			{
				//we have to pretent to be hg
				var ourName = Process.GetCurrentProcess().ProcessName;

				using(var file = TempFileFromFolder.CreateAt(setup.Root.Combine(".hg", "store", "lock"), "blah"))
				{
					Assert.IsFalse(setup.Repository.RemoveOldLocks(ourName, true));
				}
			}
		}

		[Test]
		public void CommitCommentWithDoubleQuotes_HasCorrectComment()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				File.WriteAllText(path, "new stuff");
				const string message = "New \"double quoted\" comment";
				setup.Repository.Commit(true, message);
				setup.AssertCommitMessageOfRevision("1", message);
			}
		}

		[Test]
		public void CommitWithNoUsernameInHgrcFileUsesDefaultFromEnvironment()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				var rev = setup.Repository.GetAllRevisions()[0];
				Assert.AreEqual(Environment.UserName.Replace(" ", string.Empty), rev.UserId);
			}
		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_NoCheckinsYet_GivesNull()
		{
			using (var testRoot = new TemporaryFolder("ChorusHgWrappingTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				var rev = repo.GetRevisionWorkingSetIsBasedOn();
				Assert.IsNull(rev);
			}
		}

		[Test]
		public void ManMadeRevisionNumber_HasExpectedStartingValues()
		{
			var revisionNumber = new RevisionNumber();
			Assert.That(revisionNumber.LocalRevisionNumber, Is.EqualTo("-1"));
			Assert.That(revisionNumber.LongHash, Is.EqualTo(HgRepository.EmptyRepoIdentifier));
			Assert.That(revisionNumber.Hash.Length, Is.EqualTo(12));
			Assert.That(revisionNumber.Hash, Is.EqualTo(revisionNumber.LongHash.Substring(0, 12)));
		}

		[Test]
		public void GetRevision_WithOneCommit_HasExpectedRevisionValues()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				Assert.That(repo.Identifier, Is.Null);
				var rev = repo.GetAllRevisions().FirstOrDefault();
				Assert.That(rev, Is.Null);
				using (var f = testRoot.GetNewTempFile(true))
				{
					repo.AddAndCheckinFile(f.Path);
					rev = repo.GetRevisionWorkingSetIsBasedOn();
					Assert.That(rev.Number.LocalRevisionNumber, Is.EqualTo("0"));
					Assert.That(rev.Number.LongHash, Is.EqualTo(repo.Identifier));
					Assert.That(rev.Number.Hash.Length, Is.EqualTo(12));
					Assert.That(rev.Number.Hash, Is.EqualTo(repo.Identifier.Substring(0, 12)));
					Assert.That(rev.Number.Hash, Is.EqualTo(rev.Number.LongHash.Substring(0, 12)));
				}
			}
		}

		[Test]
		public void GetRevision_WithTwoCommits_HasExpectedRevisionValuesForSecondCommit()
		{
			using (var testRoot = new TemporaryFolder("RepositoryTests"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				Assert.That(repo.Identifier, Is.Null);
				var rev = repo.GetAllRevisions().FirstOrDefault();
				Assert.That(rev, Is.Null);
				using (var file1 = testRoot.GetNewTempFile(true))
				{
					repo.AddAndCheckinFile(file1.Path);
					using (var file2 = testRoot.GetNewTempFile(true))
					{
						repo.AddAndCheckinFile(file2.Path);
						rev = repo.GetRevisionWorkingSetIsBasedOn();
						Assert.That(rev.Number.LocalRevisionNumber, Is.EqualTo("1"));
						Assert.That(rev.Number.Hash.Length, Is.EqualTo(12));
						Assert.That(rev.Number.Hash, Is.EqualTo(rev.Number.LongHash.Substring(0, 12)));
						// We can't test for the value of rev.Number.LongHash,
						// since Mercurial makes up a unique hash,
						// that is not knowable ahead of time.
					}
				}
			}
		}

		[Test]
		public void GetRevisionWorkingSetIsBasedOn_OneCheckin_Gives0()
		{
			using (var testRoot = new TemporaryFolder("ChorusHgWrappingTest"))
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
		public void EnsureRepoIdIsCorrect()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var id = setup.Repository.Identifier;
				Assert.IsTrue(String.IsNullOrEmpty(id));

				var path = setup.ProjectFolder.Combine("test.1w1");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.1w1");
				setup.AddAndCheckIn(); // Need to have one commit.

				id = setup.Repository.Identifier;
				Assert.IsFalse(String.IsNullOrEmpty(id));

				var results = HgRunner.Run("log -r0 --template " + "\"{node}\"", setup.Repository.PathToRepo, 10, setup.Progress);
				// This will probably fail, if some other version of Hg is used,
				// as it may include multiple lines (complaining about deprecated extension Chorus uses),
				// where the last one will be the id.
				Assert.AreEqual(results.StandardOutput.Trim(), id);
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

		[Test]
		public void AddAndCheckinFile_WLockExists_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			using (setup.GetWLock())
			{
				Assert.Throws<TimeoutException>(() =>
					setup.Repository.AddAndCheckinFile(setup.Root.GetNewTempFile(true).Path));
			}
		}


		[Test]
		public void Commit_WLockExists_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			using (setup.GetWLock())
			{
			   Assert.Throws<TimeoutException>(() => setup.Repository.Commit(false, "test"));
			}
		}

		[Test, Ignore("TODO: new nunit detects that actually we get threadabort, not timeout. Is that ok or not?")]
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
				   Assert.Throws<TimeoutException>(() => setup.Repository.Update());
				}
			}
		}

		[Test]
		public void SetUserNameInIni_HgrcIsOpenFromAnotherProcess_GetTimeoutException()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 1;
			using (var setup = new HgTestSetup())
			{
				setup.Repository.SetUserNameInIni("me", new NullProgress());
				using (new StreamWriter(setup.Root.Combine(".hg", "hgrc")))
				{
					Assert.Throws<TimeoutException>(() =>
						setup.Repository.SetUserNameInIni("otherme", new NullProgress()));
				}
			}
		}

		[Test, Ignore("I'm too lazy at the moment to figure out how to set up the test conditions")]
		public void GetCommonAncestorOfRevisions_Have3rdAsCommon_Get3rd()
		{

		}

		/// <summary>
		/// regression
		/// </summary>
		[Test]
		public void AddAndCheckinFiles_UserNameHasASpace_DoesnotDie()
		{
			using (var setup = new HgTestSetup())
			{
				setup.Repository.SetUserNameInIni("charlie brown", new NullProgress());
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.ChangeAndCheckinFile(path, "hello");
				//setup.Repository.AddAndCheckinFiles(new List<string>(new[]{}), );
			}
		}
		/// <summary>
		/// This is a special boundary case because hg backout fails with "cannot backout a change with no parents"
		/// </summary>
		[Test]
		public void BackoutHead_FirstChangeSetInTheRepo_Throws()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				setup.Repository.AddAndCheckinFile(path);
				Assert.Throws<ApplicationException>(() =>
					setup.Repository.BackoutHead("0", "testing"));
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
				var theMessage = "testing";
				setup.Repository.BackoutHead("1", theMessage);
				setup.AssertLocalNumberOfTip("2");
				setup.AssertHeadOfWorkingDirNumber("2");
				setup.AssertHeadCount(1);

				setup.AssertCommitMessageOfRevision("2",theMessage);
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


		[Test]
		public void MakeBundle_InvalidBase_FalseAndFileDoesNotExist()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				File.WriteAllText(path, "original");
				setup.Repository.AddAndCheckinFile(path);
				string bundleFilePath = setup.Root.GetNewTempFile(false).Path;
				Assert.That(setup.Repository.MakeBundle(new []{"fakehash"},
					bundleFilePath), Is.False);
				Assert.That(File.Exists(bundleFilePath), Is.False);
			}
		}

		[Test]
		public void MakeBundle_ValidBase_BundleFileExistsAndReturnsTrue()
		{
			using (var setup = new HgTestSetup())
			{
				var path = setup.Root.GetNewTempFile(true).Path;
				File.WriteAllText(path, "original");
				setup.Repository.AddAndCheckinFile(path);
				Revision revision = setup.Repository.GetTip();
				setup.ChangeAndCheckinFile(path, "bad");

				var bundleFilePath = setup.Root.GetNewTempFile(true).Path;
				Assert.That(setup.Repository.MakeBundle(new []{revision.Number.Hash}, bundleFilePath), Is.True);
				Assert.That(File.Exists(bundleFilePath), Is.True);
			}
		}

		[Test]
		public void Unbundle_ValidBundleFile_ReturnsTrue()
		{
			using (var setup = new RepositorySetup("unbundleTests"))
			{
				var bundleFilePath = setup.RootFolder.GetNewTempFile(false).Path;
				setup.AddAndCheckinFile(setup.ProjectFolder.GetNewTempFile(true).Path, "some file we don't care about");
				var hash = setup.Repository.GetTip().Number.Hash;
				setup.AddAndCheckinFile(setup.ProjectFolder.GetNewTempFile(true).Path, "another file we don't care about");
				setup.Repository.MakeBundle(new []{hash}, bundleFilePath);
				setup.Repository.RollbackWorkingDirectoryToLastCheckin();
				Assert.That(setup.Repository.Unbundle(bundleFilePath), Is.True);
			}
		}

		[Test]
		public void Unbundle_BadPath_ReturnsFalse()
		{
			using (var setup = new RepositorySetup("unbundleTests"))
			{
				var bundleFilePath = "bad file path";
				Assert.That(setup.Repository.Unbundle(bundleFilePath), Is.False);
			}
		}

		[Test]
		public void Unbundle_BadBundleFile_ReturnsFalse()
		{
			using (var setup = new RepositorySetup("unbundleTests"))
			{
				var bundleFilePath = setup.RootFolder.GetNewTempFile(false).Path;
				File.WriteAllText(bundleFilePath, "bogus bundle file contents");
				Assert.That(setup.Repository.Unbundle(bundleFilePath), Is.False);
			}
		}

		[Test]
		public void FileDeletedLocallyAndChangedRemotelyKeepsChanged()
		{
			using(var localRepo = new RepositorySetup("unbundleTests"))
			{
				var localFilePath = localRepo.ProjectFolder.GetNewTempFile(true).Path;
				localRepo.AddAndCheckinFile(localFilePath, "file to change and delete");
				using(var remoteRepo = new RepositorySetup("remote", localRepo))
				{
					remoteRepo.CheckinAndPullAndMerge();
					var remoteFilePath = Path.Combine(remoteRepo.ProjectFolder.Path, Path.GetFileName(localFilePath));
					Assert.That(File.Exists(remoteFilePath)); // Make sure that we have a file to delete.
					File.Delete(remoteFilePath);
					remoteRepo.SyncWithOptions( new SyncOptions { CheckinDescription = "delete file", DoMergeWithOthers = false, DoPullFromOthers = false, DoSendToOthers = false});
					Assert.That(!File.Exists(remoteFilePath)); // Make sure we actually got rid of it.
					localRepo.ChangeFileAndCommit(localFilePath, "new file contents", "changed the file");
					localRepo.CheckinAndPullAndMerge(remoteRepo);
					Assert.That(File.Exists(localFilePath), Is.True, "Did not keep changed file.");
					var chorusNotesPath = localFilePath + ".ChorusNotes";
					Assert.That(File.Exists(chorusNotesPath), "Did not record conflict");
					AssertThatXmlIn.File(chorusNotesPath).HasAtLeastOneMatchForXpath("//annotation[@class='mergeconflict']");
				}
			}
		}

		[Test]
		public void FileDeletedRemotelyAndChangedLocallyKeepsChanged()
		{
			using(var localRepo = new RepositorySetup("unbundleTests"))
			{
				var localFilePath = localRepo.ProjectFolder.GetNewTempFile(true).Path;
				localRepo.AddAndCheckinFile(localFilePath, "file to change and delete");
				using(var remoteRepo = new RepositorySetup("remote", localRepo))
				{
					remoteRepo.CheckinAndPullAndMerge();
					var remoteFilePath = Path.Combine(remoteRepo.ProjectFolder.Path, Path.GetFileName(localFilePath));
					remoteRepo.ChangeFileAndCommit(remoteFilePath, "new contents", "changed file");
					File.Delete(localFilePath);
					localRepo.SyncWithOptions(new SyncOptions { CheckinDescription = "delete file", DoMergeWithOthers = false, DoPullFromOthers = false, DoSendToOthers = false });
					localRepo.CheckinAndPullAndMerge(remoteRepo);
					Assert.That(File.Exists(localFilePath), Is.True, "Did not keep changed file.");
					var chorusNotesPath = localFilePath + ".ChorusNotes";
					Assert.That(File.Exists(chorusNotesPath), "Did not record conflict");
					AssertThatXmlIn.File(chorusNotesPath).HasAtLeastOneMatchForXpath("//annotation[@class='mergeconflict']");
				}
			}
		}
	}
}