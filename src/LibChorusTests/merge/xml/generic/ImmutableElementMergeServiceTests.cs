using System.Xml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Tests for ImmutableElementMergeService class.
	/// </summary>
	[TestFixture]
	public class ImmutableElementMergeServiceTests
	{
		[Test]
		public void AllElementsNullCausesNoTrouble()
		{
			XmlNode ours;
			XmlNode theirs;
			Assert.IsNull(DoMerge(null, null, null, new NullMergeSituation(), new ListenerForUnitTests(), out ours, out theirs));
		}

		[Test]
		public void AncestorNull_OursNull_TheirsNotNull_HasChangeReport_AndTheirsIsKeptInMerge()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge(null, null, "<NewImmutableElemment />", new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<NewImmutableElemment />", resultNode.OuterXml);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlAdditionChangeReport>();
			listener.AssertExpectedConflictCount(0);
			Assert.AreSame(ours, theirs);
			Assert.AreSame(ours, resultNode);
		}

		[Test]
		public void AncestorNull_OursNotNull_TheirsNull_HasChangeReport_AndOursIsKeptInMerge()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge(null, "<NewImmutableElemment />", null, new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<NewImmutableElemment />", resultNode.OuterXml);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlAdditionChangeReport>();
			listener.AssertExpectedConflictCount(0);
			Assert.AreSame(ours, resultNode);
			Assert.AreNotSame(ours, theirs);
		}

		[Test]
		public void AncestorNull_BothAddSameElementAndContent_HasChangeReport()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge(null, "<NewImmutableElemment />", "<NewImmutableElemment />", new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<NewImmutableElemment />", resultNode.OuterXml);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlBothAddedSameChangeReport>();
			listener.AssertExpectedConflictCount(0);
			Assert.AreSame(ours, resultNode);
			Assert.AreNotSame(ours, theirs);
		}

		[Test]
		public void BothAdded_ButNotTheSame_HasConflictForWeWin_OurChangeIsKept()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge(null, "<MyNewImmutableElemment />", "<TheirNewImmutableElemment />", new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<MyNewImmutableElemment />", resultNode.OuterXml);
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<BothAddedMainElementButWithDifferentContentConflict>();
			Assert.AreSame(ours, resultNode);
			Assert.AreNotSame(ours, theirs);
		}

		[Test]
		public void BothAdded_ButNotTheSame_HasConflictForTheyWin_TheirChangeIsKept()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge(null, "<MyNewImmutableElemment />", "<TheirNewImmutableElemment />", new NullMergeSituationTheyWin(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<TheirNewImmutableElemment />", resultNode.OuterXml);
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<BothAddedMainElementButWithDifferentContentConflict>();
			Assert.AreSame(theirs, resultNode);
			Assert.AreSame(ours, theirs);
		}

		[Test]
		public void BothDeletedElement_HasChangeReport()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge("<OriginalImmutableElemment />", null, null, new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNull(resultNode);
			Assert.IsNull(ours);
			Assert.IsNull(theirs);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlBothDeletionChangeReport>();
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void WeDeleted_HasChangeReport()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge("<OriginalImmutableElemment />", null, "<TheirImmutableElemment />", new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNull(resultNode);
			Assert.IsNull(ours);
			Assert.IsNotNull(theirs);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void TheyDeleted_HasChangeReport()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge("<OriginalImmutableElemment />", "<OurImmutableElemment />", null, new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNull(resultNode);
			Assert.IsNull(ours);
			Assert.IsNull(theirs);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void WeMadeIllegalChange_ButAncestorHasBeenRetored_HasNoReports()
		{
			var listener = new ListenerForUnitTests();
			XmlNode ours;
			XmlNode theirs;
			var resultNode = DoMerge("<OriginalImmutableElemment />", "<OurImmutableElemment />", "<TheirImmutableElemment />", new NullMergeSituation(), listener, out ours, out theirs);
			Assert.IsNotNull(resultNode);
			Assert.AreEqual("<OriginalImmutableElemment />", resultNode.OuterXml);
			Assert.IsNotNull(ours);
			Assert.IsNotNull(theirs);
			Assert.AreSame(resultNode, ours);
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void XmlMergerCallsImmutableElementMergeServiceForImmutableElement()
		{
			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituation())
			{
				EventListener = listener
			};
			var elemStrat = new ElementStrategy(false)
				{
					IsImmutable = true
				};
			merger.MergeStrategies.SetStrategy("ImmutableElemment", elemStrat);
			var ancestor = CreateNode("<ImmutableElemment attr='originalvalue' />");
			var ours = CreateNode("<ImmutableElemment attr='ourvalue' />");
			var theirs = CreateNode("<ImmutableElemment attr='theirvalue' />");
			merger.MergeInner(ref ours, theirs, ancestor);
			Assert.AreSame(ancestor, ours);
			Assert.AreEqual("<ImmutableElemment attr=\"originalvalue\" />", ancestor.OuterXml);
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void XmlMergerDoesNotCallImmutableElementMergeServiceForMutableElement()
		{
			var listener = new ListenerForUnitTests();
			var merger = new XmlMerger(new NullMergeSituation())
			{
				EventListener = listener
			};
			var elemStrat = new ElementStrategy(false)
			{
				IsImmutable = false
			};
			merger.MergeStrategies.SetStrategy("MutableElemment", elemStrat);
			var ancestor = CreateNode("<MutableElemment attr='originalvalue' />");
			var ours = CreateNode("<MutableElemment attr='ourvalue' />");
			var theirs = CreateNode("<MutableElemment attr='originalvalue' />");
			merger.MergeInner(ref ours, theirs, ancestor);
			Assert.AreNotSame(ancestor, ours);
			Assert.AreEqual("<MutableElemment attr=\"ourvalue\" />", ours.OuterXml);
			listener.AssertExpectedChangesCount(1);
			listener.AssertFirstChangeType<XmlAttributeChangedReport>();
			listener.AssertExpectedConflictCount(0);
		}

		private static XmlNode DoMerge(
			string ancestorXml, string ourXml, string theirXml,
			MergeSituation mergeSituation, IMergeEventListener listener, out XmlNode ours, out XmlNode theirs)
		{
			var merger = new XmlMerger(mergeSituation)
			{
				EventListener = listener
			};
			ours = CreateNode(ourXml);
			theirs = CreateNode(theirXml);
			var ancestorNode = CreateNode(ancestorXml);
			ImmutableElementMergeService.DoMerge(merger, ref ours, theirs, ancestorNode);
			return ours;
		}

		private static XmlNode CreateNode(string data)
		{
			return string.IsNullOrEmpty(data) ? null : XmlUtilities.GetDocumentNodeFromRawXml(data, new XmlDocument());
		}
	}
}
