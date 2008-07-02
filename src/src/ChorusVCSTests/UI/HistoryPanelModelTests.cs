using System.Collections.Generic;
using System.IO;
using Chorus.sync;
using Chorus.UI;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.UI
{
	[TestFixture]
	public class HistoryPanelModelTests
	{
		private string _pathToTestRoot;
		private HistoryPanelModel _model;
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
			_model = new HistoryPanelModel(_project, _userId, _progress);
		}

		[Test]
		public void BeforeAnySyncing_EmptyHistory()
		{
			List<HistoryItem> items = _model.GetHistoryItems();
			Assert.AreEqual(0, items.Count);
		 }

		[Test]
		public void AfterSyncingTwoTimes_CorrectHistory()
		{
			RepositoryManager repo = new RepositoryManager(_project.FolderPath, _project, "bob");
			SyncOptions options= new SyncOptions {DoPullFromOthers = false, DoMergeWithOthers = false, CheckinDescription = "first one"};

			repo.SyncNow(options, _progress);
			options.CheckinDescription = "second one";
			repo.SyncNow(options, _progress);

			List<HistoryItem> items = _model.GetHistoryItems();
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("bob", items[0].UserId);
			Assert.AreEqual("second one", items[0].Summary);

			Assert.AreEqual("bob", items[1].UserId);
			Assert.AreEqual("first one", items[1].Summary);
		}



	}
}
