using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.merge;
using LibChorus.Tests.merge.xml;
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
			var goodXmlPathname = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
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
<rt guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("</languageproject>", "<rt guid='newbieOurs'/></languageproject>");
			var theirContent = commonAncestor.Replace("</languageproject>", "<rt guid='newbieTheirs'/></languageproject>");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieOurs""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieTheirs""]");
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("</languageproject>", "<rt guid='newbieOurs'/></languageproject>");
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieOurs""]");
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
</languageproject>";
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</languageproject>", "<rt guid='newbieTheirs'/></languageproject>");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""newbieTheirs""]");
		}

		[Test]
		public void WinnerDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='goner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("<rt guid='goner'/>", null);
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
		}

		[Test]
		public void LoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='goner'/>
</languageproject>";
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<rt guid='goner'/>", null);

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
		}

		[Test]
		public void WinnerAndLoserBothDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='goner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("<rt guid='goner'/>", null);
			var theirContent = commonAncestor.Replace("<rt guid='goner'/>", null);

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@guid=""goner""]");
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
		}

		[Test]
		public void WinnerAndLoserBothChangedElementButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newWinningOwner");
			var theirContent = commonAncestor.Replace("originalOwner", "newLosingOwner");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newWinningOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""newLosingOwner""]");
		}

		[Test]
		public void WinnerChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor.Replace("originalOwner", "newOwner");
			var theirContent = commonAncestor;

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
		}

		[Test]
		public void LoserChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt guid='oldie'/>
<rt guid='dirtball' ownerguid='originalOwner'/>
</languageproject>";
			var ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("originalOwner", "newOwner");

			var result = DoMerge(commonAncestor, ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@guid=""oldie""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"languageproject/rt[@ownerguid=""newOwner""]");
			XmlTestHelper.AssertXPathIsNull(result, @"languageproject/rt[@ownerguid=""originalOwner""]");
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

				m_fwFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
			}
			return result;
		}
	}
}