﻿using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Make sure only a user lexicon settings xml file can be validated by the UserLexiconSettingsFileHandler class.
	/// </summary>
	[TestFixture]
	public class UserLexiconSettingsFileValidationTests
	{
		private IChorusFileTypeHandler _handler;
		private TempFile _goodXmlTempFile;
		private TempFile _illformedXmlTempFile;
		private TempFile _goodXmlButNotUserLexiconSettingsTempFile;
		private TempFile _nonXmlTempFile;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_handler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "UserLexiconSettingsFileHandler")).First();

			_goodXmlTempFile = TempFile.WithExtension(".ulsx");
			var nl = Environment.NewLine;
			File.WriteAllText(_goodXmlTempFile.Path, Platform.IsMono ?
				$"<?xml version='1.0' encoding='utf-8'?>{nl}<UserLexiconSettings>{nl}</UserLexiconSettings>" :
				$"<?xml version='1.0' encoding='utf-8'?>{nl}<UserLexiconSettings />");
			_illformedXmlTempFile = TempFile.WithExtension(".ulsx");
			File.WriteAllText(_illformedXmlTempFile.Path,
				$"<?xml version='1.0' encoding='utf-8'?>{nl}<UserLexiconSettings>");

			_goodXmlButNotUserLexiconSettingsTempFile = TempFile.WithExtension(".ulsx");
			File.WriteAllText(_goodXmlButNotUserLexiconSettingsTempFile.Path,
				$"<?xml version='1.0' encoding='utf-8'?>{nl}<nonUserLexiconSettingsstuff />");

			_nonXmlTempFile = TempFile.WithExtension(".txt");
			File.WriteAllText(_nonXmlTempFile.Path,
				$"This is not a user lexicon settings file.{nl}");
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_handler = null;
			_goodXmlTempFile.Dispose();
			_goodXmlTempFile = null;

			_illformedXmlTempFile.Dispose();
			_illformedXmlTempFile = null;

			_goodXmlButNotUserLexiconSettingsTempFile.Dispose();
			_goodXmlButNotUserLexiconSettingsTempFile = null;

			_nonXmlTempFile.Dispose();
			_nonXmlTempFile = null;
		}

		[Test]
		public void Cannot_Validate_Nonexistant_File()
		{
			Assert.That(_handler.CanValidateFile("bogusPathname"), Is.False);
		}

		[Test]
		public void Cannot_Validate_Null_File()
		{
			Assert.That(_handler.CanValidateFile(null), Is.False);
		}

		[Test]
		public void Cannot_Validate_Empty_String_File()
		{
			Assert.That(_handler.CanValidateFile(String.Empty), Is.False);
		}

		[Test]
		public void Cannot_Validate_Nonxml_File()
		{
			Assert.That(_handler.CanValidateFile(_nonXmlTempFile.Path), Is.False);
		}

		[Test]
		public void Can_Validate_Fw_Xml_File()
		{
			Assert.That(_handler.CanValidateFile(_goodXmlTempFile.Path), Is.True);
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Empty_Pathname()
		{
			Assert.That(_handler.ValidateFile("", null), Is.Not.Null);
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Null_Pathname()
		{
			Assert.That(_handler.ValidateFile(null, null), Is.Not.Null);
		}

		[Test]
		public void ValidateFile_Returns_Null_For_Good_File()
		{
			Assert.That(_handler.ValidateFile(_goodXmlTempFile.Path, null), Is.Null);
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Crummy_ProjectLexiconSettings_File()
		{
			Assert.That(_handler.ValidateFile(_illformedXmlTempFile.Path, null), Is.Not.Null);
		}

		[Test]
		public void ValidateFile_Returns_Message_For_Good_But_Not_ProjectLexiconSettings_File()
		{
			Assert.That(_handler.ValidateFile(_goodXmlButNotUserLexiconSettingsTempFile.Path, null), Is.Not.Null);
		}
	}
}
