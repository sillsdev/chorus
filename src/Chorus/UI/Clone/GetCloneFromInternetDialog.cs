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
		private CloneFromUsb _model;
		private IProgress _progress;
		private readonly BackgroundWorker _backgroundWorker;
		private enum State { AskingUserForURL, MakingClone, Success, Error,
		Cancelled
		}

		private InternetCloneInstructionsControl _internetCloneInstructionsControl;
		private StatusProgress _statusProgress;
		private State _state;

		public GetCloneFromInternetDialog(string parentDirectoryToPutCloneIn)
		{
//#if !MONO
			Font = SystemFonts.MessageBoxFont;
//#endif
			InitializeComponent();

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);

			_statusProgress = new StatusProgress();
			_progress = new MultiProgress(new IProgress[]{_logBox, _statusProgress});

			_internetCloneInstructionsControl = new InternetCloneInstructionsControl(parentDirectoryToPutCloneIn);
		//	_internetCloneInstructionsControl.AutoSize = true;
			_internetCloneInstructionsControl.TabIndex = 0;
			_internetCloneInstructionsControl.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			_internetCloneInstructionsControl._downloadButton.Click+=new EventHandler(OnDownloadClick);
			this.MinimumSize = new Size(_internetCloneInstructionsControl.MinimumSize.Width+20, _internetCloneInstructionsControl.MinimumSize.Height+20);
			if (_internetCloneInstructionsControl.Bottom +30> Bottom)
			{
				this.Size = new Size(this.Width,_internetCloneInstructionsControl.Bottom + 30);
			}
			this.Controls.Add(_internetCloneInstructionsControl);

			_fixSettingsButton.Left = _cancelButton.Left;
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
				HgRepository.Clone(ThreadSafeUrl, PathToNewProject, _progress);
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
			}
		}

		public CloneFromUsb Model { get { return _model; } }

		private void UpdateDisplay(State newState)
		{
			_state = newState;
			_fixSettingsButton.Visible = false;
			switch (_state)
			{
				case State.AskingUserForURL:
					_statusLabel.Visible = false;
					_statusImage.Visible   =false;
					_logBox.Visible = false;
					_okButton.Visible = false;
					_progressBar.Visible = false;
					_internetCloneInstructionsControl.Visible = true;
					 _cancelButton.Enabled = true;
				   _cancelTaskButton.Visible = false;

					break;
				case State.MakingClone:
					_internetCloneInstructionsControl.Visible = false;
					_statusImage.Visible = false;
					_progressBar.Visible = true;
					_progressBar.Style = ProgressBarStyle.Marquee;
#if MONO
					_progressBar.MarqueeAnimationSpeed = 3000;
#else
					_progressBar.MarqueeAnimationSpeed = 50;
#endif
					_statusLabel.Visible = true;
					_statusLabel.Text = "Getting project...";
					_statusLabel.Left = _progressBar.Left;
					_logBox.Visible = true;
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
					_logBox.Visible = true;
					break;
				case State.Error:
					_fixSettingsButton.Visible = true;
					_cancelButton.Enabled = false;
					_cancelButton.Text = "&Cancel";
					_cancelButton.Select();
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Failed.";
					_progressBar.Visible = false;
					_internetCloneInstructionsControl.Visible = false;
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.ImageKey = "Error";
					_statusImage.Visible = true;
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
				_logBox.Clear();
				if(_backgroundWorker.IsBusy)
					return;
				UpdateDisplay(State.MakingClone);
				ThreadSafeUrl = _internetCloneInstructionsControl.URL;
				_backgroundWorker.RunWorkerAsync(new object[] { ThreadSafeUrl, PathToNewProject, _progress });
			}

		}


		public string ThreadSafeUrl
		{
			get;
			set;
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

		private void _logBox_Load(object sender, EventArgs e)
		{
		}

		private void _fixSettingsButton_Click(object sender, EventArgs e)
		{
				UpdateDisplay(State.AskingUserForURL);
		}

	}
}
