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
	/// <summary>
	/// Test the 'IsAtomic' (ElementStrategy) merge capabilities in the MergeAtomicElementService class.
	///
	/// These tests are indirect, in that they go through the XmlMerge class,
	/// which calls the 'Run' method of MergeAtomicElementService.
	/// </summary>
	[TestFixture]
	public class MergeAtomicElementServiceTests
	{
		#region private methods

		private static void RunService(string common, string ours, string theirs,
			MergeSituation mergeSituation,
			IEnumerable<string> xpathQueriesThatMatchExactlyOneNode,
			IEnumerable<string> xpathQueriesThatReturnNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			XmlNode ancestorNode;
			ListenerForUnitTests listener;
			XmlNode result = RunServiceCore(common, ours, theirs, mergeSituation, out ancestorNode, out listener);

			var results = result == null ? ancestorNode.OuterXml : result.OuterXml;

			XmlTestHelper.CheckMergeResults(results, listener,
				xpathQueriesThatMatchExactlyOneNode,
				xpathQueriesThatReturnNull,
				expectedConflictCount, expectedConflictTypes,
				expectedChangesCount, expectedChangeTypes);
		}

		/// <summary>
		/// Runs the service for a test case where we expect a successful deletion.
		/// Deletions only happen where no conflicts are expected, so we verify that.
		/// Currently we generate no change reports either.
		/// There might plausibly be a change report generated, probably XmlDeletionChangeReport,
		/// but we are in the process (at least Randy has started) phasing out change reports, so I didn't
		/// add one for this case.
		/// </summary>
		/// <param name="common"></param>
		/// <param name="ours"></param>
		/// <param name="theirs"></param>
		/// <param name="mergeSituation"></param>
		private static void RunServiceExpectingDeletion(string common, string ours, string theirs,
			MergeSituation mergeSituation)
		{
			XmlNode ancestorNode;
			ListenerForUnitTests listener;
			XmlNode result = RunServiceCore(common, ours, theirs, mergeSituation, out ancestorNode, out listener);

			Assert.That(result, Is.Null, "Deletion merge failed to delete");
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		private static XmlNode RunServiceCore(string common, string ours, string theirs,
			MergeSituation mergeSituation, out XmlNode returnAncestorNode, out ListenerForUnitTests listener)
		{
			XmlNode ourNode;
			XmlNode ourParent;
			XmlNode theirNode;
			XmlNode ancestorNode;
			XmlTestHelper.CreateThreeNodes(ours, theirs, common, out ourNode, out ourParent, out theirNode, out ancestorNode);
			returnAncestorNode = ancestorNode;

			var merger = GetMerger(mergeSituation, out listener);
			Assert.DoesNotThrow(() => MergeAtomicElementService.Run(merger, ourParent, ref ourNode, theirNode, ancestorNode));

			return ourNode;
		}

		private static void CreateThreeNodes(XmlDocument doc, XmlNode rootNode,
			out XmlNode first, string firstAttrName, string firstAttrValue,
			out XmlNode second, string secondAttrName, string secondAttrValue,
			out XmlNode third, string thirdAttrName, string thirdAttrValue)
		{
			CreateTwoNodes(doc, rootNode,
						   out first, firstAttrName, firstAttrValue,
						   out second, secondAttrName, secondAttrValue);
			third = CreateOneNode(doc, rootNode, thirdAttrName, thirdAttrValue);
		}

		private static void CreateTwoNodes(XmlDocument doc, XmlNode rootNode,
			out XmlNode first, string firstAttrName, string firstAttrValue,
			out XmlNode second, string secondAttrName, string secondAttrValue)
		{
			first = CreateOneNode(doc, rootNode, firstAttrName, firstAttrValue);
			second = CreateOneNode(doc, rootNode, secondAttrName, secondAttrValue);
		}

		private static XmlNode CreateOneNode(XmlDocument doc, XmlNode rootNode, string attrName, string attrValue)
		{
			var newElement = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(newElement);
			var oursAttr = doc.CreateAttribute(attrName);
			oursAttr.Value = attrValue;
			newElement.Attributes.Append(oursAttr);
			return newElement;
		}

		private static XmlDocument GetDocument(out XmlNode rootNode)
		{
			var doc = new XmlDocument();
			rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);
			return doc;
		}

		private static XmlMerger GetMerger(MergeSituation mergeSituation, out ListenerForUnitTests listener)
		{
			var elementStrategy = new ElementStrategy(false)
			{
				IsAtomic = true
			};
			var merger = new XmlMerger(mergeSituation);
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			return merger;
		}

		private static XmlMerger GetMerger(out ListenerForUnitTests listener, bool isAtomic)
		{
			var elementStrategy = new ElementStrategy(false)
				{
					IsAtomic = isAtomic
				};
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			return merger;
		}

		#endregion private methods

		#region Basic tests

		[Test]
		public void DefaultIsFalse()
		{
			var elementStrategy = new ElementStrategy(false);
			Assert.IsFalse(elementStrategy.IsAtomic);
		}

		[Test]
		public void CanSetToTrue()
		{
			var elementStrategy = new ElementStrategy(false)
				{
					IsAtomic = true
				};
			Assert.IsTrue(elementStrategy.IsAtomic);
		}

		[Test]
		public void NullMergerThrows()
		{
			var doc = new XmlDocument();
			var node = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			Assert.Throws<ArgumentNullException>(() => MergeAtomicElementService.Run(null, node.ParentNode, ref node, node, node));
		}

		[Test]
		public void AllNullNodesThrows()
		{
			XmlNode node = null;
			Assert.Throws<ArgumentNullException>(() => MergeAtomicElementService.Run(new XmlMerger(new NullMergeSituation()), null, ref node, node, node));
		}

		[Test]
		public void NotAtomicStrategyReturnsFalse()
		{
			XmlNode root;
			var doc = GetDocument(out root);
			XmlNode ourNode;
			XmlNode theirNode;
			XmlNode ancestorNode;
			CreateThreeNodes(doc, root,
							 out ancestorNode, "originalAttr", "originalValue",
							 out ourNode, "originalAttr", "newValue",
							 out theirNode, "originalAttr", "originalValue");

			ListenerForUnitTests listener;
			var merger = GetMerger(out listener, false);
			Assert.Throws<InvalidOperationException>(() => MergeAtomicElementService.Run(merger, ourNode.ParentNode, ref ourNode, theirNode, ancestorNode));
		}

		#endregion Basic tests

		#region Conflicts produced

		[Test]
		public void BothEditedButDifferentlyAtomicElementWithConflict()
		{
			const string ours = @"<topatomic originalAttr='originalValue' newAttr='newValue' />";
			const string theirs = @"<topatomic originalAttr='originalValue' thirdAttr='thirdValue' />";
			const string common = @"<topatomic originalAttr='originalValue' />";

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='originalValue']",  "topatomic[@newAttr='newValue']" }, new [] {"topatomic[@thirdAttr='thirdValue']"},
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, null);

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='originalValue']", "topatomic[@thirdAttr='thirdValue']" }, new[] { "topatomic[@newAttr='newValue']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, null);
		}

		[Test]
		public void OneDeletedOtherDidNothingAtomicElement_DeletesWithNoConflict()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";

			RunServiceExpectingDeletion(common, null, common,
				new NullMergeSituation());
			RunServiceExpectingDeletion(common, common, null,
				new NullMergeSituation());
		}

		[Test]
		public void WeDeletedTheyEditedRegardlessOfMergeSituationHasConflict()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string theirs = @"<topatomic originalAttr='newValue' />";

			RunService(common, null, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='newValue']" }, new[] { "topatomic[@originalAttr='originalValue']" },
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, null);

			RunService(common, null, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='newValue']" }, new[] { "topatomic[@originalAttr='originalValue']" },
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, null);
		}

		[Test]
		public void TheyDeletedWeEditedRegardlessOfMergeSituationHasConflict()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='newValue' />";

			RunService(common, ours, null,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='newValue']" }, new [] { "topatomic[@originalAttr='originalValue']" },
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);

			RunService(common, ours, null,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='newValue']" }, new[] { "topatomic[@originalAttr='originalValue']" },
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);
		}

		[Test]
		public void BothAddedNewConflictingStuffHasConflictReport()
		{
			//const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic newAttr='ourNewValue' />";
			const string theirs = @"<topatomic newAttr='theirNewValue' />";

			RunService(null, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@newAttr='ourNewValue']" }, new[] { "topatomic[@newAttr='theirNewValue']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, null);

			RunService(null, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@newAttr='theirNewValue']" }, new[] { "topatomic[@newAttr='ourNewValue']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, null);
		}

		[Test]
		public void DeleteAtomicElementVsModifyHasConflict()
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
			strat.IsAtomic = true;
			merger.MergeStrategies.SetStrategy("objsur", strat);

			XmlTestHelper.DoMerge(merger.MergeStrategies,
				merger.MergeSituation,
				commonAncestor, lee, matthew,
				new[] { "Lexicon/LexEntry/MorphoSyntaxAnalyses/MoStemMsa/PartOfSpeech/objsur[@guid='f92dbc59-e93f-4df2-b6bd-39a53e331201']" },
				null,
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, new List<Type>());
		}

		#endregion Conflicts produced

		#region Change reports

		[Test]
		public void NobodyMadeChangesWithContent()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = common;
			const string theirs = common;

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='originalValue']" }, null,
				0, null,
				0, null);

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='originalValue']" }, null,
				0, null,
				0, null);
		}

		[Test]
		public void TheyAddedStuffHasChangeReport()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = common;
			const string theirs = @"<topatomic originalAttr='originalValue' theirAttr='theirValue' />";

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='originalValue']", "topatomic[@theirAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='originalValue']", "topatomic[@theirAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void WeAddedStuffHasChangeReport()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='originalValue' ourAttr='ourValue' />";
			const string theirs = common;

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='originalValue']", "topatomic[@ourAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='originalValue']", "topatomic[@ourAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void WeAddedNodeHasChangeReport()
		{
			const string ours = @"<topatomic ourAttr='ourValue' />";

			RunService(null, ours, null,
				new NullMergeSituation(),
				new[] { "topatomic[@ourAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });

			RunService(null, ours, null,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@ourAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void TheyAddedNodeHasChangeReport()
		{
			const string theirs = @"<topatomic theirAttr='theirValue' />";

			RunService(null, null, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@theirAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });

			RunService(null, null, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@theirAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothAddedTheSameThingHasChangeReport()
		{
			const string ours = @"<topatomic bothAttr='bothValue' />";
			const string theirs = ours;

			RunService(null, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@bothAttr='bothValue']" }, null,
				0, null,
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });

			RunService(null, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@bothAttr='bothValue']" }, null,
				0, null,
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });
		}

		[Test]
		public void BothMadeSameChangeHasChangeReport()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='newValue' />";
			const string theirs = ours;

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='newValue']" }, null,
				0, null,
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='newValue']" }, null,
				0, null,
				1, new List<Type> { typeof(BothChangedAtomicElementReport) });
		}

		[Test]
		public void WeEditedTheyDidNothingHasChangeReport()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='ourValue' />";
			const string theirs = common;

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='ourValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void WeDidNothingTheyEditedTheyWinNoConflict()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = common;
			const string theirs =  @"<topatomic originalAttr='theirValue' />";

			RunService(common, ours, theirs,
				new NullMergeSituation(),
				new[] { "topatomic[@originalAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });

			RunService(common, ours, theirs,
				new NullMergeSituationTheyWin(),
				new[] { "topatomic[@originalAttr='theirValue']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlChangedRecordReport) });
		}

		[Test]
		public void BothDeletedNode()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";

			RunService(common, null, null,
				new NullMergeSituation(),
				null, new[] { "topatomic[@originalAttr='theirValue']" },
				0, null,
				1, new List<Type> { typeof(XmlBothDeletionChangeReport) });

			RunService(common, null, null,
				new NullMergeSituationTheyWin(),
				null, new[] { "topatomic[@originalAttr='theirValue']" },
				0, null,
				1, new List<Type> { typeof(XmlBothDeletionChangeReport) });
		}

		#endregion Change reports
	}
}
