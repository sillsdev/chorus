using System;
using System.IO;
using System.Linq;
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
		private IChorusFileTypeHandler m_fwFileHandler;
		private ListenerForUnitTests m_eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_fwFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handers
							   where handler.GetType().Name == "FieldWorksFileHandler"
							   select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			m_fwFileHandler = null;
		}

		[Test]
		public void Cannot_Merge_Nonexistant_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanMergeFile("bogusPathname"));
		}

		[Test]
		public void Cannot_Merge_Empty_String_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanMergeFile(String.Empty));
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
				Assert.IsTrue(m_fwFileHandler.CanMergeFile(goodXmlPathname));
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void Cannot_Merge_Null_File()
		{
			Assert.IsFalse(m_fwFileHandler.CanMergeFile(null));
		}

		[Test, ExpectedException(typeof(NullReferenceException))]
		public void Do3WayMerge_Throws()
		{
			m_fwFileHandler.Do3WayMerge(null);
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieOurs""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieTheirs""]");
			m_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieOurs'/></languageproject>");
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieOurs""]");
			m_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</languageproject>", "<rt class='LexEntry' guid='newbieTheirs'/></languageproject>");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieTheirs""]");
			m_eventListener.AssertExpectedConflictCount(0);
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
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<rt class='LexEntry' guid='goner'/>", null);

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newWinningOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""newLosingOwner""]");
			m_eventListener.AssertExpectedConflictCount(1);
			m_eventListener.AssertFirstConflictType<BothEditedAttributeConflict>();
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
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			m_eventListener.AssertExpectedConflictCount(0);
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			m_eventListener.AssertExpectedConflictCount(1);
			m_eventListener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
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

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			m_eventListener.AssertExpectedConflictCount(1);
			m_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				m_eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = m_eventListener;

				m_fwFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
			}
			return result;
		}
	}
}