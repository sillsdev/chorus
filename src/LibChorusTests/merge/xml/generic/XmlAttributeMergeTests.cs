using System;
using System.Collections.Generic;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.generic
{
	[TestFixture]
	public class XmlAttributeMergeTests
	{
		private MergeStrategies _mergeStrategies;

		[SetUp]
		public void TestSetup()
		{
			_mergeStrategies = new MergeStrategies();
			_mergeStrategies.ElementStrategies.Add("a", ElementStrategy.CreateSingletonElement());
			_mergeStrategies.ElementStrategies.Add("b", ElementStrategy.CreateSingletonElement());
		}

		[TearDown]
		public void TestTearDown()
		{
			_mergeStrategies = null;
		}

		[Test]
		public void BothAddedSameAttributeDifferentValueWeWin()
		{
			const string ancestor = @"<a/>";
			const string ours = @"<a one='r'/>";
			const string theirs = @"<a one='b'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='r']" },
				new List<string> { "a[@one='b']" },
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				0, null);
		}

		[Test]
		public void BothAddedSameAttributeDifferentValueTheyWin()
		{
			const string ancestor = @"<a/>";
			const string ours = @"<a one='r'/>";
			const string theirs = @"<a one='b'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				new NullMergeSituationTheyWin(),
				ancestor, ours, theirs,
				new List<string> { "a[@one='b']" },
				new List<string> { "a[@one='r']" },
				1, new List<Type> { typeof(BothAddedAttributeConflict) },
				0, null);
		}

		[Test]
		public void BothAddedSameAttributeWithSameValueHasOneChangeNoConflicts()
		{
			const string ancestor = @"<a/>";
			const string ours = @"<a one='newby'/>";
			const string theirs = @"<a one='newby'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='newby']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothAddedReport) });
		}

		[Test]
		public void WeEditedTheyRemovedWeWinWithConflict()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a one='ours'/>";
			const string theirs = @"<a/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='ours']" },
				new List<string> { "a[@one='original']" },
				1, new List<Type> { typeof(EditedVsRemovedAttributeConflict) },
				0, null);
		}

		[Test]
		public void TheyEditedWeRemovedTheyWinWithConflict()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a/>";
			const string theirs = @"<a one='theirs'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='theirs']" },
				new List<string> { "a[@one='original']" },
				1, new List<Type> { typeof(RemovedVsEditedAttributeConflict) },
				0, null);
		}

		[Test]
		public void BothEditedWithConflictWeWin()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a one='ours'/>";
			const string theirs = @"<a one='theirs'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='ours']" },
				new List<string> { "a[@one='original']", "a[@one='theirs']" },
				1, new List<Type> { typeof(BothEditedAttributeConflict) },
				0, null);
		}

		[Test]
		public void BothEditedWithConflictTheyWin()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a one='ours'/>";
			const string theirs = @"<a one='theirs'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				new NullMergeSituationTheyWin(),
				ancestor, ours, theirs,
				new List<string> { "a[@one='theirs']" },
				new List<string> { "a[@one='original']", "a[@one='ours']" },
				1, new List<Type> { typeof(BothEditedAttributeConflict) },
				0, null);
		}

		[Test]
		public void WeChangedAttributeOneChangeNoConflicts()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a one='ours'/>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='ours']" },
				new List<string> { "a[@one='original']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeChangedReport) });
		}

		[Test]
		public void TheyChangedAttributeOneChangeNoConflicts()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = ancestor;
			const string theirs = @"<a one='theirs'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='theirs']" },
				new List<string> { "a[@one='original']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeChangedReport) });
		}

		[Test]
		public void TheyAddedHasOneChangeAndNoConflicts()
		{
			const string ancestor = @"<a/>";
			const string ours = ancestor;
			const string theirs = @"<a one='theirs'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='theirs']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlAttributeAddedReport) });
		}

		[Test]
		public void WeAddedHasOneChangeAndNoConflicts()
		{
			const string ancestor = @"<a/>";
			const string ours = @"<a one='ours'/>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='ours']" },
				null,
				0, null,
				1, new List<Type> { typeof(XmlAttributeAddedReport) });
		}

		[Test]
		public void BothMadeSameAttributeValueChangeHasOneChangeReportAndNoConflicts()
		{
			const string ancestor = @"<a one='original'/>";
			const string ours = @"<a one='common'/>";
			const string theirs = @"<a one='common'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@one='common']" },
				new List<string> { "a[@one='original']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		[Test]
		public void BothDeletedAttributeHasOneChangeReportAndNoConflicts()
		{
			const string ancestor = @"<a one='originalOne' two='originalTwo' />";
			const string ours = @"<a two='originalTwo'/>";
			const string theirs = @"<a two='originalTwo'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@two='originalTwo']" },
				new List<string> { "a[@one]" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothDeletedReport) });
		}

		[Test]
		public void WeDeletedAttributeHasOneChangeReportAndNoConflicts()
		{
			const string ancestor = @"<a one='originalOne' two='originalTwo' />";
			const string ours = @"<a two='originalTwo'/>";
			const string theirs = ancestor;

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@two='originalTwo']" },
				new List<string> { "a[@one]" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeDeletedReport) });
		}

		[Test]
		public void TheyDeletedAttributeHasOneChangeReportAndNoConflicts()
		{
			const string ancestor = @"<a one='originalOne' two='originalTwo' />";
			const string ours = ancestor;
			const string theirs = @"<a two='originalTwo'/>";

			XmlTestHelper.DoMerge(
				_mergeStrategies,
				ancestor, ours, theirs,
				new List<string> { "a[@two='originalTwo']" },
				new List<string> { "a[@one]" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeDeletedReport) });
		}
	}
}