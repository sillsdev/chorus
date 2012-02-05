using System;
using System.Collections.Generic;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class TextElementMergeTests
	{
		private MergeStrategies _mergeStrategies;

		[SetUp]
		public void TestSetup()
		{
			_mergeStrategies = new MergeStrategies();
			_mergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("d", ElementStrategy.CreateSingletonElement());
		}

		[TearDown]
		public void TestTearDown()
		{
			_mergeStrategies = null;
		}

		[Test]
		public void WeAddedNewTextElementToNonExistingElementTheyDidNothingHasOneChangeReport()
		{
			const string ancestor = @"<a/>";
			const string ours =
@"<a>
	<b>ourNewText</b>
</a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='ourNewText']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextAddedReport) });
		}

		[Test]
		public void WeAddedNewTextToExtantTextElementTheyDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b/>
</a>";
			const string ours =
@"<a>
	<b>ourNewText</b>
</a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='ourNewText']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextAddedReport) });
		}

		[Test]
		public void TheyAddedNewTextElementToNonExistingElementWeDidNothingHasOneChangeReport()
		{
			const string ancestor = @"<a/>";
			const string ours = ancestor;
			const string theirs =
@"<a>
	<b>theirNewText</b>
</a>";
			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='theirNewText']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextAddedReport) });
		}

		[Test]
		public void TheyAddedNewTextToExtantTextElementWeDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b/>
</a>";
			const string ours = ancestor;
			const string theirs =
@"<a>
	<b>theirNewText</b>
</a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='theirNewText']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextAddedReport) });
		}

		[Test]
		public void TheyDeletedTextNodeButNotTextParent1WeDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours = ancestor;
			const string theirs =
@"<a>
	<b></b>
</a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b" },
				new List<string> { "a/b[text()='originalText']" },
				0, null,
				1, new List<Type> { typeof(XmlTextDeletedReport) });
		}

		[Test]
		public void WeDeletedTextNodeButNotTextParent2TheyDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours =
@"<a>
	<b/>
</a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b" },
				new List<string> { "a/b[text()='originalText']" },
				0, null,
				1, new List<Type> { typeof(XmlTextDeletedReport) });
		}

		[Test]
		public void WeDeletedTextNodeButNotTextParent1TheyDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours =
@"<a>
	<b></b>
</a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b" },
				new List<string> { "a/b[text()='originalText']" },
				0, null,
				1, new List<Type> { typeof(XmlTextDeletedReport) });
		}

		[Test]
		public void TheyDeletedTextNodeButNotTextParent2WeDidNothingHasOneChangeReport()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours = ancestor;
			const string theirs =
@"<a>
	<b/>
</a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b" },
				new List<string> { "a/b[text()='originalText']" },
				0, null,
				1, new List<Type> { typeof(XmlTextDeletedReport) });
		}

		[Test]
		public void BothDeletedWithOneChangeReport1()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours =
@"<a>
	<b/>
</a>";
			const string theirs = ours;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b" },
				new List<string> { "a/b[text()='originalText']" },
				0, null,
				1, new List<Type> { typeof(XmlTextBothDeletedReport) });
		}

		[Test]
		public void BothDeletedWithOneChangeReport2()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours =
@"<a>
</a>";
			const string theirs = ours;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				null,
				new List<string> { "a/b" },
				0, null,
				1, new List<Type> { typeof(XmlTextBothDeletedReport) });
		}

		[Test]
		public void BothDeletedWithOneChangeReport3()
		{
			const string ancestor =
@"<a>
	<b>originalText</b>
</a>";
			const string ours = "<a/>";
			const string theirs = ours;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				null,
				new List<string> { "a/b" },
				0, null,
				1, new List<Type> { typeof(XmlTextBothDeletedReport) });
		}

		[Test]
		public void WeEditedTheyDidNothingOneChange()
		{
			const string ancestor = "<a><b>before</b></a>";
			const string ours = "<a><b>after</b></a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='after']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextChangedReport) });
		}

		[Test]
		public void TheyEditedWeDidNothingOneChange()
		{
			const string ancestor = "<a><b>before</b></a>";
			const string ours = ancestor;
			const string theirs = "<a><b>after</b></a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='after']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextChangedReport) });
		}

		[Test]
		public void BothEditedButNotTheSameEditsReportedAsConflict()
		{
			const string ancestor = "<a><b>original</b></a>";
			const string ours = "<a><b>weChanged</b></a>";
			const string theirs = "<a><b>theyChanged</b></a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[text()='weChanged']" },
				new List<string> { "a/b[text()='original']", "a/b[text()='theyChanged']" },
				1, new List<Type> { typeof(XmlTextBothEditedTextConflict) },
				0, null);
		}

		[Test]
		public void WeDeletedOtherwiseEmptyElementTheyDidNothingHasDeletionReport()
		{
			const string ancestor = @"<a><b/></a>";
			const string ours = @"<a></a>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> {"a"},
				new List<string> {"a/b"},
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
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

			_mergeStrategies.ElementStrategies.Clear();
			_mergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElement("key", true));
			_mergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElement("key", true));
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

		/// <summary>
		/// They delete two adjacent items; We insert a new item between the two deleted ones. Output should show the missing
		/// items deleted and the new one inserted in the right place, with the right number of reports.
		/// </summary>
		[Test]
		public void TheyDeleteNeighborsAndWeInsertInOrder()
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
									<c key='b'>second</c>
									<c key='z'>extra</c>
									<c key='c'>third</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			const string theirs = @"<a>
								<b key='one'>
									<c key='a'>first</c>
									<c key='d'>fourth</c>
							   </b>
							</a>";

			_mergeStrategies.ElementStrategies.Clear();
			_mergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateForKeyedElement("key", true));
			_mergeStrategies.ElementStrategies.Add("c", ElementStrategy.CreateForKeyedElement("key", true));

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
		public void TextElementBothEdited_OuterWhiteSpaceIgnored()
		{
			const string ancestor = "<a><b/></a>";
			const string ours = "<a><b>   flub</b></a>";
			const string theirs = "<a><b> flub      </b></a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[contains(text(),'flub')]" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextBothAddedReport) });
		}

		[Test]
		public void TheyAddedEmptyNodeWeAddedNodeAndContentHasOneAddReport()
		{
			const string ancestor = "<a/>";
			const string ours = "<a><b>hello</b></a>";
			const string theirs = "<a><b/></a>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a/b[contains(text(),'hello')]" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlTextAddedReport) });
		}
	}
}