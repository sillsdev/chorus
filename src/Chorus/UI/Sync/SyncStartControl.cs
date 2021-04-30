using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.ChorusHub;
using Chorus.Properties;
using Chorus.UI.Misc;
using Chorus.UI.Settings;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using SIL.Code;
using System.IO;
using L10NSharp;

namespace Chorus.UI.Sync
{
	internal partial class SyncStartControl : UserControl
	{
		private const float LABEL_HEIGHT = 40F;//NB: this needs to not only hold the button, but leave white space before the next control cluster
		private const float BUTTON_HEIGHT = 45F;

		private HgRepository _repository;
		private SyncStartModel _model;
		public event EventHandler<SyncStartArgs> RepositoryChosen;
		private const string _connectionDiagnostics = "There was a problem connecting to the {0}.\r\n{1}Connection attempt failed.";

		private Thread _updateInternetSituation; // Thread that runs the Internet status checking worker.
		private ConnectivityStateWorker _internetStateWorker;
		private bool _internetWorkerStarted = false; // Has worker been started?

		private Thread _updateNetworkSituation; // Thread that runs the Network Folder status checking worker.
		private ConnectivityStateWorker _networkStateWorker;
		private bool _networkWorkerStarted = false; // Has worker been started?

		private bool _exiting; // Dialog is in the process of exiting, stop the threads!
		private LANMode _lanMode = LANMode.ChorusHub;
		private ChorusHubServerInfo _chorusHubServerInfo;
		private ChorusHubClient _chorusHubClient;

		private const int WAIT_CREATE_CH_REPO = 10000; // 10-sec wait for Chorus Hub to create a new repository
		private const int STATECHECKINTERVAL = 2000; // 2 sec interval between checks of USB status.
		private const int INITIALINTERVAL = 1000; // wait only 1 sec the first time

		private delegate void UpdateInternetUICallback(bool enabled, string btnLabel, string message, string tooltip, string diagnostics);

		private delegate void UpdateNetworkUICallback(bool enabled, string message, string tooltip, string diagnostics);

		//designer only
		public SyncStartControl()
		{
			InitializeComponent();
		}

		public SyncStartControl(HgRepository repository)
		{
			InitializeComponent();
			Init(repository);
		}

		public void Init(HgRepository repository)
		{
			Guard.AgainstNull(repository, "repository");
			SetupSharedFolderAndInternetUI();

			_model = new SyncStartModel(repository);
			_repository = repository;

			SetButtonStatesFromSettings();

			// Setup Internet State Checking thread and the worker that it will run
			_internetStateWorker = new ConnectivityStateWorker(CheckInternetStatusAndUpdateUI);
			_updateInternetSituation = new Thread(_internetStateWorker.DoWork);

			// Setup Shared Network Folder Checking thread and its worker
			_networkStateWorker = new ConnectivityStateWorker(CheckNetworkStatusAndUpdateUI);
			_updateNetworkSituation = new Thread(_networkStateWorker.DoWork);

			// let the dialog display itself first, then check for connection
			_updateDisplayTimer.Interval = INITIALINTERVAL; // But check sooner than 2 seconds anyway!
			_updateDisplayTimer.Enabled = true;

			_settingsButton.LaunchSettingsCallback = DisplaySRSettingsDlg;

			if(!Properties.Settings.Default.ShowChorusHubInSendReceive)
			{
				_useLocalNetworkButton.Image = Resources.networkFolder29x32;
				_useLocalNetworkButton.Text = "Shared Network Folder";
			}

		}

		private DialogResult DisplaySRSettingsDlg()
		{
			DialogResult result;
			using (var settingsDlg = new SendReceiveSettings(_repository.PathToRepo))
			{
				result = settingsDlg.ShowDialog();
			}
			if(result == DialogResult.OK)
			{
				SetButtonStatesFromSettings();
				Parent.ClientSize = new Size(Width, DesiredHeight + 10);
				Parent.ResumeLayout(true);
				RecheckNetworkStatus();
				RecheckInternetStatus();
			}
			return result;
		}

		/// <summary>
		/// Retrieves the settings for the various S/R buttons and displays or hides them accordingly.
		/// </summary>
		private void SetButtonStatesFromSettings()
		{
			_internetDiagnosticsLink.Visible = false;
			var internetState = Properties.Settings.Default.InternetEnabled;
			_internetStatusLabel.Visible = _useInternetButton.Visible = internetState;
			var statusRow = _tableLayoutPanel.GetRow(_internetStatusLabel);
			var buttonRow = _tableLayoutPanel.GetRow(_useInternetButton);
			_tableLayoutPanel.RowStyles[statusRow].Height = internetState ? LABEL_HEIGHT : 0;
			_tableLayoutPanel.RowStyles[buttonRow].Height = internetState ? BUTTON_HEIGHT : 0;

			_sharedNetworkDiagnosticsLink.Visible = false;

			var showChorusHubButton = Properties.Settings.Default.ShowChorusHubInSendReceive;

			_useSharedFolderStatusLabel.Visible = _useLocalNetworkButton.Visible = showChorusHubButton;
			statusRow = _tableLayoutPanel.GetRow(_useSharedFolderStatusLabel);
			buttonRow = _tableLayoutPanel.GetRow(_useLocalNetworkButton);
			_tableLayoutPanel.RowStyles[statusRow].Height = showChorusHubButton ? LABEL_HEIGHT : 0;
			_tableLayoutPanel.RowStyles[buttonRow].Height = showChorusHubButton ? BUTTON_HEIGHT : 0;
		}

		private void SetupSharedFolderAndInternetUI()
		{
			const string checkingConnection = "Checking connection...";
			_useSharedFolderStatusLabel.Text = checkingConnection;
			_useLocalNetworkButton.Enabled = false;

			_internetStatusLabel.Text = checkingConnection;
			_useInternetButton.Enabled = false;
		}

		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			_updateDisplayTimer.Interval = STATECHECKINTERVAL; // more normal checking rate from here on out
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			UpdateUsbDriveSituation();
			UpdateInternetSituation();
			UpdateLocalNetworkSituation();
		}

		#region Network Status methods

		public bool ShouldShowNetworkSetUpButton
		{
			get { return (!_useLocalNetworkButton.Enabled || Control.ModifierKeys == Keys.Shift); }
		}

		private void UpdateLocalNetworkSituation()
		{
			if (!_networkWorkerStarted)
			{
				_networkWorkerStarted = true;
				_updateNetworkSituation.Start();
			}
		}

		private enum LANMode
		{
			Folder,
			ChorusHub
		};


		/// <summary>
		/// Called by our worker thread to avoid inordinate pauses in the UI while checking the
		/// Shared Network Folder to determine its status.
		/// </summary>
		private void CheckNetworkStatusAndUpdateUI()
		{
			// Check network Shared Folder status
			string message, tooltip, diagnostics;
			message = tooltip = diagnostics = "";
			bool isReady=false;
			_lanMode = LANMode.ChorusHub;

			if (Properties.Settings.Default.ShowChorusHubInSendReceive)
			{
				try
				{
					if (_chorusHubClient == null)
					{
						_chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
						if (_chorusHubServerInfo != null)
							_chorusHubClient = new ChorusHubClient(_chorusHubServerInfo);
					}
				}
				catch (Exception)
				{
					//not worth complaining about
#if DEBUG
					throw;
#endif
				}
			}
			if(_chorusHubServerInfo==null)
			{
				message = LocalizationManager.GetString("GetChorusHubStatus.NoChorusHubFound", "No Chorus Hub found on local network.");
			}
			else if (!_chorusHubServerInfo.ServerIsCompatibleWithThisClient)
			{
				message = string.Format(LocalizationManager.GetString("GetChorusHubStatus.FoundButNotCompatible",
					"Found Chorus Hub, but it is not compatible with this version of {0}."), Application.ProductName);
			}
			else
			{
				isReady = true;
				message = string.Format(LocalizationManager.GetString("GetChorusHubStatus.FoundChorusHubAt", "Found Chorus Hub at {0}"),
					_chorusHubServerInfo.HostName);
				tooltip = _chorusHubServerInfo.GetHgHttpUri(Path.GetFileName(_repository.PathToRepo));
			}

			Monitor.Enter(this);
			// Using a callback and Invoke ensures that we avoid cross-threading updates.
			if (!_exiting)
			{
				var callback = new UpdateNetworkUICallback(UpdateNetworkUI);
				Invoke(callback, new object[] { isReady, message, tooltip, diagnostics });
			}
			Monitor.Exit(this);
		}

		/// <summary>
		/// Callback method to ensure that Controls are painted on the main thread and not the worker thread.
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="message"></param>
		/// <param name="tooltip"></param>
		/// <param name="diagnostics"></param>
		private void UpdateNetworkUI(bool enabled, string message, string tooltip, string diagnostics)
		{
			_useLocalNetworkButton.Text = _lanMode == LANMode.ChorusHub ? "Chorus Hub" : "Shared Network Folder";
			_useLocalNetworkButton.Image = _lanMode == LANMode.ChorusHub ? Resources.chorusHubMedium : Resources.networkFolder29x32;
			_useLocalNetworkButton.Enabled = enabled;
			if (!string.IsNullOrEmpty(diagnostics))
				SetupNetworkDiagnosticLink(diagnostics);
			else
				_sharedNetworkDiagnosticsLink.Visible = false;

			_useSharedFolderStatusLabel.Text = message;
			_useSharedFolderStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			toolTip1.SetToolTip(_useLocalNetworkButton, tooltip);
		}

		private void SetupNetworkDiagnosticLink(string diagnosticText)
		{
			_sharedNetworkDiagnosticsLink.Tag = diagnosticText;
			_sharedNetworkDiagnosticsLink.Enabled = _sharedNetworkDiagnosticsLink.Visible = true;
		}

		#endregion // Network

		#region Internet Status methods

		public bool ShouldShowInternetSetUpButton
		{
			get { return (!_useInternetButton.Enabled || Control.ModifierKeys == Keys.Shift); }
		}

		public int DesiredHeight
		{
			get {
				float height = 0;
				for (int row = 0; row < _tableLayoutPanel.RowCount; ++row)
				{
					height += _tableLayoutPanel.RowStyles[row].Height;
				}
				return (int) height+20;
			}
		}

		/// <summary>
		/// Pings to test Internet connectivity were causing several second pauses in the dialog.
		/// Now the Internet situation is determined in a separate worker thread which reports
		/// back to the main one.
		/// </summary>
		private void UpdateInternetSituation()
		{
			if (!_internetWorkerStarted)
			{
				_internetWorkerStarted = true;
				_updateInternetSituation.Start();
			}
		}

		/// <summary>
		/// Called by our worker thread to avoid inordinate pauses in the UI while the Internet
		/// is pinged to determine its status.
		/// </summary>
		private void CheckInternetStatusAndUpdateUI()
		{
			// Check Internet status
			string buttonLabel, message, tooltip, diagnostics;
			Monitor.Enter(_model);
			bool result = _model.GetInternetStatusLink(out buttonLabel, out message, out tooltip,
													   out diagnostics);
			Monitor.Exit(_model);

			// Using a callback and Invoke ensures that we avoid cross-threading updates.
			var callback = new UpdateInternetUICallback(UpdateInternetUI);
			Monitor.Enter(this);
			if(!_exiting)
				this.Invoke(callback, new object[] { result, buttonLabel, message, tooltip, diagnostics });
			Monitor.Exit(this);
		}

		/// <summary>
		/// Callback method to ensure that Controls are painted on the main thread and not the worker thread.
		/// </summary>
		private void UpdateInternetUI(bool enabled, string btnLabel, string message, string tooltip, string diagnostics)
		{
			_useInternetButton.Enabled = enabled;
			if (!string.IsNullOrEmpty(diagnostics))
				SetupInternetDiagnosticLink(diagnostics);
			else
				_internetDiagnosticsLink.Visible = false;

			// message is empty if there is a connection, otherwise indicates the problem.
			if (string.IsNullOrEmpty(message))
			{
				// btnLabel is the web address for the repository
				_internetStatusLabel.Text = btnLabel;
				_internetStatusLabel.LinkArea = new LinkArea(0, 0);
			}
			else
			{
				_internetStatusLabel.Text = message;
				_internetStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			}
			toolTip1.SetToolTip(_useInternetButton, tooltip);
		}

		private void SetupInternetDiagnosticLink(string diagnosticText)
		{
			_internetDiagnosticsLink.Tag = diagnosticText;
			_internetDiagnosticsLink.Enabled = _internetDiagnosticsLink.Visible = true;
		}

		#endregion  // Internet

		private void UpdateUsbDriveSituation()
		{
			// usbDriveLocator is defined in the Designer
			string message;
			_useUSBButton.Enabled = _model.GetUsbStatusLink(usbDriveLocator, out message);
			_usbStatusLabel.Text = message;
		}

		private void _useUSBButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				var address = RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "USB flash drive", false);
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void _useInternetButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				_repository.RemoveCredentialsFromIniIfNecessary();
				var address = _repository.GetDefaultNetworkAddress<HttpRepositoryPath>();
				// ENHANCE (Hasso) 2021.04: prompt here for the password if the user has opted not to save it (https://jira.sil.org/browse/LT-20549)
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		/// <summary>
		/// Handles a click event on the Chorus Hub button
		/// (Shared folder mode is used for only testing purposes)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _useLocalNetworkButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				RepositoryAddress address;

				if (_lanMode == LANMode.Folder) // used for only testing
				{
					address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				}
				else // if (_lanMode == LANMode.ChorusHub)
				{
					Cursor.Current = Cursors.WaitCursor;
					string directoryName = Path.GetFileName(_repository.PathToRepo);
					var doWait  = _chorusHubClient.PrepareHubToSync(directoryName, _repository.Identifier);
					if(doWait)
					{
						// TODO: show indeterminate progress bar for this wait
						Thread.Sleep(WAIT_CREATE_CH_REPO);
					}
					address = new ChorusHubRepositorySource(_chorusHubServerInfo.HostName,
						_chorusHubServerInfo.GetHgHttpUri(RepositoryAddress.ProjectNameVariable), false,
						_chorusHubClient.GetRepositoryInformation(null));
					Cursor.Current = Cursors.Default;
				}
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void _internetStatusLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DialogResult dlgResult;
			using(var dlg = new ServerSettingsDialog(_repository.PathToRepo))
			{
				dlgResult = dlg.ShowDialog();
			}
			if (dlgResult == DialogResult.OK)
				RecheckInternetStatus();
		}

		private void RecheckInternetStatus()
		{
			_internetWorkerStarted = false;
			// Setup Internet State Checking thread and the worker that it will run
			_internetStateWorker = new ConnectivityStateWorker(CheckInternetStatusAndUpdateUI);
			_updateInternetSituation = new Thread(_internetStateWorker.DoWork);
		}

		private void RecheckNetworkStatus()
		{
			_networkWorkerStarted = false;
			// Setup Shared Network Folder Checking thread and its worker
			_networkStateWorker = new ConnectivityStateWorker(CheckNetworkStatusAndUpdateUI);
			_updateNetworkSituation = new Thread(_networkStateWorker.DoWork);
		}

		private void _internetDiagnosticsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SIL.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
				"Internet", (string)_internetDiagnosticsLink.Tag);
		}

		private void _sharedNetworkDiagnosticsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SIL.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
				"Shared Network Folder", (string)_sharedNetworkDiagnosticsLink.Tag);
		}

		/// <summary>
		/// Class to run a separate worker thread to check connectivity status.
		/// </summary>
		internal class ConnectivityStateWorker
		{
			private Action _action;

			internal ConnectivityStateWorker(Action action)
			{
				_action = action;
			}

			internal void DoWork()
			{
				_action();
			}
		}
	}

	public class SyncStartArgs : EventArgs
	{
		public SyncStartArgs(RepositoryAddress address, string commitMessage)
		{
			Address = address;
			CommitMessage = commitMessage;
		}
		public RepositoryAddress Address;
		public string CommitMessage;
	}
}
