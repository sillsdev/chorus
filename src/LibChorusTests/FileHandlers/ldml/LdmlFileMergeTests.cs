using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using LibChorus.Tests.merge.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.ldml
{
	/// <summary>
	/// Test the merge capabilities of the LdmlFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class LdmlFileMergeTests
	{
		private IChorusFileTypeHandler _ldmlFileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_ldmlFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
								where handler.GetType().Name == "LdmlFileHandler"
								select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_ldmlFileHandler = null;
			_eventListener = null;
		}

		[Test]
		public void CannotMergeNonexistantFile()
		{
			Assert.IsFalse(_ldmlFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void CannotMergeEmptyStringFile()
		{
			Assert.IsFalse(_ldmlFileHandler.CanMergeFile(String.Empty));
		}

		[Test]
		public void CanMergeGoodFwXmlFile()
		{
			var tempPath = Path.GetTempFileName();
			var goodXmlPathname = Path.ChangeExtension(tempPath, ".ldml");
			File.Delete(tempPath);
			try
			{
// ReSharper disable LocalizableElement
				File.WriteAllText(goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml />");
// ReSharper restore LocalizableElement
				Assert.IsTrue(_ldmlFileHandler.CanMergeFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void CannotMergeNullFile()
		{
			Assert.IsFalse(_ldmlFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Do3WayMergeThrowsOnNullInput()
		{
			Assert.Throws<ArgumentNullException>(() => _ldmlFileHandler.Do3WayMerge(null));
		}

		[Test]
		public void NonConflictingEditsInAtomicSpecialHasConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
<palaso:languageName value='German' />
<palaso:version value='1' />
</special>
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
<fw:windowsLCID value='1' />
</special>
</ldml>";
			var ourContent = commonAncestor.Replace("palaso:version value='1'", "palaso:version value='2'");
			// Set up for 'atomic' change test, as well.
			var theirContent = commonAncestor.Replace("palaso:languageName value='German'", "palaso:languageName value='Spanish'");
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/special/palaso:version[@value='2']", @"ldml/special/palaso:languageName[@value='German']", @"ldml/special/fw:windowsLCID[@value='1']" },
				new List<string> { @"ldml/special/palaso:version[@value='1']", @"ldml/special/palaso:languageName[@value='Spanish']" },
				1, 0);
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent,
			Dictionary<string, string> namespaces,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
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

				_ldmlFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
				foreach (var query in matchesExactlyOne)
					XmlTestHelper.AssertXPathMatchesExactlyOne(result, query, namespaces);
				if (isNull != null)
				{
					foreach (var query in isNull)
						XmlTestHelper.AssertXPathIsNull(result, query, namespaces);
				}
				_eventListener.AssertExpectedConflictCount(expectedConflictCount);
				_eventListener.AssertExpectedChangesCount(expectedChangesCount);
			}
			return result;
		}
	}
}
