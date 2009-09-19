using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.UI.Sync
{
	public partial class SyncStartControl : UserControl
	{
		private readonly HgRepository _repository;

		public SyncStartControl(HgRepository repository)
		{
			_repository = repository;
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{

		}

		private void OnUpdateDisplayTick(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			var info = new Chorus.Utilities.UsbDrive.RetrieveUsbDriveInfo();
			var drives = info.GetDrives();
			if (drives.Count ==0)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "First insert a USB flash drive";
			}
			else if (drives.Count > 1)
			{
				_useUSBButton.Enabled = false;
				_usbStatusLabel.Text = "More than one USB flash drive detected. Please remove one.";
			}
			else
			{
				_useUSBButton.Enabled = true;
				try
				{
					_usbStatusLabel.Text = "Found " + drives[0].RootDirectory;
				}
				catch(Exception error   )
				{
					_usbStatusLabel.Text = error.Message;
				}
			}
			var paths = _repository.GetRepositoryPathsInHgrc();
			_useInternetButton.Enabled = paths.Any(p => p is HttpRepositoryPath);
			if (!_useInternetButton.Enabled)
			{
				_internetStatusLabel.Text = "This project is not yet associated with an internet server";
			}
			else
			{
				_internetStatusLabel.Text = "";
				//enhance: which one will be used if I click this?
			}

			_useSharedFolderButton.Enabled = paths.Any(p => p is DirectoryRepositorySource);
			if (!_useSharedFolderButton.Enabled)
			{
				_sharedFolderLabel.Text = "This project is not yet associated with an shared folder";
			}
			else
			{
				_sharedFolderLabel.Text = "";
				//enhance: which one will be used if I click this?
			}


		}
	}
}
