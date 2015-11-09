using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.FileHandlers.ldml
{
	/// <summary>
	/// Make sure only an ldml xml file can be validated by the LdmlFileHandler class.
	/// </summary>
	[TestFixture]
	public class LdmlFileValidationTests
	{
		private IChorusFileTypeHandler _handler;
		private TempFile _goodXmlTempFile;
		private TempFile _illformedXmlTempFile;
		private TempFile _goodXmlButNotLdmlTempFile;
		private TempFile _nonXmlTempFile;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_handler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "LdmlFileHandler")).First();

			_goodXmlTempFile = TempFile.WithExtension(".ldml");
#if MONO
			File.WriteAllText(_goodXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml>" + Environment.NewLine + "</ldml>");
#else
			File.WriteAllText(_goodXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml />");
#endif
			_illformedXmlTempFile = TempFile.WithExtension(".ldml");
			File.WriteAllText(_illformedXmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml>");

			_goodXmlButNotLdmlTempFile = TempFile.WithExtension(".ldml");
			File.WriteAllText(_goodXmlButNotLdmlTempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<nonldmlstuff />");

			_nonXmlTempFile = TempFile.WithExtension(".txt");
			File.WriteAllText(_nonXmlTempFile.Path, "This is not an ldml file." + Environment.NewLine);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_handler = null;
			_goodXmlTempFile.Dispose();
			_goodXmlTempFile = null;

			_illformedXmlTempFile.Dispose();
			_illformedXmlTempFile = null;

			_goodXmlButNotLdmlTempFile.Dispose();
			_goodXmlButNotLdmlTempFile = null;

			_nonXmlTempFile.Dispose();
			_nonXmlTempFile = null;
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
			Assert.IsFalse(_handler.CanValidateFile(_nonXmlTempFile.Path));
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.IsTrue(_handler.CanValidateFile(_goodXmlTempFile.Path));
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
			Assert.IsNull(_handler.ValidateFile(_goodXmlTempFile.Path, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_Ldml_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_illformedXmlTempFile.Path, null));
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_Ldml_File()
		{
			Assert.IsNotNull(_handler.ValidateFile(_goodXmlButNotLdmlTempFile.Path, null));
		}
	}
}