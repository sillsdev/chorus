using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;

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
			using (var tempFile = TempFile.WithExtension(".lift-ranges"))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<lift-ranges />");
				Assert.IsTrue(_liftRangesFileHandler.CanMergeFile(tempFile.Path));
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
			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(tempFile.Path));
			}
		}

		[Test]
		public void CanPresentAGoodFile()
		{
			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				Assert.IsFalse(_liftRangesFileHandler.CanPresentFile(tempFile.Path));
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
				Assert.IsNull(_liftRangesFileHandler.ValidateFile(tempFile.Path, new NullProgress()));
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
