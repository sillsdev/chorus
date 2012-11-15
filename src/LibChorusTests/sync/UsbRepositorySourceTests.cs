using System.IO;
using Chorus.sync;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.Extensions;

namespace LibChorus.Tests.sync
{
	/// <summary>
	/// These tests (at least any that are not manual-only) should not actually require a usb key
	/// </summary>
	[TestFixture]
	[Category("Sync")]
	public class UsbRepositorySourceTests
	{
		private ProjectFolderConfiguration _project;
		private IProgress _progress;
		private string _pathToTestRoot;
		private string _pathToProjectRoot;

		[SetUp]
		public void Setup()
		{
			_progress = new ConsoleProgress();
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusUsbRepositorySourceTests");
			Directory.CreateDirectory(_pathToTestRoot);

			_pathToProjectRoot = Path.Combine(_pathToTestRoot, "foo project");
			Directory.CreateDirectory(_pathToProjectRoot);

			string pathToText = WriteTestFile("version one");

			RepositorySetup.MakeRepositoryForTest(_pathToProjectRoot, "bob", _progress);
			_project = new ProjectFolderConfiguration(_pathToProjectRoot);
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToProjectRoot;


			UsbKeyRepositorySource.SetRootDirForAllSourcesDuringUnitTest(_pathToTestRoot);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_pathToTestRoot, true);
		}

		private string WriteTestFile(string contents)
		{
			string pathToText = Path.Combine(_pathToProjectRoot, "foo.txt");
			File.WriteAllText(pathToText, contents);
			return pathToText;
		}


		[Test]
		public void SyncNow_OnlyABlankFauxUsbAvailable_UsbGetsClone()
		{
			Synchronizer synchronizer = Synchronizer.FromProjectConfiguration(_project, _progress);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoSendToOthers = true;
			options.RepositorySourcesToTry.Add(synchronizer.UsbPath);

			WriteTestFile("version two");

			synchronizer.SyncNow(options);
			string dir = Path.Combine(UsbKeyRepositorySource.RootDirForUsbSourceDuringUnitTest, "foo project");
			Assert.IsTrue(Directory.Exists(dir));

		}

		[Test]
		public void SyncNow_UsbGetsBareCloneWithReadme()
		{
			Synchronizer synchronizer = Synchronizer.FromProjectConfiguration(_project, _progress);
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoSendToOthers = true;
			options.RepositorySourcesToTry.Add(synchronizer.UsbPath);

			WriteTestFile("version two");

			synchronizer.SyncNow(options);
			string dir = Path.Combine(UsbKeyRepositorySource.RootDirForUsbSourceDuringUnitTest, "foo project");
			Assert.IsTrue(Directory.Exists(dir));
			Assert.IsTrue(File.Exists(dir.CombineForPath(dir, "~~Folder has an invisible repository.txt")));

		}
		[Test]
		public void SyncNow_AlreadySetupFauxUsbAvailable_UsbGetsSync()
		{
			SyncOptions options = new SyncOptions();
			Synchronizer synchronizer = Synchronizer.FromProjectConfiguration(_project, _progress);
			synchronizer.SyncNow(options);

			options.RepositorySourcesToTry.Add(synchronizer.UsbPath);
			string usbDirectory = Path.Combine(UsbKeyRepositorySource.RootDirForUsbSourceDuringUnitTest, "foo project");
			// synchronizer.MakeClone(usbDirectory, true);
			HgHighLevel.MakeCloneFromLocalToLocal(synchronizer.Repository.PathToRepo, usbDirectory, true, _progress);

			string contents = File.ReadAllText(Path.Combine(usbDirectory, "foo.txt"));
			Assert.AreEqual("version one", contents);
			WriteTestFile("version two");
			//_progress.ShowVerbose = true;
			options.CheckinDescription = "Changing to two";
			synchronizer.SyncNow(options);
			var usb = new HgRepository(usbDirectory, _progress);
			Assert.AreEqual("Changing to two", usb.GetTip().Summary);

			//did it update too (which we should do with usb, unless we switch to leave them as "bare" .hgs)?
			contents = File.ReadAllText(Path.Combine(usbDirectory, "foo.txt"));
			Assert.AreEqual("version two", contents);

		}
	}
}
