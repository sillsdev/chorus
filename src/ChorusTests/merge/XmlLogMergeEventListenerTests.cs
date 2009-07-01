using System;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.retrieval;
using Chorus.Utilities;
using NUnit.Framework;

namespace Chorus.Tests.merge
{
	[TestFixture]
	public class XmlLogMergeEventListenerTests
	{
		[Test]
		public void FileAlreadyExists_AddsNewConflicts()
		{
			using (TempFile logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (XmlLogMergeEventListener log = new XmlLogMergeEventListener(logFile.Path))
				{
					log.ConflictOccurred(new DummyConflict());
					log.ConflictOccurred(new DummyConflict());
				}
				using (XmlLogMergeEventListener log2 = new XmlLogMergeEventListener(logFile.Path))
				{
					log2.ConflictOccurred(new DummyConflict());
					log2.ConflictOccurred(new DummyConflict());
				}
				XmlDocument doc = new XmlDocument();
				doc.Load(logFile.Path);
				Assert.AreEqual(4, doc.SelectNodes("conflicts/conflict").Count);
			}
		}

		[Test]
		public void FileDidNotExist_CreatesCorrectFile()
		{
			using (TempFile logFile =  TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using(XmlLogMergeEventListener log = new XmlLogMergeEventListener(logFile.Path))
				{
					log.ConflictOccurred(new DummyConflict());
					log.ConflictOccurred(new DummyConflict());
				}
				XmlDocument doc = new XmlDocument();
				doc.Load(logFile.Path);
				Assert.AreEqual(2, doc.SelectNodes("conflicts/conflict").Count);
			}
		}

		[Test]
		public void FileDidNotExist_NoConflicts_CreatesCorrectFile()
		{
			using (TempFile logFile = TempFile.CreateAndGetPathButDontMakeTheFile())
			{
				using (XmlLogMergeEventListener log = new XmlLogMergeEventListener(logFile.Path))
				{
				 }
				XmlDocument doc = new XmlDocument();
				doc.Load(logFile.Path);
				Assert.AreEqual(1, doc.SelectNodes("conflicts").Count);
			}
		}

	}


	public class DummyConflict: IConflict
	{
		private Guid _guid = Guid.NewGuid();


		public string GetFullHumanReadableDescription()
		{
			return "hello";
		}

		public string ConflictTypeHumanName
		{
			get { return "dummy"; }
		}

		public Guid Guid
		{
			get { return _guid; }
		}

		public string PathToUnitOfConflict
		{
			get; set;
		}

		public string GetConflictingRecordOutOfSourceControl(IRetrieveFile fileRetriever, ThreeWayMergeSources.Source mergeSource)
		{
			throw new System.NotImplementedException();
		}

	}
}