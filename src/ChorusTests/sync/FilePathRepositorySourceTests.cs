using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.sync
{
	[TestFixture]
	public class FilePathRepositorySourceTests
	{
		private ProjectFolderConfiguration _project;
		private StringBuilderProgress _progress;
		private string _pathToTestRoot;
		private string _pathToProjectRoot;
		private RepositoryManager _manager;
		private string _pathToBackupFolder;
		private FilePathRepositorySource _filePathSource;

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
			_project = new ProjectFolderConfiguration(_pathToProjectRoot);
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToProjectRoot;

			_progress = new StringBuilderProgress();

			_manager = RepositoryManager.FromRootOrChildFolder(_project);
			_pathToBackupFolder = Path.Combine(_pathToTestRoot, "backup");
			Directory.CreateDirectory(_pathToBackupFolder);
			_filePathSource = new FilePathRepositorySource(_pathToBackupFolder, "SD Backup Card", false);

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
			string projectDirOnBackup = Path.Combine(_pathToBackupFolder, "foo project");
			_manager.MakeClone(projectDirOnBackup, true, _progress);

			string contents = File.ReadAllText(Path.Combine(projectDirOnBackup, "foo.txt"));
			Assert.AreEqual("version one", contents);
			WriteTestFile("version two");

			options.RepositorySourcesToTry.Add(_filePathSource);
			_manager.SyncNow(options, _progress);
			contents = File.ReadAllText(Path.Combine(projectDirOnBackup, "foo.txt"));
			Assert.AreEqual("version two", contents);
		}

		/// <summary>
		/// Here, we're testing the scenario where the user specifies a backup location, like an sd card at z:\
		/// </summary>
		[Test]
		public void SyncNow_NotSetupBefore_GetsClone()
		{
			SyncOptions options = new SyncOptions();
			options.RepositorySourcesToTry.Add(_filePathSource);

		   // WriteTestFile("version two");

			_manager.SyncNow(options, _progress);
			string dir = Path.Combine(_pathToBackupFolder, "foo project");
			Assert.IsTrue(Directory.Exists(dir));
		}
	}
}
