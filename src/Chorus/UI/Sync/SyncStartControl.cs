using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Chorus.UI.Misc;
using Chorus.Utilities;
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
		private SyncStartModel _model;
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


		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			UpdateUsbDriveSituation();
			UpdateInternetSituation();
			UpdateLocalNetworkSituation();
		}

		private void UpdateLocalNetworkSituation()
		{
			//TODO: move this to model, as we did with UpdateInternetSituation()

			RepositoryAddress address;

			try
			{
				address = _repository.GetDefaultNetworkAddress<DirectoryRepositorySource>();
			}
			catch(Exception error)//probably, hgrc is locked
			{
				_useSharedFolderButton.Enabled = false;
				_useSharedFolderStatusLabel.Text = error.Message;
			   return;
			}

			_useSharedFolderButton.Enabled = true; // can't do if address == null, but we will launch the setup dialog.
			_useSharedFolderStatusLabel.LinkArea = new LinkArea(0,0);
			if (address == null)
			{
				_useSharedFolderStatusLabel.Text = "This project is not yet associated with a shared folder";
			}
			else
			{
				_useSharedFolderStatusLabel.Text = address.Name;
				toolTip1.SetToolTip(_useSharedFolderButton, address.URI);
			}
			if (!_useSharedFolderButton.Enabled || Control.ModifierKeys == Keys.Shift)
			{
				 _useSharedFolderStatusLabel.LinkArea = new LinkArea(_useSharedFolderStatusLabel.Text.Length + 1, 1000);
				 _useSharedFolderStatusLabel.Text += " Set Up";
			}

			if (_useSharedFolderButton.Enabled)
			{
				toolTip1.SetToolTip(_useSharedFolderButton, "Press Shift to see Set Up button");
			}
		}

		private void UpdateInternetSituation()
		{
			string message,  tooltip, buttonLabel;
			_model.GetInternetStatusLink(out buttonLabel, out message, out tooltip);
			_useInternetButton.Enabled = true;
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
				_internetStatusLabel.LinkArea = new LinkArea(message.Length + 1, 1000);
			}
		}

		private void UpdateUsbDriveSituation()
		{
			//TODO: move this to model, as we did with UpdateInternetSituation()

			if (usbDriveLocator.UsbDrives.Count() == 0)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "First insert a USB flash drive";
			}
			else if (usbDriveLocator.UsbDrives.Count() > 1)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "More than one USB drive detected. Please remove one.";
			}
			else
			{
				_useUSBButton.Enabled = true;
				try
				{
					var first = usbDriveLocator.UsbDrives.First();
#if !MONO
					_usbStatusLabel.Text = first.RootDirectory + " " + first.VolumeLabel + " (" +
										   Math.Floor(first.TotalFreeSpace/1024000.0) + " Megs Free Space)";
#else
				_usbStatusLabel.Text = first.VolumeLabel;
					//RootDir & volume label are the same on linux.  TotalFreeSpace is, like, maxint or something in mono 2.0
#endif
				}
				catch (Exception error)
				{
					_usbStatusLabel.Text = error.Message;
				}
			}
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
				"Sharing repositories over a local network may sometimes cause a repository to become corrupted. This can be repaired by copying one of the good copies of the repository, but it may require expert help. If you have a good internet connection or a small enough group to pass a USB key around, we recommend one of the other Send/Receive options.",
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
				_model.SetNewSharedNetworkAddress(dlg.SelectedPath);
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
