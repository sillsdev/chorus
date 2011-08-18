using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.ldml
{
	/// <summary>
	/// Make sure only the FieldWorks 7.0 xml file can be validated by the FieldWorksFileHandler class.
	/// </summary>
	[TestFixture]
	public class LdmlFileValidationTests
	{
		private IChorusFileTypeHandler _handler;
		private string _goodXmlPathname;
		private string _illformedXmlPathname;
		private string _goodXmlButNotFwPathname;
		private string _nonXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_handler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "LdmlFileHandler")).First();
			_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ldml");
			File.WriteAllText(_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml />");
			_illformedXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ldml");
			File.WriteAllText(_illformedXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml>");
			_goodXmlButNotFwPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ldml");
			File.WriteAllText(_goodXmlButNotFwPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonldmlstuff />");
			_nonXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
			File.WriteAllText(_nonXmlPathname, "This is not an ldml file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_handler = null;
			if (File.Exists(_goodXmlPathname))
				File.Delete(_goodXmlPathname);
			if (File.Exists(_illformedXmlPathname))
				File.Delete(_illformedXmlPathname);
			if (File.Exists(_goodXmlButNotFwPathname))
				File.Delete(_goodXmlButNotFwPathname);
			if (File.Exists(_nonXmlPathname))
				File.Delete(_nonXmlPathname);
		}

		[Test]
		public void Cannot_Validate_Nonexistant_File()
		{
			Assert.IsFalse(_handler.CanValidateFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Validate_Null_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(null));
		}

		[Test]
		public void Cannot_Validate_Empty_String_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(String.Empty));
		}

		[Test]
		public void Cannot_Validate_Nonxml_File()
		{
			Assert.IsFalse(_handler.CanValidateFile(_nonXmlPathname));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(_handler.CanValidateFile(_goodXmlPathname));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.IsNotNull(_handler.ValidateFile("", null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.IsNotNull(_handler.ValidateFile(null, null));
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.IsNull(_handler.ValidateFile(_goodXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Ldml_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_illformedXmlPathname, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Ldml_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_goodXmlButNotFwPathname, null));
		}
	}
}