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

		[SetUp]
		public void Setup()
		{
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			if (Directory.Exists(_pathToTestRoot))
				Directory.Delete(_pathToTestRoot, true);
			Directory.CreateDirectory(_pathToTestRoot);

			string bobPath = Path.Combine(_pathToTestRoot, "Bob");
			Directory.CreateDirectory(bobPath);

			string pathToText = Path.Combine(bobPath, "foo.txt");
			File.WriteAllText(pathToText, "version one of my pretend txt");

			RepositoryManager.MakeRepositoryForTest(bobPath);

			ApplicationSyncContext applicationSyncContext=new ApplicationSyncContext();
			applicationSyncContext.Project.IncludePatterns.Add(pathToText);
			applicationSyncContext.Project.TopPath = bobPath;

			_progress = new StringBuilderProgress();
			_model = new SyncPanelModel(applicationSyncContext, _progress);
		}

		[Test]
		public void AfterSyncLogNotEmpty()
		{
			_model.Sync();
			Assert.IsNotEmpty(_progress.Text);
		}
	}
}
