using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using Chorus.VcsDrivers;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Extensions;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;

namespace Chorus.Tests
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
			Directory.CreateDirectory(_pathToTestRoot);


			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			File.WriteAllText(pathToText, "version one of my pretend txt");

			RepositorySetup.MakeRepositoryForTest(_pathToTestRoot, "bob",_progress);

			_project = new ProjectFolderConfiguration(_pathToTestRoot);
			_project.FolderPath = _pathToTestRoot;
			_project.IncludePatterns.Add(pathToText);
			_project.FolderPath = _pathToTestRoot;

			_synchronizer = Synchronizer.FromProjectConfiguration(_project, new NullProgress());
			_model = new SyncControlModel(_project, SyncUIFeatures.Advanced,null);
			_model.AddMessagesDisplay(_progress);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_pathToTestRoot, true);
		}

		[Test]
		[Platform(Exclude="Linux", Reason = "Known mono issue")]
		public void AfterSyncLogNotEmpty()
		{
			_model.Sync(false);
			var start = DateTime.Now;
			while(!_model.EnableSendReceive)
			{
				Thread.Sleep(100);
				Application.DoEvents();//without this, the background worker starves 'cause their's no UI
				if ((DateTime.Now.Subtract(start).Minutes > 0))
				{
					Assert.Fail("Gave up waiting.");
				}
			}
			Assert.IsNotEmpty(_progress.Text);
		}

		[Test]
		public void InitiallyHasUsbTarget()
		{
			Assert.IsNotNull(_model.GetRepositoriesToList()[0].URI == "UsbKey");
			// Assert.IsNotNull(_model.GetRepositoriesToList().Any(r => r.URI == "UsbKey"));
		}

		[Test]
		public void GetRepositoriesToList_NoRepositoriesKnown_GivesUsb()
		{
			_synchronizer.ExtraRepositorySources.Clear();
			_model = new SyncControlModel(_project, SyncUIFeatures.Advanced, null);
			_model.AddMessagesDisplay(_progress);
			Assert.AreEqual(1, _model.GetRepositoriesToList().Count);
		}

		/// <summary>
		/// when r# allows categories, change this to just Catgory["SkipBehindProxy"]
		/// </summary>
		[Test, Ignore("fails behind hatton's proxy, because it requires intervention")]
		public void Sync_NonExistantLangDepotProject_ExitsGracefullyWithCorrectErrorResult()
		{
			_model = new SyncControlModel(_project, SyncUIFeatures.Minimal, null);
			_model.SyncOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("languageforge", "http://hg-public.languagedepot.org/dummy"));
			var progress = new ConsoleProgress() {ShowVerbose = true};
			_model.AddMessagesDisplay(progress);
			SyncResults results = null;
			_model.SynchronizeOver += new EventHandler((sender, e) => results = (sender as SyncResults));
			_model.Sync(true);
			var start = DateTime.Now;
			while (results == null)
			{
				Thread.Sleep(100);
				Application.DoEvents(); //else the background worker may starve
				if ((DateTime.Now.Subtract(start).Minutes > 1))
				{
					Assert.Fail("Gave up waiting.");
				}
			}
			Assert.IsFalse(results.Succeeded);
			Assert.IsNotNull(results.ErrorEncountered);
		}

		[Test]
		[Platform(Exclude="Linux", Reason = "Known mono issue")]
		public void Sync_Cancelled_ResultsHaveCancelledEqualsTrue()
		{
			_model = new SyncControlModel(_project, SyncUIFeatures.Minimal, null);
			_model.SyncOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("languageforge", "http://hg-public.languagedepot.org/dummy"));
			var progress = new ConsoleProgress();
			_model.AddMessagesDisplay(progress);
			SyncResults results = null;
			_model.SynchronizeOver += new EventHandler((sender, e) => results = (sender as SyncResults));
			_model.Sync(true);
			Thread.Sleep(100);
			_model.Cancel();
			var start = DateTime.Now;
			while (results == null)
			{
				Thread.Sleep(100);
				Application.DoEvents(); //else the background worker may starve
				if ((DateTime.Now.Subtract(start).Minutes > 0))
				{
					Assert.Fail("Gave up waiting.");
				}
			}
			Assert.IsFalse(results.Succeeded);
			Assert.IsTrue(results.Cancelled);
			Assert.IsNull(results.ErrorEncountered);
		}

		[Test]
		public void AsyncLocalCheckIn_GivesGoodResult()
		{
			SyncResults result=null;
			_model.AsyncLocalCheckIn("testing", (r)=>result=r);
			var start = DateTime.Now;
			while (result == null)
			{
				Thread.Sleep(100);
				Application.DoEvents();//without this, the background worker starves 'cause their's no UI
				if ((DateTime.Now.Subtract(start).Minutes > 0))
				{
					Assert.Fail("Gave up waiting.");
				}
			}
			Assert.IsTrue(result.Succeeded);
		}

		/// <summary>
		/// This test is more thorough when run in debug mode since the 'Found a lock before executing' warning is debug only.
		/// </summary>
		[Test]
		public void AsyncLocalCheckIn_NoHgLockWarningWithMultipleWorkers()
		{
			SyncResults result1=null;
			SyncResults result2=null;
			SyncResults result3=null;
			_model.AsyncLocalCheckIn("testing", (r) => result1=r);
			_model.AsyncLocalCheckIn("testing", (r) => result2=r);
			_model.AsyncLocalCheckIn("testing", (r) => result3=r);
			var start = DateTime.Now;
			while(result1 == null || result2 == null || result3 == null)
			{
				Thread.Sleep(100);
				Application.DoEvents();//without this, the background worker starves 'cause their's no UI
				if((DateTime.Now.Subtract(start).Minutes > 0))
				{
					Assert.Fail("Gave up waiting.");
				}
			}
			Assert.That(result1.Succeeded, Is.True);
			Assert.That(result2.Succeeded, Is.True);
			Assert.That(result3.Succeeded, Is.True);
			Assert.That(_model.StatusProgress, Is.TypeOf(typeof(SimpleStatusProgress)));
			Assert.That(((SimpleStatusProgress)_model.StatusProgress).WarningEncountered, Is.False);
			Assert.That(_model.StatusProgress.ErrorEncountered, Is.False);
		}


		[Test]
		public void AsyncLocalCheckIn_NoPreviousRepoCreation_Throws()
		{
			Assert.Throws<InvalidOperationException>(() =>
										 {
											 //simulate not having previously created a repository
											 Palaso.IO.DirectoryUtilities.DeleteDirectoryRobust(
												 _pathToTestRoot.CombineForPath(".hg"));
											 _model.AsyncLocalCheckIn("testing", null);
										 });
		}
	}
}