using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class PullBundleHelperTests
	{

		[Test]
		public void WriteChunk_Text_WriteOk()
		{
			var bundleHelper = new PullBundleHelper();
			string sampleText = "some sample text";
			byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
			bundleHelper.WriteChunk(bytes);

			string fromFile = File.ReadAllText(bundleHelper.BundlePath);
			Assert.That(sampleText, Is.EqualTo(fromFile));
		}

		[Test]
		public void WriteChunk_WriteMultipleChunks_AssembledOk()
		{
			var bundleHelper = new PullBundleHelper();
			string[] sampleTexts = new string[3]
									   {"this is a sentence. ", "Another sentence. ", "Yet another word string."};

			string combinedText = "";
			foreach (var sampleText in sampleTexts)
			{
				combinedText += sampleText;
				byte[] bytes = Encoding.UTF8.GetBytes(sampleText);
				bundleHelper.WriteChunk(bytes);
			}
			Assert.That(combinedText, Is.EqualTo(File.ReadAllText(bundleHelper.BundlePath)));
		}
	}
}
