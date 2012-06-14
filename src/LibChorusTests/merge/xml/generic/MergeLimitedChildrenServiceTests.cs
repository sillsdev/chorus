using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class MergeLimitedChildrenServiceTests
	{
		[Test]
		public void NullMergerThrows()
		{
			XmlNode node = null;
			Assert.Throws<ArgumentNullException>(() => MergeLimitedChildrenService.Run(null, new ElementStrategy(false), ref node, node, node));
		}

		[Test]
		public void NullStrategyThrows()
		{
			var doc = new XmlDocument();
			var node = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			Assert.Throws<ArgumentNullException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()),  null, ref node, node, node));
		}

		[Test]
		public void AllNullNodesThrows()
		{
			XmlNode node = null;
			Assert.Throws<ArgumentNullException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), new ElementStrategy(false), ref node, node, node));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrMore_Throws()
		{
			var strategy = new ElementStrategy(false)
							{
								NumberOfChildren = NumberOfChildrenAllowed.ZeroOrMore
							};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_Zero_ThrowsWhenParentHasChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.Zero
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "child", null);
			parent.AppendChild(child);
			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_Zero_DoesNotThrowWhenParentHasCommentChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.Zero
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var comment = doc.CreateNode(XmlNodeType.Comment, "Some comment.", null);
			parent.AppendChild(comment);
			Assert.DoesNotThrow(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_DoesNotThrowWhenParentHasCommentChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var comment = doc.CreateNode(XmlNodeType.Comment, "Some comment.", null);
			parent.AppendChild(comment);
			var comment2 = doc.CreateNode(XmlNodeType.Comment, "Some other comment.", null);
			parent.AppendChild(comment2);
			Assert.DoesNotThrow(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_DoesNotThrowWhenParentHasOneChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "childnode", null);
			parent.AppendChild(child);
			Assert.DoesNotThrow(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_ThrowsWhenParentHasMultipleChildNodes()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "childnode", null);
			parent.AppendChild(child);
			var child2 = doc.CreateNode(XmlNodeType.Element, "secondchildnode", null);
			parent.AppendChild(child2);
			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_DoesNotThrowWhenParentHasNoChildNodes()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var parent = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			Assert.DoesNotThrow(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref parent, parent, parent));
		}

		[Test]
		public void BothAddedAtomicOwnedElementToNewProperty()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
	</LexEntry>
</Lexicon>";

			const string ours =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='c909553a-aa91-4695-8fda-c708ec969a02'>
			</LexEtymology>
		</Etymology>
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
			merger.MergeStrategies.SetStrategy("Etymology", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("LexEtymology", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[]
									{
										"Lexicon/LexEntry/Etymology/LexEtymology",
										"Lexicon/LexEntry/Etymology/LexEtymology[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']"
									},
								  null,
								  1, new List<Type> {typeof (BothAddedMainElementButWithDifferentContentConflict)},
								  1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void WeAddedAtomicOwnedElementToNewProperty_TheyDidNothing()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
	</LexEntry>
</Lexicon>";

			const string ours =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
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
			merger.MergeStrategies.SetStrategy("Etymology", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("LexEtymology", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "Lexicon/LexEntry/Etymology/LexEtymology" },
								  null,
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void TheyAddedAtomicOwnedElementToNewProperty_WeDidNothing()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
	</LexEntry>
</Lexicon>";

			const string ours =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
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
			merger.MergeStrategies.SetStrategy("Etymology", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("LexEtymology", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "Lexicon/LexEntry/Etymology/LexEtymology" },
								  null,
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}
	}
}