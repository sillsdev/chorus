using System.Linq;
using Chorus.FileTypeHanders;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks.CustomProperties
{
	/// <summary>
	/// Test the FW custom property file handler.
	/// </summary>
	[TestFixture]
	public class FieldWorksCustomPropertyFileHandlerTests
	{
		private IChorusFileTypeHandler _fwCustomPropertiesFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fwCustomPropertiesFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
											  where handler.GetType().Name == "FieldeWorksCustomPropertyFileHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fwCustomPropertiesFileHandler = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fwCustomPropertiesFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = _fwCustomPropertiesFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("CustomProperties", extensions[0]);
		}

//        [Test]
//        public void AddNewCustomProperty()
//        {
//            const string commonAncestor =
//@"<?xml version='1.0' encoding='utf-8'?>
//<languageproject version='7000016'>
//<rt class='LexEntry' guid='original'/>
//</languageproject>";
//            const string ourContent =
//@"<?xml version='1.0' encoding='utf-8'?>
//<languageproject version='7000016'>
//<rt class='LexEntry' guid='original'/>
//</languageproject>";
//            const string theirContent =
//@"<?xml version='1.0' encoding='utf-8'?>
//<languageproject version='7000016'>
//<AdditionalFields>
//<CustomField name='Certified' class='WfiWordform' type='Boolean' />
//</AdditionalFields>
//<rt class='LexEntry' guid='original'/>
//</languageproject>";

//            DoMerge(commonAncestor, ourContent, theirContent,
//                new List<string> { @"languageproject/AdditionalFields", @"languageproject/AdditionalFields/CustomField[@name=""Certified""]" },
//                null,
//                0, 1);
//        }
	}
}
