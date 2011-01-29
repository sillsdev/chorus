using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using Chorus.clone;
using Chorus.UI.Misc;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromInternetDialog : Form
	{
		private readonly GetCloneFromInternetModel _model;
		private IProgress _progress;
		private readonly BackgroundWorker _backgroundWorker;
		private enum State { AskingUserForURL, MakingClone, Success, Error,Cancelled}

		private TargetFolderControl _targetFolderControl;
		private StatusProgress _statusProgress;
		private State _state;
		private ServerSettingsControl _serverSettingsControl;

		public GetCloneFromInternetDialog(string parentDirectoryToPutCloneIn)
			:this(new GetCloneFromInternetModel(parentDirectoryToPutCloneIn))
		{
		}

		public GetCloneFromInternetDialog(GetCloneFromInternetModel model)
		{
			_model = model;
//#if !MONO
			Font = SystemFonts.MessageBoxFont;
//#endif
			InitializeComponent();

			this.Font = SystemFonts.MessageBoxFont;

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
			_backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);

			_statusProgress = new StatusProgress();
			_progress = new MultiProgress(new IProgress[]{_logBox, _statusProgress});

			_serverSettingsControl = new ServerSettingsControl(_model);
			_serverSettingsControl.TabIndex = 0;
			_serverSettingsControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left);
			this.Controls.Add(_serverSettingsControl);

			_targetFolderControl = new TargetFolderControl(_model);
			_targetFolderControl.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			_targetFolderControl._downloadButton.Click+=new EventHandler(OnDownloadClick);
			_targetFolderControl.Location = new Point(0, _serverSettingsControl.Height +10);
			this.MinimumSize = new Size(_targetFolderControl.MinimumSize.Width+20, _targetFolderControl.Bottom +20);
			if (_targetFolderControl.Bottom +30> Bottom)
			{
				this.Size = new Size(this.Width,_targetFolderControl.Bottom + 30);
			}
			_targetFolderControl.TabIndex = 1;
			this.Controls.Add(_targetFolderControl);
			_okButton.TabIndex = 90;
			_cancelButton.TabIndex = 91;

			_fixSettingsButton.Left = _cancelButton.Left;
			 _targetFolderControl._downloadButton.Top = _okButton.Top-_targetFolderControl.Top	;
			 _targetFolderControl._downloadButton.Left = _okButton.Left - 15;

			_logBox.GetDiagnosticsMethod = (progress) =>
											{
												var hg = new HgRepository(PathToNewProject, progress);
												hg.GetDiagnosticInformationForRemoteProject(progress, ThreadSafeUrl);
											};

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
					var repo = new HgRepository(_model.TargetDestination, _progress);
					var name = new Uri(_model.URL).Host;
					if (String.IsNullOrEmpty(name)) //This happens for repos on the local machine
					{
						name = "LocalRepository";
					}
					if (name.ToLower().Contains("languagedepot"))
						name = "LanguageDepot";

					var address = RepositoryAddress.Create(name, _model.URL);

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
					_targetFolderControl.Visible = true;
					 _cancelButton.Enabled = true;
				   _cancelTaskButton.Visible = false;

					break;
				case State.MakingClone:
					_progressBar.Focus();
					_progressBar.Select();
					_targetFolderControl.Visible = false;
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
					_targetFolderControl.Visible = false;
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
					_fixSettingsButton.Focus();
					_cancelButton.Enabled = false;
					_cancelButton.Text = "&Cancel";
					_cancelButton.Select();
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = "Failed.";
					_progressBar.Visible = false;
					_targetFolderControl.Visible = false;
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
					_targetFolderControl.Visible = false;
					_statusLabel.Left =  _progressBar.Left;
					_statusImage.Visible = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_serverSettingsControl.Visible = _targetFolderControl.Visible;
		}

		private void OnLoad(object sender, EventArgs e)
		{
			UpdateDisplay(State.AskingUserForURL);
			_logBox.BackColor = this.BackColor;
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   DialogResult = DialogResult.OK;
			Close();
		}

		public string PathToNewProject
		{
			get { return _model.TargetDestination; }
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
				ThreadSafeUrl = _model.URL;
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
			_statusProgress = new StatusProgress();
			UpdateDisplay(State.AskingUserForURL);
		}

		private void GetCloneFromInternetDialog_BackColorChanged(object sender, EventArgs e)
		{
			_logBox.BackColor  =this.BackColor;
		}

	}
}
