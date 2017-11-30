using System;
using System.IO;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	public class PullStorageEnvForTest : IDisposable
	{
		public TemporaryFolder BaseFolder;
		public PullStorageEnvForTest()
		{
			BaseFolder = new TemporaryFolder("PullStorageHelperTests");
		}

		public string PullDataFolderPath
		{
			get { return Path.Combine(BaseFolder.Path, "pullData"); }
		}

		public void Dispose()
		{
			BaseFolder.Dispose();
		}
	}

	[TestFixture]
	public class PullStorageManagerTests
	{

		[Test]
		public void WriteChunk_Text_WriteOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string sampleText = "some sample text";
				byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
				bundleHelper.WriteChunk(0, bytes);
				string fromFile = File.ReadAllText(bundleHelper.BundlePath);
				Assert.That(sampleText, Is.EqualTo(fromFile));
			}
		}

		[Test]
		public void StartOfWindow_WriteBytes_LengthOfBytes()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string sampleText = "some sample text";
				byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
				bundleHelper.WriteChunk(0, bytes);
				Assert.That(bundleHelper.StartOfWindow, Is.EqualTo(bytes.Length));
			}
		}

		[Test]
		public void StartOfWindow_NoWrites_Zero()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				Assert.That(bundleHelper.StartOfWindow, Is.EqualTo(0));
			}
		}

		[Test]
		public void WriteChunk_WriteMultipleChunks_AssembledOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string[] sampleTexts = new[] { "this is a sentence. ", "Another sentence. ", "Yet another word string." };

				string combinedText = "";
				int startOfWindow = 0;
				foreach (var sampleText in sampleTexts)
				{
					combinedText += sampleText;
					byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
					bundleHelper.WriteChunk(startOfWindow, bytes);
					startOfWindow += bytes.Length;
				}
				Assert.That(combinedText, Is.EqualTo(File.ReadAllText(bundleHelper.BundlePath)));
			}
		}

		[Test]
		public void WriteChunk_MultipleWritesOverWriteSameOffset_AssemblesOk()
		{
			using (var e = new PullStorageEnvForTest())
			{
				var bundleHelper = new PullStorageManager(e.BaseFolder.Path, "randomHash");
				string[] sampleTexts = new[] { "this is a sentence. ", "Another sentence. ", "Yet another word string." };

				string combinedText = "";
				int startOfWindow = 0;
				foreach (var sampleText in sampleTexts)
				{
					combinedText += sampleText;
					byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
					bundleHelper.WriteChunk(startOfWindow, bytes);
					startOfWindow += bytes.Length;
				}
				bundleHelper.WriteChunk(0, Encoding.UTF8.GetBytes(sampleTexts[0]));
				Assert.That(combinedText, Is.EqualTo(File.ReadAllText(bundleHelper.BundlePath)));
			}
		}


	}


	public class BundleStorageEnvForTest : IDisposable
	{
		public TemporaryFolder BaseFolder;
		public BundleStorageEnvForTest()
		{
			BaseFolder = new TemporaryFolder("BundleStorageHelperTests");
		}

		public string DataFolderPath
		{
			get { return Path.Combine(BaseFolder.Path, "data"); }
		}

		public void Dispose()
		{
			BaseFolder.Dispose();
		}
	}

	internal class SimpleStorageManager : BundleStorageManager
	{
		public SimpleStorageManager(string storagePath, string baseHash) : base(storagePath, "data", baseHash) {}
	}
}
