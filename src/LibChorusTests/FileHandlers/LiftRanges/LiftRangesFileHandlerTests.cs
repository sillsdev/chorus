using System;
using System.IO;
using System.Linq;
using System.Text;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.FileHandlers.LiftRanges
{
	/// <summary>
	/// Test the LiftRangesFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class LiftRangesFileHandlerTests
	{
		private IChorusFileTypeHandler _liftRangesFileHandler;
		private ListenerForUnitTests _eventListener;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_liftRangesFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   where handler.GetType().Name == "LiftRangesFileTypeHandler"
						 select handler).First();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_liftRangesFileHandler = null;
			_eventListener = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _liftRangesFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void GetExtensionsOfKnownTextFileTypesIsLiftRanges()
		{
			var extensions = _liftRangesFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("lift-ranges", extensions[0]);
		}

		[Test]
		public void CannotDiffAFile()
		{
			Assert.That(_liftRangesFileHandler.CanDiffFile(null), Is.False);
		}

		[Test]
		public void CannotValidateAFile()
		{
			Assert.That(_liftRangesFileHandler.CanValidateFile(null), Is.False);
		}

		[Test]
		public void CanMergeAFile()
		{
			using (var tempFile = TempFile.WithExtension(".lift-ranges"))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<lift-ranges />");
				Assert.That(_liftRangesFileHandler.CanMergeFile(tempFile.Path), Is.True);
			}
		}

		[Test]
		public void CannotPresentANullFile()
		{
			Assert.That(_liftRangesFileHandler.CanPresentFile(null), Is.False);
		}

		[Test]
		public void CannotPresentAnEmptyFileName()
		{
			Assert.That(_liftRangesFileHandler.CanPresentFile(""), Is.False);
		}

		[Test]
		public void CannotPresentAFileWithOtherExtension()
		{
			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				Assert.That(_liftRangesFileHandler.CanPresentFile(tempFile.Path), Is.False);
			}
		}

		[Test]
		public void CanPresentAGoodFile()
		{
			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				Assert.That(_liftRangesFileHandler.CanPresentFile(tempFile.Path), Is.False);
			}
		}

		public void Find2WayDifferencesThrows()
		{
			Assert.Throws<ApplicationException>(() => _liftRangesFileHandler.Find2WayDifferences(null, null, null));
		}

		[Test]
		public void ValidateFileHasNoResultsForValiidFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone' />
</lift-ranges>";
			using (var tempFile = new TempFile(data))
			{
				Assert.That(_liftRangesFileHandler.ValidateFile(tempFile.Path, new NullProgress()), Is.Null);
			}
		}

		[Test]
		public void NobodyDidAnything()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone' />
</lift-ranges>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone' />
</lift-ranges>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone' />
</lift-ranges>";
			var result = DoMerge(common, ours, theirs, 0, 0).Replace("\"", "'").Replace("\r\n", "\n");
			Assert.AreEqual(common.Replace("\r\n", "\n"), result);
			Assert.AreEqual(ours.Replace("\r\n", "\n"), result);
			Assert.AreEqual(theirs.Replace("\r\n", "\n"), result);
		}

		[Test]
		public void BothDoSameEdit()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone'/>
</lift-ranges>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone'
		attr='data' />
</lift-ranges>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone'
		attr='data' />
</lift-ranges>";
			var result = DoMerge(common, ours, theirs, 0, 0).Replace("\"", "'").Replace("\r\n", "\n");
			Assert.AreEqual(ours.Replace("\r\n", "\n"), result);
			Assert.AreEqual(theirs.Replace("\r\n", "\n"), result);
		}

		[Test]
		public void WeEditTheyDoNothingSoWeWinOnMerge()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone'/>
</lift-ranges>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		id='theone'
		attr='data' />
</lift-ranges>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone'/>
</lift-ranges>";
			var result = DoMerge(common, ours, theirs, 0, 0).Replace("\"", "'").Replace("\r\n", "\n");
			Assert.AreEqual(ours.Replace("\r\n", "\n"), result);
		}

		[Test]
		public void TheyEditWeDoNothingSoTheyWinOnMerge()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone' />
</lift-ranges>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone'/>
</lift-ranges>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		attr='data'
		id='theone' />
</lift-ranges>";
			var result = DoMerge(common, ours, theirs, 0, 0).Replace("\"", "'").Replace("\r\n", "\n");
			Assert.AreEqual(theirs.Replace("\r\n", "\n"), result);
		}

		[Test]
		public void BothEditWithConflictAndWeWin()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='theone'/>
</lift-ranges>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		attr='ourdata'
		id='theone' />
</lift-ranges>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
	<range
		attr='theirdata'
		id='theone' />
</lift-ranges>";
			var result = DoMerge(common, ours, theirs, 1, 0).Replace("\"", "'").Replace("\r\n", "\n");
			Assert.AreEqual(ours.Replace("\r\n", "\n"), result);
		}

		/// <summary>
		/// A range-element id is the possibility's own name, so FLEx respelling names on export
		/// (LT-22697 normalizes them) moves the id without the possibility having changed.
		/// </summary>
		[Test]
		public void RespelledIdMergesAsAnEditWhenTheGuidIsUnchanged()
		{
			var common = RangesWithPartOfSpeech(kDecomposedId, kPosGuid, "old");
			// We upgraded, so our export normalizes the id.
			var ours = RangesWithPartOfSpeech(kComposedId, kPosGuid, "old");
			// They did not upgrade, and they edited the abbreviation.
			var theirs = RangesWithPartOfSpeech(kDecomposedId, kPosGuid, "new");

			var result = DoMerge(common, ours, theirs, 0, 2);

			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				string.Format("//range-element[@guid='{0}' and @id='{1}']", kPosGuid, kComposedId), 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				"//range-element/abbrev/form/text[text()='new']", 1);
		}

		/// <summary>
		/// The guid is optional in LIFT, so a file written without one must still merge on the id.
		/// </summary>
		[Test]
		public void RangeElementWithoutAGuidStillMergesOnItsId()
		{
			var common = RangesWithPartOfSpeech("Noun", null, "old");
			var ours = RangesWithPartOfSpeech("Noun", null, "old");
			var theirs = RangesWithPartOfSpeech("Noun", null, "new");

			var result = DoMerge(common, ours, theirs, 0, 0);

			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				"//range-element/abbrev/form/text[text()='new']", 1);
		}

		/// <summary>
		/// An element that names a guid is matched only against elements that likewise name one, so a
		/// writer that drops the guid a previous writer wrote loses the object's identity: the element
		/// reads as a deletion and its guid-less replacement as an addition. That is the cost of keeping
		/// the two sets apart, and a project whose writers disagree over the guid pays it on every merge
		/// between them.
		/// </summary>
		[Test]
		public void DroppingTheGuidReadsAsADeletionPlusAnAddition()
		{
			var common = RangesWithPartOfSpeech("Noun", kPosGuid, "old");
			// Our writer omits the guid the ancestor names.
			var ours = RangesWithPartOfSpeech("Noun", null, "old");
			var theirs = RangesWithPartOfSpeech("Noun", kPosGuid, "new");

			var result = DoMerge(common, ours, theirs, 1, 1);

			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 2);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				"//range-element[not(@guid)]/abbrev/form/text[text()='old']", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(string.Format(
				"//range-element[@guid='{0}']/abbrev/form/text[text()='new']", kPosGuid), 1);
		}

		/// <summary>
		/// Two possibilities that happen to share a name are still two possibilities. Matching them
		/// on the id would merge them into one and lose a guid.
		/// </summary>
		[Test]
		public void RangeElementsWithDifferentGuidsAreNotMatched()
		{
			const string theirGuid = "9c4d3e2b-77a6-4b1e-8d55-6a0f2c3e14bb";
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='grammatical-info' />
</lift-ranges>";
			var ours = RangesWithPartOfSpeech("Noun", kPosGuid, "n");
			var theirs = RangesWithPartOfSpeech("Noun", theirGuid, "n");

			var result = DoMerge(common, ours, theirs, 0, 2);

			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 2);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				string.Format("//range-element[@guid='{0}']", kPosGuid), 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				string.Format("//range-element[@guid='{0}']", theirGuid), 1);
		}

		/// <summary>
		/// A sibling that still carries the old id, written by a tool that omits guids, must not be
		/// taken as the partner just because it comes first in the file.
		/// </summary>
		[Test]
		public void GuidMatchIsPreferredOverAnEarlierSiblingMatchingOnTheOldId()
		{
			var common = RangesWithPartOfSpeech(kDecomposedId, kPosGuid, "old");
			// A guid-less element carrying the old id sorts ahead of our respelled one.
			var ours = string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='grammatical-info'>
<range-element id='{0}'>
<abbrev><form lang='en'><text>unrelated</text></form></abbrev>
</range-element>
<range-element id='{1}' guid='{2}'>
<abbrev><form lang='en'><text>old</text></form></abbrev>
</range-element>
</range>
</lift-ranges>", kDecomposedId, kComposedId, kPosGuid);
			var theirs = RangesWithPartOfSpeech(kDecomposedId, kPosGuid, "new");

			// No conflict: the guid-less sibling is matched only against other guid-less elements, so it
			// cannot also claim the element the guid already speaks for.
			var result = DoMerge(common, ours, theirs, 0, 3);

			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 2);
			// Their edit belongs to the element sharing the guid, not to the one sharing the old id.
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(string.Format(
				"//range-element[@guid='{0}']/abbrev/form/text[text()='new']", kPosGuid), 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(
				"//range-element[not(@guid)]/abbrev/form/text[text()='unrelated']", 1);
		}

		/// <summary>
		/// A merge done before @guid was preferred could leave one possibility standing as two
		/// range-elements, spelled differently. Matching on the guid value makes them ambiguous siblings,
		/// which the merger collapses back to one. The warning must name @guid as the attribute whose
		/// values are the same, and quote the shared value, since the differing ids are not what made
		/// them indistinguishable.
		/// </summary>
		[Test]
		public void RangeElementsDuplicatedByAnEarlierMergeCollapseToOne()
		{
			var duplicated = string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='grammatical-info'>
<range-element id='{0}' guid='{2}'>
<abbrev><form lang='en'><text>old</text></form></abbrev>
</range-element>
<range-element id='{1}' guid='{2}'>
<abbrev><form lang='en'><text>old</text></form></abbrev>
</range-element>
</range>
</lift-ranges>", kDecomposedId, kComposedId, kPosGuid);

			var result = DoMerge(duplicated, duplicated, duplicated, 0, 0);

			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//range/range-element", 1);
			Assert.That(_eventListener.Warnings, Is.Not.Empty, "the dropped duplicate should be reported");
			var warning = _eventListener.Warnings[0].GetFullHumanReadableDescription();
			Assert.That(warning, Does.Contain("'guid'").And.Contain(kPosGuid), warning);
			Assert.That(warning, Does.Not.Contain("'id'"), warning);
		}

		// The two @id values a part of speech named "Compléments" is exported under. Built rather than written
		// out, since the two spellings are indistinguishable in source and an editor or a text filter that
		// normalizes on save would silently make them the same string.
		private static readonly string kDecomposedId = "Compléments".Normalize(NormalizationForm.FormD);
		private static readonly string kComposedId = "Compléments".Normalize(NormalizationForm.FormC);
		private const string kPosGuid = "e8c4b4b0-1a2f-4f9e-9f39-3f2b0d8f7a11";

		/// <summary>A null <paramref name="posGuidOrNull"/> leaves @guid off the element altogether.</summary>
		private static string RangesWithPartOfSpeech(string posId, string posGuidOrNull, string abbrevText)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='grammatical-info'>
<range-element id='{0}'{1}>
<abbrev><form lang='en'><text>{2}</text></form></abbrev>
</range-element>
</range>
</lift-ranges>", posId, posGuidOrNull == null ? string.Empty : string.Format(" guid='{0}'", posGuidOrNull), abbrevText);
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent,
			int expectedConflictCount, int expectedChangesCount)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				_eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = _eventListener;

				_liftRangesFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
				_eventListener.AssertExpectedConflictCount(expectedConflictCount);
				_eventListener.AssertExpectedChangesCount(expectedChangesCount);
			}
			return result;
		}
	}
}
