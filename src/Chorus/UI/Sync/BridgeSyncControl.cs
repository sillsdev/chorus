using System;
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
		private readonly SyncControlModel _model;
		private bool _didAttemptSync;
		private string _originalComment;

		/// <summary></summary>
		public BridgeSyncControl()
		{
			InitializeComponent();
		}

		/// <summary></summary>
		public BridgeSyncControl(HgRepository repository, ProjectFolderConfiguration projectFolderConfiguration)
			: this()
		{
			_model = new SyncControlModel(projectFolderConfiguration, SyncUIFeatures.Log | SyncUIFeatures.PlaySoundIfSuccessful, null);
			_model.SynchronizeOver += _model_SynchronizeOver;
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

		void _model_SynchronizeOver(object sender, EventArgs e)
		{
			//Cursor.Current = Cursors.Default;
			progressBar1.MarqueeAnimationSpeed = 0;
			progressBar1.Style = ProgressBarStyle.Continuous;
			progressBar1.Maximum = 100;
			progressBar1.Value = progressBar1.Maximum;
			_didAttemptSync = true;
			UpdateDisplay();
		}

		private void SelectedRepository(object sender, SyncStartArgs e)
		{
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
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			_successIcon.Visible = _didAttemptSync && !(_model.StatusProgress.WarningEncountered || _model.StatusProgress.ErrorEncountered);
			_warningIcon.Visible = (_model.StatusProgress.WarningEncountered || _model.StatusProgress.ErrorEncountered);
			progressBar1.Visible = _model.SynchronizingNow;// || _didAttemptSync;
			_statusText.Visible = progressBar1.Visible || _didAttemptSync;
			_statusText.Text = _model.StatusProgress.LastStatus;
			_logBox.Enabled = _model.SynchronizingNow || _didAttemptSync;
		}
	}
}
