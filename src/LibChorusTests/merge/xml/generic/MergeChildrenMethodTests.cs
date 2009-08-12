using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// NB: the original TDD tests were done up on the XmlMerger (which uses the MergeMethod),
	/// (perhaps because the MergeChildrenMethod class was factored out to its own class later?) hence the paucity of tests here.
	/// </summary>
	[TestFixture]
	public class MergeChildrenMethodTests
	{
		[Test]
		public void Run_BothChangedSingletonNode_GetBothEdittedConflict()
		{
			string ours = @"<gloss lang='a'>
									<text>ourGloss</text>
							</gloss>";

			string theirs = @"<gloss lang='a'>
									<text>theirGloss</text>
								 </gloss>";
			string ancestor = @"<gloss lang='a'>
									<text>original</text>
								 </gloss>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			//without this stategy, we'd get an AmbiguousInsertConflict
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());

			TestMerge<BothEdittedTextConflict>(merger, ours, theirs, ancestor, "//gloss");
		}

		private void TestMerge<TConflictType>(XmlMerger merger, string ours, string theirs, string ancestors, string xpathToElementsToMerge)
		{
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			var ourNode = GetNode(ours, xpathToElementsToMerge);
			var method = new MergeChildrenMethod(ourNode,
												 GetNode(theirs,  xpathToElementsToMerge),
												 GetNode(ancestors,xpathToElementsToMerge),
												 merger);
			method.Run();
			 listener.AssertExpectedConflictCount(1);
		   var conflict = listener.Conflicts[0];
			Assert.AreEqual(typeof(TConflictType), conflict.GetType());
		}
		private void TestMergeWithoutConflicts(XmlMerger merger, string ours, string theirs, string ancestors, string xpathToElementsToMerge)
		{
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			var ourNode = GetNode(ours, xpathToElementsToMerge);
			var method = new MergeChildrenMethod(ourNode,
												 GetNode(theirs, xpathToElementsToMerge),
												 GetNode(ancestors, xpathToElementsToMerge),
												 merger);
			method.Run();
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Run_BothChangedKeyedNode_GetBothEdittedConflict()
		{
			string ours = @"<sense id='123'><gloss lang='a'>
									<text>ourGloss</text>
							</gloss></sense>";

			string theirs = @"<sense id='123'><gloss lang='a'>
									<text>theirGloss</text>
							</gloss></sense>";
			string ancestor = @"<sense id='123'><gloss lang='a'>
									<text>original</text>
							</gloss></sense>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			//without this stategy, we'd get an AmbiguousInsertConflict
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("sense", ElementStrategy.CreateForKeyedElement("id", true));
			merger.MergeStrategies.SetStrategy("gloss", ElementStrategy.CreateForKeyedElement("lang", true));

			TestMerge<BothEdittedTextConflict>(merger, ours, theirs, ancestor, "//sense");
		}
		[Test]
		public void Run_BothAddedDifferentKeyedNodes_OrderIrrelevant_NoConflict()
		{
			string ours = @"<foo><gloss lang='a'>
									<text>aGloss</text>
							</gloss></foo>";

			string theirs = @"<foo><gloss lang='b'>
									<text>bGloss</text>
							</gloss></foo>";
			string ancestor = @"<foo/>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			merger.MergeStrategies.SetStrategy("gloss", ElementStrategy.CreateForKeyedElement("lang", false));
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());

			TestMergeWithoutConflicts(merger, ours, theirs, ancestor, "//foo");
		}
		[Test]
		public void Run_BothAddedDifferentKeyedNodes_OrderIsRelevant_OrderAmbiguityConflict()
		{
			string ours = @"<sense id='123'><gloss lang='a'>
									<text>aGloss</text>
							</gloss></sense>";

			string theirs = @"<sense id='123'><gloss lang='b'>
									<text>bGloss</text>
							</gloss></sense>";
			string ancestor = @"<sense id='123'></sense>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			merger.MergeStrategies.SetStrategy("sense", ElementStrategy.CreateForKeyedElement("id", true));
			merger.MergeStrategies.SetStrategy("gloss", ElementStrategy.CreateForKeyedElement("lang", true));
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());

			TestMerge<AmbiguousInsertConflict>(merger, ours, theirs, ancestor, "//sense");
		}

		private XmlNode GetNode(string contents, string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(contents);
			return doc.SelectSingleNode(xpath);
		}
	}
}