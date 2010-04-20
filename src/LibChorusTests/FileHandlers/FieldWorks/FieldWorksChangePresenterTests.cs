using System;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the FieldWorks implementation of the IChangePresenter interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksChangePresenterTests
	{
		private IChorusFileTypeHandler m_fwFileHandler;
		private IChangePresenter m_changePresenter;
		private string m_goodXmlPathname;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_fwFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
							   where handler.GetType().Name == "FieldWorksFileHandler"
							   select handler).First();
			m_changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					null));
			m_goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
// ReSharper disable LocalizableElement
			File.WriteAllText(m_goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<languageproject version='7000016' />");
// ReSharper restore LocalizableElement
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			m_fwFileHandler = null;
			m_changePresenter = null;
			if (File.Exists(m_goodXmlPathname))
				File.Delete(m_goodXmlPathname);
		}

		[Test]
		public void Cannot_PresentFile_For_NonExtant_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanPresentFile("bogusPathname"));
		}

		[Test]
		public void Cannot_PresentFile_For_Null_Pathname()
		{
			Assert.IsFalse(m_fwFileHandler.CanPresentFile(null));
		}

		[Test]
		public void Cannot_PresentFile_For_Empty_String_Pathname()
		{
			Assert.IsFalse(m_fwFileHandler.CanPresentFile(string.Empty));
		}

		[Test]
		public void Cannot_PresentFile_For_Nonfw_Pathname()
		{
			Assert.IsFalse(m_fwFileHandler.CanPresentFile("bogus.txt"));
		}

		[Test]
		public void Can_Present_Good_Fw_Xml_File()
		{
			Assert.IsTrue(m_fwFileHandler.CanPresentFile(m_goodXmlPathname));
		}

		[Test]
		public void GetChangePresenter_Has_Three_Presentations()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9ba4a4-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a6-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a7-4a25-11df-9879-0800200c9a66' />
</languageproject>";
			// One deletion, one change, one insertion, and three reordered, but not changed.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='3d9ba4a1-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a0-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9b7d90-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66'/>
<rt guid='3d9ba4a6-4a25-11df-9879-0800200c9a66' ownerguid='3d9ba4a8-4a25-11df-9879-0800200c9a66' />
</languageproject>";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				const string stylsheet = @"<style type='text/css'><!--

BODY { font-family: verdana,arial,helvetica,sans-serif; font-size: 12px;}

span.langid {color: 'gray'; font-size: xx-small;position: relative;
	top: 0.3em;
}

span.fieldLabel {color: 'gray'; font-size: x-small;}

div.entry {color: 'blue';font-size: x-small;}

td {font-size: x-small;}

span.en {
color: 'green';
}
span.es {
color: 'green';
}
span.fr {
color: 'green';
}
span.tpi {
color: 'purple';
}

--></style>";
				repositorySetup.AddAndCheckinFile("fwtest.xml", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.xml", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				foreach (var report in m_fwFileHandler.Find2WayDifferences(
					hgRepository.GetFilesInRevision(allRevisions[0]).First(),
					hgRepository.GetFilesInRevision(allRevisions[1]).First(),
					hgRepository))
				{
					IChangePresenter presenter;
					string normalHtml;
					string rawHtml;
					switch (report.GetType().Name)
					{
						case "XmlDeletionChangeReport":
							presenter = m_fwFileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
						case "XmlChangedRecordReport":
							presenter = m_fwFileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
						case "XmlAdditionChangeReport":
							presenter = m_fwFileHandler.GetChangePresenter(report, hgRepository);
							normalHtml = presenter.GetHtml("normal", stylsheet);
							rawHtml = presenter.GetHtml("raw", stylsheet);
							Assert.AreEqual(normalHtml, rawHtml);
							break;
					}
				}
			}
		}

		[Test]
		public void GetDataLabel_Is_LexEntry()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("languageproject"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("LexEntry", changePresenter.GetDataLabel());
		}

		[Test]
		public void GetActionLabel_Is_Change()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("languageproject"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("Change", changePresenter.GetActionLabel());
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void GetHtml_Not_Implemented()
		{
			m_changePresenter.GetHtml(null, null);
		}

		[Test]
		public void GetTypeLabel_Is_FieldWorks_Data_Object()
		{
			var doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("languageproject"));
			var changePresenter = new FieldWorksChangePresenter(
				new XmlChangedRecordReport(
					null,
					null,
					null,
					XmlUtilities.GetDocumentNodeFromRawXml(@"<rt guid='3d9ba4a5-4a25-11df-9879-0800200c9a66' class='LexEntry'/>", doc)));
			Assert.AreEqual("FieldWorks data object", changePresenter.GetTypeLabel());
		}

		[Test]
		public void GetIconName_Is_File()
		{
			Assert.AreEqual("file", m_changePresenter.GetIconName());
		}
	}
}