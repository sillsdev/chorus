using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the merge capabilities of the FieldWorksFileHandler implementation of the IChorusFileTypeHandler interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksFileMergeTests
	{
		private IChorusFileTypeHandler _fwFileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fwFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
							   where handler.GetType().Name == "FieldWorksFileHandler"
							   select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fwFileHandler = null;
		}

		[Test]
		public void Cannot_Merge_Nonexistant_File()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Merge_Empty_String_File()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile(String.Empty));
		}

		[Test]
		public void Can_Merge_Good_Fw_Xml_File()
		{
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".fwdata");
			try
			{
// ReSharper disable LocalizableElement
				File.WriteAllText(goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<languageproject version='7000016' />");
// ReSharper restore LocalizableElement
				Assert.IsTrue(_fwFileHandler.CanMergeFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void Cannot_Merge_Null_File()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Do3WayMerge_Throws()
		{
			Assert.Throws<NullReferenceException>(() => _fwFileHandler.Do3WayMerge(null));
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieOurs'/></languageproject>");
			var theirContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieTheirs'/></languageproject>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@guid=""newbieOurs""]", @"languageproject/rt[@guid=""newbieTheirs""]"}, null,
				0, 0);
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			// Add the optional AdditionalFields element to flush out a merge problem,
			// and ensure it stays fixed.
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>
<rt class='LexEntry' guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieOurs'/></languageproject>");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@guid=""newbieOurs""]" }, null,
				0, 0);
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
</languageproject>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieTheirs'/></languageproject>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@guid=""newbieTheirs""]" }, null,
				0, 0);
		}

		[Test]
		public void WinnerDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]" },
				new List<string> { @"languageproject/rt[@guid=""goner""]" },
				0, 0);
		}

		[Test]
		public void LoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</languageproject>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]" },
				new List<string> { @"languageproject/rt[@guid=""goner""]" },
				0, 0);
		}

		[Test]
		public void WinnerAndLoserBothDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]" },
				new List<string> { @"languageproject/rt[@guid=""goner""]" },
				0, 0);
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]",  @"languageproject/rt[@ownerguid=""newOwner""]"},
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]" },
				0, 0);
		}

		[Test]
		public void WinnerAndLoserBothChangedElementButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newWinningOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newLosingOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@ownerguid=""newWinningOwner""]" },
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]", @"languageproject/rt[@ownerguid=""newLosingOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<BothEditedAttributeConflict>();
		}

		[Test]
		public void WinnerChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt		class='LexEntry' guid='oldie'/>
<rt
	class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]" },
				0, 0);
		}

		[Test]
		public void LoserChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]" },
				0, 0);
		}

		[Test]
		public void WinnerEditedButLoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
		}

		[Test]
		public void WinnerDeletedButLoserEditedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>", null);
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt[@guid=""oldie""]", @"languageproject/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"languageproject/rt[@ownerguid=""originalOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		[Test]
		public void AddNewCustomProperty()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='original'/>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='original'/>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>
<rt class='LexEntry' guid='original'/>
</languageproject>";

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/AdditionalFields", @"languageproject/AdditionalFields/CustomField[@name=""Certified""]" },
				null,
				0, 0);
		}

		[Test]
		public void RemovePartOfMultiString()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt/Comment/AStr[@ws='en']", @"languageproject/rt/Comment/AStr/Run[@ws='en']" },
				new List<string> { @"languageproject/rt/Comment/AStr/Run[@ws='es']" },
				0, 0);
		}

		[Test]
		public void EditDifferentPartsOfMultiStringGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variantNew </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>varianteNew</Run>
</AStr>
</Comment>
</rt>
</languageproject>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt/Comment/AStr[@ws='en']",
					@"languageproject/rt/Comment/AStr/Run[@ws='en']",
					@"languageproject/rt/Comment/AStr/Run[@ws='es']" },
				null,
				1, 0);
			var doc = XDocument.Parse(result);
			var commentElement = doc.Element("languageproject").Element("rt").Element("Comment");
			var enAlt = commentElement.Element("AStr");
			var runs = enAlt.Descendants("Run");
			Assert.AreEqual("variantNew ", runs.ElementAt(0).Value);
			Assert.AreEqual("variante", runs.ElementAt(1).Value);
		}

		[Test]
		public void EditDifferentPartsOfMultiStringGeneratesConflictReport_ButNewAltAddedWithChangeReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variantNew </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>varianteNew</Run>
</AStr>
<AStr ws='es'>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</languageproject>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt/Comment/AStr[@ws='en']",
					@"languageproject/rt/Comment/AStr[@ws='en']/Run[@ws='en']",
					@"languageproject/rt/Comment/AStr[@ws='en']/Run[@ws='es']",
					@"languageproject/rt/Comment/AStr[@ws='es']",
					@"languageproject/rt/Comment/AStr[@ws='es']/Run[@ws='es']" },
				null,
				1, 1); // 1 conflict, since both edited the 'en' alternative: 1 change, since 'they' added the new 'es' altenative.
			var doc = XDocument.Parse(result);
			var commentElement = doc.Element("languageproject").Element("rt").Element("Comment");
			var enAlt = commentElement.Element("AStr");
			var runs = enAlt.Descendants("Run");
			Assert.AreEqual("variantNew ", runs.ElementAt(0).Value);
			Assert.AreEqual("variante", runs.ElementAt(1).Value);
		}

		[Test]
		public void BothEditMultuUnicodePropertyGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech We Changed</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech They Changed</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</languageproject>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt/Name/AUni[@ws='en']",
					@"languageproject/rt/Name/AUni[@ws='es']",
					@"languageproject/rt/Name/AUni[@ws='fr']"},
				null,
				1, 0); // 1 conflict, since both edited the 'en' alternative: 0 changes.
		}

		[Test]
		public void EachDeletedOneAltWithOneChangeReported()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</languageproject>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</languageproject>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
</Name>
</rt>
</languageproject>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"languageproject/rt/Name/AUni[@ws='en']" },
				new List<string> { @"languageproject/rt/Name/AUni[@ws='es']",
					@"languageproject/rt/Name/AUni[@ws='fr']" },
				0, 1);
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent,
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

				_fwFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
				foreach (var query in matchesExactlyOne)
					XmlTestHelper.AssertXPathMatchesExactlyOne(result, query);
				if (isNull != null)
				{
					foreach (var query in isNull)
						XmlTestHelper.AssertXPathIsNull(result, query);
				}
				_eventListener.AssertExpectedConflictCount(expectedConflictCount);
				_eventListener.AssertExpectedChangesCount(expectedChangesCount);
			}
			return result;
		}
	}
}