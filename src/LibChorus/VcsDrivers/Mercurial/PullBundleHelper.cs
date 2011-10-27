using System;
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
		public int StartOfWindow;
		public PullStatus Status;
	}

	internal enum PullStatus
	{
		OK = 0,
		NoChange = 1,
		Fail = 2
	}

	internal class PushResponse
	{
		public int StartOfWindow;
		public int ChunkSize;
		public PushStatus Status;
	}

	internal enum PushStatus
	{
		Complete = 0,
		Received = 1,
		Reset = 3,
		Fail = 4
	}

	internal class PushBundleHelper : IDisposable
	{
		public string BundlePath;
		private TemporaryFolder _folder;

		public PushBundleHelper()
		{
			_folder = new TemporaryFolder("HgResume - PushBundleHelper");
			BundlePath = _folder.GetNewTempFile(false).Path;
		}

		public PushBundleHelper(string filePath)
		{
			BundlePath = filePath;
		}

		public byte[] GetChunk(int offset, int length)
		{
			using (var fs = new FileStream(BundlePath, FileMode.Open, FileAccess.Read))
			{
				fs.Seek(offset, SeekOrigin.Begin);
				var chunk = new byte[length];
				fs.Read(chunk, 0, length);
				return chunk;
			}
		}

		public void Dispose()
		{
			if (_folder != null)
			{
				_folder.Dispose();
			}
		}
	}
}