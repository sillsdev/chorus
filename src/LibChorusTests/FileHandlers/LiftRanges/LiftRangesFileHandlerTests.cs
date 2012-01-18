using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

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

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_liftRangesFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
							   where handler.GetType().Name == "LiftRangesFileTypeHandler"
						 select handler).First();
		}

		[TestFixtureTearDown]
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
			Assert.IsFalse(_liftRangesFileHandler.CanDiffFile(null));
		}

		[Test]
		public void CannotValidateAFile()
		{
			Assert.IsFalse(_liftRangesFileHandler.CanValidateFile(null));
		}

		[Test]
		public void CanMergeAFile()
		{
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".lift-ranges");
			try
			{
// ReSharper disable LocalizableElement
				File.WriteAllText(goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<lift-ranges />");
// ReSharper restore LocalizableElement
				Assert.IsTrue(_liftRangesFileHandler.CanMergeFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void CannotPresentANullFile()
		{
			Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(null));
		}

		[Test]
		public void CannotPresentAnEmptyFileName()
		{
			Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(""));
		}

		[Test]
		public void CannotPresentAFileWithOtherExtension()
		{
			var badXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".ClassData");
			try
			{
				Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(badXmlPathname));
			}
			finally
			{
				File.Delete(badXmlPathname);
			}
		}

		[Test]
		public void CanPresentAGoodFile()
		{
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".lift-ranges");
			try
			{
				Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		public void Find2WayDifferencesThrows()
		{
			Assert.Throws<ApplicationException>(() => _liftRangesFileHandler.Find2WayDifferences(null, null, null));
		}

		[Test]
		public void ValidateFileThrows()
		{
			Assert.Throws<NotImplementedException>(() => _liftRangesFileHandler.ValidateFile(null, null));
		}

		[Test]
		public void NobodyDidAnything()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			var result = DoMerge(common, ours, theirs, 0, 0);
			Assert.AreEqual(common, result);
			Assert.AreEqual(ours, result);
			Assert.AreEqual(theirs, result);
		}

		[Test]
		public void BothDoSameEdit()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='data' />
</>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='data' />
</>";
			var result = DoMerge(common, ours, theirs, 0, 1);
			Assert.AreEqual(ours, result);
			Assert.AreEqual(theirs, result);
		}

		[Test]
		public void WeEditTheyDoNothingSoWeWinOnMerge()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='data' />
</>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			var result = DoMerge(common, ours, theirs, 0, 1);
			Assert.AreEqual(ours, result);
		}

		[Test]
		public void TheyEditWeDoNothingSoTheyWinOnMerge()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='data' />
</>";
			var result = DoMerge(common, ours, theirs, 0, 1);
			Assert.AreEqual(theirs, result);
		}

		[Test]
		public void BothEditWithConflictAndWeWin()
		{
			const string common =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range />
</>";
			const string ours =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='ourdata' />
</>";
			const string theirs =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range attr='theirdata' />
</>";
			var result = DoMerge(common, ours, theirs, 1, 0);
			Assert.AreEqual(ours, result);
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
