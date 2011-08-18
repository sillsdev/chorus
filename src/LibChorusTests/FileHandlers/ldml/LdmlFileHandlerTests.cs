using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.ldml
{
	/// <summary>
	/// Test class for LdmlFileHandler.
	/// </summary>
	[TestFixture]
	public class LdmlFileHandlerTests
	{
		private IChorusFileTypeHandler _ldmlFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_ldmlFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "LdmlFileHandler")).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_ldmlFileHandler = null;
		}

		[Test]
		public void HandlerShouldProcessMaximumFileSize()
		{
			Assert.AreEqual(UInt32.MaxValue, _ldmlFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerOnlySupportsldmlExtension()
		{
			var extensions = _ldmlFileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Count() == 1);
			Assert.AreEqual("ldml", extensions.First());
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _ldmlFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}
	}
}
