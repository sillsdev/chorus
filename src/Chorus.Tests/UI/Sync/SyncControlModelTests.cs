using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Sync;
using Chorus.VcsDrivers;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.Extensions;
using SIL.IO;
using SIL.Progress;

namespace Chorus.Tests.UI.Sync
{
	[TestFixture]
	public class SyncControlModelTests
	{
		private string _pathToTestRoot;
		private SyncControlModel _model;
		private StringBuilderProgress _progress;
		private ProjectFolderConfiguration _project;
		private Synchronizer _synchronizer;

		private void WaitForTasksToFinish(int timeoutMinutes, params WaitHandle[] waitHandles)
		{
			var timeout = new TimeSpan(0, timeoutMinutes, 0);
			Assert.That(WaitHandle.WaitAll(waitHandles, (int)timeout.TotalMilliseconds), Is.True,
				string.Format("Tasks timed out after {0} min.", timeout.TotalMinutes));
		}

		private void WaitForSyncToFinish(ref SyncResults results)
		{
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
		}

		[SetUp]
		public void Setup()
		{
			_progress = new StringBuilderProgress();
			_pathToTestRoot = Path.Combine(Path.GetTempPath(), "ChorusTest");
			Directory.CreateDirectory(_pathToTestRoot);


			string pathToText = Path.Combine(_pathToTestRoot, "foo.txt");
			// ReSharper disable once LocalizableElement
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
		public void InitiallyHasUsbTarget()
		{
			Assert.That(_model.GetRepositoriesToList()[0].URI, Is.EqualTo("UsbKey"));
		}

		[Test]
		public void GetRepositoriesToList_NoRepositoriesKnown_GivesUsb()
		{
			_synchronizer.ExtraRepositorySources.Clear();
			_model = new SyncControlModel(_project, SyncUIFeatures.Advanced, null);
			_model.AddMessagesDisplay(_progress);
			Assert.That(_model.GetRepositoriesToList().Count, Is.EqualTo(1));
			Assert.That(_model.GetRepositoriesToList()[0].URI, Is.EqualTo("UsbKey"));
		}

		[Test]
		[Platform(Exclude="Linux", Reason = "Known mono issue")]
		public void Sync_AfterSyncLogNotEmpty()
		{
			_model.Sync(false);
			// NOTE: we can't use AutoResetEvent with Sync() - for some reason this doesn't work
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
		[Category("SkipBehindProxy")]
		[Category("SkipOnTeamCity")]
		public void Sync_NonExistantLangDepotProject_ExitsGracefullyWithCorrectErrorResult()
		{
			_model = new SyncControlModel(_project, SyncUIFeatures.Minimal, null);
			_model.SyncOptions.RepositorySourcesToTry.Add(RepositoryAddress.Create("languageforge", "http://hg-public.languagedepot.org/dummy"));
			var progress = new ConsoleProgress() {ShowVerbose = true};
			_model.AddMessagesDisplay(progress);
			SyncResults results = null;
			_model.SynchronizeOver += (sender, e) => results = sender as SyncResults;
			_model.Sync(true);
			// NOTE: we can't use AutoResetEvent with Sync() - for some reason this doesn't work
			WaitForSyncToFinish(ref results);

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
			_model.SynchronizeOver += (sender, e) => results = sender as SyncResults;
			_model.Sync(true);
			Thread.Sleep(100);
			_model.Cancel();
			// NOTE: we can't use AutoResetEvent with Sync() - for some reason this doesn't work
			WaitForSyncToFinish(ref results);

			Assert.IsFalse(results.Succeeded);
			Assert.IsTrue(results.Cancelled);
			Assert.IsNull(results.ErrorEncountered);
		}

		[Test]
		public void AsyncLocalCheckIn_GivesGoodResult()
		{
			SyncResults result=null;
			var waitHandle = new AutoResetEvent(false);
			_model.AsyncLocalCheckIn("testing", r => { result=r; waitHandle.Set();});
			WaitForTasksToFinish(1, waitHandle);
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
			var waitHandle1 = new AutoResetEvent(false);
			var waitHandle2 = new AutoResetEvent(false);
			var waitHandle3 = new AutoResetEvent(false);
			_model.AsyncLocalCheckIn("testing", r => { result1=r; waitHandle1.Set(); });
			_model.AsyncLocalCheckIn("testing", r => { result2=r; waitHandle2.Set(); });
			_model.AsyncLocalCheckIn("testing", r => { result3=r; waitHandle3.Set(); });
			WaitForTasksToFinish(3, waitHandle1, waitHandle2, waitHandle3);

			Assert.IsFalse(_model.StatusProgress.WarningEncountered, "There was a warning encountered during the sync: {0}", _model.StatusProgress.LastWarning);
			Assert.IsFalse(_model.StatusProgress.ErrorEncountered, "There was an error encountered during the sync: {0}", _model.StatusProgress.LastError);
			Assert.IsTrue(result1.Succeeded && result2.Succeeded && result3.Succeeded, "One of the CheckIn attempts did not succeed.");
		}

		[Test]
		public void AsyncLocalCheckIn_NoPreviousRepoCreation_Throws()
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				//simulate not having previously created a repository
				RobustIO.DeleteDirectoryAndContents(_pathToTestRoot.CombineForPath(".hg"));
				_model.AsyncLocalCheckIn("testing", null);
			});
		}
	}
}