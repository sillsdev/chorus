using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
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
		[Test]
		public void DefaultIsFalse()
		{
			var elementStrategy = new ElementStrategy(false);
			Assert.IsFalse(elementStrategy.IsAtomic);
		}

		[Test]
		public void CanSetToTrue()
		{
			var elementStrategy = new ElementStrategy(false) {IsAtomic = true};
			Assert.IsTrue(elementStrategy.IsAtomic);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void NullMergerThrows()
		{
			var doc = new XmlDocument();
			var node = doc.CreateNode(XmlNodeType.Element, "somenode", null);
			MergeAtomicElementService.Run(null, ref node, node, node);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AllNullNodesThrows()
		{
			XmlNode node = null;
			MergeAtomicElementService.Run(new XmlMerger(new NullMergeSituation()), ref node, node, node);
		}

		[Test]
		public void TopLevelAtomicElementNoConflictsWithIsAtomicBeingFalse()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='originalValue' newAttr='newValue' />";
			const string theirs = @"<topatomic originalAttr='originalValue' thirdAttr='thirdValue' />";

			var elementStrategy = new ElementStrategy(false) {IsAtomic = false};
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var results = merger.Merge(ours, theirs, common);
			Assert.AreEqual(0, results.Conflicts.Count);
// ReSharper disable PossibleNullReferenceException
			Assert.AreEqual(3, results.MergedNode.Attributes.Count);
// ReSharper restore PossibleNullReferenceException
		}

		[Test]
		public void TopLevelAtomicElementHasConflictsWithIsAtomicBeingTrue()
		{
			const string common = @"<topatomic originalAttr='originalValue' />";
			const string ours = @"<topatomic originalAttr='originalValue' newAttr='newValue' />";
			const string theirs = @"<topatomic originalAttr='originalValue' thirdAttr='thirdValue' />";

			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var results = merger.Merge(ours, theirs, common);
			Assert.AreEqual(1, results.Conflicts.Count);
			var mergedNode = results.MergedNode;
// ReSharper disable PossibleNullReferenceException
			Assert.AreEqual(2, mergedNode.Attributes.Count);
// ReSharper restore PossibleNullReferenceException
			CheckAttribute(mergedNode, "originalAttr", "originalValue");
			CheckAttribute(mergedNode, "newAttr", "newValue");
			Assert.IsNull(mergedNode.Attributes["thirdAttr"]);
		}

		[Test]
		public void TheyAddedNode()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			XmlNode ours = null;
			var theirsNode = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			doc.AppendChild(theirsNode);
			var theirsAttr = doc.CreateAttribute("originalAttr");
			theirsAttr.Value = "originalValue";
			theirsNode.Attributes.Append(theirsAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, theirsNode, null);
			Assert.IsTrue(result);
			Assert.AreSame(ours, theirsNode);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeAddedNode()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			doc.AppendChild(ours);
			var oursAttr = doc.CreateAttribute("originalAttr");
			oursAttr.Value = "originalValue";
			ours.Attributes.Append(oursAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, null, null);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothDeletedNode()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			doc.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);
			XmlNode ours = null;
			var result = MergeAtomicElementService.Run(merger, ref ours, null, ancestor);
			Assert.IsTrue(result);
			Assert.IsNull(ours);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void NeitherMadeChanges()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			doc.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);
			var ours = ancestor;
			var result = MergeAtomicElementService.Run(merger, ref ours, ancestor, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeDeletedTheyEditSoTheyWin()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);
			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);
			XmlNode ours = null;
			var theirsNode = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(theirsNode);
			var theirAttr = doc.CreateAttribute("originalAttr");
			theirAttr.Value = "newValue";
			theirsNode.Attributes.Append(theirAttr);
			var result = MergeAtomicElementService.Run(merger, ref ours, theirsNode, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void TheyDeletedWeEditSoWeWin()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);

			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);

			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ours);
			var ourAttr = doc.CreateAttribute("originalAttr");
			ourAttr.Value = "newValue";
			ours.Attributes.Append(ourAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, null, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeEditedTheyDidNothingWeWinNoConflict()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);

			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);

			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ours);
			var oursAttr = doc.CreateAttribute("originalAttr");
			oursAttr.Value = "newValue";
			ours.Attributes.Append(oursAttr);

			var theirs = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(theirs);
			var theirAttr = doc.CreateAttribute("originalAttr");
			theirAttr.Value = "originalValue";
			theirs.Attributes.Append(theirAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeDidNothingTheyEditedTheyWinNoConflict()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);

			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);

			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ours);
			var oursAttr = doc.CreateAttribute("originalAttr");
			oursAttr.Value = "originalValue";
			ours.Attributes.Append(oursAttr);

			var theirs = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(theirs);
			var theirAttr = doc.CreateAttribute("originalAttr");
			theirAttr.Value = "newValue";
			theirs.Attributes.Append(theirAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			Assert.AreSame(ours, theirs);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothMadeSameChangeNoConflict()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);

			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);

			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ours);
			var oursAttr = doc.CreateAttribute("originalAttr");
			oursAttr.Value = "newValue";
			ours.Attributes.Append(oursAttr);

			var theirs = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(theirs);
			var theirAttr = doc.CreateAttribute("originalAttr");
			theirAttr.Value = "newValue";
			theirs.Attributes.Append(theirAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothMadeChangesButWithConflict()
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = true };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			var doc = new XmlDocument();
			var rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);

			var ancestor = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ancestor);
			var ancestorAttr = doc.CreateAttribute("originalAttr");
			ancestorAttr.Value = "originalValue";
			ancestor.Attributes.Append(ancestorAttr);

			var ours = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(ours);
			var oursAttr = doc.CreateAttribute("originalAttr");
			oursAttr.Value = "newValue";
			ours.Attributes.Append(oursAttr);

			var theirs = doc.CreateNode(XmlNodeType.Element, "topatomic", null);
			rootNode.AppendChild(theirs);
			var theirAttr = doc.CreateAttribute("originalAttr");
			theirAttr.Value = "nutherValue";
			theirs.Attributes.Append(theirAttr);

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertExpectedChangesCount(0);
		}

		private static void CheckAttribute(XmlNode node, string attribute, string value)
		{
// ReSharper disable PossibleNullReferenceException
			var attr = node.Attributes[attribute];
// ReSharper restore PossibleNullReferenceException
			Assert.IsNotNull(attr);
			Assert.AreEqual(value, attr.Value);
		}
	}
}
