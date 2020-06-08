using System.Collections.Generic;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.sync;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.image
{
	/// <summary>
	/// Test class for ImageFileTypeHandler.
	/// </summary>
	[TestFixture]
	public class ImageFileTypeHandlerTests
	{
		private IChorusFileTypeHandler _imageFileHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_imageFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "ImageFileTypeHandler")).First();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_imageFileHandler = null;
		}

		[Test]
		public void HandlerShouldOnlyProcessMegabyteSizedFiles()
		{
			Assert.AreEqual(LargeFileFilter.Megabyte, _imageFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerSupportsCorrectExtensions()
		{
			var extensions = _imageFileHandler.GetExtensionsOfKnownTextFileTypes().ToList();
			var expectedExtensions = new HashSet<string> { "bmp", "jpg", "jpeg", "gif", "png", "tif", "tiff", "ico", "wmf", "pcx", "cgm" };
			Assert.AreEqual(expectedExtensions.Count, extensions.Count);
			foreach (var expectedExtension in expectedExtensions)
				Assert.Contains(expectedExtension, extensions);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _imageFileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void LargeFilesWithSupportedExtension_ShouldNotBeInRepo()
		{
			foreach (var extension in _imageFileHandler.GetExtensionsOfKnownTextFileTypes())
				LargeFileIntegrationTestService.TestThatALargeFileIsNotInRepository(extension);
		}
	}
}
