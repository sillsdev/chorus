using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
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
		public void UsingWith_NumberOfChildrenAllowed_Zero_ThrowsWhenOursHasChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.Zero
			};
			var doc = new XmlDocument();
			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ourChild = doc.CreateNode(XmlNodeType.Element, "child", null);
			ours.AppendChild(ourChild);

			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_Zero_ThrowsWhenTheirsHasChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.Zero
			};
			var doc = new XmlDocument();
			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var theirChild = doc.CreateNode(XmlNodeType.Element, "child", null);
			theirs.AppendChild(theirChild);

			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_Zero_ThrowsWhenAncestorHasChildNode()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.Zero
			};
			var doc = new XmlDocument();
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ancestorChild = doc.CreateNode(XmlNodeType.Element, "child", null);
			ancestor.AppendChild(ancestorChild);

			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
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
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_ThrowsWhenOursHasMultipleChildNodes()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "childnode", null);
			ours.AppendChild(child);
			var child2 = doc.CreateNode(XmlNodeType.Element, "secondchildnode", null);
			ours.AppendChild(child2);

			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_ThrowsWhenTheirsHasMultipleChildNodes()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "childnode", null);
			theirs.AppendChild(child);
			var child2 = doc.CreateNode(XmlNodeType.Element, "secondchildnode", null);
			theirs.AppendChild(child2);

			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
		}

		[Test]
		public void UsingWith_NumberOfChildrenAllowed_ZeroOrOne_ThrowsWhenAncestorHasMultipleChildNodes()
		{
			var strategy = new ElementStrategy(false)
			{
				NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne
			};
			var doc = new XmlDocument();
			var ancestor = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var child = doc.CreateNode(XmlNodeType.Element, "childnode", null);
			ancestor.AppendChild(child);
			var child2 = doc.CreateNode(XmlNodeType.Element, "secondchildnode", null);
			ancestor.AppendChild(child2);

			var theirs = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			var ours = doc.CreateNode(XmlNodeType.Element, "somenode", null);

			Assert.Throws<InvalidOperationException>(() => MergeLimitedChildrenService.Run(new XmlMerger(new NullMergeSituation()), strategy, ref ours, theirs, ancestor));
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
		public void OneDeletedAndAdded_TheOtherEditedOriginal()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<LexemeForm>
			<Stem
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
				<form>original</form>
			</Stem>
		</LexemeForm>
	</LexEntry>
</Lexicon>";
			const string deleteAdd =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<LexemeForm>
			<Stem
				guid='76dbd845-915a-4cbd-886f-eebef34fa04e'>
				<form>form of new stem</form>
			</Stem>
		</LexemeForm>
	</LexEntry>
</Lexicon>";
			const string edit =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<LexemeForm>
			<Stem
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
				<form>update form of original stem</form>
			</Stem>
		</LexemeForm>
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
			merger.MergeStrategies.SetStrategy("LexemeForm", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("Stem", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("form", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, deleteAdd, edit,
								  new[]
									{
										"Lexicon/LexEntry/LexemeForm/Stem",
										"Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']",
										"Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']/form[text()='update form of original stem']"
									},
								  new[] { "Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd845-915a-4cbd-886f-eebef34fa04e']" },
								  1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
								  0, new List<Type>());

			// Now, go the other way on two users.
			listener = new ListenerForUnitTests();
			merger = new XmlMerger(new NullMergeSituation())
			{
				EventListener = listener
			};
			merger.MergeStrategies.SetStrategy("Lexicon", ElementStrategy.CreateSingletonElement());

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("LexEntry", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("LexemeForm", strat);

			strat = ElementStrategy.CreateForKeyedElement("guid", false);
			strat.AttributesToIgnoreForMerging.Add("guid");
			merger.MergeStrategies.SetStrategy("Stem", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("form", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, edit, deleteAdd,
								  new[]
									{
										"Lexicon/LexEntry/LexemeForm/Stem",
										"Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']",
										"Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd844-915a-4cbd-886f-eebef34fa04e']/form[text()='update form of original stem']"
									},
								  new[] { "Lexicon/LexEntry/LexemeForm/Stem[@guid='76dbd845-915a-4cbd-886f-eebef34fa04e']" },
								  1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
								  0, new List<Type>());
		}

		[Test]
		public void BothAddedDifferentAtomicOwnedElementToNewProperty()
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
								  1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
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

		[Test]
		public void BothAddedSameAtomicOwnedElementToNewProperty()
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
								  1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void BothAddedSameAtomicOwnedElementToExtantPropertyButWithDifferencesInTheInnards()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<ImportResidue />
	</LexEntry>
</Lexicon>";

			var ours = commonAncestor.Replace("<ImportResidue />", "<ImportResidue><Str><Run ws='en'>OurAddition</Run></Str></ImportResidue>");
			var theirs = commonAncestor.Replace("<ImportResidue />", "<ImportResidue><Str><Run ws='en'>TheirAddition</Run></Str></ImportResidue>");

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
			merger.MergeStrategies.SetStrategy("ImportResidue", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			strat.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("Str", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "Lexicon/LexEntry/ImportResidue/Str/Run[text()='OurAddition']" },
				new List<string> { @"Lexicon/LexEntry/ImportResidue/Str/Run[text()='TheirAddition']" },
								  1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
								  1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothAddedSameAtomicOwnedElementToNewPropertyButWithDifferencesInTheInnards()
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
				<Source>
					<Uni>our stuff</Uni>
				</Source>
			</LexEtymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
				<Source>
					<Uni>their stuff</Uni>
				</Source>
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

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("Source", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.Zero;
			merger.MergeStrategies.SetStrategy("Uni", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "Lexicon/LexEntry/Etymology/LexEtymology/Source/Uni[text()='our stuff']" },
								  null,
								  1, new List<Type> { typeof(XmlTextBothAddedTextConflict) },
								  3, new List<Type> { typeof(XmlBothAddedSameChangeReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void MoStemMsaHasMergeConflictOnPartOfSpeechAddedByBoth()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<MorphoSyntaxAnalyses>
			<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />
		</MorphoSyntaxAnalyses>
	</LexEntry>
</Lexicon>";

			var ours = commonAncestor.Replace("<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />", "<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675'><PartOfSpeech><objsur guid='c1ed94d4-e382-11de-8a39-0800200c9a66' t='r' /></PartOfSpeech></MoStemMsa>");
			var theirs = commonAncestor.Replace("<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675' />", "<MoStemMsa guid='2d6a109e-1178-4b31-bf5b-8dca5e843675'><PartOfSpeech><objsur guid='c1ed94d5-e382-11de-8a39-0800200c9a66' t='r' /></PartOfSpeech></MoStemMsa>");

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
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrMore;
			merger.MergeStrategies.SetStrategy("MoStemMsa", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("PartOfSpeech", strat); ;

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.Zero;
			strat.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("objsur", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d4-e382-11de-8a39-0800200c9a66']" },
				new List<string> { @"Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='c1ed94d5-e382-11de-8a39-0800200c9a66']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void TestFromFb()
		{
			const string commonAncestor =
@"<RnGenericRec
	guid='4a25547a-8ec7-4650-b53b-27c6f6d37fd4'>
	<TextProp>
		<Text
			guid='f123e367-df11-439a-9b7a-7adff0ba1efc'>
			<TextContents>
				<StText
					guid='f0392d21-e8eb-4c05-8bc7-783ce641223b'>
					<Paragraphs>
						<ownseqatomic
							class='StTxtPara'
							guid='d8c66454-63e4-4052-9fa9-eb1b1b0e4c6d'>
							<ParaContents>
								<Str>
									<Run
										ws='fr'>Ding, dong, la cloche.</Run>
								</Str>
							</ParaContents>
						</ownseqatomic>
					</Paragraphs>
				</StText>
			</TextContents>
		</Text>
	</TextProp>
</RnGenericRec>";

			const string ours =
@"<RnGenericRec
	guid='4a25547a-8ec7-4650-b53b-27c6f6d37fd4'>
	<TextProp>
		<Text
			guid='f123e367-df11-439a-9b7a-7adff0ba1efc'>
			<TextContents>
				<StText
					guid='f0392d21-e8eb-4c05-8bc7-783ce641223b'>
					<Paragraphs>
						<ownseqatomic
							class='StTxtPara'
							guid='378fe472-be32-43a9-a6e5-6212813050ae'>
							<ParaContents>
								<Str>
									<Run
										ws='fr'>new</Run>
								</Str>
							</ParaContents>
						</ownseqatomic>
					</Paragraphs>
				</StText>
			</TextContents>
		</Text>
	</TextProp>
</RnGenericRec>";

			const string theirs =
@"<RnGenericRec
	guid='4a25547a-8ec7-4650-b53b-27c6f6d37fd4'>
	<TextProp>
		<Text
			guid='f123e367-df11-439a-9b7a-7adff0ba1efc'>
			<TextContents>
				<StText
					guid='f0392d21-e8eb-4c05-8bc7-783ce641223b'>
					<Paragraphs />
				</StText>
			</TextContents>
		</Text>
	</TextProp>
</RnGenericRec>";

			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituationTheyWin())
			{
				EventListener = listener
			};
			var mergeStrat = merger.MergeStrategies;
			mergeStrat.SetStrategy("RnGenericRec", ElementStrategy.CreateForKeyedElement("guid", false));

			var elStrat = ElementStrategy.CreateSingletonElement();
			elStrat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			mergeStrat.SetStrategy("TextProp", elStrat);

			elStrat = ElementStrategy.CreateForKeyedElement("guid", false);
			mergeStrat.SetStrategy("Text", elStrat);

			elStrat = ElementStrategy.CreateSingletonElement();
			elStrat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			mergeStrat.SetStrategy("TextContents", elStrat);

			elStrat = ElementStrategy.CreateForKeyedElement("guid", false);
			mergeStrat.SetStrategy("StText", elStrat);

			elStrat = ElementStrategy.CreateSingletonElement();
			elStrat.OrderIsRelevant = true;
			mergeStrat.SetStrategy("Paragraphs", elStrat);

			elStrat = ElementStrategy.CreateForKeyedElement("guid", false);
			elStrat.IsAtomic = true;
			mergeStrat.SetStrategy("ownseqatomic", elStrat);

			elStrat = ElementStrategy.CreateSingletonElement();
			elStrat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			mergeStrat.SetStrategy("ParaContents", elStrat);

			elStrat = ElementStrategy.CreateSingletonElement();
			elStrat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			elStrat.IsAtomic = true;
			mergeStrat.SetStrategy("Str", elStrat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "RnGenericRec/TextProp/Text/TextContents/StText/Paragraphs/ownseqatomic[@guid='378fe472-be32-43a9-a6e5-6212813050ae']/ParaContents/Str/Run[text()='new']" },
								  null,
								  0, new List<Type>(),
								  2, new List<Type> {typeof(XmlBothDeletionChangeReport), typeof(XmlAdditionChangeReport)});
		}

		[Test]
		public void BothAddedSameAtomicOwnedElementToNewPropertyButWithDifferencesInTheInnards_TheyWin()
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
				<Source>
					<Uni>our stuff</Uni>
				</Source>
			</LexEtymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
				<Source>
					<Uni>their stuff</Uni>
				</Source>
			</LexEtymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituationTheyWin())
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

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.ZeroOrOne;
			merger.MergeStrategies.SetStrategy("Source", strat);

			strat = ElementStrategy.CreateSingletonElement();
			strat.NumberOfChildren = NumberOfChildrenAllowed.Zero;
			merger.MergeStrategies.SetStrategy("Uni", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies, merger.MergeSituation,
								  commonAncestor, ours, theirs,
								  new[] { "Lexicon/LexEntry/Etymology/LexEtymology/Source/Uni[text()='their stuff']" },
								  null,
								  1, new List<Type> { typeof(XmlTextBothAddedTextConflict) },
								  3, new List<Type> { typeof(XmlBothAddedSameChangeReport), typeof(XmlAttributeBothAddedReport), typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void BothDeletedSameAtomicOwnedElement()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
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
	</LexEntry>
</Lexicon>";

			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituationTheyWin())
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
								  null,
								  new[] { "Lexicon/LexEntry/Etymology" },
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlBothDeletionChangeReport) });
		}

		[Test]
		public void WeDeletedSameAtomicOwnedElement_TheyDidNothing()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
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
			var merger = new XmlMerger(new NullMergeSituationTheyWin())
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
								  null,
								  new[] { "Lexicon/LexEntry/Etymology" },
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void TheyDeletedSameAtomicOwnedElement_WeDidNothing()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
			<LexEtymology
				guid='76dbd844-915a-4cbd-886f-eebef34fa04e'>
			</LexEtymology>
		</Etymology>
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
			var merger = new XmlMerger(new NullMergeSituationTheyWin())
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
								  null,
								  new[] { "Lexicon/LexEntry/Etymology" },
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void BothAddedNewProperty_WeAddedAtomicValue_TheyDidNotAddAtomicValue()
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
								  2, new List<Type> { typeof(XmlBothAddedSameChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothAddedNewProperty_ButNothingElse()
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
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
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
								  new[] { "Lexicon/LexEntry/Etymology" },
								  null,
								  0, new List<Type>(),
								  1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void BothAddedNewProperty_TheyAddedAtomicValue_WeDidNotAddAtomicValue()
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
		</Etymology>
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
								  2, new List<Type> { typeof(XmlBothAddedSameChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void AllHadProperty_ButNothingElse()
		{
			const string commonAncestor =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string ours =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
		</Etymology>
	</LexEntry>
</Lexicon>";

			const string theirs =
@"<Lexicon>
	<LexEntry guid='c1ed94c5-e382-11de-8a39-0800200c9a66'>
		<Etymology>
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
								  new[] { "Lexicon/LexEntry/Etymology" },
								  null,
								  0, new List<Type>(),
								  0, new List<Type>());
		}
	}
}