using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Chorus.UI;

namespace Chorus.Tests.UI.Sync
{
	class MockUsbDriveLocator : IUsbDriveLocator
	{
		// Mock locator gives tester more control over the list of drives.
		private readonly List<DriveInfo> m_usbDrives = new List<DriveInfo>();

		public void BeginInit()
		{
			throw new System.NotImplementedException();
		}

		public void EndInit()
		{
			throw new System.NotImplementedException();
		}

		public bool CanExtend(object extendee)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<DriveInfo> UsbDrives
		{
			get { return m_usbDrives; }
		}

		/// <summary>
		/// For test setup. This requires running tests by hand, because not every computer has the same
		/// drive letters. Depending on the number of desired Usb drives for testing, this generates
		/// C: D: F:, etc. If they don't exist on the computer, it will fail.
		/// </summary>
		/// <param name="desiredNumberOfUsbDrives"></param>
		public void Init(int desiredNumberOfUsbDrives)
		{
			for (var i = 0; i < desiredNumberOfUsbDrives; i++)
			{
				string str = Convert.ToChar('C' + i).ToString(CultureInfo.InvariantCulture);
				m_usbDrives.Add(new DriveInfo(str));
			}
		}
	}
}
