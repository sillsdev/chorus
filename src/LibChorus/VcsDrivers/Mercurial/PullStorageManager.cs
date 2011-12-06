using System;
using System.IO;
using Palaso.TestUtilities;

namespace Chorus.VcsDrivers.Mercurial
{

	// TODO: this class needs to keep track of incomplete pulls an allow them to be resumed.  data should not be distroyed until the bundle has been fully received.
	internal class PullStorageManager
	{
		public string BundlePath;
		private const int _numberOfHoursToKeepIncompletePullData = 24;
		private string _pullDataFolderPath;
		private string _storagePath;
		private string _baseHash;
		public readonly string TransactionId;

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

		public PullStorageManager(string storagePath, string baseHash)
		{
			_storagePath = storagePath;
			_baseHash = baseHash;
			_pullDataFolderPath = Path.Combine(storagePath, "pullData");

			Directory.CreateDirectory(_storagePath); // create if necessary
			Directory.CreateDirectory(_pullDataFolderPath); // create if necessary

			CleanUpExpiredPullData();

			TransactionId = GetTransactionId();
			BundlePath = Path.Combine(_pullDataFolderPath, String.Format("{0}.bundle.part", TransactionId));
			if (!File.Exists(BundlePath))
			{
				var fs = new FileInfo(BundlePath).Create();
				fs.Close();
			}
		}

		private string GetTransactionId()
		{
			string idFilePath = Path.Combine(_pullDataFolderPath, string.Format("{0}.transid", _baseHash));
			if (File.Exists(idFilePath))
			{
				return File.ReadAllText(idFilePath).Trim();
			}
			string id = Guid.NewGuid().ToString();
			File.WriteAllText(idFilePath, id);
			return id;
		}

		public int StartOfWindow
		{
			get { return (int) (new FileInfo(BundlePath)).Length; }
		}

		private void CleanUpExpiredPullData()
		{
			DateTime currentTime = DateTime.Now;
			foreach (string filePath in Directory.GetFiles(_pullDataFolderPath))
			{
				DateTime fileCreateTime = (new FileInfo(filePath)).CreationTime.AddHours(_numberOfHoursToKeepIncompletePullData);
				if (currentTime.CompareTo(fileCreateTime) > 0)
				{
					File.Delete(filePath);
				}
			}
		}
	}

	internal class PullResponse
	{
		public int BundleSize;
		public string Checksum;
		public byte[] Chunk;
		public PullStatus Status;
		public int ChunkSize;
	}

	internal enum PullStatus
	{
		OK = 0,
		NoChange = 1,
		Fail = 2
	}
}