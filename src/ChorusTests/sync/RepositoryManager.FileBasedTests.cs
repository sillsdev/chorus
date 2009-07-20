using System.IO;
using Chorus.merge.xml.generic;
using Chorus.sync;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	/// <summary>
	/// Review: what's the unifying idea here?
	/// </summary>
	[TestFixture]
	public class FilePathRepositoryTests
	{
		private ProjectFolderConfiguration _project;
		private StringBuilderProgress _progress;
		private string _pathToTestRoot;
		private string _pathToProjectRoot;
		private RepositoryManager _manager;
		private string _pathToBackupFolder;
		private DirectoryRepositorySource _directorySource;

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);

			//nb: the ".2" here is significant; there was an issue where anything after a "." got stripped
			_pathToProjectRoot = Path.Combine(_pathToTestRoot, "foo project.2");
			Directory.CreateDirectory(_pathToProjectRoot);

			string pathToText = WriteTestFile("version one");

			RepositoryManager.MakeRepositoryForTest(_pathToProjectRoot, "bob");
			_project = new ProjectFolderConfiguration(_pathToProjectRoot);
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToProjectRoot;

			_progress = new StringBuilderProgress();

			_manager = RepositoryManager.FromRootOrChildFolder(_project);
			_pathToBackupFolder = Path.Combine(_pathToTestRoot, "backup");
			Directory.CreateDirectory(_pathToBackupFolder);
			_directorySource = new DirectoryRepositorySource("SD Backup Card", Path.Combine(_pathToBackupFolder,RepositoryAddress.RepositoryNameVariable), false);

		}

		private string WriteTestFile(string contents)
		{
			string pathToText = Path.Combine(_pathToProjectRoot, "foo.txt");
			File.WriteAllText(pathToText, contents);
			return pathToText;
		}


		[Test]
		public void SyncNow_BackupAlreadySetUp_GetsSync()
		{
			SyncOptions options = new SyncOptions();
			_manager.SyncNow(options, _progress);
			string projectDirOnBackup = Path.Combine(_pathToBackupFolder, "foo project.2");
			_manager.MakeClone(projectDirOnBackup, true, _progress);

			string contents = File.ReadAllText(Path.Combine(projectDirOnBackup, "foo.txt"));
			Assert.AreEqual("version one", contents);
			WriteTestFile("version two");

			options.RepositorySourcesToTry.Add(_directorySource);
			_manager.SyncNow(options, _progress);
			contents = File.ReadAllText(Path.Combine(projectDirOnBackup, "foo.txt"));
			Assert.AreEqual("version two", contents);
		}

		[Test]
		public void SyncNow_FileMissing_GetsRemoved()
		{
			SyncOptions options = new SyncOptions();
			_manager.SyncNow(options, _progress);

			string path = Path.Combine(_pathToProjectRoot, "foo.txt");
			Assert.IsTrue(File.Exists(path));
			_manager.SyncNow(options, _progress);
			File.Delete(path);
			_manager.SyncNow(options, _progress);

			Assert.IsFalse(File.Exists(path));
		}

		/// <summary>
		/// Here, we're testing the scenario where the user specifies a backup location, like an sd card at z:\
		/// </summary>
		[Test]
		public void SyncNow_NotSetupBefore_GetsClone()
		{
			SyncOptions options = new SyncOptions();
			options.RepositorySourcesToTry.Add(_directorySource);

		   // WriteTestFile("version two");

			_manager.SyncNow(options, _progress);
			string dir = Path.Combine(_pathToBackupFolder, "foo project.2");
			Assert.IsTrue(Directory.Exists(dir));
		}

	}
}
