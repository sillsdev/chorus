using System.Collections.Generic;
using System.IO;

namespace Chorus.UI
{
	interface IUsbDriveLocator
	{
		void BeginInit();

		void EndInit();

		bool CanExtend(object extendee);

		IEnumerable<DriveInfo> UsbDrives { get; }
	}
}
