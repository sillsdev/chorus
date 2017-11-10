using System;
using System.IO;

namespace Chorus.VcsDrivers.Mercurial
{
	internal abstract class BundleStorageManager
	{
		public string BundlePath;
		private const int NumberOfHoursToKeepIncompleteData = 24;
		private string _idFilePath;
		public readonly string TransactionId;

		protected BundleStorageManager(string storagePath, string storageFolderName, string bundleIdFilename)
		{
			var dataFolderPath = Path.Combine(storagePath, storageFolderName);

			Directory.CreateDirectory(storagePath); // create if necessary
			Directory.CreateDirectory(dataFolderPath); // create if necessary

			CleanUpExpiredData(dataFolderPath);

			TransactionId = GetTransactionId(dataFolderPath, bundleIdFilename);
			BundlePath = Path.Combine(dataFolderPath, String.Format("{0}.bundle", TransactionId));
			if (!File.Exists(BundlePath))
			{
				var fs = new FileInfo(BundlePath).Create();
				fs.Close();
			}
		}

		private static void CleanUpExpiredData(string dataFolderPath)
		{
			var currentTime = DateTime.Now;
			var fileList = Directory.GetFiles(dataFolderPath);
			foreach (var filePath in fileList)
			{
				var fileLastWriteTime = new FileInfo(filePath).LastWriteTime.AddHours(NumberOfHoursToKeepIncompleteData);
				if (currentTime.CompareTo(fileLastWriteTime) > 0)
				{
					File.Delete(filePath);
				}
			}
		}

		private string GetTransactionId(string dataFolderPath, string bundleIdFilename)
		{
			_idFilePath = Path.Combine(dataFolderPath, string.Format("{0}.transid", bundleIdFilename));
			if (File.Exists(_idFilePath))
			{
				return File.ReadAllText(_idFilePath).Trim();
			}
			var id = Guid.NewGuid().ToString();
			File.WriteAllText(_idFilePath, id);
			return id;
		}

		public void Reset()
		{
			File.WriteAllBytes(BundlePath, new byte[0]);
		}

		public void Cleanup()
		{
			File.Delete(BundlePath);
			File.Delete(_idFilePath);
		}
	}
}