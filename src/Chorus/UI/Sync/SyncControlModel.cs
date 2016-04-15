// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Chorus.sync;
using Palaso.UI.WindowsForms.Progress;
using Chorus.VcsDrivers;
using Palaso.Progress;
using System.Threading;

namespace Chorus.UI.Sync
{
	public class SyncControlModel: SyncControlModelSimple
	{
		public SyncControlModel(ProjectFolderConfiguration projectFolderConfiguration,
			SyncUIFeatures uiFeatureFlags, IChorusUser user): base(projectFolderConfiguration,
				user, new SimpleStatusProgress())
		{
			Features = uiFeatureFlags;
			SyncOptions.CheckinDescription = string.Format("[{0}: {1}] sync",
				Application.ProductName, Application.ProductVersion);
		}

		protected override void OnBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			UnmanagedMemoryStream stream = null;

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

			base.OnBackgroundWorkerRunWorkerCompleted(sender, e);
		}

		public void PathEnabledChanged(RepositoryAddress address, CheckState state)
		{
			base.PathEnabledChanged(address, state == CheckState.Checked);
		}

		/// <summary>
		/// Gets a value indicating whether to enable send receive.
		/// </summary>
		public bool EnableSendReceive
		{
			get { return !EnableClose && HasFeature(SyncUIFeatures.SendReceiveButton) && !_backgroundWorker.IsBusy; }
		}

		/// <summary>
		/// Gets a value indicating whether to enable cancel.
		/// </summary>
		public bool EnableCancel
		{
			get
			{
				return _backgroundWorker.IsBusy;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to show tabs.
		/// </summary>
		public bool ShowTabs
		{
			get
			{
				return HasFeature(SyncUIFeatures.Log) ||
					ShowAdvancedSelector ||
					HasFeature(SyncUIFeatures.TaskList);
			}
		}

		private bool ShowAdvancedSelector
		{
			get
			{
				return !HasFeature(SyncUIFeatures.SimpleRepositoryChooserInsteadOfAdvanced)
					&& !HasFeature(SyncUIFeatures.Minimal);
			}
		}

		/// <summary>
		/// Gets a value indicating whether to show sync button.
		/// </summary>
		public bool ShowSyncButton
		{
			get { return !EnableClose && HasFeature(SyncUIFeatures.SendReceiveButton); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to enable close.
		/// </summary>
		public bool EnableClose { get; set; }

		/// <summary>
		/// Gets or sets the progress indicator.
		/// </summary>
		public IProgressIndicator ProgressIndicator
		{
			get { return _progress.ProgressIndicator; }
			set { _progress.ProgressIndicator = value; }
		}

		/// <summary>
		/// Sets the user interface context.
		/// </summary>
		public SynchronizationContext UIContext
		{
			set { _progress.SyncContext = value; }
		}

		/// <summary>
		/// Adds the messages display.
		/// </summary>
		public void AddMessagesDisplay(IProgress progress)
		{
			_progress.AddMessageProgress(progress);
		}

		/// <summary>
		/// Adds the status display.
		/// </summary>
		public void AddStatusDisplay(IProgress progress)
		{
			_progress.AddStatusProgress(progress);
		}

		/// <summary>
		/// Gets the diagnostics.
		/// </summary>
		/// <param name="progress">Progress.</param>
		public void GetDiagnostics(IProgress progress)
		{
			_synchronizer.Repository.GetDiagnosticInformation(progress);
		}

		/// <summary>
		/// Sets the synchronizer adjunct.
		/// </summary>
		public void SetSynchronizerAdjunct(ISychronizerAdjunct adjunct)
		{
			_synchronizer.SynchronizerAdjunct = adjunct;
		}

		/// <summary>
		/// Cancel the current operation
		/// </summary>
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

		/// <summary>
		/// Gets or sets the features.
		/// </summary>
		public SyncUIFeatures Features { get; set; }

		/// <summary>
		/// Determines whether this instance has the specified feature.
		/// </summary>
		public bool HasFeature(SyncUIFeatures feature)
		{
			return (Features & feature) == feature;
		}

	}
}
