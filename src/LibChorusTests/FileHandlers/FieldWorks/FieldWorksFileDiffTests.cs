using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.FileTypeHanders.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the FieldWorksFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileDiffTests
	{
		private IChorusFileTypeHandler m_fwFileHandler;
		private string m_goodXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_fwFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
							   where handler.GetType().Name == "FieldWorksFileHandler"
							   select handler).First();
			m_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
			File.WriteAllText(m_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<languageproject version='7000016' />");
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			m_fwFileHandler = null;
			if (File.Exists(m_goodXmlPathname))
				File.Delete(m_goodXmlPathname);
		}

		[Test]
		public void Cannot_Diff_Nonexistant_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanDiffFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Diff_Null_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanDiffFile(null));
		}

		[Test]
		public void Cannot_Diff_Empty_String_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanDiffFile(String.Empty));
		}

		[Test]
		public void Can_Diff_Good_Fw_Xml_File()
		{
			Assert.IsTrue(m_fwFileHandler.CanDiffFile(m_goodXmlPathname));
		}

		// This test is of a series that test:
		//	1. No changes (Is this needed? If Hg can commit unchanged files, probably.)
		//	DONE: 2. <rt> element added to child.
		//	DONE: 3. <rt> element removed from child.
		//	DONE: 4. <rt> element changed in child.
		//	5. Custom field stuff.
		//	6. Model version same or different. (Child is lower number?)
		[Test]
		public void NewObjectInChild_Reported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
</languageproject>";
			// Third <rt> element (3d9ba4a1-4a25-11df-9879-0800200c9a66) is new.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'/>
</languageproject>";
			var listener = new ListenerForUnitTests();
			var differ = FieldWorks2WayDiffer.CreateFromStrings(parent, child, listener);
			differ.ReportDifferencesToListener();
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void ObjectDeletedInChild_Reported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'/>
</languageproject>";
			// Third <rt> element (3d9ba4a1-4a25-11df-9879-0800200c9a66) in parent is removed in child.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
</languageproject>";
			var listener = new ListenerForUnitTests();
			var differ = FieldWorks2WayDiffer.CreateFromStrings(parent, child, listener);
			differ.ReportDifferencesToListener();
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void ObjectChangedInChild_Reported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a2-4a25-11df-9879-0800200c9a66' />
</languageproject>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a3-4a25-11df-9879-0800200c9a66' />
</languageproject>";
			var listener = new ListenerForUnitTests();
			var differ = FieldWorks2WayDiffer.CreateFromStrings(parent, child, listener);
			differ.ReportDifferencesToListener();
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlChangedRecordReport>();
		}
	}
}