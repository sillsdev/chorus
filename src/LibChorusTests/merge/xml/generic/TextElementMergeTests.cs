using System;
using System.Collections.Generic;
using Chorus.FileTypeHanders.xml;
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

		// TODO: Both made same edits change report (both?).

		// TODO: RemovedVsEdit conflict report (both).
		// TODO: EditVsRemove conflict report (both).
		// TODO: Conflicting edits conflict report (both).
	}
}