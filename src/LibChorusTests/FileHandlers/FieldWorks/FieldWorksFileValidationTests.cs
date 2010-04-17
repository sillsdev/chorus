using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Make sure only the FieldWorks 7.0 xml file can be validated by the FieldWorksFileHandler class.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileValidationTests
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
	}
}