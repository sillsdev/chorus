using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Chorus.Properties;
using Chorus.UI.Misc;
using Chorus.UI.Settings;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using ChorusHub;
using Palaso.Code;
using System.IO;

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
		private LANMode lanMode = LANMode.ChorusHub;
		private ChorusHubInfo _chorusHubInfo;

		private const int STATECHECKINTERVAL = 2000; // 2 sec interval between checks of USB status.
		private const int INITIALINTERVAL = 1000; // only wait 1 sec, the first time

		private delegate void UpdateInternetUICallback(bool enabled, string btnLabel, string message, string tooltip, string diagnostics);

		private delegate void UpdateNetworkUICallback(bool enabled, string message, string tooltip, string diagnostics, LANMode mode);

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
		}

		private DialogResult DisplaySRSettingsDlg()
		{
			var settingsDlg = new SendReceiveSettings(_repository.PathToRepo);
			var result = settingsDlg.ShowDialog();
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
			var folderState = Properties.Settings.Default.SharedFolderEnabled;
			_useSharedFolderStatusLabel.Visible = _useSharedFolderButton.Visible = folderState;
			statusRow = _tableLayoutPanel.GetRow(_useSharedFolderStatusLabel);
			buttonRow = _tableLayoutPanel.GetRow(_useSharedFolderButton);
			_tableLayoutPanel.RowStyles[statusRow].Height = folderState ? LABEL_HEIGHT : 0;
			_tableLayoutPanel.RowStyles[buttonRow].Height = folderState ? BUTTON_HEIGHT : 0;
		}

		private void SetupSharedFolderAndInternetUI()
		{
			const string checkingConnection = "Checking connection...";
			_useSharedFolderStatusLabel.Text = checkingConnection;
			_useSharedFolderButton.Enabled = false;

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
			get { return (!_useSharedFolderButton.Enabled || Control.ModifierKeys == Keys.Shift); }
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
			bool isReady=false;
			LANMode mode=LANMode.ChorusHub;

			var finder = new ChorusHub.Finder();
			_chorusHubInfo = finder.Find();
			if (_chorusHubInfo != null)
			{
				isReady = true;
				message = string.Format("Found Chorus Hub at {0}", _chorusHubInfo.HostName);
				tooltip = _chorusHubInfo.GetUri(Path.GetFileName(Path.GetDirectoryName(_repository.PathToRepo)));
				diagnostics = "";
			}
			else
			{
				Monitor.Enter(_model);
				 isReady = _model.GetNetworkStatusLink(out message, out tooltip, out diagnostics);
				if(isReady)
					mode = LANMode.Folder;
				Monitor.Exit(_model);
			}

			Monitor.Enter(this);
			// Using a callback and Invoke ensures that we avoid cross-threading updates.
			if (!_exiting)
			{
				var callback = new UpdateNetworkUICallback(UpdateNetworkUI);
				this.Invoke(callback, new object[] { isReady, message, tooltip, diagnostics, mode });
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
		private void UpdateNetworkUI(bool enabled, string message, string tooltip, string diagnostics, LANMode mode)
		{
			_useSharedFolderButton.Text = mode == LANMode.ChorusHub ? "Chorus Hub" : "Shared Network Folder";
			_useSharedFolderButton.Image = mode == LANMode.ChorusHub ? Resources.chorusHubMedium : Resources.networkFolder29x32;
			_useSharedFolderButton.Enabled = enabled;
			if (!string.IsNullOrEmpty(diagnostics))
				SetupNetworkDiagnosticLink(diagnostics);
			else
				_sharedNetworkDiagnosticsLink.Visible = false;

			_useSharedFolderStatusLabel.Text = message;
			_useSharedFolderStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			toolTip1.SetToolTip(_useSharedFolderButton, tooltip);
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
				var address = _repository.GetDefaultNetworkAddress<HttpRepositoryPath>();
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void _useSharedFolderButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				RepositoryAddress address;

				if (lanMode == LANMode.Folder)
				{
					address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				}
				else
				{
					address = new HttpRepositoryPath(_chorusHubInfo.HostName, _chorusHubInfo.GetUri(Path.GetFileName(Path.GetDirectoryName(_repository.PathToRepo))), false);
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
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
				"Internet", (string)_internetDiagnosticsLink.Tag);
		}

		private void _sharedNetworkDiagnosticsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Palaso.Reporting.ErrorReport.NotifyUserOfProblem(_connectionDiagnostics,
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
