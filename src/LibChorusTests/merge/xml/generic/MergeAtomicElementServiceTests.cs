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
			Assert.AreEqual(3, results.MergedNode.Attributes.Count);
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
			Assert.AreEqual(2, mergedNode.Attributes.Count);
			CheckAttribute(mergedNode, "originalAttr", "originalValue");
			CheckAttribute(mergedNode, "newAttr", "newValue");
			Assert.IsNull(mergedNode.Attributes["thirdAttr"]);
		}

		private static void CheckAttribute(XmlNode node, string attribute, string value)
		{
			var attr = node.Attributes[attribute];
			Assert.IsNotNull(attr);
			Assert.AreEqual(value, attr.Value);
		}
	}
}
