using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.lift;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
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
		public void Run_BothDeletedNonTextNodeHasChangeReport()
		{
			const string ancestor = @"<gloss lang='a'>
									<text id='me' />
								 </gloss>";
			const string ours = @"<gloss lang='a'>
							</gloss>";
			const string theirs = ours;

			var merger = new XmlMerger(new NullMergeSituation());

			TestMergeWithChange<XmlBothDeletionChangeReport>(merger, ours, theirs, ancestor, "//gloss");
		}

		[Test]
		public void Run_WeRemoved_TheyEdited_TextNode_GetConflictReport()
		{
			string ancestor = @"<gloss lang='a'>
									<text>original</text>
								 </gloss>";
			string ours = @"<gloss lang='a'>
							</gloss>";

			string theirs = @"<gloss lang='a'>
									<text>theirGloss</text>
								 </gloss>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			//without this stategy, we'd get an AmbiguousInsertConflict
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());

			TestMerge<XmlTextRemovedVsEditConflict>(merger, ours, theirs, ancestor, "//gloss");
		}

		[Test]
		public void Run_BothChangedSingletonNode_GetBothEditedConflict()
		{
			string ancestor = @"<gloss lang='a'>
									<text>original</text>
								 </gloss>";
			string ours = @"<gloss lang='a'>
									<text>ourGloss</text>
							</gloss>";

			string theirs = @"<gloss lang='a'>
									<text>theirGloss</text>
								 </gloss>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			//without this stategy, we'd get an AmbiguousInsertConflict
			merger.MergeStrategies.SetStrategy("text", ElementStrategy.CreateSingletonElement());

			TestMerge<XmlTextBothEditedTextConflict>(merger, ours, theirs, ancestor, "//gloss");
		}

		private void TestMergeWithChange<TChangeType>(XmlMerger merger, string ours, string theirs, string ancestors, string xpathToElementsToMerge)
		{
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			var ourNode = GetNode(ours, xpathToElementsToMerge);
			var method = new MergeChildrenMethod(ourNode,
												 GetNode(theirs, xpathToElementsToMerge),
												 GetNode(ancestors, xpathToElementsToMerge),
												 merger);
			method.Run();
			listener.AssertExpectedChangesCount(1);
			var change = listener.Changes[0];
			Assert.AreEqual(typeof(TChangeType), change.GetType());
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
		public void Run_BothChangedKeyedNode_GetBothEditedConflict()
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

			TestMerge<XmlTextBothEditedTextConflict>(merger, ours, theirs, ancestor, "//sense");
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
		public void Run_BothMovedDifferentKeyedNodes_OrderIrrelevant_NoDuplicatesCreated()
		{
			string ours = @"<foo><gloss lang='c'/><gloss lang='b'/><gloss lang='a'/></foo>";

			string theirs = @"<foo><gloss lang='a'/><gloss lang='c'/><gloss lang='b'/></foo>";
			string ancestor = @"<foo><gloss lang='a'/><gloss lang='b'/><gloss lang='c'/></foo>";

			XmlMerger merger = new XmlMerger(new NullMergeSituation());

			merger.MergeStrategies.SetStrategy("foo", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("gloss", ElementStrategy.CreateForKeyedElement("lang", false));

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

		/// <summary>
		/// Test for LT-13794.
		/// </summary>
		[Test]
		public void DuplicateRelationElementsDoesNotThrow()
		{
			const string ours =
@"<sense id='ours'>
<grammatical-info value='Noun' />
<gloss lang='en' />
<gloss lang='tpi' />
<definition />
<note type='encyclopedic' />
<note>
<form lang='en'>
<text>myform</text>
</form>
</note>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid3' order='2'/>
<relation type='P' ref='dupid2' order='1'/>
<relation type='P' ref='dupid4' order='2'/>
<relation type='P' ref='dupid5' order='3'/>
<relation type='P' ref='dupid1' order='4'/>
<relation type='P' ref='dupid3' order='5'/>
<relation type='C' ref='nondupdupid1' order='1'/>
<relation type='C' ref='dupid2' order='2'/>
<relation type='C' ref='dupid4' order='3'/>
<relation type='C' ref='dupid5' order='4'/>
<relation type='C' ref='dupid1' order='5'/>
<relation type='P' ref='dupid2' order='1'/>
<relation type='P' ref='dupid4' order='2'/>
<relation type='P' ref='dupid5' order='3'/>
<relation type='P' ref='dupid1' order='4'/>
<relation type='P' ref='nondupid2' order='5'/>
</sense>";
			const string theirs =
@"<sense id='theirs'>
<grammatical-info value='Noun' />
<gloss lang='en' />
<gloss lang='tpi' />
<definition />
<note type='encyclopedic' />
<note>
<form lang='en'>
<text>myform</text>
</form>
</note>
<relation type='C' ref='nondupdupid1' order='1'/>
<relation type='C' ref='dupid2' order='2'/>
<relation type='C' ref='dupid4' order='3'/>
<relation type='C' ref='dupid5' order='4'/>
<relation type='C' ref='dupid1' order='5'/>
<relation type='P' ref='dupid2' order='1'/>
<relation type='P' ref='dupid4' order='2'/>
<relation type='P' ref='dupid5' order='3'/>
<relation type='P' ref='dupid1' order='4'/>
<relation type='P' ref='nondupid2' order='5'/>
</sense>";
			const string ancestors =
@"<sense id='common'>
<grammatical-info value='Noun' />
<gloss lang='en' />
<gloss lang='tpi' />
<definition />
<note type='encyclopedic' />
<note>
<form lang='en'>
<text>myform</text>
</form>
</note>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid3' order='2'/>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid3' order='2'/>
<relation type='P' ref='dupid1' order='1'/>
<relation type='P' ref='dupid3' order='1'/>
<relation type='P' ref='dupid1' order='2'/>
<relation type='C' ref='nondupdupid1' order='1'/>
<relation type='C' ref='dupid2' order='2'/>
<relation type='C' ref='dupid4' order='3'/>
<relation type='C' ref='dupid5' order='4'/>
<relation type='C' ref='dupid1' order='5'/>
<relation type='P' ref='dupid2' order='1'/>
<relation type='P' ref='dupid4' order='2'/>
<relation type='P' ref='dupid5' order='3'/>
<relation type='P' ref='dupid1' order='4'/>
<relation type='P' ref='dupid3' order='5'/>
</sense>";

			var merger = new XmlMerger(new NullMergeSituation());
			LiftElementStrategiesMethod.AddLiftElementStrategies(merger.MergeStrategies);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var xpathToElementsToMerge = "//sense";

			var ourNode = GetNode(ours, xpathToElementsToMerge);
			var method = new MergeChildrenMethod(ourNode,
												 GetNode(theirs, xpathToElementsToMerge),
												 GetNode(ancestors, xpathToElementsToMerge),
												 merger);
			method.Run();
			// This is such a mess that the desirable outcome is not at all obvious. We created this test because
			// at one point data like this was producing crashes. It remains valuable as a torture test, but
			// rather than locking in some exact outcome, let's just check a few basic expectations.
			Assert.That(listener.Conflicts, Has.Count.GreaterThan(0)); // should find SOME problems!

			// Common has six nodes with type='P' ref='dupid1', which are supposed to be the key attributes.
			// theirs has only one, while ours has three. Seems pretty clear that at least one and not more than three should survive.
			Assert.That(CountNodesWithKeys("relation", "P", "dupid1", ourNode.ChildNodes), Is.GreaterThan(0).And.LessThanOrEqualTo(3));
			// Common has four nodes with type='P' an ref='dupid3'. Theirs has none, while ours has two. One of the survivors
			// has been "modified" (different order) so that at least should survive.
			Assert.That(CountNodesWithKeys("relation", "P", "dupid3", ourNode.ChildNodes), Is.GreaterThan(0).And.LessThanOrEqualTo(2));
			// There is exactly one node with type='C' ref='nondupdupid1'. It should certainly survive.
			Assert.That(CountNodesWithKeys("relation", "C", "nondupdupid1", ourNode.ChildNodes), Is.EqualTo(1));
			// Likewise for the pair (C, dupid2), (C, dupid4), (C, dupid5), and (C, dupid1)
			Assert.That(CountNodesWithKeys("relation", "C", "dupid2", ourNode.ChildNodes), Is.EqualTo(1));
			Assert.That(CountNodesWithKeys("relation", "C", "dupid4", ourNode.ChildNodes), Is.EqualTo(1));
			Assert.That(CountNodesWithKeys("relation", "C", "dupid5", ourNode.ChildNodes), Is.EqualTo(1));
			Assert.That(CountNodesWithKeys("relation", "C", "dupid1", ourNode.ChildNodes), Is.EqualTo(1));
			// Started with one (P, dupid4), which survived in theirs; ours has two. Similarly with (P, dupid5). Two should survive.
			Assert.That(CountNodesWithKeys("relation", "P", "dupid4", ourNode.ChildNodes), Is.EqualTo(2));
			Assert.That(CountNodesWithKeys("relation", "P", "dupid5", ourNode.ChildNodes), Is.EqualTo(2));
		}

		[Test]
		public void DeleteNonAtomicElementVsModifyHasConflict()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa
				guid='33adabe9-a02e-42cb-b942-277a7be5c841'>
				<PartOfSpeech>
					<objsur
						guid='e72dbc59-e93f-4df2-b6bd-39a53e331201'
						t='r' />
				</PartOfSpeech>
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
		<Senses/>
	</LexEntry>
</Lexicon>";
			const string matthew =
@"<Lexicon>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa
				guid='33adabe9-a02e-42cb-b942-277a7be5c841'>
				<PartOfSpeech>
					<objsur
						guid='f92dbc59-e93f-4df2-b6bd-39a53e331201'
						t='r' />
				</PartOfSpeech>
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
		<Senses/>
	</LexEntry>
</Lexicon>";
			const string lee =
@"<Lexicon>
	<LexEntry guid='ffdc58c9-5cc3-469f-9118-9f18c0138d02'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa
				guid='33adabe9-a02e-42cb-b942-277a7be5c841'>
				<PartOfSpeech />
			</MoStemMsa>
		</MorphoSyntaxAnalyses>
		<Senses/>
	</LexEntry>
</Lexicon>";

			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituation())
			{
				EventListener = listener
			};
			merger.MergeStrategies.SetStrategy("Lexicon", ElementStrategy.CreateSingletonElement());

			var strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("LexEntry", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("MorphoSyntaxAnalyses", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("MoStemMsa", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("PartOfSpeech", strat);

			strat = ElementStrategy.CreateSingletonElement();
			merger.MergeStrategies.SetStrategy("objsur", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies,
				merger.MergeSituation,
				commonAncestor, lee, matthew,
				new[] { "Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='f92dbc59-e93f-4df2-b6bd-39a53e331201']" },
				null,
				1, new List<Type> {typeof(RemovedVsEditedElementConflict)},
				0, new List<Type>());
		}

		int CountNodesWithKeys(string name, string typeKey, string refKey, XmlNodeList nodes)
		{
			int result = 0;
			foreach (XmlNode node in nodes)
			{
				if (node.Name != name)
					continue;
				if (XmlUtilities.GetOptionalAttributeString(node, "type") != typeKey)
					continue;
				if (XmlUtilities.GetOptionalAttributeString(node, "ref") != refKey)
					continue;
				result++;
			}
			return result;
		}
	}
}