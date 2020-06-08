using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.sync;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.Default
{
	/// <summary>
	/// Test class for DefaultFileTypeHandler.
	/// </summary>
	[TestFixture]
	public class DefaultFileTypeHandlerTests
	{
		private IChorusFileTypeHandler _defaultFileHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_defaultFileHandler = new DefaultFileTypeHandler();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_defaultFileHandler = null;
		}

		[Test]
		public void HandlerShouldOnlyProcessMegabyteSizedFiles()
		{
			Assert.AreEqual(LargeFileFilter.Megabyte, _defaultFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerSupportsCorrectExtensions()
		{
			var extensions = _defaultFileHandler.GetExtensionsOfKnownTextFileTypes().ToList();
			Assert.IsTrue(extensions.Count == 0);
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _defaultFileHandler.DescribeInitialContents(null, null).ToList();
			Assert.AreEqual(1, initialContents.Count);
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void LargeFileWithUnsupportedExtension_ShouldNotBeInRepo()
		{
			LargeFileIntegrationTestService.TestThatALargeFileIsNotInRepository("docx");
		}
	}
}
