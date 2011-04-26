using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

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
		public void CannotMergeNonexistantFile()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void CannotMergeEmptyStringFile()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile(String.Empty));
		}

		[Test]
		public void CanMergeGoodFwXmlFile()
		{
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".fwdata");
			try
			{
// ReSharper disable LocalizableElement
				File.WriteAllText(goodXmlPathname, "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine + "<fwdata />");
// ReSharper restore LocalizableElement
				Assert.IsTrue(_fwFileHandler.CanMergeFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void CannotMergeNullFile()
		{
			Assert.IsFalse(_fwFileHandler.CanMergeFile(null));
		}

		[Test]
		public void Do3WayMergeThrowsOnNUllInput()
		{
			Assert.Throws<NullReferenceException>(() => _fwFileHandler.Do3WayMerge(null));
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("</fwdata>", "<rt class='LexEntry' guid='newbieOurs'/></fwdata>");
			var theirContent = commonAncestor.Replace("</fwdata>", "<rt class='LexEntry' guid='newbieTheirs'/></fwdata>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@guid=""newbieOurs""]", @"fwdata/rt[@guid=""newbieTheirs""]" }, null,
				0, 2);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("</fwdata>", "<rt class='LexEntry' guid='newbieOurs'/></fwdata>");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@guid=""newbieOurs""]" }, null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
</fwdata>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</fwdata>", "<rt class='LexEntry' guid='newbieTheirs'/></fwdata>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@guid=""newbieTheirs""]" }, null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void WinnerDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]" },
				new List<string> { @"fwdata/rt[@guid=""goner""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void LoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</fwdata>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]" },
				new List<string> { @"fwdata/rt[@guid=""goner""]" },
				0, 0);
		}

		[Test]
		public void WinnerAndLoserBothDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='goner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]" },
				new List<string> { @"fwdata/rt[@guid=""goner""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void WinnerAndLoserBothChangedElementButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("originalOwner", "newWinningOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newLosingOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newWinningOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]", @"fwdata/rt[@ownerguid=""newLosingOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<BothEditedAttributeConflict>();
		}

		[Test]
		public void WinnerChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt		class='LexEntry' guid='oldie'/>
<rt
	class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void LoserChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void WinnerEditedButLoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
		}

		[Test]
		public void WinnerDeletedButLoserEditedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='oldie'/>
<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>
</fwdata>";
			var ourContent = commonAncestor.Replace("<rt class='LexEntry' guid='dirtball' ownerguid='originalOwner'/>", null);
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt[@guid=""oldie""]", @"fwdata/rt[@ownerguid=""newOwner""]" },
				new List<string> { @"fwdata/rt[@ownerguid=""originalOwner""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		[Test]
		public void RemovePartOfMultiString()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Comment/AStr[@ws='en']", @"fwdata/rt/Comment/AStr/Run[@ws='en']" },
				new List<string> { @"fwdata/rt/Comment/AStr/Run[@ws='es']" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void EditDifferentPartsOfMultiStringGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variantNew </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>varianteNew</Run>
</AStr>
</Comment>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Comment/AStr[@ws='en']",
					@"fwdata/rt/Comment/AStr/Run[@ws='en']",
					@"fwdata/rt/Comment/AStr/Run[@ws='es']" },
				null,
				1, 0);

			var doc = XDocument.Parse(result);
// ReSharper disable PossibleNullReferenceException
			var commentElement = doc.Element("fwdata").Element("rt").Element("Comment");
			var enAlt = commentElement.Element("AStr");
			var runs = enAlt.Descendants("Run");
// ReSharper restore PossibleNullReferenceException
			Assert.AreEqual("variantNew ", runs.ElementAt(0).Value);
			Assert.AreEqual("variante", runs.ElementAt(1).Value);
		}

		[Test]
		public void EditDifferentPartsOfMultiStringGeneratesConflictReportButNewAltAddedWithChangeReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variant </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='LexEntry' guid='9bffac9d-432a-43ce-a947-8e9f93074d65'>
<Comment>
<AStr ws='en'>
<Run ws='en'>variantNew </Run>
<Run ws='es'>variante</Run>
</AStr>
</Comment>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
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
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Comment/AStr[@ws='en']",
					@"fwdata/rt/Comment/AStr[@ws='en']/Run[@ws='en']",
					@"fwdata/rt/Comment/AStr[@ws='en']/Run[@ws='es']",
					@"fwdata/rt/Comment/AStr[@ws='es']",
					@"fwdata/rt/Comment/AStr[@ws='es']/Run[@ws='es']" },
				null,
				1, 1); // 1 conflict, since both edited the 'en' alternative: 1 change, since 'they' added the new 'es' altenative.

			var doc = XDocument.Parse(result);
// ReSharper disable PossibleNullReferenceException
			var commentElement = doc.Element("fwdata").Element("rt").Element("Comment");
			var enAlt = commentElement.Element("AStr");
			var runs = enAlt.Descendants("Run");
// ReSharper restore PossibleNullReferenceException
			Assert.AreEqual("variantNew ", runs.ElementAt(0).Value);
			Assert.AreEqual("variante", runs.ElementAt(1).Value);
		}

		[Test]
		public void BothEditMultuUnicodePropertyGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech We Changed</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech They Changed</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Name/AUni[@ws='en']",
					@"fwdata/rt/Name/AUni[@ws='es']",
					@"fwdata/rt/Name/AUni[@ws='fr']"},
				null,
				1, 0); // 1 conflict, since both edited the 'en' alternative: 0 changes.

			var doc = XDocument.Parse(result);
// ReSharper disable PossibleNullReferenceException
			var nameElement = doc.Element("fwdata").Element("rt").Element("Name");
			var enAlt = (from auniElement in nameElement.Elements("AUni")
							where auniElement.Attribute("ws").Value == "en"
							select auniElement).First();
			Assert.AreEqual("Parts Of Speech We Changed", enAlt.Value);
// ReSharper restore PossibleNullReferenceException
		}

		[Test]
		public void EachDeletedOneAltWithOneChangeReported()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='fr'>Parties du Discours</AUni>
</Name>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmPossibilityList' guid='d72a1748-be3b-4164-9858-bc99de37e434' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
<Name>
<AUni ws='en'>Parts Of Speech</AUni>
<AUni ws='es'>Categorías Gramáticas</AUni>
</Name>
</rt>
</fwdata>";

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Name/AUni[@ws='en']" },
				new List<string> { @"fwdata/rt/Name/AUni[@ws='es']",
					@"fwdata/rt/Name/AUni[@ws='fr']" },
				0, 1);
		}

		[Test]
		public void BothEditedTsStringWhichReturnsAConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='PunctuationForm' guid='81bf4802-e411-42f7-98c7-319b13ed2e0b'>
	<Form>
		<Str>
			<Run ws='x-ezpi'>.</Run>
		</Str>
	</Form>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='PunctuationForm' guid='81bf4802-e411-42f7-98c7-319b13ed2e0b'>
	<Form>
		<Str>
			<Run ws='x-ezpi'>!</Run>
		</Str>
	</Form>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='PunctuationForm' guid='81bf4802-e411-42f7-98c7-319b13ed2e0b'>
	<Form>
		<Str>
			<Run ws='x-ezpi'>?</Run>
		</Str>
	</Form>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Form/Str/Run[@ws='x-ezpi']" },
				null,
				1, 0);

			var doc = XDocument.Parse(result);
// ReSharper disable PossibleNullReferenceException
			var runElement = doc.Element("fwdata").Element("rt").Element("Form").Element("Str").Element("Run");
			Assert.AreEqual("!", runElement.Value);
// ReSharper restore PossibleNullReferenceException
		}

		[Test]
		public void BothEditedTxtPropWhichReturnsAConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='StStyle' guid='ee3c33fa-8141-4efe-a2c5-3e867c554b07' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
	<Rules>
		<Prop firstIndent='-36000' leadingIndent='9000' spaceBefore='1000' spaceAfter='2000' />
	</Rules>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='StStyle' guid='ee3c33fa-8141-4efe-a2c5-3e867c554b07' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
	<Rules>
		<Prop firstIndent='-36000' leadingIndent='10000' spaceBefore='1000' spaceAfter='2000' />
	</Rules>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='StStyle' guid='ee3c33fa-8141-4efe-a2c5-3e867c554b07' ownerguid='9719a466-2240-4dea-9722-9fe0746a30a6'>
	<Rules>
		<Prop firstIndent='-36000' leadingIndent='9000' spaceBefore='1000' spaceAfter='2000' bold='true' />
	</Rules>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Rules/Prop" },
				null,
				1, 0);

			var doc = XDocument.Parse(result);
// ReSharper disable PossibleNullReferenceException
			var propElement = doc.Element("fwdata").Element("rt").Element("Rules").Element("Prop");
			Assert.AreEqual("10000", propElement.Attribute("leadingIndent").Value);
			Assert.IsNull(propElement.Attribute("bold"));
// ReSharper restore PossibleNullReferenceException
		}

		[Test]
		public void BothEditedAtomicReferenceProducesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmTranslation' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Type>
<objsur t='r' guid='original' />
</Type>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmTranslation' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Type>
<objsur t='r' guid='ourNew' />
</Type>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmTranslation' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Type>
<objsur t='r' guid='theirNew' />
</Type>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Type/objsur[@guid='ourNew']" },
				new List<string> { @"fwdata/rt/Type/objsur[@guid='original']", @"fwdata/rt/Type/objsur[@guid='theirNew']" },
				1, 0);
		}

		[Test]
		public void BothEditedReferenceSequenceGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Analyses>
<objsur t='r' guid='original1' />
<objsur t='r' guid='original2' />
<objsur t='r' guid='original3' />
</Analyses>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Analyses>
<objsur t='r' guid='ourNew1' />
<objsur t='r' guid='ourNew2' />
</Analyses>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Analyses>
<objsur t='r' guid='theirNew1' />
</Analyses>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Analyses/objsur[@guid='ourNew1']", @"fwdata/rt/Analyses/objsur[@guid='ourNew2']" },
				new List<string> { @"fwdata/rt/Analyses/objsur[@guid='original1']", @"fwdata/rt/Analyses/objsur[@guid='original2']", @"fwdata/rt/Analyses/objsur[@guid='original3']",
					@"fwdata/rt/Analyses/objsur[@guid='theirNew1']" },
				1, 0);
		}

		[Test]
		public void BothEditedOwningSequenceGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Notes>
<objsur t='r' guid='original1' />
<objsur t='r' guid='original2' />
<objsur t='r' guid='original3' />
</Notes>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Notes>
<objsur t='r' guid='ourNew1' />
<objsur t='r' guid='ourNew2' />
</Notes>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='Segment' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<Notes>
<objsur t='r' guid='theirNew1' />
</Notes>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/Notes/objsur[@guid='ourNew1']", @"fwdata/rt/Notes/objsur[@guid='ourNew2']" },
				new List<string> { @"fwdata/rt/Notes/objsur[@guid='original1']", @"fwdata/rt/Notes/objsur[@guid='original2']", @"fwdata/rt/Notes/objsur[@guid='original3']",
					@"fwdata/rt/Notes/objsur[@guid='theirNew1']" },
				1, 0);
		}

		[Test]
		public void BothEditedOwningCollectionGeneratesConflictReport()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmFolder' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<SubFolders>
<objsur t='r' guid='original1' />
<objsur t='r' guid='original2' />
<objsur t='r' guid='original3' />
</SubFolders>
</rt>
</fwdata>";
			const string ourContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmFolder' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<SubFolders>
<objsur t='r' guid='ourNew1' />
<objsur t='r' guid='ourNew2' />
</SubFolders>
</rt>
</fwdata>";
			const string theirContent =
@"<?xml version='1.0' encoding='utf-8'?>
<fwdata>
<rt class='CmFolder' guid='8e81ab31-31be-49e9-84ee-72a29f6ac50b' ownerguid='d9bc88e2-eeb3-4d99-91e4-99517ab1f9d4'>
<SubFolders>
<objsur t='r' guid='original1' />
<objsur t='r' guid='theirNew1' />
</SubFolders>
</rt>
</fwdata>";

			var result = DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"fwdata/rt/SubFolders/objsur[@guid='ourNew1']", @"fwdata/rt/SubFolders/objsur[@guid='ourNew2']", @"fwdata/rt/SubFolders/objsur[@guid='theirNew1']" },
				new List<string> { @"fwdata/rt/SubFolders/objsur[@guid='original1']", @"fwdata/rt/SubFolders/objsur[@guid='original2']", @"fwdata/rt/SubFolders/objsur[@guid='original3']" },
				0, 0);
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