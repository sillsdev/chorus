using System;
using System.Collections.Generic;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Tests of the code used to merge sequences of children where order matters and duplicates are allowed.
	/// Note that because of the way CheckOneWay sets things up, elements a, b, c, and d are special: they are
	/// all known to the merger to occur in lists and be matched up by the 'key' attribute.
	/// </summary>
	[TestFixture]
	public class XmlListMergerTests
	{

		[Test]
		public void AmbiguousInsertWhenOtherReordered()
		{
			new UniqueKeysListMergerTests().AmbiguousInsertWhenOtherReordered();
		}

		[Test]
		public void BothAddedANephewElementWithKeyAttr()
		{
			new UniqueKeysListMergerTests().BothAddedANephewElementWithKeyAttr();
		}
		[Test]
		public void BothAddedAnUnkeyableNephewElement()
		{
			new UniqueKeysListMergerTests().BothAddedAnUnkeyableNephewElement();
		}
		[Test]
		public void BothInsertedStartEndInOrder()
		{
			new UniqueKeysListMergerTests().BothInsertedStartEndInOrder();
		}
		[Test]
		public void BothSameReorder()
		{
			new UniqueKeysListMergerTests().BothSameReorder();
		}
		[Test]
		public void ConflictingReorder()
		{
			new UniqueKeysListMergerTests().ConflictingReorder();
		}
		/// <summary>
		/// We delete two adjacent items; They inserts a new item between the two deleted items. Output should show the missing
		/// items deleted and the new one inserted in the right place, with the right number of reports.
		/// </summary>
		[Test]
		public void WeDeleteNeighborsAndTheyInsertInOrder()
		{
			const string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			const string ours = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			const string theirs = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='z'>extra</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			MergeStrategies _mergeStrategies = new MergeStrategies(); ;
			_mergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElementInList("key"));
			_mergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElementInList("key"));
			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> {
					"a/b[@key='one']/c[1][@key='a' and text()='first']",
					"a/b[@key='one']/c[2][@key='z' and text()='extra']",
					"a/b[@key='one']/c[3][@key='d' and text()='fourth']" },
				new List<string> { "a/b[@key='one']/c[@key='b']", "a/b[@key='one']/c[@key='c']" },
				0, null,
				3, new List<Type> { typeof(XmlTextDeletedReport), typeof(XmlTextDeletedReport), typeof(XmlTextAddedReport) });
		}
		[Test]
		public void DifferentInsertsAtSamePlaceConflict()
		{
			new UniqueKeysListMergerTests().DifferentInsertsAtSamePlaceConflict();
		}

		[Test]
		public void EachAddedDifferentSyblings_GetBoth()
		{
			new UniqueKeysListMergerTests().EachAddedDifferentSyblings_GetBoth();
		}


		[Test]
		public void ElementDeleteAndModifyConflict()
		{
			new UniqueKeysListMergerTests().ElementDeleteAndModifyConflict();
		}
		[Test]
		public void InsertAtEndInOrder()
		{
			new UniqueKeysListMergerTests().InsertAtEndInOrder();
		}
		[Test]
		public void InsertAtStartInOrder()
		{
			new UniqueKeysListMergerTests().InsertAtStartInOrder();
		}
		[Test]
		public void InsertInMiddleInOrder()
		{
			new UniqueKeysListMergerTests().InsertInMiddleInOrder();
		}
		[Test]
		public void OneAddedASyblingElement_GetBoth()
		{
			new UniqueKeysListMergerTests().OneAddedASyblingElement_GetBoth();
		}
		[Test]
		public void OneAddedNewChildElement()
		{
			new UniqueKeysListMergerTests().OneAddedNewChildElement();
		}
		[Test]
		public void OneAddedSomethingDeep()
		{
			new UniqueKeysListMergerTests().OneAddedSomethingDeep();
		}

		[Test]
		public void OneEditedDeepChildOfElementOtherDeleted()
		{
			new UniqueKeysListMergerTests().OneEditedDeepChildOfElementOtherDeleted();
		}
		[Test]
		public void OnePutTextContentInPreviouslyElement()
		{
			new UniqueKeysListMergerTests().OnePutTextContentInPreviouslyElement();
		}
		[Test]
		public void ReorderModifyAndInsert()
		{
			new UniqueKeysListMergerTests().ReorderModifyAndInsert();
		}
		[Test]
		public void TextElement_BothDeleted_NoConflicts()
		{
			new UniqueKeysListMergerTests().TextElement_BothDeleted_NoConflicts();
		}
		[Test]
		public void TextElement_TheyEditedWeDeleted_EditedIsKept_ConflictRegistered()
		{
			new UniqueKeysListMergerTests().TextElement_TheyEditedWeDeleted_EditedIsKept_ConflictRegistered();
		}

		[Test]
		public void TextElement_WeEditedTheyDeleted_OursKept_ConflictRegistered()
		{
			new UniqueKeysListMergerTests().TextElement_WeEditedTheyDeleted_OursKept_ConflictRegistered();
		}
		[Test]
		public void TheyDeleteAnElement_Removed()
		{
			new UniqueKeysListMergerTests().TheyDeleteAnElement_Removed();
		}
		[Test]
		public void WeDeletedAnElement_Removed()
		{
			new UniqueKeysListMergerTests().WeDeletedAnElement_Removed();
		}

		[Test]
		public void InsertedAdjacentDuplicateKey()
		{
			string red = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
							</a>";
			string ancestor = red;

			string blue = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='one'>
									<c key='two'>second</c>
								</b>
						  </a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='one']/c[text()='first']",
				"a/b[@key='one']/following-sibling::b/c[text()='second']");
		}

		/// <summary>
		/// This one is debateable. In the sequence case, we would allow A and B to both insert different items in different
		/// places; and in a list, it doesn't matter if the two items inserted are the same. For example, if we were merging
		/// a list of ComponentLexemes in a LexEntryRef, it's possible that the same component occurs twice (say a phrase
		/// containing 'the' twice), and it's possible that it was omitted in both places and each user corrected one of
		/// the missing occurrences. But it's worth reviewing. Any many other ref sequences should not actually have
		/// duplicates at all.
		/// </summary>
		[Test]
		public void BothInsertedSameInDifferentPlaces()
		{
			new UniqueKeysListMergerTests().BothInsertedSameInDifferentPlaces();
		}

		[Test]
		public void InsertedDuplicateKeyNotAdjacent()
		{
			string red = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='two'>
									<c key='two'>second</c>
								</b>
						</a>";
			string ancestor = red;

			string blue = @"<a key='one'>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='two'>
									<c key='two'>second</c>
								</b>
								<b key='one'>
									<c key='three'>third</c>
								</b>
						  </a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='one']/c[text()='first']",
				"a/b[@key='one']/following-sibling::b/c[text()='second']",
				"a/b[@key='two']/following-sibling::b/c[text()='third']");
		}

		[Test]
		public void InsertedOtherKeyBetweenDuplicates()
		{
			string red = @"<a>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='one'>
									<c key='two'>second</c>
								</b>
						</a>";
			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='three'>
									<c key='three'>third</c>
								</b>
								<b key='one'>
									<c key='two'>second</c>
								</b>
						  </a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='one']/c[text()='first']",
				"a/b[@key='one']/following-sibling::b/c[text()='third']",
				"a/b[@key='three']/following-sibling::b/c[text()='second']");
		}

		[Test]
		public void InsertedDuplicatesAtStartAndEnd()
		{
			string red = @"<a>
								<b key='one'>
									<c key='one'>first</c>
								</b>
						</a>";
			string ancestor = red;

			string blue = @"<a>
							   <b key='two'>
									<c key='two'>second</c>
							   </b>
							   <b key='one'>
									<c key='one'>first</c>
								</b>
								<b key='two'>
									<c key='three'>third</c>
								</b>
						  </a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='two']/c[text()='second']",
				"a/b[@key='two']/following-sibling::b/c[text()='first']",
				"a/b[@key='one']/following-sibling::b/c[text()='third']");
		}

		private void CheckBothWaysNoConflicts(string red, string blue, string ancestor, params string[] xpaths)
		{
			ChangeAndConflictAccumulator r = CheckOneWay(red, blue, ancestor, xpaths);
			AssertNoConflicts(r);

			r = CheckOneWay(blue, red, ancestor, xpaths);
			AssertNoConflicts(r);
		}

		private static void AssertNoConflicts(ChangeAndConflictAccumulator r)
		{
			if (r.Conflicts.Count > 0)
			{
				foreach (IConflict conflict in r.Conflicts)
				{
					Console.WriteLine("*Unexpected: " + conflict.GetFullHumanReadableDescription());
				}
			}
			Assert.AreEqual(0, r.Conflicts.Count, "There were unexpected conflicts.");
		}

		private ChangeAndConflictAccumulator CheckOneWay(string ours, string theirs, string ancestor, params string[] xpaths)
		{
			XmlMerger m = new XmlMerger(new NullMergeSituation());
			m.MergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("d", ElementStrategy.CreateForKeyedElementInList("key"));
			var result = m.Merge(ours, theirs, ancestor);
			foreach (string xpath in xpaths)
			{
				XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, xpath);
			}
			return result;
		}
	}

	/// <summary>
	/// Most of the tests relevant to XmlMerger are relevant also to a list merger: in the absence of duplicates,
	/// it should give the same results. So many of the tests of ListMerger just involve adapting the XmlMerger tests
	/// to use the list merger.
	/// </summary>
	internal class UniqueKeysListMergerTests : XmlMergerTests
	{
		protected override void AddMergeStrategies(XmlMerger m)
		{
			m.MergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElementInList("key"));
			m.MergeStrategies.ElementStrategies.Add("d", ElementStrategy.CreateForKeyedElementInList("key"));
		}
	}
}
