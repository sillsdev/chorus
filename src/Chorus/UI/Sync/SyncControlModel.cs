using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;
using System.Linq;
using Chorus.VcsDrivers;

namespace Chorus.UI.Sync
{
	public class SyncControlModel
	{
		private readonly Synchronizer _synchronizer;
		private readonly BackgroundWorker _backgroundWorker;
		public event EventHandler SynchronizeOver;
		private readonly MultiProgress _progress;
		private SyncOptions _syncOptions;
		public StatusProgress StatusProgress { get; private set; }

		public SyncControlModel(ProjectFolderConfiguration projectFolderConfiguration, SyncUIFeatures uiFeatureFlags)
		{
			StatusProgress = new StatusProgress();
			_progress = new MultiProgress(new[] { StatusProgress });
			Features = uiFeatureFlags;
			_synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, _progress);
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += worker_DoWork;

			//clients will normally change these
			SyncOptions = new SyncOptions();
			SyncOptions.CheckinDescription = "["+Application.ProductName+"] sync";
			SyncOptions.DoPullFromOthers = true;
			SyncOptions.DoMergeWithOthers = true;
			SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled));
		}

		void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (SynchronizeOver != null)
			{
				UnmanagedMemoryStream stream=null;
				if (this.StatusProgress.ErrorEncountered)
				{
					stream = Properties.Resources.errorSound;
				}
				else if (this.StatusProgress.WarningEncountered)
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
						player.Play();
					}
					stream.Dispose();
				}

				var synchResults = e.Result as SyncResults;
				SynchronizeOver.Invoke(synchResults, null);
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
			get { return _backgroundWorker.IsBusy; }
		}

		public bool ShowTabs
		{
			get {
				return HasFeature(SyncUIFeatures.Log) ||
					!HasFeature(SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced) ||
					HasFeature(SyncUIFeatures.TaskList); }
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
			lock (this)
			{
				if(_backgroundWorker.IsBusy)
					return;
				if (!useTargetsAsSpecifiedInSyncOptions)
				{
					foreach (var address in GetRepositoriesToList().Where(r => !r.Enabled))
					{
						SyncOptions.RepositorySourcesToTry.RemoveAll(x => x.URI == address.URI);
					}

					SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled && !SyncOptions.RepositorySourcesToTry.Any(x=>x.URI ==r.URI)));
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

				_backgroundWorker.CancelAsync();
			}
		}

		static void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			object[] args = e.Argument as object[];
			Synchronizer synchronizer = args[0] as Synchronizer;
			e.Result =  synchronizer.SyncNow(sender as BackgroundWorker, e, args[1] as SyncOptions);
		}

		public void PathEnabledChanged(RepositoryAddress address, CheckState state)
		{
			address.Enabled = (state == CheckState.Checked);

			//NB: we may someday decide to distinguish between this chorus-app context of "what
			//repos I used last time" and the hgrc default which effect applications (e.g. wesay)
			_synchronizer.SetIsOneOfDefaultSyncAddresses(address, address.Enabled);
		}

		public void AddProgressDisplay(IProgress progress)
		{
			_progress.Add(progress);
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