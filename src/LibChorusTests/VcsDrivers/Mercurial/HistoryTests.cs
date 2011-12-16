using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.Tests.merge;
using NUnit.Framework;
using System.Linq;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HistoryTests
	{
		private string _pathToTestRoot;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;
		private string _pathToText;
		private HgRepository _repository;

		[SetUp]
		public void Setup()
		{
			_progress = new StringBuilderProgress();
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);


			_pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(_pathToText, "version one of my pretend txt");

			RepositorySetup.MakeRepositoryForTest(_pathToTestRoot, "bob",_progress);

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(_pathToText);
			_project.FolderPath = _pathToTestRoot;

			_repository = new HgRepository(_project.FolderPath, _progress);
		}



		[Test]
		public void GetAllRevisionss_BeforeAnySyncing_EmptyHistory()
		{
			List<Revision> items = _repository.GetAllRevisions();
			Assert.AreEqual(0, items.Count);
		}

		[Test]
		public void GetAllRevisionss_AfterSyncingTwoTimes_CorrectHistory()
		{
			Synchronizer setup = new Synchronizer(_project.FolderPath, _project, _progress);
			SyncOptions options = new SyncOptions();
			options.DoPullFromOthers = false;
			options.DoMergeWithOthers = false;
			options.CheckinDescription = "first one";
			options.DoSendToOthers = false;

			setup.SyncNow(options);
			File.WriteAllText(_pathToText, "version two of my pretend txt");
			options.CheckinDescription = "second one";
			setup.SyncNow(options);

			List<Revision> items = _repository.GetAllRevisions();
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("bob", items[0].UserId);
			Assert.AreEqual("second one", items[0].Summary);

			Assert.AreEqual("bob", items[1].UserId);
			Assert.AreEqual("first one", items[1].Summary);
		}

		[Test]
		public void GetTip_BeforeAnySyncing_EmptyString()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				Assert.IsNull(setup.Repository.GetTip());
			}
		}

		[Test]
		public void GetTip_AfterSyncing_GetTip()
		{
			using (var setup = new RepositoryWithFilesSetup("dontMatter", "foo.txt", ""))
			{
				Assert.AreEqual("0", setup.Repository.GetTip().Number.LocalRevisionNumber);
			}
		}
	}
}