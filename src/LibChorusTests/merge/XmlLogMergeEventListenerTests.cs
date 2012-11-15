using System;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Xml;

namespace LibChorus.Tests.merge
{
	[TestFixture]
	public class XmlLogMergeEventListenerTests
	{
		[Test]
		public void FileAlreadyExists_AddsNewConflicts()
		{
			using (TempFile logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (ChorusNotesMergeEventListener log = new ChorusNotesMergeEventListener(logFile.Path))
				{
					log.ConflictOccurred(new DummyConflict());
					log.ConflictOccurred(new DummyConflict());
				}
				using (ChorusNotesMergeEventListener log2 = new ChorusNotesMergeEventListener(logFile.Path))
				{
					log2.ConflictOccurred(new DummyConflict());
					log2.ConflictOccurred(new DummyConflict());
				}
				XmlDocument doc = new XmlDocument();
				doc.Load(logFile.Path);
				Assert.AreEqual(4, doc.SafeSelectNodes("notes/annotation").Count);
			}
		}

		[Test]
		public void FileOutput_WithContent_UsesCanonicalXmlSettings()
		{
			string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
				+ "<notes\r\n"
				+ "\tversion=\"0\">\r\n"
				+ "\t<annotation>Dummy</annotation>\r\n"
				+ "</notes>";
			using (var logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (var log = new ChorusNotesMergeEventListener(logFile.Path))
				{
					log.ConflictOccurred(new DummyConflict());
				}
				string result = File.ReadAllText(logFile.Path);
				Assert.AreEqual(expected, result);
			}
		}

		//[Test] The new implementation keeps a stream open, and empty files are removed, I don't think this can be tested anymore,
		// also I don't think it is relevant since an empty file will never be written out.
		//public void FileOutput_DefaultFile_UsesCanonicalXmlSettings()
		//{
		//    string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
		//                      + "<notes\r\n"
		//                      + "\tversion=\"0\" />";
		//    using (var logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
		//    {
		//        using (new ChorusNotesMergeEventListener(logFile.Path))
		//        {
		//            string result = File.ReadAllText(logFile.Path);
		//            Assert.AreEqual(expected, result);
		//        }
		//    }
		//}

		[Test]
		public void FileDidNotExist_CreatesCorrectFile()
		{
			using (TempFile logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (ChorusNotesMergeEventListener log = new ChorusNotesMergeEventListener(logFile.Path))
				{
					log.ConflictOccurred(new DummyConflict());
					log.ConflictOccurred(new DummyConflict());
				}
				XmlDocument doc = new XmlDocument();
				doc.Load(logFile.Path);
				Assert.AreEqual(2, doc.SelectNodes("notes/annotation").Count);
			}
		}

		[Test]
		public void FileDidNotExist_NoConflicts_DoesNotCreatesEmptyNotesFile()
		{
			var tempFile = Path.GetTempFileName();
			File.Delete(tempFile);
			using (ChorusNotesMergeEventListener log = new ChorusNotesMergeEventListener(tempFile))
			{
			}
			Assert.IsFalse(File.Exists(tempFile));
		}

	}

	[TypeGuid("18C7E1A2-2F69-442F-9057-6B3AC9833675")]
	public class DummyConflict: IConflict
	{
		private Guid _guid = Guid.NewGuid();

		public string RelativeFilePath { get; set; }

		public ContextDescriptor Context
		{
			get { return null; }
			set { ; }
		}

		public string GetFullHumanReadableDescription()
		{
			return "hello";
		}

		public string Description
		{
			get { return "dummy"; }
		}

		public string HtmlDetails
		{
			get { return "<body>dummy</body>"; }
		}

		public void MakeHtmlDetails(XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlMaker)
		{

		}

		public string WinnerId
		{
			get { return null; }
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public MergeSituation Situation
		{
			get { return new NullMergeSituation(); }
			set { throw new NotImplementedException(); }
		}

		public string PathToUnitOfConflict
		{
			get; set;
		}

		public string RevisionWhereMergeWasCheckedIn
		{
			get { return string.Empty; }
		}

		public string GetConflictingRecordOutOfSourceControl(IRetrieveFileVersionsFromRepository fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new System.NotImplementedException();
		}

		public void WriteAsChorusNotesAnnotation(XmlWriter writer)
		{
			writer.WriteElementString("annotation", string.Empty, "Dummy");
		}
	}
}