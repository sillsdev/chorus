using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.retrieval;
using NUnit.Framework;
using Palaso.TestUtilities;
using TempFile=Chorus.Utilities.TempFile;

namespace Chorus.Tests.merge
{


   [TestFixture]
   public class ConflictXmlRecordRetrievalTests
	{
		[Test]
		public void GetRecord_GoodRecordInfo_ReturnsRecord()
		{
			var docA = new XmlDocument();
			docA.LoadXml(@"<doc><test id='2'>a</test></doc>");
			var docX = new XmlDocument();
			docX.LoadXml(@"<doc><test id='2'>x</test></doc>");
			var docY = new XmlDocument();
			docY.LoadXml(@"<doc><test id='2'>y</test></doc>");
			var situation = new MergeSituation("ARelativePath", "x", "x1", "y", "y1");
			var conflict = new BothEdittedTextConflict(docX.SelectSingleNode("doc/test"),
			  docY.SelectSingleNode("doc/test"),
			  docA.SelectSingleNode("doc/test"),
			  situation);
			conflict.Context = new ContextDescriptor("dummy","//test[@id='2']");
			var retriever = new DummyXmlRetriever(docA,docX, docY);
			var result = conflict.GetConflictingRecordOutOfSourceControl(retriever, ThreeWayMergeSources.Source.UserX);
			Assert.AreEqual("<test id=\"2\">x</test>", result);
		}
	}
   class DummyXmlRetriever : IRetrieveFile
   {
	   private readonly XmlDocument _docA;
	   private readonly XmlDocument _docX;
	   private readonly XmlDocument _docY;

	   public DummyXmlRetriever(XmlDocument docA, XmlDocument docX, XmlDocument docY)
	   {
		   _docA = docA;
		   _docX = docX;
		   _docY = docY;
	   }

	   public string RetrieveHistoricalVersionOfFile(string relativePath, string versionDescriptor)
	   {
		   switch (versionDescriptor )
		   {
			   case "x1":
				   return new TempFile(_docX.OuterXml).Path;
			   case "y1":
				   return new TempFile(_docY.OuterXml).Path;
			   case "a1":
				   return new TempFile(_docA.OuterXml).Path;
			   default:
				   return null;
		   }
	   }
   }

   class DummyRetriever : IRetrieveFile
   {
	   public string RetrieveHistoricalVersionOfFile(string relativePath, string versionDescriptor)
	   {
		   return new TempFile(new string[] { "one", "two", "three" }).Path;
	   }
   }
}
