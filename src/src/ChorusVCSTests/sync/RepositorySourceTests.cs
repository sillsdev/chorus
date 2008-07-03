using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	[TestFixture]
	public class RepositorySourceTests
	{
		private ProjectFolderConfiguration _project;
		private StringBuilderProgress _progress;
		private string _pathToTestRoot;
		private string _pathToProjectRoot;

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);

			_pathToProjectRoot = Path.Combine(_pathToTestRoot, "foo project");
			Directory.CreateDirectory(_pathToProjectRoot);

			string pathToText = WriteTestFile("version one");

			RepositoryManager.MakeRepositoryForTest(_pathToProjectRoot, "bob");

			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToProjectRoot;

			_progress = new StringBuilderProgress();
		}

		private string WriteTestFile(string contents)
		{
			string pathToText = Path.Combine(_pathToProjectRoot, "foo.txt");
			File.WriteAllText(pathToText, contents);
			return pathToText;
		}

//        [Test]
//        public void NoSourcesAvailable()
//        {
//        }

		[Test]
		public void SyncNow_OnlyBlankFauxUsbAvailable_UsbGetsClone()
		{
			RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(_project);

			string pathToFauxUsbRoot = Path.Combine(_pathToTestRoot, "usb");
			Directory.CreateDirectory(pathToFauxUsbRoot);

			UsbKeyRepositorySource usbSource = manager.KnownRepositories[0] as UsbKeyRepositorySource;
			usbSource.PathToPretendUsbKeyForTesting=pathToFauxUsbRoot;
			SyncOptions options = new SyncOptions();
			options.RepositoriesToTry.Add(usbSource);

			WriteTestFile("version two");

			manager.SyncNow(options, _progress);
			string dir = Path.Combine(pathToFauxUsbRoot, "foo project");
			Assert.IsTrue(Directory.Exists(dir));

		}

		[Test]
		public void SyncNow_AlreadySetupFauxUsbAvailable_UsbGetsSync()
		{
			SyncOptions options = new SyncOptions();
			RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(_project);
			manager.SyncNow(options, _progress);

			string pathToFauxUsbRoot = Path.Combine(_pathToTestRoot, "usb");
			Directory.CreateDirectory(pathToFauxUsbRoot);

			UsbKeyRepositorySource usbSource = manager.KnownRepositories[0] as UsbKeyRepositorySource;
			usbSource.PathToPretendUsbKeyForTesting = pathToFauxUsbRoot;


			options.RepositoriesToTry.Add(usbSource);
			manager.MakeClone(usbSource.ResolveUri("foo project", _progress), true, _progress);
			string dir = Path.Combine(pathToFauxUsbRoot, "foo project");
			string contents = File.ReadAllText(Path.Combine(dir, "foo.txt"));
			Assert.AreEqual("version one", contents);
			WriteTestFile("version two");
			manager.SyncNow(options, _progress);
			contents = File.ReadAllText(Path.Combine(dir, "foo.txt"));
			Assert.AreEqual("version two", contents);
		}
	}
}
