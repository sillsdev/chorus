using System.IO;
using System.Linq;
using Chorus.sync;
using Chorus.UI.Review;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;

namespace Chorus.Tests
{
	[TestFixture]
	public class HistoryPanelModelTests
	{
		private string _pathToTestRoot;
		private RevisionInRepositoryModel _model;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;

		//Not much to test here yet, as the history-getting itself is tested at a lower level

		[SetUp]
		public void Setup()
		{
			_progress = new StringBuilderProgress();
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusHistoryPaneTest"); // Don't use 'standard' ChorusTest, since it will fial, if the tests are run in seperate processes (R# 6).
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);

			string pathToText = WriteTestFile("version one of my pretend txt");

			RepositorySetup.MakeRepositoryForTest(_pathToTestRoot, "bob",_progress);

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToTestRoot;

			var revisionListOptions = new RevisionListOptions();
			revisionListOptions.RevisionsToShowFilter = ShowRevisionPredicate;

			_model = new RevisionInRepositoryModel(HgRepository.CreateOrUseExisting(_project.FolderPath, new NullProgress()),
													null,
													revisionListOptions);
			_model.ProgressDisplay = _progress;
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_pathToTestRoot, true);
		}

		private bool ShowRevisionPredicate(Revision revision)
		{
			return !revision.Summary.ToLower().Contains("hide");
		}

		private string WriteTestFile(string contents)
		{
			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(pathToText, contents);
			return pathToText;
		}

		[Test]
		public void BeforeAnySyncing_EmptyHistory()
		{
			var items = _model.GetAllRevisions();
			Assert.AreEqual(0, items.Count());
		}

		[Test]
		public void After2Syncs_HistoryHas2()
		{
			var synchronizer = Synchronizer.FromProjectConfiguration(_project, new NullProgress());
			synchronizer.SyncNow(new SyncOptions());
			WriteTestFile("two");
			synchronizer.SyncNow(new SyncOptions());
			WriteTestFile("three");
			synchronizer.SyncNow(new SyncOptions());
			var items = _model.GetAllRevisions();
			Assert.AreEqual(3, items.Count());
		}


		[Test]
		public void After2Syncs_WithFilter_OnlyFilteredItemsShown()
		{
			var synchronizer = Synchronizer.FromProjectConfiguration(_project, new NullProgress());
			synchronizer.SyncNow(new SyncOptions() { CheckinDescription = "show me" });
			WriteTestFile("two");
			synchronizer.SyncNow(new SyncOptions(){CheckinDescription = "hide me"});
			WriteTestFile("three");
			synchronizer.SyncNow(new SyncOptions() { CheckinDescription = "show me" });
			var items = _model.GetAllRevisions();
			Assert.AreEqual(2, items.Count());
		}
	}
}