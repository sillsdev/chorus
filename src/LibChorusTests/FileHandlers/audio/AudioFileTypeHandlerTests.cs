using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.sync;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.audio
{
	/// <summary>
	/// Test class for AudioFileTypeHandler.
	/// </summary>
	[TestFixture]
	public class AudioFileTypeHandlerTests
	{
		private IChorusFileTypeHandler _audioFileHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_audioFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "AudioFileTypeHandler")).First();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_audioFileHandler = null;
		}

		[Test]
		public void HandlerShouldOnlyProcessMegabyteSizedFiles()
		{
			Assert.AreEqual(LargeFileFilter.Megabyte, _audioFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerSupportsCorrectExtensions()
		{
			var extensions = _audioFileHandler.GetExtensionsOfKnownTextFileTypes().ToList();
			Assert.IsTrue(extensions.Count() == 8);
			var expectedExtensions = new HashSet<string> {"wav", "snd", "au", "aif", "aifc", "aiff", "wma", "mp3"};
			foreach (var expectedExtension in expectedExtensions)
				Assert.Contains(expectedExtension, extensions);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _audioFileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void LargeFilesWithSupportedExtension_ShouldNotBeInRepo()
		{
			foreach (var extension in _audioFileHandler.GetExtensionsOfKnownTextFileTypes())
			{
				if (extension == "wav")
					continue; // Nasty hack, but "wav" is put into repo no matter its size. TODO: FIX THIS, if Hg ever works right for "wav" files.
				LargeFileIntegrationTestService.TestThatALargeFileIsNotInRepository(extension);
			}
		}
	}
}
