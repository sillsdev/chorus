using System.IO;
using Palaso.TestUtilities;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class PullBundleHelper
	{
		public string BundlePath;
		private TemporaryFolder _folder;

		public void WriteChunk(byte[] data)
		{
			using (FileStream filestream = new FileStream(BundlePath, FileMode.Append, FileAccess.Write))
			{
				filestream.Write(data, 0, data.Length);
			}
		}

		public PullBundleHelper()
		{
			_folder = new TemporaryFolder("HgResume-PullBundleHelper");
			BundlePath = _folder.GetNewTempFile(true).Path;
		}

		public void Dispose()
		{
			if (_folder != null)
			{
				_folder.Dispose();
			}
		}
	}

	internal class PullResponse
	{
		public int BundleSize;
		public string Checksum;
		public byte[] Chunk;
		public PullStatus Status;
	}

	internal enum PullStatus
	{
		OK = 0,
		NoChange = 1,
		Fail = 2
	}
}