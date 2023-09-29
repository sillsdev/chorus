using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.Help;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using SIL.PlatformUtilities;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromInternetDialog : Form, ICloneSourceDialog
	{
		private readonly GetCloneFromInternetModel _model;
		private readonly BackgroundWorker _backgroundWorker;
		private enum State { AskingUserForURL, MakingClone, Success, Error,Cancelled}

		private TargetFolderControl _targetFolderControl;
		private State _state;
		private ServerSettingsControl _serverSettingsControl;
		private bool _resizing = false;

		public GetCloneFromInternetDialog(string parentDirectoryToPutCloneIn)
			:this(new GetCloneFromInternetModel(parentDirectoryToPutCloneIn))
		{
		}

		public GetCloneFromInternetDialog(GetCloneFromInternetModel model)
		{
			_model = model;
			Font = SystemFonts.MessageBoxFont;
			InitializeComponent();

			Font = SystemFonts.MessageBoxFont;

			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
			_backgroundWorker.DoWork += _backgroundWorker_DoWork;

			_logBox.ShowCopyToClipboardMenuItem = true;
			_logBox.ShowDetailsMenuItem = true;
			_logBox.ShowDiagnosticsMenuItem = true;
			_logBox.ShowFontMenuItem = true;


			_model.AddProgress(_statusProgress);
			_statusProgress.Text = "";
			_statusProgress.Visible = false;
			_model.AddMessageProgress(_logBox);
			_model.ProgressIndicator = _progressBar;
			_model.UIContext = SynchronizationContext.Current;

			_serverSettingsControl = new ServerSettingsControl(){Model=_model};
			_serverSettingsControl.TabIndex = 0;
			_serverSettingsControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			Controls.Add(_serverSettingsControl);

			_targetFolderControl = new TargetFolderControl(_model);
			_targetFolderControl.Anchor = (AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			_targetFolderControl._downloadButton.Click+=OnDownloadClick;
			_targetFolderControl.Location = new Point(0, _serverSettingsControl.Height +10);
			var minimumWidth = Math.Max(_serverSettingsControl.MinimumSize.Width, _targetFolderControl.MinimumSize.Width) + 20;
			MinimumSize = new Size(minimumWidth, _targetFolderControl.Bottom + 20);
			// On Linux, we have to set the dialog width, then set the control width back to what it had been. TODO: different order
			var sscWidth =  _serverSettingsControl.Width;
			Width = sscWidth + 30;
			_serverSettingsControl.Width = sscWidth;
			if (_targetFolderControl.Bottom +30> Bottom)
			{
				this.Size = new Size(this.Width,_targetFolderControl.Bottom + 30);
			}
			_targetFolderControl.TabIndex = 1;
			this.Controls.Add(_targetFolderControl);

			_fixSettingsButton.Left = _okButton.Left = _cancelButton.Left;
			var fixBtnWidth = _fixSettingsButton.Width;
			_fixSettingsButton.AutoSize = true;
			if (_fixSettingsButton.Width > fixBtnWidth)
			{
				// The button was too small before autosizing, but now it may extend off the dialog...
				var diff = _fixSettingsButton.Width - fixBtnWidth;
				if (diff < _cancelButton.Left)
					_fixSettingsButton.Left = _cancelButton.Left - diff;
			}
			_targetFolderControl._downloadButton.Top = _cancelButton.Top-_targetFolderControl.Top	;
			_targetFolderControl._downloadButton.Left = _cancelButton.Left - 100;

			_logBox.GetDiagnosticsMethod = (progress) =>
											{
												var hg = new HgRepository(PathToNewlyClonedFolder, progress);
												hg.GetDiagnosticInformationForRemoteProject(progress, ThreadSafeUrl);
											};

		}

		private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_statusProgress.ErrorEncountered)
			{
				UpdateDisplay(State.Error);
				_model.CleanUpAfterErrorOrCancel();
				_statusProgress.Reset();
			}
			else if (_model.CancelRequested)
			{
				_model.CancelRequested = false;
				UpdateDisplay(State.Cancelled);
				_model.CleanUpAfterErrorOrCancel();
				_statusProgress.Reset();
			}
			else
			{
				UpdateDisplay(_model.SetRepositoryAddress() ? State.Success : State.Error);
			}
		}

		void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			_model.DoClone();
		}

		private void UpdateDisplay(State newState)
		{
			_state = newState;
			_fixSettingsButton.Visible = false;
			_serverSettingsControl.Visible = false;
			_targetFolderControl.Visible = false;
			switch (_state)
			{
				case State.AskingUserForURL:
					_statusLabel.Visible = false;
					_statusImage.Visible   =false;
					_logBox.Visible = false;
					_okButton.Visible = false;
					_progressBar.Visible = false;
					_serverSettingsControl.Visible = true;
					_targetFolderControl.UpdateDisplay();
					_cancelButton.Enabled = true;
					_cancelButton.Visible = true;
					_cancelTaskButton.Visible = false;
					_statusProgress.Visible = false;
					_statusProgress.Text = "";
					_serverSettingsControl.DisplayUpdated += ServerSettingsControlOnDisplayUpdated;

					break;
				case State.MakingClone:
					_serverSettingsControl.DisplayUpdated -= ServerSettingsControlOnDisplayUpdated;
					_progressBar.Focus();
					_progressBar.Select();
					_statusImage.Visible = false;
					_progressBar.Visible = true;
					_progressBar.Style = ProgressBarStyle.Marquee;
					_progressBar.MarqueeAnimationSpeed = Platform.IsMono ? 3000 : 50;

					_statusLabel.Visible = true;
					_statusLabel.Text = LocalizationManager.GetString("Messages.GettingProject", "Getting project...");
					_statusLabel.Left = _progressBar.Left;
					_statusProgress.Left = _progressBar.Left;
					_logBox.Visible = true;
					_cancelTaskButton.Visible = true;
					_cancelButton.Visible = false;
					_statusProgress.Visible = true;
					break;
				case State.Success:
					_serverSettingsControl.DisplayUpdated -= ServerSettingsControlOnDisplayUpdated;
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = LocalizationManager.GetString("Messages.Done", "Done.");
					_progressBar.Visible = false;
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.Visible = true;
					_statusImage.ImageKey=LocalizationManager.GetString("Messages.Success", "Success");
					_statusLabel.Text = string.Format(LocalizationManager.GetString("Messages.Finished", "Finished"));
					_okButton.Visible = true;
					_cancelButton.Visible = false;
					_logBox.Visible = true;
					_statusProgress.Visible = false;
					break;
				case State.Error:
					_serverSettingsControl.DisplayUpdated -= ServerSettingsControlOnDisplayUpdated;
					_fixSettingsButton.Visible = true;
					_fixSettingsButton.Focus();
					_cancelButton.Visible = true;
					if (!Platform.IsMono)
					{
						_cancelButton.Text = LocalizationManager.GetString("Common.Cancel", "&Cancel");
					}
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = LocalizationManager.GetString("Messages.Failed", "Failed.");
					_progressBar.Visible = false;
					_statusLabel.Left = _statusImage.Right + 10;
					_statusImage.ImageKey = LocalizationManager.GetString("Common.Error", "Error");
					_statusImage.Visible = true;
					_statusProgress.Visible = false;
					break;
				case State.Cancelled:
					_serverSettingsControl.DisplayUpdated -= ServerSettingsControlOnDisplayUpdated;
					_cancelButton.Visible = true;
					_cancelButton.Text = LocalizationManager.GetString("Common.Close", "&Close");
					_cancelButton.Select();
					_cancelTaskButton.Visible = false;
					_statusLabel.Visible = true;
					_statusLabel.Text = LocalizationManager.GetString("Messages.Cancelled", "Cancelled.");
					_progressBar.Visible = false;
					_statusLabel.Left =  _progressBar.Left;
					_statusImage.Visible = false;
					_statusProgress.Visible = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

		}

		private void ServerSettingsControlOnDisplayUpdated(object sender, EventArgs eventArgs)
		{
			_targetFolderControl.UpdateDisplay();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			UpdateDisplay(State.AskingUserForURL);
			_logBox.BackColor = this.BackColor;
			//_targetFolderControl.UpdateDisplay();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   DialogResult = DialogResult.OK;
			Close();
		}


		/// <summary>
		/// After a successful clone, this will have the path to the folder that we just copied to the computer
		/// </summary>
		public string PathToNewlyClonedFolder
		{
			get { return _model.TargetDestination; }
		}

		/// <summary>
		/// **** Currently this is not implemented on this class
		/// </summary>
		public void SetFilePatternWhichMustBeFoundInHgDataFolder(string pattern)
		{
			//TODO
			//no don't do throw. doing it means client need special code for each clone method
			//  throw new NotImplementedException();

		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
				Close();
		}

		private void OnDownloadClick(object sender, EventArgs e)
		{
			StartClone();
		}

		
		/// <summary>
		/// Starts a clone operation with the supplied information.
		/// <param name="username">Username for Mercurial authentication</param>
		/// <param name="password">Password for Mercurial authentication</param>
		/// <param name="host">Host name (with scheme), e.g. https://www.google.com</param>
		/// <param name="projectName">Name of the project to clone (will be combined with the host Uri)</param>
		/// </summary>
		public void StartClone(string username, string password, Uri host, string projectName)
		{
			_model.Username = username;
			_model.Password = password;
			_model.CustomUrl = new Uri(host, projectName).ToString();
			_model.IsCustomUrl = true;
			_model.LocalFolderName = projectName;

			StartClone();
		}

		private void StartClone()
		{
			lock (this)
			{
				_logBox.Clear();
				if (_backgroundWorker.IsBusy)
					return;
				UpdateDisplay(State.MakingClone);
				_model.SaveUserSettings();
				ThreadSafeUrl = _model.URL;
				//_backgroundWorker.RunWorkerAsync(new object[] { ThreadSafeUrl, PathToNewProject, _progress });
				_backgroundWorker.RunWorkerAsync(new object[0]);
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
					_model.CancelRequested = true; //but it will be monitoring this
				}
			}
		}

		private void _logBox_Load(object sender, EventArgs e)
		{
		}

		private void _fixSettingsButton_Click(object sender, EventArgs e)
		{
			_model.Click_FixSettingsButton();
			UpdateDisplay(State.AskingUserForURL);
		}

		private void _helpButton_Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, HelpUtils.GetHelpFile(), @"/Tasks/Use_Get_Project_from_Internet_dialog_box.htm");
		}

		private void GetCloneFromInternetDialog_BackColorChanged(object sender, EventArgs e)
		{
			_logBox.BackColor  =this.BackColor;
		}

		private void GetCloneFromInternetDialog_ResizeBegin(object sender, EventArgs e)
		{
			_resizing = true;
		}

		private void GetCloneFromInternetDialog_ResizeEnd(object sender, EventArgs e)
		{
			_resizing = false;
		}

		private void GetCloneFromInternetDialog_Paint(object sender, PaintEventArgs e)
		{
			if (_resizing)
			{
				_targetFolderControl._downloadButton.Top = _cancelButton.Top - _targetFolderControl.Top;
				_targetFolderControl._downloadButton.Left = _cancelButton.Left - 100;
			}
		}
	}
}
