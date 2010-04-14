using System;
using System.IO;
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
		private IChorusFileTypeHandler m_handler;
		private string m_goodXmlPathname;
		private string m_illformedXmlPathname;
		private string m_goodXmlButNotFwPathname;
		private string m_nonXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_handler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
						 where handler.GetType().Name == "FieldWorksFileHandler"
						 select handler).First();
			m_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
			File.WriteAllText(m_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<languageproject version='7000016' />");
			m_illformedXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
			File.WriteAllText(m_illformedXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<languageproject version='7000016'>");
			m_goodXmlButNotFwPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
			File.WriteAllText(m_goodXmlButNotFwPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonfwstuff />");
			m_nonXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
			File.WriteAllText(m_nonXmlPathname, "This is not an xml file." + Environment.NewLine);

		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			m_handler = null;
			if (File.Exists(m_goodXmlPathname))
				File.Delete(m_goodXmlPathname);
			if (File.Exists(m_illformedXmlPathname))
				File.Delete(m_illformedXmlPathname);
			if (File.Exists(m_goodXmlButNotFwPathname))
				File.Delete(m_goodXmlButNotFwPathname);
			if (File.Exists(m_nonXmlPathname))
				File.Delete(m_nonXmlPathname);
		}

		[Test]
		public void Cannot_Diff_Nonexistant_File()
		{
			Assert.IsFalse(m_handler.CanDiffFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Diff_Null_File()
		{
			Assert.IsFalse(m_handler.CanDiffFile(null));
		}

		[Test]
		public void Cannot_Diff_Empty_String_File()
		{
			Assert.IsFalse(m_handler.CanDiffFile(String.Empty));
		}

		[Test]
		public void Cannot_Diff_Good_Fw_Xml_File_Yet()
		{
			//Assert.IsTrue(m_handler.CanDiffFile(m_goodXmlPathname));
			Assert.IsFalse(m_handler.CanDiffFile(m_goodXmlPathname));
		}

		[Test]
		public void Cannot_Merge_Null_File()
		{
			Assert.IsFalse(m_handler.CanMergeFile(null));
		}

		[Test]
		public void Cannot_PresentFile_Yet()
		{
			Assert.IsFalse(m_handler.CanPresentFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Validate_Nonexistant_File()
		{
			Assert.IsFalse(m_handler.CanValidateFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Validate_Null_File()
		{
			Assert.IsFalse(m_handler.CanValidateFile(null));
		}

		[Test]
		public void Cannot_Validate_Empty_String_File()
		{
			Assert.IsFalse(m_handler.CanValidateFile(String.Empty));
		}

		[Test]
		public void Cannot_Validate_Nonxml_File()
		{
			Assert.IsFalse(m_handler.CanValidateFile(m_nonXmlPathname));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(m_handler.CanValidateFile(m_goodXmlPathname));
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void Do3WayMerge_Throws()
		{
			m_handler.Do3WayMerge(null);
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void GetChangePresenter_Throws()
		{
			m_handler.GetChangePresenter(null, null);
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.IsNotNull(m_handler.ValidateFile("", null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.IsNotNull(m_handler.ValidateFile(null, null));
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.IsNull(m_handler.ValidateFile(m_goodXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Xml_File()
		{
			Assert.IsNotNull(m_handler.ValidateFile(m_illformedXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Fw_Xml_File()
		{
			Assert.IsNotNull(m_handler.ValidateFile(m_goodXmlButNotFwPathname, null));
		}

		[Test]
		public void DescribeInitialContents_Should_Have_Added_For_Label()
		{
			var initialContents = m_handler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void GetExtensionsOfKnownTextFileTypes_Is_Xml()
		{
			var extensions = m_handler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("xml", extensions[0]);
		}
	}
}
