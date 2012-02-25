using System;
using System.IO;

namespace Chorus.VcsDrivers.Mercurial
{
	abstract class BundleStorageManager
	{
		public string BundlePath;
		private const int _numberOfHoursToKeepIncompleteData = 24;
		private string _dataFolderPath;
		private string _storagePath;
		private string _bundleId;
		private string _idFilePath;
		public readonly string TransactionId;

		public virtual string StorageFolderName
		{
			get { return "overrideme"; }
		}

		public BundleStorageManager(string storagePath, string bundleId)
		{
			_storagePath = storagePath;
			_bundleId = bundleId;
			_dataFolderPath = Path.Combine(storagePath, StorageFolderName);

			Directory.CreateDirectory(_storagePath); // create if necessary
			Directory.CreateDirectory(_dataFolderPath); // create if necessary

			CleanUpExpiredData();

			TransactionId = GetTransactionId();
			BundlePath = Path.Combine(_dataFolderPath, String.Format("{0}.bundle", TransactionId));
			if (!File.Exists(BundlePath))
			{
				var fs = new FileInfo(BundlePath).Create();
				fs.Close();
			}
		}

		private void CleanUpExpiredData()
		{
			DateTime currentTime = DateTime.Now;
			var fileList = Directory.GetFiles(_dataFolderPath);
			foreach (string filePath in fileList)
			{
				DateTime fileLastWriteTime = (new FileInfo(filePath)).LastWriteTime.AddHours(_numberOfHoursToKeepIncompleteData);
				if (currentTime.CompareTo(fileLastWriteTime) > 0)
				{
					File.Delete(filePath);
				}
			}
		}

		private string GetTransactionId()
		{
			_idFilePath = Path.Combine(_dataFolderPath, string.Format("{0}.transid", _bundleId));
			if (File.Exists(_idFilePath))
			{
				return File.ReadAllText(_idFilePath).Trim();
			}
			string id = Guid.NewGuid().ToString();
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