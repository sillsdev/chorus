using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.Settings;
using Chorus.FileTypeHandlers.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Test the merge capabilities of the UserLexiconSettingsFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class UserLexiconSettingsFileMergeTests
	{
		private IChorusFileTypeHandler _userLexiconSettingsFileHandler;
		private ListenerForUnitTests _eventListener;

		private static string CommonAncestorTemplate(string element, string grcValue, string sehIpaValue)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<UserLexiconSettings>
	<WritingSystems>
		<WritingSystem id='grc'>
			<{0}>{1}</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>{2}</{0}>
		</WritingSystem>
	</WritingSystems>
</UserLexiconSettings>".Replace("'", "\""), element, grcValue, sehIpaValue);
		}

		private static string OurContentTemplate(string element, string grcValue, string hboValue, string sehIpaValue)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<UserLexiconSettings>
	<WritingSystems>
		<WritingSystem id='grc'>
			<{0}>{1}</{0}>
		</WritingSystem>
		<WritingSystem id='hbo'>
			<{0}>{2}</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>{3}</{0}>
		</WritingSystem>
	</WritingSystems>
</UserLexiconSettings>".Replace("'", "\""), element, grcValue, hboValue, sehIpaValue);
		}

		private static string TheirContentTemplate(string element, string grcValue, string sehIpaValue, string sehValue)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<UserLexiconSettings>
	<WritingSystems>
		<WritingSystem id='grc'>
			<{0}>{1}</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>{2}</{0}>
		</WritingSystem>
		<WritingSystem id='seh'>
			<{0}>{3}</{0}>
		</WritingSystem>
	</WritingSystems>
</UserLexiconSettings>".Replace("'", "\""), element, grcValue, sehIpaValue, sehValue);
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_userLexiconSettingsFileHandler =
				(from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
				 where handler.GetType().Name == "UserLexiconSettingsFileHandler"
				 select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_userLexiconSettingsFileHandler = null;
			_eventListener = null;
		}

		[Test]
		public void CannotMergeNonexistantFile()
		{
			Assert.IsFalse(_userLexiconSettingsFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void CanMergeGoodUserLexiconSettingsFile()
		{
			using (var tempFile = TempFile.WithExtension(".ulsx"))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<UserLexiconSettings />");
				Assert.IsTrue(_userLexiconSettingsFileHandler.CanMergeFile(tempFile.Path));
			}
		}

		[Test]
		public void CannotMergeEmptyStringFile()
		{
			Assert.IsFalse(_userLexiconSettingsFileHandler.CanMergeFile(String.Empty));
		}

		[Test]
		public void CannotMergeNullFile()
		{
			Assert.IsFalse(_userLexiconSettingsFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Do3WayMerge_NullInput_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => _userLexiconSettingsFileHandler.Do3WayMerge(null));
		}

		[Test]
		public void DuplicateWritingSystemsElementsAreRemoved()
		{
			const string badData =
@"<UserLexiconSettings>
<WritingSystems />
<WritingSystems goner='true' />
</UserLexiconSettings>";
			var doc = new XmlDocument();
			var badRootNode = XmlUtilities.GetDocumentNodeFromRawXml(badData, doc);
			var merger = new XmlMerger(new NullMergeSituation());
			merger.EventListener = new ListenerForUnitTests();
			ProjectLexiconSettingsFileHandler.SetupElementStrategies(merger);
			var oldValue = XmlMergeService.RemoveAmbiguousChildNodes;
			XmlMergeService.RemoveAmbiguousChildNodes = true;
			XmlMergeService.RemoveAmbiguousChildren(merger.EventListener, merger.MergeStrategies, badRootNode);
			XmlMergeService.RemoveAmbiguousChildNodes = oldValue;
			var childNodes = badRootNode.SelectNodes("WritingSystems");
			Assert.IsNotNull(childNodes);
			Assert.IsTrue(childNodes.Count == 1);
			Assert.IsNull(childNodes[0].Attributes["goner"]);
		}

		[Test]
		public void WritingSystemLocalKeyboardElements_AreMerged()
		{
			const string element = "LocalKeyboard";
			string commonAncestor = CommonAncestorTemplate(element, "grc", "seh-ipa");
			string ourContent = OurContentTemplate(element, "grc", "hbo", "our_seh-ipa");
			string theirContent = TheirContentTemplate(element, "grc", "their_seh-ipa", "seh");
			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='grc']/LocalKeyboard['grc']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/LocalKeyboard['hbo']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/LocalKeyboard['our_seh-ipa']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh']/LocalKeyboard['seh']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				2, new List<Type>
				{
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
				});
		}

		[Test]
		public void WritingSystemDefaultFontNameElements_AreMerged()
		{
			const string element = "DefaultFontName";
			string commonAncestor = CommonAncestorTemplate(element, "GrcFont", "SehIpaFont");
			string ourContent = OurContentTemplate(element, "GrcFont", "HboFont", "OurSehIpaFont");
			string theirContent = TheirContentTemplate(element, "GrcFont", "TheirSehIpaFont", "SehFont");

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='grc']/DefaultFontName['GrcFont']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/DefaultFontName['HboFont']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/DefaultFontName['OurSehIpaFont']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh']/DefaultFontName['SehFont']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				2, new List<Type>
				{
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
				});
		}

		[Test]
		public void WritingSystemDefaultFontSizeElements_AreMerged()
		{
			const string element = "DefaultFontSize";
			string commonAncestor = CommonAncestorTemplate(element, "10", "11");
			string ourContent = OurContentTemplate(element, "10", "12", "13");
			string theirContent = TheirContentTemplate(element, "10", "14", "15");

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='grc']/DefaultFontSize['10']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/DefaultFontSize['12']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/DefaultFontSize['13']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh']/DefaultFontSize['15']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				2, new List<Type>
				{
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
				});
		}

		[Test]
		public void WritingSystemIsGraphiteEnabledElements_AreMerged()
		{
			const string element = "IsGraphiteEnabled";
			string commonAncestor = CommonAncestorTemplate(element, "true", "");
			string ourContent = OurContentTemplate(element, "true", "false", "true");
			string theirContent = TheirContentTemplate(element, "true", "false", "false");

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='grc']/IsGraphiteEnabled['true']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/IsGraphiteEnabled['false']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/IsGraphiteEnabled['false']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh']/IsGraphiteEnabled['false']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				2, new List<Type>
				{
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
				});
		}

		[Test]
		public void WritingSystemKnownKeyboardsElements_AreMerged()
		{
			const string element = "KnownKeyboards";
			string commonAncestor = CommonAncestorTemplate(element, "<KnownKeyboard>grc</KnownKeyboard>", "<KnownKeyboard>seh-ipa</KnownKeyboard>");
			string ourContent = OurContentTemplate(element, "<KnownKeyboard>grc</KnownKeyboard>", "<KnownKeyboard>hbo1</KnownKeyboard><KnownKeyboard>hbo2</KnownKeyboard>", "<KnownKeyboard>seh-ipa</KnownKeyboard><KnownKeyboard>our_seh-ipa</KnownKeyboard>");
			string theirContent = TheirContentTemplate(element, "<KnownKeyboard>grc</KnownKeyboard>", "<KnownKeyboard>their_seh-ipa</KnownKeyboard><KnownKeyboard>seh-ipa</KnownKeyboard>", "<KnownKeyboard>seh</KnownKeyboard>");

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='grc']/KnownKeyboards[KnownKeyboard='grc']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/KnownKeyboards[KnownKeyboard='hbo1' and KnownKeyboard='hbo2']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/KnownKeyboards[KnownKeyboard='seh-ipa' and KnownKeyboard='our_seh-ipa']",
					@"UserLexiconSettings/WritingSystems/WritingSystem[@id='seh']/KnownKeyboards[KnownKeyboard='seh']"
				},
				new List<string>(0),
				1, new List<Type>
				{
					typeof (BothEditedTheSameAtomicElement),
				},
				2, new List<Type>
				{
					typeof(XmlAdditionChangeReport),
					typeof(XmlAdditionChangeReport),
				});
		}

		private void DoMerge(string commonAncestor, string ourContent, string theirContent,
			Dictionary<string, string> namespaces,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				_eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = _eventListener;

				_userLexiconSettingsFileHandler.Do3WayMerge(mergeOrder);
				string result = File.ReadAllText(ours.Path);
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
		}
	}
}
