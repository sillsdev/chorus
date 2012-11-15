using System;
using System.IO;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class BundleStorageManagerTests
	{
		[Test]
		public void Constructor_NoPreviousTransactionIdFile_FileIsCreatedAndPopulated()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string idFilePath = Path.Combine(e.DataFolderPath, "abcde123.transid");
				Assert.That(File.Exists(idFilePath), Is.False);
				new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(File.Exists(idFilePath), Is.True);
				Assert.That(File.ReadAllText(idFilePath).Trim(), Is.Not.Empty);
			}
		}

		[Test]
		public void Constructor_PreviousTransactionIdFileExists_ReturnsFoundTransactionId()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				var bundleHelper = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				var bundleHelper2 = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(bundleHelper.TransactionId, Is.EqualTo(bundleHelper2.TransactionId));
			}
		}

		[Test]
		public void Constructor_PreviousTransactionIdFileNotExists_GetNewId()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string idFilePath = Path.Combine(e.DataFolderPath, "abcde123.transid");
				var bundleHelper = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				File.Delete(idFilePath);
				var bundleHelper2 = new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(bundleHelper.TransactionId, Is.Not.EqualTo(bundleHelper2.TransactionId));
			}
		}

		[Test]
		public void Constructor_PullDataFolderNotExist_FolderCreated()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				Assert.That(Directory.Exists(e.DataFolderPath), Is.False);
				new SimpleStorageManager(e.BaseFolder.Path, "abcde123");
				Assert.That(Directory.Exists(e.DataFolderPath), Is.True);
			}
		}

		[Test]
		public void Constructor_MultipleInstances_ManagesMultipleInstancesOk()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				var pull1a = new SimpleStorageManager(e.BaseFolder.Path, "number1");
				var pull2a = new SimpleStorageManager(e.BaseFolder.Path, "number2");
				var pull3a = new SimpleStorageManager(e.BaseFolder.Path, "number3");
				var pull4a = new SimpleStorageManager(e.BaseFolder.Path, "number4");
				var pull1b = new SimpleStorageManager(e.BaseFolder.Path, "number1");
				var pull2b = new SimpleStorageManager(e.BaseFolder.Path, "number2");
				var pull3b = new SimpleStorageManager(e.BaseFolder.Path, "number3");
				var pull4b = new SimpleStorageManager(e.BaseFolder.Path, "number4");
				Assert.That(pull1a.TransactionId, Is.EqualTo(pull1b.TransactionId));
				Assert.That(pull2a.TransactionId, Is.EqualTo(pull2b.TransactionId));
				Assert.That(pull3a.TransactionId, Is.EqualTo(pull3b.TransactionId));
				Assert.That(pull4a.TransactionId, Is.EqualTo(pull4b.TransactionId));
			}
		}

		[Test]
		public void Constructor_ExpiredData_DataIsRemoved()
		{
			using (var e = new BundleStorageEnvForTest())
			{
				string expiredFile = Path.Combine(e.DataFolderPath, "123.transid");
				var expiredTime = DateTime.Now;
				expiredTime = expiredTime.AddHours(-50);
				Directory.CreateDirectory(e.DataFolderPath);

				using (var fs = File.Create(expiredFile)) {}
				File.SetLastWriteTime(expiredFile, expiredTime);

				Assert.That(File.Exists(expiredFile), Is.True);
				var sm = new SimpleStorageManager(e.BaseFolder.Path, "abc");
				Assert.That(File.Exists(expiredFile), Is.False);
			}
		}
	}
}