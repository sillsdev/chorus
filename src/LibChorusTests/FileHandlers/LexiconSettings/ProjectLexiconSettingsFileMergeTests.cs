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
	/// Test the merge capabilities of the ProjectLexiconSettingsFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class ProjectLexiconSettingsFileMergeTests
	{
		private IChorusFileTypeHandler _projectLexiconSettingsFileHandler;
		private ListenerForUnitTests _eventListener;

		private static string CommonAncestorTemplate(string element)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
	<WritingSystems addToSldr='true'>
		<WritingSystem id='grc'>
			<{0}>grc</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>Sen</{0}>
		</WritingSystem>
	</WritingSystems>
</ProjectLexiconSettings>".Replace("'", "\""), element);
		}

		private static string OurContentTemplate(string element)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
	<WritingSystems addToSldr='true'>
		<WritingSystem id='grc'>
			<{0}>grc</{0}>
		</WritingSystem>
		<WritingSystem id='hbo'>
			<{0}>hbo</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>our-Sen</{0}>
		</WritingSystem>
	</WritingSystems>
</ProjectLexiconSettings>".Replace("'", "\""), element);
		}

		private static string TheirContentTemplate(string element)
		{
			return string.Format(
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
	<WritingSystems addToSldr='true'>
		<WritingSystem id='grc'>
			<{0}>grc</{0}>
		</WritingSystem>
		<WritingSystem id='seh-fonipa-x-etic'>
			<{0}>their-Sen</{0}>
		</WritingSystem>
		<WritingSystem id='seh'>
			<{0}>Sen</{0}>
		</WritingSystem>
	</WritingSystems>
</ProjectLexiconSettings>".Replace("'", "\""), element);
		}

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_projectLexiconSettingsFileHandler =
				(from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
					where handler.GetType().Name == "ProjectLexiconSettingsFileHandler"
					select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_projectLexiconSettingsFileHandler = null;
			_eventListener = null;
		}

		[Test]
		public void CannotMergeNonexistantFile()
		{
			Assert.IsFalse(_projectLexiconSettingsFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void CanMergeGoodProjectLexiconSettingsFile()
		{
			using (var tempFile = TempFile.WithExtension(".plsx"))
			{
				File.WriteAllText(tempFile.Path, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<ProjectLexiconSettings />");
				Assert.IsTrue(_projectLexiconSettingsFileHandler.CanMergeFile(tempFile.Path));
			}
		}

		[Test]
		public void CannotMergeEmptyStringFile()
		{
			Assert.IsFalse(_projectLexiconSettingsFileHandler.CanMergeFile(String.Empty));
		}

		[Test]
		public void CannotMergeNullFile()
		{
			Assert.IsFalse(_projectLexiconSettingsFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Do3WayMerge_NullInput_Throws()
		{
			Assert.Throws<ArgumentNullException>(() => _projectLexiconSettingsFileHandler.Do3WayMerge(null));
		}

		[Test]
		public void DuplicateWritingSystemsElementsAreRemoved()
		{
			const string badData =
@"<ProjectLexiconSettings>
<WritingSystems addToSldr='true' />
<WritingSystems addToSldr='true' goner='true' />
</ProjectLexiconSettings>";
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
		public void WritingSystemAbbreviationElements_AreMerged()
		{
			const string element = "Abbreviation";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);
			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/Abbreviation['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/Abbreviation['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/Abbreviation['our-Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/Abbreviation['Sen']"
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
		public void WritingSystemLanguageNameElements_AreMerged()
		{
			const string element = "LanguageName";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/LanguageName['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/LanguageName['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/LanguageName['our-Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/LanguageName['Sen']"
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
		public void WritingSystemScriptNameElements_AreMerged()
		{
			const string element = "ScriptName";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/ScriptName['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/ScriptName['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/ScriptName['our-Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/ScriptName['Sen']"
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
		public void WritingSystemRegionNameElements_AreMerged()
		{
			const string element = "RegionName";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string> ();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/RegionName['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/RegionName['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/RegionName['our-Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/RegionName['Sen']"
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
		public void WritingSystemSpellCheckingIdElements_AreMerged()
		{
			const string element = "SpellCheckingId";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string> ();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/SpellCheckingId['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/SpellCheckingId['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/SpellCheckingId['our_Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/SpellCheckingId['Sen']"
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
		public void WritingSystemLegacyMappingElements_AreMerged()
		{
			const string element = "LegacyMapping";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string> ();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/LegacyMapping['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/LegacyMapping['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/LegacyMapping['our_Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/LegacyMapping['Sen']"
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
		public void WritingSystemKeyboardElements_AreMerged()
		{
			const string element = "Keyboard";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string> ();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/Keyboard['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/Keyboard['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/Keyboard['our_Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/Keyboard['Sen']"
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
		public void WritingSystemSystemCollationElements_AreMerged()
		{
			const string element = "SystemCollation";
			string commonAncestor = CommonAncestorTemplate(element);
			string ourContent = OurContentTemplate(element);
			string theirContent = TheirContentTemplate(element);

			var namespaces = new Dictionary<string, string>();

			DoMerge(commonAncestor, ourContent, theirContent,
				namespaces,
				new List<string>
				{
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='grc']/SystemCollation['grc']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='hbo']/SystemCollation['hbo']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh-fonipa-x-etic']/SystemCollation['our_Sen']",
					@"ProjectLexiconSettings/WritingSystems/WritingSystem[@id='seh']/SystemCollation['Sen']"
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

				_projectLexiconSettingsFileHandler.Do3WayMerge(mergeOrder);
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
