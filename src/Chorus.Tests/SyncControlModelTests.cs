using System.Collections.Generic;
using System.IO;
using System.Threading;
using Chorus.sync;
using Chorus.UI.Sync;
using Chorus.Utilities;
using NUnit.Framework;
using System.Linq;

namespace LibChorus.Tests
{
	[TestFixture]
	public class SyncControlModelTests
	{
		private string _pathToTestRoot;
		private SyncControlModel _model;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;
		private Synchronizer _synchronizer;

		[SetUp]
		public void Setup()
		{
			_progress = new StringBuilderProgress();
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);


			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(pathToText, "version one of my pretend txt");

			RepositorySetup.MakeRepositoryForTest(_pathToTestRoot, "bob",_progress);

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToTestRoot;

			_synchronizer = Synchronizer.FromProjectConfiguration(_project, new NullProgress());
			_model = new SyncControlModel(_project, SyncUIFeatures.Everything);
			_model.AddProgressDisplay(_progress);
		}

		[Test]
		public void AfterSyncLogNotEmpty()
		{
			_model.Sync(false);
			while(!_model.EnableSendReceive)
				Thread.Sleep(100);
			Assert.IsNotEmpty(_progress.Text);
		}

		[Test]
		public void InitiallyHasUsbTarget()
		{
			Assert.IsNotNull(_model.GetRepositoriesToList().First(r => r.URI == "UsbKey"));
		}



		[Test]
		public void GetRepositoriesToList_NoRepositoriesKnown_GivesUsbAndDepot()
		{
			_synchronizer.ExtraRepositorySources.Clear();
			_model = new SyncControlModel(_project, SyncUIFeatures.Everything);
			_model.AddProgressDisplay(_progress);
			Assert.AreEqual(2, _model.GetRepositoriesToList().Count);
		}
	}
}