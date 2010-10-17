using System;
using System.IO;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Sync
{
	/// <summary>
	/// Control that allows the "Bridge" applications to use the
	/// SyncStartControl and the Log tab of the SyncControl.
	/// </summary>
	public partial class BridgeSyncControl : UserControl
	{
		public event SyncStartingEventHandler SyncStarting;
		public delegate void SyncStartingEventHandler(object sender, SyncStartingEventArgs e);
		public event SyncFinishedEventHandler SyncFinished;
		public delegate void SyncFinishedEventHandler(object sender, SyncFinishedEventArgs e);

		private readonly SyncControlModel _model;
		private bool _didAttemptSync;
		private string _originalComment;
		private SyncResults _results;
		private readonly ProjectFolderConfiguration _projectFolderConfig;

		/// <summary></summary>
		public BridgeSyncControl()
		{
			InitializeComponent();
		}

		/// <summary></summary>
		public BridgeSyncControl(HgRepository repository, ProjectFolderConfiguration projectFolderConfiguration)
			: this()
		{
			_projectFolderConfig = projectFolderConfiguration;
			_model = new SyncControlModel(projectFolderConfiguration, SyncUIFeatures.Log | SyncUIFeatures.PlaySoundIfSuccessful, null);
			_model.SynchronizeOver += ModelSynchronizeOver;
			_model.AddProgressDisplay(_logBox);
			try
			{
				_syncStartControl.Init(repository);
			}
			catch (Exception)
			{
				_syncStartControl.Dispose(); // without this, the usbdetector just goes on and on
				throw;
			}
			_logBox.GetDiagnosticsMethod = _model.GetDiagnostics;
			UpdateDisplay();
		}

		public bool ShowAllControls { get; set; }

		void ModelSynchronizeOver(object syncResults, EventArgs e)
		{
			//Cursor.Current = Cursors.Default;
			_results = syncResults as SyncResults;
			progressBar1.MarqueeAnimationSpeed = 0;
			progressBar1.Style = ProgressBarStyle.Continuous;
			progressBar1.Maximum = 100;
			progressBar1.Value = progressBar1.Maximum;
			_didAttemptSync = true;
			UpdateDisplay();
			OnSyncFinished();
		}

		private bool OnSyncStarting()
		{
			var handler = SyncStarting;
			if (handler != null)
			{
				var cancelArgs = new SyncStartingEventArgs(Directory.GetFiles(_projectFolderConfig.FolderPath, "*.lift")[0]);
				handler(this, cancelArgs);
				return cancelArgs.Cancel;
			}
			return false; // Do not cancel, if there is no handler.
		}

		private void OnSyncFinished()
		{
			var handler = SyncFinished;
			if (handler != null)
				handler(this, new SyncFinishedEventArgs(_results));
		}

		private void SyncStartControlRepositoryChosen(object sender, SyncStartArgs e)
		{
			if (OnSyncStarting())
			{
				// TODO: Show some message about it being cancelled?
				return;
			}

			_didAttemptSync = false;
			UpdateDisplay();
			_statusText.Visible = false;

			_model.SyncOptions.RepositorySourcesToTry.Clear();
			_model.SyncOptions.RepositorySourcesToTry.Add(e.Address);
			if (_originalComment == null)
			{
				var desc = _model.SyncOptions.CheckinDescription;
				_originalComment = string.IsNullOrEmpty(desc) ? string.Empty : _model.SyncOptions.CheckinDescription = ": ";
				if (_originalComment == string.Empty)
					_model.SyncOptions.CheckinDescription = _originalComment;
			}
			if (!string.IsNullOrEmpty(e.CommitMessage))
				_model.SyncOptions.CheckinDescription = _originalComment + e.CommitMessage;

			progressBar1.Style = ProgressBarStyle.Marquee;
#if MONO
			progressBar1.MarqueeAnimationSpeed = 3000;
#else
			progressBar1.MarqueeAnimationSpeed = 50;
#endif
			_logBox.Clear();
			_logBox.Enabled = true;
			_logBox.WriteStatus("Syncing...");
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				_model.Sync(true);
				// Don't do this here. It has to be done at the end of the _model_SynchronizeOver handler.
				//OnSyncFinished();
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			if (ShowAllControls)
			{
				_successIcon.Visible = _didAttemptSync && !(_model.StatusProgress.WarningEncountered || _model.StatusProgress.ErrorEncountered);
				_warningIcon.Visible = (_model.StatusProgress.WarningEncountered || _model.StatusProgress.ErrorEncountered);
				progressBar1.Visible = _model.SynchronizingNow;// || _didAttemptSync;
				_statusText.Visible = progressBar1.Visible || _didAttemptSync;
				_statusText.Text = _model.StatusProgress.LastStatus;
				_logBox.Enabled = _model.SynchronizingNow || _didAttemptSync;
			}
			else
			{
				_successIcon.Visible = false;
				_warningIcon.Visible = false;
				progressBar1.Visible = false;
				_statusText.Visible = false;
				_statusText.Text = null;
				_logBox.Visible = false;
			}
		}
	}
}
