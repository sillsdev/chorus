using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using System.Linq;
using Chorus.VcsDrivers;
using SIL.Code;
using SIL.Extensions;
using SIL.Progress;
using SIL.Windows.Forms.Progress;

namespace Chorus.UI.Sync
{
	public class SyncControlModel
	{
		private static Object _lockToken = new object();
		private readonly IChorusUser _user;
		private readonly Synchronizer _synchronizer;
		private readonly BackgroundWorker _backgroundWorker;
		public event EventHandler SynchronizeOver;
		private readonly MultiProgress _progress;
		private SyncOptions _syncOptions;
		private BackgroundWorker _asyncLocalCheckInWorker;

		public SimpleStatusProgress StatusProgress { get; set; }

		public SyncControlModel(ProjectFolderConfiguration projectFolderConfiguration,
			SyncUIFeatures uiFeatureFlags,
			IChorusUser user)
		{
			_user = user;
			_progress = new MultiProgress();
			StatusProgress = new SimpleStatusProgress();
			_progress.Add(StatusProgress);
			Features = uiFeatureFlags;
			_synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, _progress);
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += worker_DoWork;

			//clients will normally change these
			SyncOptions = new SyncOptions();
			SyncOptions.CheckinDescription = "[" + Application.ProductName + ": " + Application.ProductVersion + "] sync";
			SyncOptions.DoPullFromOthers = true;
			SyncOptions.DoMergeWithOthers = true;
			SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled));
		}

		void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (SynchronizeOver != null)
			{
				UnmanagedMemoryStream stream=null;

				if (_progress.ErrorEncountered)
				{
					stream = Properties.Resources.errorSound;
				}
				else if (_progress.WarningsEncountered)
				{
					stream = Properties.Resources.warningSound;
				}
				else
				{
					if (HasFeature(SyncUIFeatures.PlaySoundIfSuccessful))
						stream = Properties.Resources.finishedSound;
				}

				if (stream != null)
				{
					using (SoundPlayer player = new SoundPlayer(stream))
					{
						player.PlaySync();
					}
					stream.Dispose();
				}

				if (e.Cancelled)
				{
					var r = new SyncResults();
					r.Succeeded = false;
					r.Cancelled = true;
					SynchronizeOver.Invoke(r, null);
				}
				else //checking e.Result if there was a cancellation causes an InvalidOperationException
				{
					SynchronizeOver.Invoke(e.Result as SyncResults, null);
				}
			}
		}

		public string UserName
		{
			get
			{
				if(_user==null)
				{
					return "anonymous";
				}

				return _user.Name;
			}
		}

		public SyncUIFeatures Features
		{ get; set; }

		public bool EnableSendReceive
		{
			get { return !EnableClose && HasFeature(SyncUIFeatures.SendReceiveButton) && !_backgroundWorker.IsBusy; }
		}

		public bool EnableCancel
		{
			get
			{
				if (_backgroundWorker.IsBusy)
					return true;
				else
					return false;
			}
		}

		public bool ShowTabs
		{
			get {
				return HasFeature(SyncUIFeatures.Log) ||
					ShowAdvancedSelector ||
					HasFeature(SyncUIFeatures.TaskList); }
		}
		private bool ShowAdvancedSelector
		{
			get
			{
				return !HasFeature(SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced)
					   && !HasFeature(SyncUIFeatures.Minimal);
			}
		}

		public bool ShowSyncButton
		{
			get { return !EnableClose && HasFeature(SyncUIFeatures.SendReceiveButton); }
		}

		public bool EnableClose{get;set;}

		public bool SynchronizingNow
		{
			get { return _backgroundWorker.IsBusy; }
		}

		public SyncOptions SyncOptions
		{
			get { return _syncOptions; }
			set { _syncOptions = value; }
		}

		public bool CancellationPending
		{
			get { return _backgroundWorker.CancellationPending;  }
		}

		public IProgressIndicator ProgressIndicator
		{
			get { return _progress.ProgressIndicator; }
			set { _progress.ProgressIndicator = value; }
		}

		public SynchronizationContext UIContext
		{
			set { _progress.SyncContext = value; }
		}

		public bool ErrorsOrWarningsEncountered
		{
			get { return _progress.ErrorEncountered || _progress.WarningsEncountered; }
		}

		public bool HasFeature(SyncUIFeatures feature)
		{
			return (Features & feature) == feature;
		}

		public List<RepositoryAddress> GetRepositoriesToList()
		{
			//nb: at the moment, we can't just get it new each time, because it stores the
			//enabled state of the check boxes
			return _synchronizer.GetPotentialSynchronizationSources();
		}

		/// <summary>
		/// Sync
		/// </summary>
		/// <param name="useTargetsAsSpecifiedInSyncOptions">This is a bit of a hack
		/// until I figure out something better... it will be true for cases where
		/// the app is just doing a backup..., false were we want to sync to whatever
		/// sites the user has indicated</param>
		public void Sync(bool useTargetsAsSpecifiedInSyncOptions)
		{
			_progress.WriteStatus("Syncing...");
			lock (this)
			{
				if(_backgroundWorker.IsBusy)
					return;
				if (!useTargetsAsSpecifiedInSyncOptions)
				{
//                    foreach (var address in GetRepositoriesToList().Where(r => !r.Enabled))
//                    {
//                        SyncOptions.RepositorySourcesToTry.RemoveAll(x => x.URI == address.URI);
//                    }
				   // SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled && !SyncOptions.RepositorySourcesToTry.Any(x=>x.URI ==r.URI)));
					SyncOptions.RepositorySourcesToTry.Clear();
					SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled ));

				}
				_backgroundWorker.RunWorkerAsync(new object[] {_synchronizer, SyncOptions, _progress});
			}
		}

		public void Cancel()
		{
			lock (this)
			{
				if(!_backgroundWorker.IsBusy)
					return;

				_backgroundWorker.CancelAsync();//this only gets picked up when the synchronizer checks it, which may be after some long operation is finished
				_progress.CancelRequested = true;//this gets picked up by the low-level process reader
			}
		}

		static void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			object[] args = e.Argument as object[];
			Synchronizer synchronizer = args[0] as Synchronizer;
			// We need to make sure that only one thread at a time goes through SyncNow()
			// to avoid race conditions accessing the repository.
			lock (_lockToken)
			{
				e.Result = synchronizer.SyncNow(sender as BackgroundWorker, e, args[1] as SyncOptions);
			}
		}

		public void PathEnabledChanged(RepositoryAddress address, CheckState state)
		{
			address.Enabled = (state == CheckState.Checked);

			//NB: we may someday decide to distinguish between this chorus-app context of "what
			//repos I used last time" and the hgrc default which effect applications (e.g. wesay)
			_synchronizer.SetIsOneOfDefaultSyncAddresses(address, address.Enabled);
		}

		public void AddMessagesDisplay(IProgress progress)
		{
			_progress.AddMessageProgress(progress);
		}

		public void AddStatusDisplay(IProgress progress)
		{
			_progress.AddStatusProgress(progress);
		}

		public void GetDiagnostics(IProgress progress)
		{
			_synchronizer.Repository.GetDiagnosticInformation(progress);
		}

		public void SetSynchronizerAdjunct(ISychronizerAdjunct adjunct)
		{
			_synchronizer.SynchronizerAdjunct = adjunct;
		}


		/// <summary>
		/// Check in, to the local disk repository, any changes to this point.
		/// </summary>
		/// <param name="checkinDescription">A description of what work was done that you're wanting to checkin. E.g. "Delete a Book"</param>
		/// <param name="callbackWhenFinished">Can be null if you don't want any progress report</param>
		public void AsyncLocalCheckIn(string checkinDescription, Action<SyncResults> callbackWhenFinished)
		{
			var repoPath = this._synchronizer.Repository.PathToRepo.CombineForPath(".hg");
			Require.That(Directory.Exists(repoPath), "The repository should already exist before calling AsyncLocalCheckIn(). Expected to find the hg folder at " + repoPath);

			//NB: if someone were to call this fast and repeatedly, I won't vouch for any kind of safety here.
			//This is just designed for checking in occasionally, like as users do some major new thing, or finish some task.
			if (_asyncLocalCheckInWorker != null && !_asyncLocalCheckInWorker.IsBusy)
			{
				_asyncLocalCheckInWorker.Dispose(); //timidly avoid a leak
			}
			_asyncLocalCheckInWorker = new BackgroundWorker();
			_asyncLocalCheckInWorker.DoWork += (o, args) =>
			{

				var options = new SyncOptions
				{
					CheckinDescription = checkinDescription,
					DoMergeWithOthers = false,
					DoPullFromOthers = false,
					DoSendToOthers = false
				};
				// We need to make sure that only one thread at a time goes through SyncNow()
				// to avoid race conditions accessing the repository.
				SyncResults result;
				lock (_lockToken)
				{
					result = _synchronizer.SyncNow(options);
				}
				callbackWhenFinished?.Invoke(result);
			};
			_asyncLocalCheckInWorker.RunWorkerAsync();
		}
	}

	[Flags]
	public enum SyncUIFeatures
	{
		Minimal =0,
		SendReceiveButton=2,
		TaskList=4,
		Log = 8,
		SimpleRepositoryChooserInsteadOfAdvanced = 16,
		PlaySoundIfSuccessful = 32,
		NormalRecommended = 0xFFFF - (SendReceiveButton),
		Advanced = 0xFFFF - (SimpleRepositoryChooserInsteadOfAdvanced)
	}
}