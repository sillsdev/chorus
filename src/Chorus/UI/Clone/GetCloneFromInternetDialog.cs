using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using Chorus.clone;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromInternetDialog : Form
	{
		private readonly string _parentDirectoryToPutCloneIn;
		private CloneFromUsb _model;
		private IProgress _progress;
		private readonly BackgroundWorker _backgroundWorker;
		private enum State { AskingUserForURL, MakingClone, Success, Error,
		Cancelled
		}

		private InternetCloneInstructionsControl _internetCloneInstructionsControl;
		private StatusProgress _statusProgress;
		private State _state;
		private string _failureMessage;

		public GetCloneFromInternetDialog(string parentDirectoryToPutCloneIn)
		{
			_parentDirectoryToPutCloneIn = parentDirectoryToPutCloneIn;
#if !MONO
			Font = SystemFonts.MessageBoxFont;
#endif
			InitializeComponent();

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);

			_statusProgress = new StatusProgress();
			var verboseProgress = new TextBoxProgress(_progressLogVerbose);
			verboseProgress.ShowVerbose = true;
			_progress = new MultiProgress(new IProgress[]{new TextBoxProgress(_progressLog), verboseProgress, _statusProgress});

			_internetCloneInstructionsControl = new InternetCloneInstructionsControl(parentDirectoryToPutCloneIn);
		//	_internetCloneInstructionsControl.AutoSize = true;
			_internetCloneInstructionsControl.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			_internetCloneInstructionsControl._downloadButton.Click+=new EventHandler(OnDownloadClick);
			this.MinimumSize = new Size(_internetCloneInstructionsControl.MinimumSize.Width+20, _internetCloneInstructionsControl.MinimumSize.Height+20);
			if (_internetCloneInstructionsControl.Bottom +30> Bottom)
			{
				this.Size = new Size(this.Width,_internetCloneInstructionsControl.Bottom + 30);
			}
			this.Controls.Add(_internetCloneInstructionsControl);
			_progressLogVerbose.Visible = false;
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
					var repo = new HgRepository(_internetCloneInstructionsControl.TargetDestination, _progress);
					var name = new Uri(_internetCloneInstructionsControl.URL).Host;
					if (name.ToLower().Contains("languagedepot"))
						name = "LanguageDepot";

					var address = RepositoryAddress.Create(name,_internetCloneInstructionsControl.URL);

					//this will also remove the "default" one that hg puts in, which we don't really want.
					repo.SetKnownRepositoryAddresses(new[] {address});
					repo.SetIsOneDefaultSyncAddresses( address, true);
					UpdateDisplay(State.Success);
				}
				catch (Exception error)
				{
					_progress.WriteError(error.Message);
					UpdateDisplay(State.Error);
				}
			}
		}

		void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				//review: do we need to get these out of the DoWorkEventArgs instead?
				HgRepository.Clone(URL, PathToNewProject, _progress);
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.finishedSound))
				{
					player.Play();
				}

			}
			catch (Exception error)
			{
				_progress.WriteError(error.Message);
				using (SoundPlayer player = new SoundPlayer(Properties.Resources.errorSound))
				{
					player.Play();
				}
				_failureMessage = error.Message;
			}
		}

		public CloneFromUsb Model { get { return _model; } }

		private void UpdateDisplay(State newState)
		{
			_state = newState;
			switch (_state)
			{
				case State.AskingUserForURL:
					_statusLabel.Visible = false;
					_statusImage.Visible   =false;
					_progressLog.Visible = false;
					_okButton.Visible = false;
					_progressBar.Visible = false;
					_showVerboseLink.Visible = false;
					_internetCloneInstructionsControl.Visible = true;
					 _cancelButton.Enabled = true;
				   _cancelTaskButton.Visible = false;

					break;
				case State.MakingClone:
					_internetCloneInstructionsControl.Visible = false;
					_statusImage.Visible = false;
					_progressBar.Visible = true;
					_progressBar.Style = ProgressBarStyle.Marquee;
					_progressBar.MarqueeAnimationSpeed = 50;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Getting project...";
					_statusLabel.Left = _progressBar.Left;
					_progressLog.Visible = true;
					_showVerboseLink.Visible = true;
					_cancelTaskButton.Visible = true;
					_cancelButton.Enabled = false;
					break;
				case State.Success:
					_cancelButton.Enabled = false;
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Done.";
					_progressBar.Visible = false;
					_internetCloneInstructionsControl.Visible = false;
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.Visible = true;
					_statusImage.ImageKey="Success";
					_statusLabel.Text = string.Format("Finished");
					_okButton.Visible = true;
					_cancelButton.Enabled = false;
					_showVerboseLink.Visible = true;
					_progressLog.Visible = true;
					break;
				case State.Error:
					_cancelButton.Enabled = true;
					_cancelButton.Text = "Close";
					_cancelButton.Select();
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Failed.";
					_progressBar.Visible = false;
					_internetCloneInstructionsControl.Visible = false;
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.ImageKey = "Error";
					_statusImage.Visible = true;
					_showVerboseLink.Visible = true;
					break;
				case State.Cancelled:
					_cancelButton.Enabled = true;
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Cancelled.";
					_progressBar.Visible = false;
					_internetCloneInstructionsControl.Visible = false;
					_statusLabel.Left =  _progressBar.Left;
					_statusImage.Visible = false;
					_showVerboseLink.Visible = true;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			UpdateDisplay(State.AskingUserForURL);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   DialogResult = DialogResult.OK;
			Close();
		}

		public string PathToNewProject
		{
			get { return _internetCloneInstructionsControl.TargetDestination; }
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
				DialogResult = DialogResult.Cancel;
				Close();
		}

		private void OnDownloadClick(object sender, EventArgs e)
		{
			lock (this)
			{
				if(_backgroundWorker.IsBusy)
					return;
				UpdateDisplay(State.MakingClone);
				_backgroundWorker.RunWorkerAsync(new object[] {URL, PathToNewProject, _progress});
			}

		}


		public string URL
		{
			get { return _internetCloneInstructionsControl.URL; }
			set { _internetCloneInstructionsControl.URL = value; }
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (_state == State.MakingClone)
			{
				lock (this)
				{
					if (!_backgroundWorker.IsBusy)
						return;

					_backgroundWorker.CancelAsync();//the hg call will know nothing of this
					_progress.CancelRequested = true; //but it will be monitoring this
				}
			}
		}

		private void _showVerboseLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_progressLog.Visible = false;
			_progressLogVerbose.Bounds = _progressLog.Bounds;
			_progressLogVerbose.Visible = true;
			_showVerboseLink.Enabled = false;
		}
	}
}
