using System;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the FieldWorksFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileHandlerTests
	{
		private IChorusFileTypeHandler m_fwFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_fwFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
						 where handler.GetType().Name == "FieldWorksFileHandler"
						 select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			m_fwFileHandler = null;
		}

		[Test]
		public void Cannot_Merge_Null_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Cannot_PresentFile_Yet()
		{
			Assert.IsFalse(m_fwFileHandler.CanPresentFile("bogusPathname"));
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void Do3WayMerge_Throws()
		{
			m_fwFileHandler.Do3WayMerge(null);
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void GetChangePresenter_Throws()
		{
			m_fwFileHandler.GetChangePresenter(null, null);
		}

		[Test]
		public void DescribeInitialContents_Should_Have_Added_For_Label()
		{
			var initialContents = m_fwFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void GetExtensionsOfKnownTextFileTypes_Is_Xml()
		{
			var extensions = m_fwFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("xml", extensions[0]);
		}
	}
}
