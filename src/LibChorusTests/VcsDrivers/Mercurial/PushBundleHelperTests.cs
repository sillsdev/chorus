using System.IO;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class PushBundleHelperTests
	{
		[Test]
		public void GetChunk_First5Bytes_Ok()
		{
			string text = "This is some sample text to be used by the push bundle helper tests";
			var bundleHelper = new PushBundleHelper();
			File.WriteAllText(bundleHelper.BundlePath, text);
			byte[] chunk = bundleHelper.GetChunk(0, 5);
			Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("This "));
		}

		[Test]
		public void GetChunk_MiddleBytes_Ok()
		{
			string text = "This is some sample text to be used by the push bundle helper tests";
			var bundleHelper = new PushBundleHelper();
			File.WriteAllText(bundleHelper.BundlePath, text);
			byte[] chunk = bundleHelper.GetChunk(10, 5);
			Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("me sa"));
		}

		[Test]
		public void GetChunk_OffsetOutOfRange_EmptyByteArray()
		{
			string text = "sample";
			var bundleHelper = new PushBundleHelper();
			File.WriteAllText(bundleHelper.BundlePath, text);
			byte[] chunk = bundleHelper.GetChunk(10, 5); // offset is greater than string length
			Assert.That(chunk.Length, Is.EqualTo(0));
		}

		[Test]
		public void GetChunk_LengthTooLarge_ReturnsAdjustedByteArray()
		{
			string text = "sample";
			var bundleHelper = new PushBundleHelper();
			File.WriteAllText(bundleHelper.BundlePath, text);
			byte[] chunk = bundleHelper.GetChunk(3, 10); // offset is greater than string length
			Assert.That(Encoding.UTF8.GetString(chunk), Is.EqualTo("ple"));
		}
	}
}