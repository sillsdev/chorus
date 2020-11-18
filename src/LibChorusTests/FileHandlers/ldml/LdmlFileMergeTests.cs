using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.ldml;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.Extensions;
using SIL.IO;
using SIL.Providers;
using SIL.TestUtilities;
using SIL.TestUtilities.Providers;

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
		private DateTime _expectedUtcDateTime;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_ldmlFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
								where handler.GetType().Name == "LdmlFileHandler"
								select handler).First();
			_expectedUtcDateTime = DateTime.UtcNow;
			DateTimeProvider.SetProvider(new ReproducibleDateTimeProvider(_expectedUtcDateTime));

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
<special xmlns:sil='urn://www.sil.org/ldml/0.1' />
<special xmlns:sil='urn://www.sil.org/ldml/0.1' goner='true' />
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
			Assert.IsTrue(childNodes.Count == 4);
			for (var idx = 0; idx < 4; ++idx)
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
					case 3:
						Assert.IsNotNull(currentNode.Attributes["xmlns:sil"]);
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

		#region Top level LDML elements (V3)

		[Test]
		public void TopLevelElementsAreSingleton()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version>ancestory</version>
		<generation date='2015-03-13T22:33:45Z' />
	</identity>
	<localeDisplayNames />
	<layout />
	<contextTransforms />
	<characters />
	<delimiters />
	<dates />
	<numbers />
	<units />
	<listPatterns />
	<collations />
	<posix />
	<segmentations />
	<rbnf />
	<metadata />
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version>Our version</version>
		<generation date='2015-03-13T22:33:45Z' />
	</identity>
	<localeDisplayNames>Our localeDisplayNames</localeDisplayNames>
	<layout>Our layout</layout>
	<contextTransforms>Our contextTransforms</contextTransforms>
	<characters>Our characters</characters>
	<delimiters>Our delimiters</delimiters>
	<dates>Our dates</dates>
	<numbers>Our numbers</numbers>
	<units>Our units</units>
	<listPatterns>Our listPatterns</listPatterns>
	<collations>Our collations</collations>
	<posix>Our posix</posix>
	<segmentations>Our segmentations</segmentations>
	<rbnf>Our rbnf</rbnf>
	<metadata>Our metadata</metadata>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version>Their version</version>
		<generation date='2015-03-13T22:33:45Z' />
	</identity>
	<localeDisplayNames>Their localeDisplayNames</localeDisplayNames>
	<layout>Their layout</layout>
	<contextTransforms>Their contextTransforms</contextTransforms>
	<characters>Their characters</characters>
	<delimiters>Their delimiters</delimiters>
	<dates>Their dates</dates>
	<numbers>Their numbers</numbers>
	<units>Their units</units>
	<listPatterns>Their listPatterns></listPatterns>
	<collations>Their collations</collations>
	<posix>Their posix</posix>
	<segmentations>Their segmentations</segmentations>
	<rbnf>Their rbnf</rbnf>
	<metadata>Their metadata</metadata>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/identity/version[text()='Our version']",
					@"ldml/localeDisplayNames[text()='Our localeDisplayNames']",
					@"ldml/layout[text()='Our layout']",
					@"ldml/contextTransforms[text()='Our contextTransforms']",
					@"ldml/characters[text()='Our characters']",
					@"ldml/delimiters[text()='Our delimiters']",
					@"ldml/dates[text()='Our dates']",
					@"ldml/numbers[text()='Our numbers']",
					@"ldml/units[text()='Our units']",
					@"ldml/listPatterns[text()='Our listPatterns']",
					@"ldml/collations[text()='Our collations']",
					@"ldml/posix[text()='Our posix']",
					@"ldml/segmentations[text()='Our segmentations']",
					@"ldml/rbnf[text()='Our rbnf']",
					@"ldml/metadata[text()='Our metadata']"
				},
				new List<string>(0),
				3, new List<Type>
				{
					typeof (XmlTextBothEditedTextConflict),
					typeof (BothEditedTheSameAtomicElement),
					typeof (BothEditedTheSameAtomicElement)
				},
				1, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport)
				});
		}

		[Test]
		public void IdentityIsMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'>Ancestor Identity version description</version>
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity uid='ancestor' windowsLCID='anc123' defaultRegion='ANC' variantName='2008' />
		</special>
	</identity>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'>Our Identity version description</version>
		<generation date='2012-06-07T09:36:30Z' />
		<language type='es' />
		<script type='Spanish' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity uid='ourabc' windowsLCID='our555' defaultRegion='OUR' variantName='2014' />
			<sil:identity newAttribute='thisWillNotBePreserved' />
		</special>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<someNew />
		</special>
	</identity>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $'>Their Identity version description</version>
		<generation date='2012-06-08T09:36:30Z' />
		<language type='th' />
		<script type='Thai' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity uid='theirabc' windowsLCID='their123' defaultRegion='THEIR' variantName='2558' />
		</special>
	</identity>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/identity/language[@type='es']",
					@"ldml/identity/script[@type='Spanish']",
					@"ldml/identity/special/sil:identity[@uid='ourabc' and @windowsLCID='our555' and @defaultRegion='OUR' and @variantName='2014']"
				},
				new List<string>(0),
				7, new List<Type>
				{
					typeof (XmlTextBothEditedTextConflict),
					typeof (BothEditedAttributeConflict),
					typeof (BothEditedAttributeConflict),
					typeof (BothEditedAttributeConflict),
					typeof (BothEditedAttributeConflict),
					typeof (BothEditedAttributeConflict),
					typeof (BothEditedAttributeConflict)
				},
				1, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport)
				});
		}

		[Test]
		public void CharactersAreMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<characters>
		<exemplarCharacters>[1 2 3 4 5 6 7 8 9 0]</exemplarCharacters>
	</characters>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters type='punctuation'>[\- ‐ – — ]</exemplarCharacters>
		<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:exemplarCharacters type='ourCharacters'>[! @ # $ % ^]</sil:exemplarCharacters>
		</special>
	</characters>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<characters>
		<exemplarCharacters type='index'>[A B C D E F G H I J K L M N O P Q R S T U V W X Y Z]</exemplarCharacters>
		<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:exemplarCharacters type='theirCharacters'>[) ( * \&amp; ^]</sil:exemplarCharacters>
		</special>
	</characters>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[@type='index' and text()='[A B C D E F G H I J K L M N O P Q R S T U V W X Y Z]']",
					@"ldml/characters/exemplarCharacters[@type='auxiliary' and text()='[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]']",
					@"ldml/characters/exemplarCharacters[@type='punctuation' and text()='[\- ‐ – — ]']",
					@"ldml/characters/exemplarCharacters[text()='[a b c d e f g h i j k l m n o p q r s t u v w x y z]']",
					@"ldml/characters/special/sil:exemplarCharacters[text()='[! @ # $ % ^]']",
					@"ldml/characters/special/sil:exemplarCharacters[@type='theirCharacters']"
				},
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[text()='[1 2 3 4 5 6 7 8 9 0]']"
				},
				3, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
					typeof (BothEditedTheSameAtomicElement),
					typeof (AmbiguousInsertConflict)
				},
				5, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlTextBothMadeSameChangeReport), typeof(XmlAttributeBothAddedReport),
					typeof(XmlTextAddedReport), typeof(XmlTextAddedReport)
				});
		}

		[Test]
		public void Merging_Different_Characters_Changes_Works()
		{
			const string baseCharacters =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters type='index'>[A B C D E F G H I J K L M N O P Q R S T U V W X Y Z]</exemplarCharacters> 
		<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
		<ellipsis type='initial'>…{ 0}</ellipsis>
	</characters>
	</ldml>";

			const string oursDeletesMFromIndex =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
			<exemplarCharacters type='index'>[A B C D E F G H I J K L N O P Q R S T U V W X Y Z]</exemplarCharacters> 
			<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
			<ellipsis type='final'>{0}…</ellipsis>
			<ellipsis type='initial'>…{ 0}</ellipsis>
			<moreInformation>?</moreInformation>
	</characters>
</ldml>";
			const string theirsAddsChDigraphToExemplar =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters type='index'>[A B C D E F G H I J K L M N O P Q R S T U V W X Y Z]</exemplarCharacters> 
		<exemplarCharacters>[a b c [ch] d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
		<ellipsis type='final'>{0}…</ellipsis>
		<ellipsis type='initial'>…{ 0}</ellipsis>
		<moreInformation>?</moreInformation>
	 </characters>
</ldml>";
			var namespaces = new Dictionary<string, string>
			{
				{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
				{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"},
				{"sil", "urn://www.sil.org/ldml/0.1" }
			};

			DoMerge(baseCharacters, oursDeletesMFromIndex, theirsAddsChDigraphToExemplar,
				namespaces,
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[@type='auxiliary']",
					@"ldml/characters/exemplarCharacters[@type='index' and text()='[A B C D E F G H I J K L N O P Q R S T U V W X Y Z]']",
					@"ldml/characters/exemplarCharacters[text()='[a b c [ch] d e f g h i j k l m n o p q r s t u v w x y z]']",
					@"ldml/characters/ellipsis[@type='final']",
					@"ldml/characters/ellipsis[@type='initial']",
					@"ldml/characters/moreInformation"
				},
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[text()='[a b c d e f g h i j k l m n o p q r s t u v w x y z]']"
				},
				0, null,
				5, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlChangedRecordReport),
					typeof(XmlChangedRecordReport), typeof(XmlTextBothAddedReport), typeof(XmlTextBothAddedReport) });
		}

		[Test]
		public void Merging_Both_Change_AttributelessExemplar_Works()
		{
			const string baseCharacters =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters>[a b c d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
	</characters>
	</ldml>";

			const string oursAddsChDigraph =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters>[a b c [ch] d e f g h i j k l m n o p q r s t u v w x y z]</exemplarCharacters>
	</characters>
</ldml>";
			const string theirsDeletesM =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<characters>
		<exemplarCharacters type='auxiliary'>[á à ă â å ä ã ā æ ç é è ĕ ê ë ē í ì ĭ î ï ī ñ ó ò ŏ ô ö ø ō œ ú ù ŭ û ü ū ÿ]</exemplarCharacters>
		<exemplarCharacters>[a b c d e f g h i j k l n o p q r s t u v w x y z]</exemplarCharacters>
	</characters>
</ldml>";
			var namespaces = new Dictionary<string, string>
			{
				{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
				{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"},
				{"sil", "urn://www.sil.org/ldml/0.1" }
			};

			DoMerge(baseCharacters, oursAddsChDigraph, theirsDeletesM,
				namespaces,
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[@type='auxiliary']",
					@"ldml/characters/exemplarCharacters[text()='[a b c [ch] d e f g h i j k l m n o p q r s t u v w x y z]']", // ours wins
				},
				new List<string>
				{
					@"ldml/characters/exemplarCharacters[text()='[a b c d e f g h i j k l m n o p q r s t u v w x y z]']", // original is gone
					@"ldml/characters/exemplarCharacters[text()='[a b c d e f g h i j k l n o p q r s t u v w x y z]']" // theirs loses and is gone
				},
				1, new List<Type> { typeof(BothEditedTheSameAtomicElement) }, 
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		[Test]
		public void DelimitersAreMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<delimiters>
		<quotationStart>«</quotationStart>
		<alternateQuotationStart>“</alternateQuotationStart>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:quotation-marks paraContinueType='all'>
				<sil:quotationContinue>»</sil:quotationContinue>
				<sil:quotation level='3' open='‘' close='’' continue='’'/>
				<sil:quotation type='narrative' level='1' open='—'/>
			</sil:quotation-marks>
		</special>
	</delimiters>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<delimiters>
		<quotationEnd>»</quotationEnd>
		<alternateQuotationEnd>»</alternateQuotationEnd>
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:quotation-marks paraContinueType='all'>
				<sil:alternateQuotationContinue>”</sil:alternateQuotationContinue>
			</sil:quotation-marks>
		</special>
	</delimiters>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/delimiters/quotationStart[text()='«']",
					@"ldml/delimiters/alternateQuotationStart[text()='“']",
					@"ldml/delimiters/special/sil:quotation-marks[@paraContinueType = 'all']/sil:quotationContinue[text()='»']",
					@"ldml/delimiters/special/sil:quotation-marks[@paraContinueType = 'all']/sil:quotation[@level='3' and @open='‘' and @close='’' and @continue='’']",
					@"ldml/delimiters/special/sil:quotation-marks[@paraContinueType = 'all']/sil:quotation[@type='narrative' and @level='1' and @open='—']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				1, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport)
				});
		}

		[Test]
		public void LayoutIsMerged()
		{
			string commonAncestor = 
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
			<lineOrder>top-to-bottom</lineOrder>
		</orientation>
	</layout>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>right-to-left</characterOrder>
		</orientation>
	</layout>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
		<script type='Latn' />
	</identity>
	<layout>
		<orientation>
			<characterOrder>left-to-right</characterOrder>
			<lineOrder>bottom-to-top</lineOrder>
		</orientation>
	</layout>
</ldml>".Replace("'", "\"");

			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/layout/orientation/characterOrder[text()='right-to-left']",
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				1, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport),
				});
		}

		[Test]
		public void NumbersIsMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<numbers>
	</numbers>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<numbers>
		<defaultNumberingSystem>standard</defaultNumberingSystem>
		<numberingSystem id='standard' type='numeric'>Latn</numberingSystem>
	</numbers>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<numbers>
		<defaultNumberingSystem>nonStandard</defaultNumberingSystem>
		<numberingSystem id='nonStandard' type='numeric'>Thai</numberingSystem>
	</numbers>
</ldml>".Replace("'", "\"");

			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/numbers/defaultNumberingSystem[text()='standard']",
					@"ldml/numbers/numberingSystem[@id='standard' and @type='numeric' and text()='Latn']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				1, new List<Type>
				{
					typeof(XmlAttributeBothMadeSameChangeReport),
				});
		}

		#endregion

		#region Top level SIL:Special Elements

		[Test]
		public void KnownKeyboards_AreMergedV3()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<somethingHere />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:kbd id='Compiled Keyman9'>
				<sil:url>http://wirl.scripts.sil.org/ourKeyman9</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman9</sil:url>
			</sil:kbd>
			<sil:kbd id='Compiled Keyman9' alt='draft'>
				<sil:url>http://wirl.scripts.sil.org/ourKeyman9Draft</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman9Draft</sil:url>
			</sil:kbd>
			<sil:kbd id='Compiled Keyman10'>
				<sil:url>http://wirl.scripts.sil.org/ourKeyman10</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman10</sil:url>
			</sil:kbd>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
				string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:kbd id='Compiled Keyman9' type='kmx'>
				<sil:url>https://not.included.org</sil:url>
				<sil:url>http://also.not.included.org</sil:url>
			</sil:kbd>
			<sil:kbd id='Compiled Keyman11' type='kmx'>
				<sil:url>http://wirl.scripts.sil.org/theirKeyman11</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirKeyman11</sil:url>
			</sil:kbd>
			<somethingElse />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and not(@alt)]/sil:url[text()='http://wirl.scripts.sil.org/ourKeyman9']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and not(@alt)]/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman9']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @alt='draft']/sil:url[text()='http://wirl.scripts.sil.org/ourKeyman9Draft']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman9' and @alt='draft']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman9Draft']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman10']/sil:url[text()='http://wirl.scripts.sil.org/ourKeyman10']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman10']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=ourKeyman10']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman11']/sil:url[text()='http://wirl.scripts.sil.org/theirKeyman11']",
					@"ldml/special/sil:external-resources/sil:kbd[@id='Compiled Keyman11']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=theirKeyman11']"
				},
				new List<string>(0),
				2, new List<Type>
				{
					typeof(AmbiguousInsertConflict),
					typeof(BothEditedTheSameAtomicElement)
				},
				6, new List<Type> 
				{ 
					typeof(XmlAttributeBothMadeSameChangeReport), 
					typeof(XmlBothDeletionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport)
				});
		}

		[Test]
		public void FontsAreMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<somethingHere />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:31Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:font name='Padauk' types='default emphasis' size='2.0'>
				<sil:url>http://wirl.scripts.sil.org/ourPadauk</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=ourPadauk</sil:url>
			</sil:font>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:32Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:font name='Padauk' types='emphasis' size='3.0'>
				<sil:url>http://wirl.scripts.sil.org/theirPadauk</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirPadauk</sil:url>
			</sil:font>
			<sil:font name='Padauk' types='emphasis' size='34.0' alt='draft'>
				<sil:url>http://wirl.scripts.sil.org/theirPadaukDraft</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirPadaukDraft</sil:url>
			</sil:font>
			<sil:font name='Doulos' types='heading' size='4.0'>
				<sil:url>http://wirl.scripts.sil.org/theirDoulos</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirDoulos</sil:url>
			</sil:font>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='default emphasis' and @size='2.0' and not(@alt)]/sil:url[text()='http://wirl.scripts.sil.org/ourPadauk']",
					@"/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='default emphasis' and @size='2.0' and not(@alt)]/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=ourPadauk']",
					@"/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='emphasis' and @size='34.0' and @alt='draft']/sil:url[text()='http://wirl.scripts.sil.org/theirPadaukDraft']",
					@"/ldml/special/sil:external-resources/sil:font[@name='Padauk' and @types='emphasis' and @size='34.0' and @alt='draft']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=theirPadaukDraft']",
					@"/ldml/special/sil:external-resources/sil:font[@name='Doulos' and @types='heading' and @size='4.0' and not(@alt)]/sil:url[text()='http://wirl.scripts.sil.org/theirDoulos']",
					@"/ldml/special/sil:external-resources/sil:font[@name='Doulos' and @types='heading' and @size='4.0' and not(@alt)]/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=theirDoulos']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof(BothEditedTheSameAtomicElement),
				},
				4, new List<Type> 
				{ 
					typeof(XmlAttributeBothMadeSameChangeReport), 
					typeof(XmlBothDeletionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport) 
				});
		}

		[Test]
		public void SpellchecksAreMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<somethingHere />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:spellcheck type='hunspell'>
				<sil:url>http://wirl.scripts.sil.org/ourHunspell</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=ourHunspell</sil:url>
			</sil:spellcheck>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:spellcheck type='hunspell'>
				<sil:url>http://wirl.scripts.sil.org/theirHunspell</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirHunspell</sil:url>
			</sil:spellcheck>
			<sil:spellcheck type='hunspell' alt='draft'>
				<sil:url>http://wirl.scripts.sil.org/theirHunspellDraft</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirHunspellDraft</sil:url>
			</sil:spellcheck>
			<sil:spellcheck type='wordlist'>
				<sil:url>http://wirl.scripts.sil.org/theirWordlist</sil:url>
				<sil:url>http://scripts.sil.org/cms/scripts/page.php?item_id=theirWordlist</sil:url>
			</sil:spellcheck>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell' and not(@alt)]/sil:url[text()='http://wirl.scripts.sil.org/ourHunspell']",
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell' and not(@alt)]/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=ourHunspell']",
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell' and @alt='draft']/sil:url[text()='http://wirl.scripts.sil.org/theirHunspellDraft']",
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='hunspell' and @alt='draft']/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=theirHunspellDraft']",
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='wordlist' and not(@alt)]/sil:url[text()='http://wirl.scripts.sil.org/theirWordlist']",
					@"/ldml/special/sil:external-resources/sil:spellcheck[@type='wordlist' and not(@alt)]/sil:url[text()='http://scripts.sil.org/cms/scripts/page.php?item_id=theirWordlist']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof(BothEditedTheSameAtomicElement),
				},
				4, new List<Type> 
				{ 
					typeof(XmlAttributeBothMadeSameChangeReport), 
					typeof(XmlBothDeletionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport) 
				});
		}

		[Test]
		public void TransformsAreMerged()
		{
			string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<somethingHere />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:transform from='ourFrom' to='ourTo' type='python' direction='forward' function='ourFunction' />
			<sil:transform from='ourFrom' to='ourTo' type='python' direction='forward' function='ourFunction' alt='ourAlt'/>
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='$Revision: 11161 $' />
		<generation date='2012-06-06T09:36:30Z' />
		<language type='en' />
	</identity>
	<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
		<sil:external-resources>
			<sil:transform from='ourFrom' to='ourTo' type='python' direction='forward' function='ourFunction' />
			<sil:transform from='ourFrom' to='ourTo' type='python' direction='forward' function='ourFunction' alt='theirAlt' />
			<sil:transform from='ourFrom' to='ourTo' type='python' direction='forward' function='theirFunction' alt='theirAlt'/>
			<sil:transform from='theirFrom' to='theirTo' type='perl' direction='backward' function='theirFunction' />
		</sil:external-resources>
	</special>
</ldml>".Replace("'", "\"");
			var namespaces = new Dictionary<string, string>
								{
									{"sil", "urn://www.sil.org/ldml/0.1"},
								};

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"/ldml/special/sil:external-resources/sil:transform[@from='ourFrom' and @to='ourTo' and @type='python' and @direction='forward' and @function='ourFunction' and not(@alt)]",
					@"/ldml/special/sil:external-resources/sil:transform[@from='ourFrom' and @to='ourTo' and @type='python' and @direction='forward' and @function='ourFunction' and @alt='ourAlt']",
					@"/ldml/special/sil:external-resources/sil:transform[@from='theirFrom' and @to='theirTo' and @type='perl' and @direction='backward' and @function='theirFunction']"
				},
				new List<string>(0),
				0, new List<Type>(0),
				7, new List<Type> 
				{ 
					typeof(XmlAttributeBothMadeSameChangeReport), 
					typeof(XmlBothDeletionChangeReport),
					typeof(XmlBothAddedSameChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport) 
				});
		}

		#endregion

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
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		[Test]
		public void KnownKeyboards_AreMergedV2()
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
				3, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
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
				3, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlBothAddedSameChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void GenerateDateAttr_IsPreMerged()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30Z' />
</identity>
</ldml>";

			var ourContent = commonAncestor.Replace("09:36:30", "09:37:30");
			// Make a meaningless format change to verify that it doesn't break the logic which tests if there
			// are other real changes to the xml
			ourContent = ourContent.Replace($"/>{Environment.NewLine}</identity>", "/></identity>");
			var theirContent = commonAncestor.Replace("09:36:30", "09:38:30");
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:38:30Z']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30Z']", @"ldml/identity/generation[@date='2012-06-08T09:37:30Z']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });

			// They made the change
			DoMerge(commonAncestor, theirContent, ourContent,
				namespaces,
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:38:30Z']" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30Z']", @"ldml/identity/generation[@date='2012-06-08T09:37:30Z']" },
				0, null,
				1, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport) });
		}

		[Test]
		public void PreMergeCollationDoesNotDisruptDateOrLoseTheirChanges()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30Z' />
</identity>
<collations>
<collation></collation>
</collations>
</ldml>";

			var ourContent = commonAncestor.Replace("09:36:30", "09:37:30");
			var theirContent = commonAncestor.Replace("09:36:30", "09:38:30").Replace("</collations>", "</collations><special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1'><fw:windowsLCID value='1' /></special>");
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			// We made the change
			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string> { $"ldml/identity/generation[@date='{_expectedUtcDateTime.ToISO8601TimeFormatWithUTCString()}']", @"ldml/special" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30Z']", @"ldml/identity/generation[@date='2012-06-08T09:37:30Z']" },
				0, null,
				2, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlAdditionChangeReport) });

			// They made the change
			DoMerge(commonAncestor, theirContent, ourContent,
				namespaces,
				new List<string> { $"ldml/identity/generation[@date='{_expectedUtcDateTime.ToISO8601TimeFormatWithUTCString()}']", @"ldml/special" },
				new List<string> { @"ldml/identity/generation[@date='2012-06-08T09:36:30Z']", @"ldml/identity/generation[@date='2012-06-08T09:37:30Z']" },
				0, null,
				2, new List<Type> { typeof(XmlAttributeBothMadeSameChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void PreMergeDoesNotThrowWhenCommonIsEmptyAndBothAdded()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity>
<generation date='2012-06-08T09:36:30Z' />
</identity>
<collations>
<collation></collation>
</collations>
</ldml>";
			var namespaces = new Dictionary<string, string>
								{
									{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
									{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"}
								};

			Assert.DoesNotThrow( ()=> DoMerge(null, data, data,
				namespaces,
				new List<string> { $"ldml/identity/generation[@date='{_expectedUtcDateTime.ToISO8601TimeFormatWithUTCString()}']" },
				new List<string>(),
				0, null,
				1, new List<Type> {typeof (XmlBothAddedSameChangeReport)}));
		}

		[Test]
		public void PreMerge_DoesNotCrashIfChangesAreMissingCollation()
		{
			const string baseHasCollationMissingDefaultAttr =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version
			number='' />
		<generation
			date='2018-02-19T18:03:34' />
		<language
			type='fr' />
	</identity>
	<collations>
		<collation>
			<base>
				<alias
					source='fr' />
			</base>
			<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1'>
				<palaso:sortRulesType
					value='OtherLanguage' />
			</special>
		</collation>
	</collations>
</ldml>";

			const string oursMissingCollation =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='2018-02-19T19:48:02Z' />
		<language type='fr' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity windowsLCID='4108' />
		</special>
	</identity>
</ldml>";
			const string theirsMissingCollations =
				@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
	<identity>
		<version number='' />
		<generation date='2018-02-19T19:21:59Z' />
		<language type='fr' />
		<special xmlns:sil='urn://www.sil.org/ldml/0.1'>
			<sil:identity windowsLCID='4108' />
		</special>
	</identity>
</ldml>";
			var namespaces = new Dictionary<string, string>
			{
				{"palaso", "urn://palaso.org/ldmlExtensions/v1"},
				{"fw", "urn://fieldworks.sil.org/ldmlExtensions/v1"},
				{"sil", "urn://www.sil.org/ldml/0.1" }
			};

			Assert.DoesNotThrow(() => DoMerge(baseHasCollationMissingDefaultAttr, oursMissingCollation, theirsMissingCollations,
				namespaces,
				new List<string>(),
				new List<string>(),
				0, null, 
				3, new List<Type> { typeof(XmlBothDeletionChangeReport), typeof(XmlAttributeBothMadeSameChangeReport) , typeof(XmlBothAddedSameChangeReport) }));

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
