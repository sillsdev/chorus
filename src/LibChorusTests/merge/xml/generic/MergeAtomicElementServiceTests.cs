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

			ListenerForUnitTests listener;
			var merger = GetMerger(out listener, false);

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

			ListenerForUnitTests listener;
			var merger = GetMerger(out listener, true);

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
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ours = null;
			var theirsNode = CreateOneNode(doc, rootNode, "originalAttr", "originalValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirsNode, null);
			Assert.IsTrue(result);
			Assert.AreSame(ours, theirsNode);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeAddedNode()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			var ours = CreateOneNode(doc, rootNode, "originalAttr", "originalValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, null, null);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothDeletedNode()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			var ancestor = CreateOneNode(doc, rootNode, "originalAttr", "originalValue");
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
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			var ancestor = CreateOneNode(doc, rootNode, "originalAttr", "originalValue");
			var ours = ancestor;

			var result = MergeAtomicElementService.Run(merger, ref ours, ancestor, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeDeletedTheyEditSoTheyWin()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ours = null;
			XmlNode ancestor;
			XmlNode theirs;
			CreateTwoNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out theirs, "originalAttr", "newValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void TheyDeletedWeEditSoWeWin()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ancestor;
			XmlNode ours;
			CreateTwoNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out ours, "originalAttr", "newValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, null, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeEditedTheyDidNothingWeWinNoConflict()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ancestor;
			XmlNode ours;
			XmlNode theirs;
			CreateThreeNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out ours, "originalAttr", "newValue",
							 out theirs, "originalAttr", "originalValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void WeDidNothingTheyEditedTheyWinNoConflict()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ancestor;
			XmlNode ours;
			XmlNode theirs;
			CreateThreeNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out ours, "originalAttr", "originalValue",
							 out theirs, "originalAttr", "newValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			Assert.AreSame(ours, theirs);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothMadeSameChangeNoConflict()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ancestor;
			XmlNode ours;
			XmlNode theirs;
			CreateThreeNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out ours, "originalAttr", "newValue",
							 out theirs, "originalAttr", "newValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void BothMadeChangesButWithConflict()
		{
			XmlNode rootNode;
			XmlMerger merger;
			ListenerForUnitTests listener;
			var doc = SetupMergerAndDocument(out rootNode, out merger, out listener);

			XmlNode ancestor;
			XmlNode ours;
			XmlNode theirs;
			CreateThreeNodes(doc, rootNode,
							 out ancestor, "originalAttr", "originalValue",
							 out ours, "originalAttr", "newValue",
							 out theirs, "originalAttr", "nutherValue");

			var result = MergeAtomicElementService.Run(merger, ref ours, theirs, ancestor);
			Assert.IsTrue(result);
			listener.AssertExpectedConflictCount(1);
			listener.AssertExpectedChangesCount(0);
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
// ReSharper disable PossibleNullReferenceException
			newElement.Attributes.Append(oursAttr);
// ReSharper restore PossibleNullReferenceException
			return newElement;
		}

		private static XmlDocument GetDocument(out XmlNode rootNode)
		{
			var doc = new XmlDocument();
			rootNode = doc.CreateNode(XmlNodeType.Element, "root", null);
			doc.AppendChild(rootNode);
			return doc;
		}

		private static XmlMerger GetMerger(out ListenerForUnitTests listener, bool isAtomic)
		{
			var elementStrategy = new ElementStrategy(false) { IsAtomic = isAtomic };
			var merger = new XmlMerger(new NullMergeSituation());
			merger.MergeStrategies.SetStrategy("topatomic", elementStrategy);
			listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			return merger;
		}

		private static void CheckAttribute(XmlNode node, string attribute, string value)
		{
// ReSharper disable PossibleNullReferenceException
			var attr = node.Attributes[attribute];
// ReSharper restore PossibleNullReferenceException
			Assert.IsNotNull(attr);
			Assert.AreEqual(value, attr.Value);
		}

		private static XmlDocument SetupMergerAndDocument(out XmlNode rootNode, out XmlMerger merger, out ListenerForUnitTests listener)
		{
			merger = GetMerger(out listener, true);

			return GetDocument(out rootNode);
		}
	}
}
