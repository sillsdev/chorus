using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chorus.UI
{
  //  [Designer(typeof (LocalizationHelperDesigner))]
	[ToolboxItem(true)]
	public partial class UsbDriveLocator : Component, ISupportInitialize, IExtenderProvider, IDisposable
	{

		#region Extender Stuff
		public UsbDriveLocator()
		{
			InitializeComponent();
		}

		public UsbDriveLocator(IContainer container)
		{
			container.Add(this);

			InitializeComponent();
		}

		public void BeginInit() { }

		public void EndInit()
		{
			if (DesignMode)
				return;

			_keepRunning = true;
			_worker = new Thread(ScanForUsbDrives);
			_worker.Start();
		}

		public bool CanExtend(object extendee)
		{
			return (extendee is UserControl);
		}

		#endregion

		#region DriveScanning

		private List<DriveInfo> _usbDrives = new List<DriveInfo>();
		private Thread _worker;
		private bool _keepRunning;


		private void ScanForUsbDrives()
		{
			while (_keepRunning)
			{
				var usbDrives = new List<DriveInfo>();
				var info =
					new Chorus.Utilities.UsbDrive.RetrieveUsbDriveInfo();
				var drives = info.GetDrives();
				if (drives.Count > 0)
				{
					foreach (var drive in System.IO.DriveInfo.GetDrives())
					{
						if (drive.RootDirectory.FullName ==
							drives[0].RootDirectory.FullName)
						{
							usbDrives.Add(drive);
						}
					}
				}
				lock (this)
				{
					_usbDrives.Clear();
					_usbDrives.AddRange(usbDrives);
				}
				Thread.Sleep(3000); // check again after 1 second
			}
		}

		public IEnumerable<DriveInfo> UsbDrives
		{
			get
			{
				lock (_usbDrives)
				{
					return _usbDrives;
				}
			}
		}

		~UsbDriveLocator()
		{
			if (_keepRunning)
			{

#if DEBUG
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName);
#else
			   _keepRunning = false ;
#endif
			}
		#endregion

		}
	}
}
