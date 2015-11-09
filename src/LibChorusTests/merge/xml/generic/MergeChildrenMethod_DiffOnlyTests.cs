using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class MergeChildrenMethod_DiffOnlyTests
	{
		[Test]
		public void Run_WeAddedElement_ListenerGetsAdditionReport()
		{
			string ours = @"<a><b>new</b></a>";
			string ancestor = @"<a/>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());
			TestCompare<XmlTextAddedReport>(merger, ours, ancestor, "//a");
		}

		[Test]
		public void Run_WeEditedTextElementInsideSingleton_ListenerGetsTextEditReport()
		{
			string ours = @"<a><b>new</b></a>";
			string ancestor = @"<a><b>old</b></a>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("b", ElementStrategy.CreateSingletonElement());
			TestCompare<XmlTextChangedReport>(merger, ours, ancestor, "//a");
		}
		[Test]
		public void Run_WeEditedTextElementInsideKeyedElement_ListenerGetsTextEditReport()
		{
			// Gets report from MergeTextNodesMethod
			string ours = @"<a><b id='foo'>new</b>  </a>";
			string ancestor = @"<a><b id='foo'>old</b> </a>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("b", ElementStrategy.CreateForKeyedElement("id", false));
			TestCompare<XmlTextChangedReport>(merger, ours, ancestor, "//a");
		}

		[Test]
		public void Run_WeEditedTextElementInsideOneOfTWoKeyedElements_ListenerGetsTextEditReport()
		{
			// Gets report from MergeTextNodesMethod
			string ours = @"<a><b id='foo'>new</b>   <b id='gaa'>same</b></a>";
			string ancestor = @"<a><b id='foo'>old</b>   <b id='gaa'>same</b></a>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("b", ElementStrategy.CreateForKeyedElement("id", false));
			TestCompare<XmlTextChangedReport>(merger, ours, ancestor, "//a");
		}

		[Test]
		public void Run_WeDeletedElement_ListenerGetsDeletionEditReport()
		{
			string ancestor = @"<a><b/></a>";
			string ours = @"<a></a>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());
			TestCompare<XmlDeletionChangeReport>(merger, ours, ancestor, "//a");
		}

		private void TestCompare<TChangeReport>(XmlMerger merger, string ours, string ancestors, string xpathToElementsToMerge)
		{
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			var method = new MergeChildrenMethod(GetNode(ours, xpathToElementsToMerge),
												 GetNode(ancestors, xpathToElementsToMerge),
												 merger);
			method.Run();
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(1);
			Assert.AreEqual(typeof(TChangeReport), listener.Changes[0].GetType());

		}


		private XmlNode GetNode(string contents, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(contents);
			return doc.SelectSingleNode(xpath);
		}
	}
}
