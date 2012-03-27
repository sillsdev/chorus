using System;
using System.Windows.Forms;
using Chorus.Properties;
using Chorus.UI.Misc;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;
using Palaso.Code;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Sync
{
	internal partial class SyncStartControl : UserControl
	{
		private HgRepository _repository;
		private ISyncStartModel _model;
		public event EventHandler<SyncStartArgs> RepositoryChosen;

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
			_internetStatusLabel.Text = string.Empty;
			Guard.AgainstNull(repository, "repository");
			_model = new SyncStartModel(repository);
			_repository = repository;
			_updateDisplayTimer.Enabled = true;
			_userName.Text = repository.GetUserIdInUse();
			UpdateDisplay();//don't wait 2 seconds
		}

		public void InitAlternateModel(HgRepository repository)
		{
			_internetStatusLabel.Text = Resources.ksCheckingConnection;
			_useSharedFolderStatusLabel.Text = Resources.ksCheckingConnection;
			_useInternetButton.Enabled = false;
			_useSharedFolderButton.Enabled = false;
			Guard.AgainstNull(repository, "repository");
			_model = new SyncStartAlternateModel(repository);
			_repository = repository;
			_updateDisplayTimer.Enabled = true;
			_userName.Text = repository.GetUserIdInUse();
			//UpdateDisplay(); // let the dialog display itself first, then check for connection
			_updateDisplayTimer.Interval = 500; // But check sooner than 2 seconds anyway!
		}

		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateDisplay();
			_updateDisplayTimer.Interval = 2000; // more normal checking
		}

		private void UpdateDisplay()
		{
			UpdateUsbDriveSituation();
			UpdateInternetSituation();
			UpdateLocalNetworkSituation();
		}

		private void UpdateLocalNetworkSituation()
		{
			string message, tooltip;
			_model.GetNetworkStatusLink(out message, out tooltip);
			_useSharedFolderButton.Enabled = message != Resources.ksSharedFolderInaccessible;
			_useSharedFolderStatusLabel.Text = message;
			_useSharedFolderStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			if (_useSharedFolderButton.Enabled)
			{
				tooltip += System.Environment.NewLine + "Press Shift to see Set Up button";
			}
			toolTip1.SetToolTip(_useSharedFolderButton, tooltip);

			if (!_useSharedFolderButton.Enabled || Control.ModifierKeys == Keys.Shift)
			{
				 _useSharedFolderStatusLabel.Text += " Set Up";
			}
		}

		private void UpdateInternetSituation()
		{
			string message,  tooltip, buttonLabel;
			_model.GetInternetStatusLink(out buttonLabel, out message, out tooltip);
			_useInternetButton.Enabled = message != Resources.ksNoInternetAccess;
			_useInternetButton.Text = buttonLabel;
			_internetStatusLabel.Text = message;
			_internetStatusLabel.LinkArea = new LinkArea(message.Length+1, 1000);
			if(_useInternetButton.Enabled )
			{
				tooltip += System.Environment.NewLine+"Press Shift to see Set Up button";
			}
			toolTip1.SetToolTip(_useInternetButton, tooltip);

			if (!_useInternetButton.Enabled || Control.ModifierKeys == Keys.Shift)
			{
				_internetStatusLabel.Text += " Set Up";
				// hasn't this just been done above?
				//_internetStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			}
		}

		private void UpdateUsbDriveSituation()
		{
			// usbDriveLocator is defined in the Designer?
			string message;
			_useUSBButton.Enabled = _model.GetUsbStatusLink(usbDriveLocator, out message);
			_usbStatusLabel.Text = message;
		}




		private void _useUSBButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				UpdateName();
				var address = RepositoryAddress.Create(RepositoryAddress.HardWiredSources.UsbKey, "USB flash drive", false);
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void UpdateName()
		{
			if (_repository.GetUserIdInUse() != _userName.Text.Trim() && _userName.Text.Trim().Length>0)
			{
				_repository.SetUserNameInIni(_userName.Text.Trim(), new NullProgress());
			}
		}

		private void _useInternetButton_Click(object sender, EventArgs e)
		{
			string message, tooltip, buttonLabel;
			if(!_model.GetInternetStatusLink(out buttonLabel, out message, out tooltip))
			{
				_internetStatusLabel_LinkClicked(null, null);
				if (!_model.GetInternetStatusLink(out buttonLabel, out message, out tooltip))
					return; // still no good.
			}
			if (RepositoryChosen != null)
			{
				UpdateName();
				RepositoryChosen.Invoke(this, new SyncStartArgs(_repository.GetDefaultNetworkAddress<HttpRepositoryPath>(), _commitMessageText.Text));
			}

		}

		private bool IsSharedFolderAddressAvailable()
		{
			try
			{
				var address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				return address != null;
			}
			catch (Exception error)//probably, hgrc is locked
			{
				return false;
			}
		}

		private void _useSharedFolderButton_Click(object sender, EventArgs e)
		{
			// Instead of disabling the button when we have no address, launch the dialog for choosing one.
			if (!IsSharedFolderAddressAvailable())
			{
				_sharedFolderStatusLabel_LinkClicked(null, null);
				if (!IsSharedFolderAddressAvailable())
					return; // if the user canceled or otherwise didn't set it up, don't try to S/R.
			}
			if (RepositoryChosen != null)
			{
				UpdateName();
				var address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void SyncStartControl_Load(object sender, EventArgs e)
		{

		}

		private void _internetStatusLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using(var dlg = new ServerSettingsDialog(_repository.PathToRepo))
			{
				dlg.ShowDialog();
			}
			UpdateInternetSituation();
		}

		private void _sharedFolderStatusLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(DialogResult.Cancel ==
				MessageBox.Show(
				"Note, due to some limitations in the underlying system (Mercurial), connecting to a shared folder hosted by a Windows computer is not recommended. If the server is Linux, it's OK.",
				"Warning", MessageBoxButtons.OKCancel))
			{
				return;
			}
			using (var dlg =  new System.Windows.Forms.FolderBrowserDialog())
			{
				dlg.ShowNewFolderButton = true;
				dlg.Description = "Choose the folder containing the project with which you want to synchronize.";
				if (DialogResult.OK != dlg.ShowDialog())
					return;
				_model.SetNewSharedNetworkAddress(_repository, dlg.SelectedPath);
			}

			UpdateLocalNetworkSituation();
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
