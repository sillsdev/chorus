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
				// A single Read is allowed to come up short even when the file has more to give, and a
				// short chunk costs an extra round trip, so keep reading until we have what we asked for
				// or hit the end of the bundle.
				int bytesRead = 0;
				while (bytesRead < length)
				{
					int read = fs.Read(chunk, bytesRead, length - bytesRead);
					if (read == 0)
					{
						break; // end of file
					}
					bytesRead += read;
				}
				if (bytesRead != length)
				{
					var smallerChunk = new byte[bytesRead];
					chunk.CopyTo(smallerChunk, 0, bytesRead);
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