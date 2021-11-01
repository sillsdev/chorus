using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SIL.Reporting;

namespace Chorus.UI
{
  //  [Designer(typeof (LocalizationHelperDesigner))]
	[ToolboxItem(true)]
	public partial class UsbDriveLocator : Component, ISupportInitialize, IExtenderProvider, IUsbDriveLocator
	{
		#region Extender Stuff
		public UsbDriveLocator()
		{
			InitializeComponent();
		}

		public UsbDriveLocator(IContainer container)
		{
			container.Add(this);
			this.Disposed += new EventHandler(UsbDriveLocator_Disposed);
			InitializeComponent();
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
			// https://github.com/sillsdev/chorus/issues/261
			// some users have problems with their USB drive (hardware?) such that GetDrives throws
			//	an exception, which if not caught, results in a fatal exception for Chorus
			try
			{
				while (_keepRunning)
				{
					var usbRoots = new SIL.UsbDrive.RetrieveUsbDriveInfo().GetDrives()
						.Select(u => u.RootDirectory.FullName);
					// In Balsa, the boot device (SD Card or USB Stick) shows up as one of the
					// drives and it shouldn't be used for S/R.  Balsa will be changed to
					// include a .chorus-hidden file in the root paritions that should not be
					// used for S/R.  A user can include this file in USB attached drive that
					// they don't want Chorus to use.
					var usbDrives = System.IO.DriveInfo.GetDrives()
						.Where(d => usbRoots.Contains(d.RootDirectory.FullName) &&
									d.RootDirectory.GetFiles(".chorus-hidden").Count() == 0);
					lock (_usbDrives)
					{
						_usbDrives.Clear();
						_usbDrives.AddRange(usbDrives);
					}

					Thread.Sleep(3000); // check again after 3 second
				}
			}
			catch (Exception ex)
			{
				ErrorReport.ReportNonFatalException(ex);
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
