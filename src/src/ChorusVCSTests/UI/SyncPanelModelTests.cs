using System.IO;
using Chorus.sync;
using Chorus.UI;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.UI
{
	[TestFixture]
	public class SyncPanelModelTests
	{
		private string _pathToTestRoot;
		private SyncPanelModel _model;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;
		private string _userId="";

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);


			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(pathToText, "version one of my pretend txt");

			RepositoryManager.MakeRepositoryForTest(_pathToTestRoot);

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToTestRoot;

			_progress = new StringBuilderProgress();
			_model = new SyncPanelModel(_project, _userId, _progress);
		}

		[Test]
		public void AfterSyncLogNotEmpty()
		{
			_model.Sync();
			Assert.IsNotEmpty(_progress.Text);
		}

		[Test]
		public void InitiallyHasUsbTarget()
		{
			Assert.AreEqual("UsbKey",_model.RepositoriesToTry[0].URI);
		}



		[Test]
		public void TargetsChosen_SyncEnabled()
		{
			_model.RepositoriesToTry.Add(_model.RepositoriesToList[0]);
			Assert.IsTrue(_model.EnableSync);
		}


	}
}
