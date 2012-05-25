using System.Collections.Generic;
using System.IO;

namespace Chorus.UI
{
	public interface IUsbDriveLocator
	{
		void BeginInit();

		void EndInit();

		bool CanExtend(object extendee);

		IEnumerable<DriveInfo> UsbDrives { get; }
	}
}
