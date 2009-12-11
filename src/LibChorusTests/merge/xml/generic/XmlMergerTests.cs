using System;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class XmlMergerTests
	{
		[Test]
		public void OneAddedNewChildElement()
		{
			string red = @"<a/>";
			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor, "a/b[@key='one']/c[text()='first']");
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
					Console.WriteLine("*Unexpected: "+ conflict.GetFullHumanReadableDescription());
				}
			}
			Assert.AreEqual(0, r.Conflicts.Count, "There were unexpected conflicts.");
		}

		private ChangeAndConflictAccumulator CheckOneWay(string ours, string theirs, string ancestor, params string[] xpaths)
		{
			XmlMerger m = new XmlMerger(new NullMergeSituation());
			m.MergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateForKeyedElement("key", true));
			m.MergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElement("key", true));
			m.MergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElement("key", true));
			var result = m.Merge(ours, theirs, ancestor);
			foreach (string xpath in xpaths)
			{
				XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, xpath);
			}
			return result;
		}

		[Test]
		public void TextElement_OneAdded_NoConflicts()
		{
			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r/>", "<r/>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");

			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r><t/></r>", "<r/>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");

			CheckBothWaysNoConflicts("<r><t>hello</t></r>", "<r><t/></r>", "<r><t/></r>",
									 "r[count(t)=1]",
									 "r/t[text()='hello']");
		}

		[Test]
		public void TextElement_BothDeleted_NoConflicts()
		{
			CheckBothWaysNoConflicts("<r><t/></r>", "<r><t></t></r>", "<r><t>hello</t></r>",
									 "r/t[not(text())]",
									 "r[count(t)=1]");
		}

		[Test]
		public void TextElement_OneEditted_NoConflicts()
		{
			CheckBothWaysNoConflicts("<r><t>after</t></r>", "<r><t>before</t></r>", "<r><t>before</t></r>",
									 "r/t[contains(text(),'after')]");
		}



		[Test, Ignore("Not yet. The matcher using xmldiff sees the parent objects as different")]
		public void TextElement_BothEditted_OuterWhiteSpaceIgnored()
		{
			CheckBothWaysNoConflicts("<r><t>   flub</t></r>", "<r><t> flub      </t></r>", "<r><t/></r>",
									 "r/t[contains(text(),'flub')]");
		}


		[Test]
		public void TextElement_EachEditted_OursKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t>mine</t>";
			string theirs = @"<t>theirs</t>";

			XmlMerger m = new XmlMerger(new MergeSituation("pretendPath", "userX", "XRev","userY", "YRev", MergeOrder.ConflictHandlingModeChoices.WeWin));
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='mine']");
			Assert.AreEqual("pretendPath", result.Conflicts[0].RelativeFilePath);

			Assert.AreEqual(typeof (BothEdittedTextConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void TextElement_WeEdittedTheyDeleted_OursKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t>mine</t>";
			string theirs = @"<t></t>";

			XmlMerger m = new XmlMerger(new NullMergeSituation());
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='mine']");

			Assert.AreEqual(typeof(RemovedVsEdittedTextConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void TextElement_TheyEdittedWeDeleted_EditedIsKept_ConflictRegistered()
		{
			string ancestor = @"<t>original</t>";
			string ours = @"<t></t>";
			string theirs = @"<t>change</t>";

			XmlMerger m = new XmlMerger(new NullMergeSituation());
			var result = m.Merge(ours, theirs, ancestor);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result.MergedNode, "t[text()='change']");

			Assert.AreEqual(typeof(RemovedVsEdittedTextConflict), result.Conflicts[0].GetType());
		}

		[Test]
		public void EachAddedDifferentSyblings_GetBoth()
		{

			string ancestor = @"<a/>";
			string red = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			string blue = @"<a>
								<b key='two'>
									<c>second</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a/b[@key='one']/c[text()='first']",
										"a/b[@key='two']/c[text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins (they will be in a different order, but we don't care which)
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a/b[@key='one']/c[text()='first']",
										 "a/b[@key='two']/c[text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void OneAddedASyblingElement_GetBoth()
		{
			string red = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
								<b key='two'>
									<c>second</c>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/c[text()='first']",
									 "a/b[@key='two']/c[text()='second']");
		}

		[Test]
		public void OneAddedSomethingDeep()
		{
			string red = @"<a>
								<b key='one'/>
							</a>";

			string ancestor = red;

			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";

			Assert.IsFalse(XmlUtilities.AreXmlElementsEqual(red, blue));

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/c[text()='first']");
		}

		// JohnT: changed <c> elements to <t>. Otherwise, since c expects a key, <c/> does not 'correspond'
		// to <c>first</c>, and the merger tries to insert both, and we get an ambiguous insert exception.
		// Review JohnH(JohnT): should the FindByKeyAttribute also allow a secondary match on a keyless
		// empty element?
		[Test]
		public void OnePutTextContentInPreviouslyElement()
		{
			string red = @"<a>
								<b key='one'><t/></b>
							</a>";

			string ancestor = red;


			string blue = @"<a>
								<b key='one'>
									<t>first</t>
								</b>
							</a>";

			CheckBothWaysNoConflicts(red, blue, ancestor,
									 "a/b[@key='one']/t[text()='first']");
		}

		[Test]
		public void WeDeletedAnElement_Removed()
		{
			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";
			string ancestor = blue;

			string red = @"<a></a>";

			CheckOneWay(blue, red, ancestor, "a[ not(b)]");
		}

		[Test]
		public void TheyDeleteAnElement_Removed()
		{
			string red = @"<a></a>";
			string blue = @"<a>
								<b key='one'>
									<c>first</c>
								</b>
							</a>";
			string ancestor = blue;

			CheckOneWay(blue, red, ancestor, "a[ not(b)]");
		}


		[Test]
		public void OneAddedAttribute()
		{
			string red = @"<a/>";
			string ancestor = red;
			string blue = @"<a one='1'/>";

			CheckBothWaysNoConflicts(blue, red, ancestor, "a[@one='1']");
		}

		[Test]
		public void BothAddedSameAttributeSameValue()
		{
			string ancestor = @"<a/>";
			string red = @"<a one='1'/>";
			string blue = @"<a one='1'/>";

			CheckBothWaysNoConflicts(blue, red, ancestor, "a[@one='1']");
		}

		[Test]
		public void BothAddedSameAttributeDifferentValue()
		{
			string ancestor = @"<a/>";
			string red = @"<a one='r'/>";
			string blue = @"<a one='b'/>";

			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor, "a[@one='b']");
			Assert.AreEqual(typeof(BothEdittedAttributeConflict), r.Conflicts[0].GetType());

			r =CheckOneWay(red, blue, ancestor, "a[@one='r']");
			Assert.AreEqual(typeof(BothEdittedAttributeConflict), r.Conflicts[0].GetType());
		}

		[Test]
		public void OneRemovedAttribute()
		{
			string red = @"<a one='1'/>";
			string ancestor = red;
			string blue = @"<a/>";

			CheckBothWaysNoConflicts(blue, red, ancestor, "a[not(@one)]");
		}
		[Test]
		public void OneMovedAndChangedAttribute()
		{
			string red = @"<a one='1' two='2'/>";
			string ancestor = red;
			string blue = @"<a two='22' one='1'/>";

			CheckBothWaysNoConflicts(blue, red, ancestor, "a[@one='1' and @two='22']");
		}

		[Test]
		public void BothAddedAnUnkeyableNephewElement()
		{
			string ancestor = @"<a>
								<b key='one'>
									<cx>first</cx>
								</b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<cx>first</cx>
									<cx>second</cx>
								</b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<cx>first</cx>
									<cx>third</cx>
								</b>
							</a>";

			CheckOneWay(red, blue, ancestor,
						"a[count(b)='1']",
						"a/b[count(cx)='3']",
						"a/b[@key='one']/cx[text()='first']",
						"a/b[@key='one']/cx[text()='second']",
						"a/b[@key='one']/cx[text()='third']");

			CheckOneWay(blue, red, ancestor,
						"a[count(b)='1']",
						"a/b[count(cx)='3']",
						"a/b[@key='one']/cx[text()='first']",
						"a/b[@key='one']/cx[text()='second']",
						"a/b[@key='one']/cx[text()='third']");

		}


		[Test]
		public void BothAddedANephewElementWithKeyAttr()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
								</b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
								</b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='z'>third</c>
								</b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='3']",
										"a/b[@key='one']/c[text()='first']",
										"a/b[@key='one']/c[text()='second']",
										"a/b[@key='one']/c[text()='third']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins (they will be in a different order, but we don't care which)
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='3']",
										 "a/b[@key='one']/c[text()='first']",
										 "a/b[@key='one']/c[text()='second']",
										 "a/b[@key='one']/c[text()='third']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		[Test]
		public void InsertInMiddleInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='z'>third</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='x' and text()='first']",
									 "a/b[@key='one']/c[2][@key='z' and text()='third']",
									 "a/b[@key='one']/c[3][@key='y' and text()='second']");

		}
		[Test]
		public void InsertAtStartInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
									<c key='z'>third</c>
								   <c key='x'>first</c>
									 <c key='y'>second</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='z' and text()='third']",
									 "a/b[@key='one']/c[2][@key='x' and text()='first']",
									 "a/b[@key='one']/c[3][@key='y' and text()='second']");

		}

		[Test]
		public void InsertAtEndInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = ancestor;


			string blue = @"<a>
								<b key='one'>
								   <c key='x'>first</c>
									 <c key='y'>second</c>
									<c key='z'>third</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='x' and text()='first']",
									 "a/b[@key='one']/c[2][@key='y' and text()='second']",
									 "a/b[@key='one']/c[3][@key='z' and text()='third']");
		}

		/// <summary>
		/// Red deletes two adjacent items; blue inserts a new item between them. Output should show the missing
		/// items deleted and the new one inserted in the right place.
		/// </summary>
		[Test]
		public void DeleteNeighborsAndInsertInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='z'>extra</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='z' and text()='extra']",
									 "a/b[@key='one']/c[3][@key='d' and text()='fourth']");
		}
		[Test]
		public void HattonTempCheck()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='z'>extra</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='3']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='z' and text()='extra']",
									 "a/b[@key='one']/c[3][@key='d' and text()='fourth']");
		}
		/// <summary>
		/// Red inserted at the start, blue at the end. Both should be in the right place.
		/// </summary>
		[Test]
		public void BothInsertedStartEndInOrder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='r'>red</c>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='bl'>blue</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='5']",
									 "a/b[@key='one']/c[1][@key='r' and text()='red']",
									 "a/b[@key='one']/c[2][@key='a' and text()='first']",
									 "a/b[@key='one']/c[3][@key='b' and text()='second']",
									 "a/b[@key='one']/c[4][@key='c' and text()='third']",
									 "a/b[@key='one']/c[5][@key='bl' and text()='blue']");
		}
		/// <summary>
		/// Red moved two items and changed one of them, and blue inserted something between them.
		/// </summary>
		[Test]
		public void ReorderModifyAndInsert()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>red</c>
									<c key='e'>fifth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='z'>extra</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='6']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='d' and text()='red']",
									 "a/b[@key='one']/c[3][@key='z' and text()='extra']",
									 "a/b[@key='one']/c[4][@key='e' and text()='fifth']",
									 "a/b[@key='one']/c[5][@key='b' and text()='second']",
									 "a/b[@key='one']/c[6][@key='c' and text()='third']");
		}

		/// <summary>
		/// Red deleted an item, and blue edited it. Regardless of who initiated the merge,
		/// we should keep the edit.
		/// </summary>
		[Test]
		public void ElementDeleteAndModifyConflict()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>blue</c>
							   </b>
							</a>";

			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='2']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='y' and text()='blue']");
			Assert.AreEqual(typeof(RemovedVsEditedElementConflict), r.Conflicts[0].GetType());

			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='2']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='y' and text()='blue']");
			Assert.AreEqual(typeof(RemovedVsEditedElementConflict), r2.Conflicts[0].GetType());
		}

		/// <summary>
		/// Both re-orderd things differently.
		/// </summary>
		[Test]
		public void ConflictingReorder()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='e'>fifth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='e'>fifth</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='5']",
										"a/b[@key='one']/c[1][@key='a' and text()='first']",
										"a/b[@key='one']/c[2][@key='b' and text()='second']",
										"a/b[@key='one']/c[3][@key='c' and text()='third']",
										"a/b[@key='one']/c[4][@key='e' and text()='fifth']",
										"a/b[@key='one']/c[5][@key='d' and text()='fourth']");
			Assert.AreEqual(typeof(BothReorderedElementConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='5']",
										 "a/b[@key='one']/c[1][@key='a' and text()='first']",
										 "a/b[@key='one']/c[2][@key='e' and text()='fifth']",
										 "a/b[@key='one']/c[3][@key='b' and text()='second']",
										 "a/b[@key='one']/c[4][@key='c' and text()='third']",
										 "a/b[@key='one']/c[5][@key='d' and text()='fourth']");
			Assert.AreEqual(typeof(BothReorderedElementConflict), r2.Conflicts[0].GetType());
		}

		/// <summary>
		/// Both re-ordered things the same way; blue also edited.
		/// </summary>
		[Test]
		public void BothSameReorder()
		{
			string ancestor =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
							<c key='e'>fifth</c>
					   </b>
					</a>";

			string red =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='e'>fifth</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
					   </b>
					</a>";


			string blue =
				@"<a>
						<b key='one'>
							<c key='a'>first</c>
							<c key='e'>blue</c>
							<c key='b'>second</c>
							<c key='c'>third</c>
							<c key='d'>fourth</c>
					   </b>
					</a>";
			CheckBothWaysNoConflicts(blue, red, ancestor,
									 "a[count(b)='1']",
									 "a/b[count(c)='5']",
									 "a/b[@key='one']/c[1][@key='a' and text()='first']",
									 "a/b[@key='one']/c[2][@key='e' and text()='blue']",
									 "a/b[@key='one']/c[3][@key='b' and text()='second']",
									 "a/b[@key='one']/c[4][@key='c' and text()='third']",
									 "a/b[@key='one']/c[5][@key='d' and text()='fourth']");
		}

		/// <summary>
		/// Test inserting different things at the same spot. Arguably this is a less serious conflict
		/// than many others, as all of both inserts happens; but we don't know what the order should be.
		///
		/// It is therefore somewhat arbitrary 'their' insert comes before 'ours'. I just made the test
		/// conform to the current implementation plan.
		/// </summary>
		[Test]
		public void DifferentInsertsAtSamePlaceConflict()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='r'>red</c>
									<c key='y'>second</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='x'>first</c>
									<c key='b'>blue</c>
									<c key='y'>second</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='4']",
										"a/b[@key='one']/c[1][@key='x' and text()='first']",
										"a/b[@key='one']/c[2][@key='r' and text()='red']",
										"a/b[@key='one']/c[3][@key='b' and text()='blue']",
										"a/b[@key='one']/c[4][@key='y' and text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='4']",
										 "a/b[@key='one']/c[1][@key='x' and text()='first']",
										 "a/b[@key='one']/c[2][@key='b' and text()='blue']",
										 "a/b[@key='one']/c[3][@key='r' and text()='red']",
										 "a/b[@key='one']/c[4][@key='y' and text()='second']");
			Assert.AreEqual(typeof(AmbiguousInsertConflict), r2.Conflicts[0].GetType());
		}

		/// <summary>
		/// This one is subtle. Red re-ordered fourth before second. Blue inserted something after fourth.
		/// Since only red re-ordered things, red's order basically wins. But we don't really know whether
		/// to insert 'z' after 'e' or before 'e', since those are no longer the same position.
		/// Arbitrarily, after 'd' currently wins out.
		/// </summary>
		[Test]
		public void AmbiguousInsertWhenOtherReordered()
		{
			string ancestor = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			string red = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";


			string blue = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='b'>second</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
									<c key='z'>blue</c>
									<c key='e'>fifth</c>
							   </b>
							</a>";

			// blue wins
			ChangeAndConflictAccumulator r = CheckOneWay(blue, red, ancestor,
										"a[count(b)='1']",
										"a/b[count(c)='6']",
										"a/b[@key='one']/c[1][@key='a' and text()='first']",
										"a/b[@key='one']/c[2][@key='d' and text()='fourth']",
										"a/b[@key='one']/c[3][@key='z' and text()='blue']",
										"a/b[@key='one']/c[4][@key='b' and text()='second']",
										"a/b[@key='one']/c[5][@key='c' and text()='third']",
										"a/b[@key='one']/c[6][@key='e' and text()='fifth']"
				);
			Assert.AreEqual(typeof(AmbiguousInsertReorderConflict), r.Conflicts[0].GetType());
			// red wins
			ChangeAndConflictAccumulator r2 = CheckOneWay(red, blue, ancestor,
										 "a[count(b)='1']",
										 "a/b[count(c)='6']",
										 "a/b[@key='one']/c[1][@key='a' and text()='first']",
										 "a/b[@key='one']/c[2][@key='d' and text()='fourth']",
										 "a/b[@key='one']/c[3][@key='z' and text()='blue']",
										 "a/b[@key='one']/c[4][@key='b' and text()='second']",
										 "a/b[@key='one']/c[5][@key='c' and text()='third']",
										 "a/b[@key='one']/c[6][@key='e' and text()='fifth']"
				);
			Assert.AreEqual(typeof(AmbiguousInsertReorderConflict), r2.Conflicts[0].GetType());
		}
	}
}