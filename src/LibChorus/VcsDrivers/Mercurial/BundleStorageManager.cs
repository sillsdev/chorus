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
			foreach (string filePath in Directory.GetFiles(_dataFolderPath))
			{
				DateTime fileCreateTime = (new FileInfo(filePath)).CreationTime.AddHours(_numberOfHoursToKeepIncompleteData);
				if (currentTime.CompareTo(fileCreateTime) > 0)
				{
					File.Delete(filePath);
				}
			}
		}

		private string GetTransactionId()
		{
			string idFilePath = Path.Combine(_dataFolderPath, string.Format("{0}.transid", _bundleId));
			if (File.Exists(idFilePath))
			{
				return File.ReadAllText(idFilePath).Trim();
			}
			string id = Guid.NewGuid().ToString();
			File.WriteAllText(idFilePath, id);
			return id;
		}

		public void Reset()
		{
			File.WriteAllBytes(BundlePath, new byte[0]);
		}
	}
}