using System;
using System.Collections.Generic;
using System.IO;

namespace Chorus.Utilities.UsbDrive
{
	public interface IRetrieveUsbDriveInfo
	{
		List<IUsbDriveInfo> GetDrives();
	}
	public class RetrieveUsbDriveInfo : IRetrieveUsbDriveInfo
	{
		public List<IUsbDriveInfo> GetDrives()
		{
			return UsbDriveInfo.GetDrives();
		}
	}

	/// <summary>
	/// This class allows tests to set up pretend usb drives
	/// </summary>
	public class RetrieveUsbDriveInfoForTests : IRetrieveUsbDriveInfo
	{
		private readonly List<IUsbDriveInfo> _driveInfos;

		public RetrieveUsbDriveInfoForTests(List<IUsbDriveInfo> driveInfos)
		{
			_driveInfos = driveInfos;
		}

		public List<IUsbDriveInfo> GetDrives()
		{
			return _driveInfos;
		}
	}

	public interface IUsbDriveInfo
	{
		bool IsReady { get; }
		DirectoryInfo RootDirectory { get; }
		ulong TotalSize { get; }
		ulong TotalFreeSpace { get; }
	}

	/// <summary>
	/// This class allows tests to set up pretend usb drives, in order to test situations like
	/// 1) no drives found
	/// 2) multiple drives
	/// 3) full drives
	/// 4) locked drives(not today, but maybe soon)
	/// </summary>
	///
	public class UsbDriveInfoForTests : IUsbDriveInfo
	{
		public UsbDriveInfoForTests(string path)
		{
			IsReady = true;
			TotalSize = ulong.MaxValue;
			TotalFreeSpace = ulong.MinValue;
			RootDirectory =  new DirectoryInfo(path);
		}

		public bool IsReady{get;set;}
		public DirectoryInfo RootDirectory { get; set; }
		public ulong TotalSize { get; set; }
		public ulong TotalFreeSpace{get; set;}
	}

	[CLSCompliant (false)]
	public abstract class UsbDriveInfo : IUsbDriveInfo
	{
		public abstract bool IsReady
		{
			get;
		}

		public abstract DirectoryInfo RootDirectory
		{
			get;
		}

		public abstract ulong TotalSize{ get;}
		public abstract ulong TotalFreeSpace { get; }

		public static List<IUsbDriveInfo> GetDrives()
		{
#if MONO
			return UsbDriveInfoLinux.GetDrives();
#else
			return UsbDriveInfoWindows.GetDrives();
#endif

		}
	}

	/*
	public class Test
	{
		static void la()
		{
			List<UsbDriveInfo> drives = UsbDriveInfo.GetDrives();
			DirectoryInfo path = drives[0].RootDirectory;
		}
	}
	*/
}
