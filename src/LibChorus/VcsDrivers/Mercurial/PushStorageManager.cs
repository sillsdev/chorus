using System.IO;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class PushStorageManager : BundleStorageManager
	{
		public PushStorageManager(string storagePath, string bundleIdFilename) : base(storagePath, "pushData", bundleIdFilename) {}

		public byte[] GetChunk(int offset, int length)
		{
			using (var fs = new FileStream(BundlePath, FileMode.Open, FileAccess.Read))
			{
				fs.Seek(offset, SeekOrigin.Begin);
				var chunk = new byte[length];
				int bytesRead = fs.Read(chunk, 0, length);
				if (bytesRead != length)
				{
					var smallerChunk = new byte[bytesRead];
					for (int i = 0; i < bytesRead; i++)
					{
						smallerChunk[i] = chunk[i];
					}
					return smallerChunk;
				}
				return chunk;
			}
		}
	}

	internal class PushResponse
	{
		public int StartOfWindow;
		public int ChunkSize;
		public PushStatus Status;

		public PushResponse() {}

		public PushResponse(PushStatus status)
		{
			Status = status;
		}
	}

	internal enum PushStatus
	{
		Complete = 0,
		Received = 1,
		Fail = 2,
		Reset = 3,
		NotAvailable = 4,
		Timeout = 5,
		InvalidHash = 6
	}
}