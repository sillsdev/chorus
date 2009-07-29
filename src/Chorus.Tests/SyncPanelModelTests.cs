using System.Collections.Generic;
using System.IO;
using System.Threading;
using Chorus.sync;
using Chorus.Tests;
using Chorus.UI;
using Chorus.Utilities;
using NUnit.Framework;
using System.Linq;

namespace Baton.Tests
{
	[TestFixture]
	public class SyncPanelModelTests
	{
		private string _pathToTestRoot;
		private SyncPanelModel _model;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;
		private Synchronizer _synchronizer;

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);


			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(pathToText, "version one of my pretend txt");

			EmptyRepositorySetup.MakeRepositoryForTest(_pathToTestRoot, "bob");

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToTestRoot;

			_progress = new StringBuilderProgress();
			_synchronizer = Synchronizer.FromProjectConfiguration(_project, new NullProgress());
			_model = new SyncPanelModel(_project);
			_model.ProgressDisplay = _progress;
		}

		[Test]
		public void AfterSyncLogNotEmpty()
		{
			_model.Sync();
			while(!_model.EnableSync)
				Thread.Sleep(100);
			Assert.IsNotEmpty(_progress.Text);
		}

		[Test]
		public void InitiallyHasUsbTarget()
		{
			Assert.IsNotNull(_model.GetRepositoriesToList().First(r => r.URI == "UsbKey"));
		}



		[Test]
		public void GetRepositoriesToList_NoRepositoriesKnown_GivesEmptyList()
		{
			_synchronizer.ExtraRepositorySources.Clear();
			_model = new SyncPanelModel(_project);
			_model.ProgressDisplay = _progress;
			Assert.AreEqual(0, _model.GetRepositoriesToList().Count);
		}
	}
}