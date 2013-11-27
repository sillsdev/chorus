using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.ldml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
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
			using (var tempFile = TempFile.WithExtension(".ldml"))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ldml />");
				Assert.IsTrue(_ldmlFileHandler.CanMergeFile(tempFile.Path));
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
		public void DuplicateSpecialElementsAreRemoved()
		{
			const string badData =
@"<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' goner='true' />
<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2' />
<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2' goner='true' />
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1' />
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1' goner='true' />
</ldml>";
			var doc = new XmlDocument();
			var badRootNode = XmlUtilities.GetDocumentNodeFromRawXml(badData, doc);
			var merger = new XmlMerger(new NullMergeSituation());
			merger.EventListener = new ListenerForUnitTests();
			LdmlFileHandler.SetupElementStrategies(merger);
			var oldValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			XmlMergeService.RemoveAmbiguousChildren(merger.EventListener, merger.MergeStrategies, badRootNode);
			XmlMergeService.RemoveAmbiguousChildNodes = oldValue;
			var childNodes = badRootNode.SelectNodes("special");
			Assert.IsTrue(childNodes.Count == 3);
			for (var idx = 0; idx < 3; ++idx)
			{
				XmlNode currentNode = childNodes[idx];
				switch (idx)
				{
					case 0:
						Assert.IsNotNull(currentNode.Attributes["xmlns:palaso"]);
						break;
					case 1:
						Assert.IsNotNull(currentNode.Attributes["xmlns:palaso2"]);
						break;
					case 2:
						Assert.IsNotNull(currentNode.Attributes["xmlns:fw"]);
						break;
				}
				Assert.IsNull(currentNode.Attributes["goner"]);
			}
		}

		[Test]
		public void KeyAttrAddedByCodeBeforeMergeIsRemoved()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30' />
</identity>
<collations>
<collation />
</collations>
</ldml>";
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			DoMerge(commonAncestor, commonAncestor, commonAncestor,
				namespaces,
				new List<string>(),
				new List<string> { @"ldml/collations/collation[@type='standard']" }, // Should not be present, since the premerge code added it.
				0, null,
				0, null);
		}

		[Test]
		public void NonConflictingEditsInAtomicSpecialHasConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30' />
</identity>
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

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/special/palaso:version[@value='2']", @"ldml/special/palaso:languageName[@value='German']", @"ldml/special/fw:windowsLCID[@value='1']" },
				new List<string> { @"ldml/special/palaso:version[@value='1']", @"ldml/special/palaso:languageName[@value='Spanish']" },
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) },
				0, null);
		}

		[Test]
		public void KnownKeyboards_AreMerged()
		{
			const string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<generation date='2012-06-08T09:36:30' />
	</identity>
	<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
		<palaso2:knownKeyboards>
			<palaso2:keyboard
				layout='MyFavoriteKeyboard'
				locale='en-US'
				os='MacOSX' />{0}
		</palaso2:knownKeyboards>
		<palaso2:version
			value='2' />
	</special>
</ldml>";
			const string ourExtra =
				@"
			<palaso2:keyboard
				layout='SusannasFavoriteKeyboard'
				locale='en-GB'
				os='Unix' />";
			const string theirExtra =
			@"
			<palaso2:keyboard
				layout='KensFavoriteKeyboard'
				locale='en-GB'
				os='Unix' />";

			var commonAncestor = string.Format(pattern, "");
			var ourContent = string.Format(pattern, ourExtra);
			var theirContent = string.Format(pattern, theirExtra);
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"palaso2", "urn://palaso.org/ldmlExtensions/v2"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='SusannasFavoriteKeyboard']",
				@"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='KensFavoriteKeyboard']",
				@"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='MyFavoriteKeyboard']"},
				new List<string>(0),
				0, null,
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void IdenticalNewKnownKeyboards_AreMerged()
		{
			const string pattern =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<generation date='2012-06-08T09:36:30' />
	</identity>
	<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
		<palaso2:knownKeyboards>
			<palaso2:keyboard
				layout='MyFavoriteKeyboard'
				locale='en-US'
				os='MacOSX' />{0}
		</palaso2:knownKeyboards>
		<palaso2:version
			value='2' />
	</special>
</ldml>";
			const string ourExtra =
				@"
			<palaso2:keyboard
				layout='SusannasFavoriteKeyboard'
				locale='en-GB'
				os='Unix' />";
			const string theirExtra =
			@"
			<palaso2:keyboard
				layout='SusannasFavoriteKeyboard'
				locale='en-GB'
				os='Unix' />
			<palaso2:keyboard
				layout='KensFavoriteKeyboard'
				locale='en-GB'
				os='Unix' />";

			var commonAncestor = string.Format(pattern, "");
			var ourContent = string.Format(pattern, ourExtra);
			var theirContent = string.Format(pattern, theirExtra);
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"palaso2", "urn://palaso.org/ldmlExtensions/v2"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='SusannasFavoriteKeyboard']",
				@"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='KensFavoriteKeyboard']",
				@"ldml/special/palaso2:knownKeyboards/palaso2:keyboard[@layout='MyFavoriteKeyboard']"},
				new List<string>(0),
				0, null,
				2, new List<Type> { typeof(XmlBothAddedSameChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void GenerateDateAttr_IsPreMerged()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30' />
</identity>
</ldml>";

			var ourContent = commonAncestor.Replace("09:36:30", "09:37:30");
			var theirContent = commonAncestor.Replace("09:36:30", "09:38:30");
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:38:30']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30']", @"ldml/identity/generation[@date='2012-06-08T09:37:30']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });

			// They made the change
			DoMerge(commonAncestor, theirContent, ourContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:38:30']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30']", @"ldml/identity/generation[@date='2012-06-08T09:37:30']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		[Test]
		public void GenerateDateAttr_IsPreMergedWhenOtherElementsConflict()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2013-11-26T21:17:55' />
		<language
			type='pcf' />
	</identity>
	<collations>
		<collation>
			<base>
				<alias
					source='ta' />
			</base>
			<special
				xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
				<palaso:sortRulesType
					value='OtherLanguage' />
			</special>
		</collation>
	</collations>
	<special
		xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='Tam' />
		<palaso:defaultFontFamily
			value='Latha' />
		<palaso:defaultKeyboard
			value='ISIS-Tamil' />
		<palaso:languageName
			value='Tamil' />
		<palaso:spellCheckingId
			value='pcf' />
		<palaso:version
			value='2' />
	</special>
	<special
		xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
		<palaso2:knownKeyboards>
			<palaso2:keyboard
				layout='ISIS-Tamil'
				locale='ta-IN'
				os='Win32NT' />
			<palaso2:keyboard
				layout='m17n:ta:tamil99'
				locale='ta'
				os='Unix' />
			<palaso2:keyboard
				layout='Tamil'
				locale='ta-IN'
				os='Win32NT' />
		</palaso2:knownKeyboards>
		<palaso2:version
			value='2' />
	</special>
	<special
		xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled
			value='False' />
	</special>
</ldml>";

			var ourContent = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2013-11-26T21:20:49' />
		<language
			type='pcf' />
	</identity>
	<collations>
		<collation>
			<base>
				<alias
					source='ta' />
			</base>
			<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
				<palaso:sortRulesType
					value='OtherLanguage' />
			</special>
		</collation>
	</collations>
	<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='Tam' />
		<palaso:defaultFontFamily
			value='Latha' />
		<palaso:defaultKeyboard
			value='ISIS-Tamil' />
		<palaso:languageName
			value='Tamil' />
		<palaso:spellCheckingId
			value='pcf' />
		<palaso:version
			value='2' />
	</special>
	<special xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
		<palaso2:knownKeyboards>
			<palaso2:keyboard
				layout='ISIS-Tamil'
				locale='ta-IN'
				os='Win32NT' />
			<palaso2:keyboard
				layout='m17n:ta:tamil99'
				locale='ta'
				os='Unix' />
			<palaso2:keyboard
				layout='Tamil'
				locale='ta-IN'
				os='Win32NT' />
		</palaso2:knownKeyboards>
		<palaso2:version
			value='2' />
	</special>
	<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled
			value='False' />
	</special>
</ldml>";
			var theirContent = @"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2013-11-26T21:23:22' />
		<language
			type='pcf' />
	</identity>
	<collations>
		<collation>
			<base>
				<alias
					source='ta' />
			</base>
			<special
				xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
				<palaso:sortRulesType
					value='OtherLanguage' />
			</special>
		</collation>
	</collations>
	<special
		xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
		<palaso:abbreviation
			value='Tam' />
		<palaso:defaultFontFamily
			value='Latha' />
		<palaso:defaultKeyboard
			value='ISIS-Tamil' />
		<palaso:languageName
			value='Tamil' />
		<palaso:spellCheckingId
			value='pcf' />
		<palaso:version
			value='2' />
	</special>
	<special
		xmlns:palaso2='urn://palaso.org/ldmlExtensions/v2'>
		<palaso2:knownKeyboards>
			<palaso2:keyboard
				layout='ISIS-Tamil'
				locale='ta-IN'
				os='Win32NT' />
			<palaso2:keyboard
				layout='m17n:ta:tamil99'
				locale='ta'
				os='Unix' />
			<palaso2:keyboard
				layout='Tamil'
				locale='ta-IN'
				os='Win32NT' />
		</palaso2:knownKeyboards>
		<palaso2:version
			value='2' />
	</special>
	<special
		xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'>
		<fw:graphiteEnabled
			value='False' />
		<fw:validChars
			value='&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;&#xA;&lt;ValidCharacters&gt;&#xA;  &lt;WordForming /&gt;&#xA;  &lt;Numeric /&gt;&#xA;  &lt;Other /&gt;&#xA;&lt;/ValidCharacters&gt;' />
	</special>
</ldml>";
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2013-11-26T21:23:22']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30']", @"ldml/identity/generation[@date='2012-06-08T09:37:30']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });

			// They made the change
			DoMerge(commonAncestor, theirContent, ourContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2013-11-26T21:23:22']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30']", @"ldml/identity/generation[@date='2012-06-08T09:37:30']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent,
			Dictionary<string, string> namespaces,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
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
				expectedConflictTypes = expectedConflictTypes ?? new List<Type>();
				Assert.AreEqual(expectedConflictTypes.Count, _eventListener.Conflicts.Count,
								"Expected conflict count and actual number found differ.");
				for (var idx = 0; idx < expectedConflictTypes.Count; ++idx)
					Assert.AreSame(expectedConflictTypes[idx], _eventListener.Conflicts[idx].GetType());

				_eventListener.AssertExpectedChangesCount(expectedChangesCount);
				expectedChangeTypes = expectedChangeTypes ?? new List<Type>();
				Assert.AreEqual(expectedChangeTypes.Count, _eventListener.Changes.Count,
								"Expected change count and actual number found differ.");
				for (var idx = 0; idx < expectedChangeTypes.Count; ++idx)
					Assert.AreSame(expectedChangeTypes[idx], _eventListener.Changes[idx].GetType());
			}
			return result;
		}
	}
}
