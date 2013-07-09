using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Palaso.UsbDrive;

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

		private List<IUsbDriveInfo> _usbDrives = new List<IUsbDriveInfo>();
		private Thread _worker;
		private bool _keepRunning;


		private void ScanForUsbDrives()
		{
			while (_keepRunning)
			{
				var usbDrives = UsbDriveInfo.GetDrives();
				lock (this)
				{
					_usbDrives.Clear();
					_usbDrives.AddRange(usbDrives);
				}
				Thread.Sleep(3000); // check again after 1 second
			}
		}

		// TODO: resolve merge problems...
		public IEnumerable<DriveInfo> UsbDrives
		{
			get { throw new NotImplementedException(); }
		}

		public IEnumerable<IUsbDriveInfo> UsbDrivesLinux
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
