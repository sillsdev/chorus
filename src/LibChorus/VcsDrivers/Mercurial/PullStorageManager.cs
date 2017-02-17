using System.IO;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class PullStorageManager : BundleStorageManager
	{
		public PullStorageManager(string storagePath, string bundleIdFilename) : base(storagePath, "pullData", bundleIdFilename) { }

		public void AppendChunk(byte[] data)
		{
			using (var filestream = new FileStream(BundlePath, FileMode.Append, FileAccess.Write))
			{
				filestream.Write(data, 0, data.Length);
			}
		}

		public void WriteChunk(int offset, byte[] data)
		{
			using (var filestream = new FileStream(BundlePath, FileMode.Open, FileAccess.Write))
			{
				filestream.Seek(offset, SeekOrigin.Begin);
				filestream.Write(data, 0, data.Length);
			}
		}

		public int StartOfWindow
		{
			get { return (int) (new FileInfo(BundlePath)).Length; }
		}
	}

	internal class PullResponse
	{
		public int BundleSize;
		public byte[] Chunk;
		public PullStatus Status;
		public int ChunkSize;

		public PullResponse(PullStatus status)
		{
			Status = status;
		}

		public PullResponse() {}
	}

	internal enum PullStatus
	{
		OK = 0,
		NoChange = 1,
		Fail = 2,
		Reset = 3,
		NotAvailable = 4,
		Timeout = 5,
		InvalidHash = 6,
		InProgress = 7,
		Unauthorized = 8
	}
}