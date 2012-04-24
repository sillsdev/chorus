using System;
using System.IO;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class PushStorageEnvForTest : IDisposable
	{
		public TemporaryFolder BaseFolder;
		public PushStorageEnvForTest()
		{
			BaseFolder = new TemporaryFolder("PushStorageHelperTests");
		}

		public string PullDataFolderPath
		{
			get { return Path.Combine(BaseFolder.Path, "pushData"); }
		}

		public void Dispose()
		{
			BaseFolder.Dispose();
		}
	}

	[TestFixture]
	public class PushStorageManagerTests
	{
		[Test]
		public void GetChunk_First5Bytes_Ok()
		{
			using (var e = new PushStorageEnvForTest())
			{
				string text = "This is some sample text to be used by the push bundle helper tests";
				var bundleHelper = new PushStorageManager(e.PullDataFolderPath, "randomHash");
				File.WriteAllText(bundleHelper.BundlePath, text);
				byte[] chunk = bundleHelper.GetChunk(0, 5);
				Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("This "));
			}
		}

		[Test]
		public void GetChunk_MiddleBytes_Ok()
		{
			using (var e = new PushStorageEnvForTest())
			{
				string text = "This is some sample text to be used by the push bundle helper tests";
				var bundleHelper = new PushStorageManager(e.PullDataFolderPath, "randomHash");
				File.WriteAllText(bundleHelper.BundlePath, text);
				byte[] chunk = bundleHelper.GetChunk(10, 5);
				Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("me sa"));
			}

		}

		[Test]
		public void GetChunk_OffsetOutOfRange_EmptyByteArray()
		{
			using (var e = new PushStorageEnvForTest())
			{
				string text = "sample";
				var bundleHelper = new PushStorageManager(e.PullDataFolderPath, "randomHash");
				File.WriteAllText(bundleHelper.BundlePath, text);
				byte[] chunk = bundleHelper.GetChunk(10, 5); // offset is greater than string length
				Assert.That(chunk.Length, Is.EqualTo(0));
			}
		}

		[Test]
		public void GetChunk_LengthTooLarge_ReturnsAdjustedByteArray()
		{
			using (var e = new PushStorageEnvForTest())
			{
				string text = "sample";
				var bundleHelper = new PushStorageManager(e.PullDataFolderPath, "randomHash");
				File.WriteAllText(bundleHelper.BundlePath, text);
				byte[] chunk = bundleHelper.GetChunk(3, 10); // offset is greater than string length
				Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("ple"));
			}
		}
	}
}