using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Chorus.UI
{
  //  [Designer(typeof (LocalizationHelperDesigner))]
	[ToolboxItem(true)]
	public partial class UsbDriveLocator : Component, ISupportInitialize, IExtenderProvider, IUsbDriveLocator
	{
#if MONO
		static bool _appExitSet;
		static void AppExit(object sender, EventArgs e)
		{
			// The Palaso code for finding USB drives uses NDesk.DBus.Bus.System.GetObject()
			// to retrieve the list of currently available drives.  This implicitly creates
			// a thread that opens a socket for communication which will hang the program on
			// exit.  We need to close NDesk.DBus.Bus.System to prevent this, but don't want
			// to do this until the program finishes in case we retrieve this information
			// multiple times.
			// This can't be done in Palaso because it's in an area of code that refuses to
			// have anything to do with System.Windows.Forms.
			NDesk.DBus.Bus.System.Close();
		}
#endif

		#region Extender Stuff
		public UsbDriveLocator()
		{
			InitializeComponent();
			FinishInitialization();
		}

		public UsbDriveLocator(IContainer container)
		{
			container.Add(this);
			this.Disposed += new EventHandler(UsbDriveLocator_Disposed);
			InitializeComponent();
			FinishInitialization();
		}

		private void FinishInitialization()
		{
#if MONO
			if (!_appExitSet)
			{
				Application.ApplicationExit += AppExit;
				_appExitSet = true;
			}
#endif
		}

		void UsbDriveLocator_Disposed(object sender, EventArgs e)
		{
			_keepRunning = false;
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
					new Palaso.UsbDrive.RetrieveUsbDriveInfo();
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

		}
		#endregion
	}
}
