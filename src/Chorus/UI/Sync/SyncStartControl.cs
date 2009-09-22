using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.UI.Sync
{
	internal partial class SyncStartControl : UserControl
	{
		private HgRepository _repository;
		public event EventHandler<SyncStartArgs> RepositoryChosen;

		//designer only
		public SyncStartControl()
		{
			InitializeComponent();
		}


		public HgRepository Repository
		{
			get { return _repository; }
			set
			{
				_repository = value;
				if (_repository != null)
				{
					_updateDisplayTimer.Enabled = true;
				}
			}
		}


		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			var drives = GetUsbDriveInfo();

			if (drives.Count()==0)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "First insert a USB flash drive";
			}
			else if (drives.Count() > 1)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "More than one USB flash drive detected. Please remove one.";
			}
			else
			{
				_useUSBButton.Enabled = true;
				try
				{
					var first = drives.First();
#if !MONO
					_usbStatusLabel.Text = first.RootDirectory + " " + first.VolumeLabel + " (" + Math.Floor(first.TotalFreeSpace/1024000.0)+" Megs Free Space)";
#else
					_usbStatusLabel.Text = first.VolumeLabel;//RootDir & volume label are the same on linux.  TotalFreeSpace is, like, maxint or something in mono 2.0
#endif
				}
				catch(Exception error   )
				{
					_usbStatusLabel.Text = error.Message;
				}
			}
			var address = GetDefaultNetworkAddress<HttpRepositoryPath>();
			_useInternetButton.Enabled = address != null;
			if (address==null)
			{
				_internetStatusLabel.Text = "This project is not yet associated with an internet server";
			}
			else
			{
				_internetStatusLabel.Text = address.Name;
				toolTip1.SetToolTip(_useInternetButton, address.URI);
				//enhance: which one will be used if I click this?
			}

			address = GetDefaultNetworkAddress<DirectoryRepositorySource>();
			_useSharedFolderButton.Enabled = address != null;
			if (address == null)
			{
				_sharedFolderLabel.Text = "This project is not yet associated with a shared folder";
			}
			else
			{
				_sharedFolderLabel.Text = address.Name ;
				toolTip1.SetToolTip(_useSharedFolderButton, address.URI);
			}
		}

		private IEnumerable<DriveInfo> GetUsbDriveInfo()
		{
			var info = new Chorus.Utilities.UsbDrive.RetrieveUsbDriveInfo();
			var drives = info.GetDrives();
			if (drives.Count > 0)
			{
				foreach (var drive in System.IO.DriveInfo.GetDrives())
				{
					if (drive.RootDirectory.FullName == drives[0].RootDirectory.FullName)
					{
						yield return drive;
					}
				}
			}
			//MessageBox.Show("There was a problem getting USB drive information.","Error", MessageBoxButtons.OK,MessageBoxIcon.Warning);
		}

		private RepositoryAddress GetDefaultNetworkAddress<T>()
		{
			var paths = Repository.GetRepositoryPathsInHgrc();
			var networkPaths = paths.Where(p => p is T);

			//none found in the hgrc
			if(networkPaths.Count()==0)
				return null;

			//the first one found in the default list
			var defaultAliases = Repository.GetDefaultSyncAliases();
			foreach (var path in networkPaths)
			{
				if(defaultAliases.Any(a=> a==path.Name))
					return path;
			}

			//the first one
			return networkPaths.First();
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
				RepositoryChosen.Invoke(this, new SyncStartArgs(GetDefaultNetworkAddress<HttpRepositoryPath>(), _commitMessageText.Text));
			}

		}

		private void _useSharedFolderButton_Click(object sender, EventArgs e)
		{
			if (RepositoryChosen != null)
			{
				var address = GetDefaultNetworkAddress<DirectoryRepositorySource>();
				RepositoryChosen.Invoke(this, new SyncStartArgs(address, _commitMessageText.Text));
			}
		}

		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void SyncStartControl_Load(object sender, EventArgs e)
		{
		}
	}
	public class SyncStartArgs : EventArgs
	{
		public SyncStartArgs(RepositoryAddress address, string comittMessage)
		{
			Address = address;
			ComittMessage = comittMessage;
		}
		public RepositoryAddress Address;
		public string ComittMessage;
	}

}
