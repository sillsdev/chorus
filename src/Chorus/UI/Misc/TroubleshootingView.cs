using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Media;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Misc
{
	public partial class TroubleshootingView : UserControl
	{
		private readonly HgRepository _repository;
		private readonly BackgroundWorker _backgroundWorker;
		private enum State
		{
			WaitingForUserToStart, GatheringInfo, Success, Error,Cancelled
		}
		private State _state;
		private IProgress _progress;
		private StatusProgress _statusProgress;

		public TroubleshootingView(HgRepository repository)
		{
			_state = State.WaitingForUserToStart;
			_repository = repository;
			InitializeComponent();
			_progress = new TextBoxProgress(_outputBox);
			_statusProgress = new StatusProgress();
			_progress = new MultiProgress(new IProgress[] { new TextBoxProgress(_outputBox), _statusProgress, new LabelStatus(_statusLabel) });
			_statusLabel.Text = string.Empty;

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
		}

		private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_statusProgress.ErrorEncountered)
				UpdateDisplay(State.Error);
			else if (_statusProgress.WasCancelled)
				UpdateDisplay(State.Cancelled);
			else
			{
				try
				{
					UpdateDisplay(State.Success);
				}
				catch (Exception error)
				{
					_progress.WriteError(error.Message);
					UpdateDisplay(State.Error);
				}
			}
		}

		private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				_repository.GetDiagnosticInformation(_progress);
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.finishedSound))
				{
					player.PlaySync();
				}
			}
			catch (Exception error)
			{
				_progress.WriteError(error.Message);
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.errorSound))
				{
					player.PlaySync();
				}

			}
		}

		private void _runDiagnosticsButton_Click(object sender, EventArgs e)
		{
			_outputBox.Text = string.Empty;
			if (_backgroundWorker.IsBusy)
				return;
			UpdateDisplay(State.GatheringInfo);
			_backgroundWorker.RunWorkerAsync();
		}

		private void _copyLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!string.IsNullOrEmpty(_outputBox.Text))
			{
				Clipboard.SetText(_outputBox.Text);
			}
		}
		private void UpdateDisplay(State newState)
		{
			_state = newState;
			switch (_state)
			{
				case State.WaitingForUserToStart:
					_runDiagnosticsButton.Enabled = true;
				   // _cancelTaskButton.Visible = false;
					break;

				case State.GatheringInfo:
					_runDiagnosticsButton.Enabled = false;
					Cursor = Cursors.WaitCursor;
					break;
			  case State.Success:
					Cursor = Cursors.Default;
					_runDiagnosticsButton.Enabled = true;
					break;
				case State.Error:
					Cursor = Cursors.Default;
					_runDiagnosticsButton.Enabled = true;
					break;
				case State.Cancelled:
					Cursor = Cursors.Default;
					_runDiagnosticsButton.Enabled = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
