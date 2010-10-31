using System.Xml;
using Chorus.FileTypeHanders.FieldWorks;
using Chorus.merge;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.FieldWorks
{
	/// <summary>
	/// Test the merge override capabilities of the FieldWorksMergingStrategy implementation of the IMergeStrategy interface.
	/// </summary>
	[TestFixture]
	public class FieldWorksMergingStrategyTests
	{
		private ListenerForUnitTests _eventListener;
		private FieldWorksMergingStrategy _fwMergeStrategy;
		private MetadataCache _mdc;

		[SetUp]
		public void TestSetup()
		{
			_mdc = new MetadataCache();
			_eventListener = new ListenerForUnitTests();
			_fwMergeStrategy = new FieldWorksMergingStrategy(new NullMergeSituation(), _mdc);
		}

		[TearDown]
		public void TestTeardown()
		{
			_eventListener = null;
			_fwMergeStrategy = null;
		}

		[Test]
		public void NewerTimestampInOurWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie' >
<DateModified val='2000-1-1 23:59:59.000' />
</rt>
</languageproject>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void NewerTimestampInTheirsWins()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='LexEntry' guid='oldie' >
<DateModified val='2000-1-1 23:59:59.000' />
</rt>
</languageproject>";
			var ourContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2001-1-1 23:59:59.000");
			var theirContent = commonAncestor.Replace("2000-1-1 23:59:59.000", "2002-1-1 23:59:59.000");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("2002-1-1 23:59:59.000"));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Conflict_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</languageproject>";
			var ourContent = commonAncestor.Replace("val='1'", "val='2'");
			var theirContent = commonAncestor.Replace("val='1'", "val='3'");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Missing_From_Ours_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</languageproject>";
			var ourContent = commonAncestor.Replace("<Checksum val='1' />", null);
			var theirContent = commonAncestor.Replace("val='1'", "val='3'");

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Checksum_Missing_From_Theirs_Merges_To_Zero()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<languageproject version='7000016'>
<rt class='WfiWordform' guid='someguid' >
<Checksum val='1' />
</rt>
</languageproject>";
			var ourContent = commonAncestor.Replace("val='1'", "val='2'");
			var theirContent = commonAncestor.Replace("<Checksum val='1' />", null);

			XmlNode theirNode;
			XmlNode ancestorNode;
			var ourNode = CreateNodes(commonAncestor, ourContent, theirContent, out theirNode, out ancestorNode);

			var result = _fwMergeStrategy.MakeMergedEntry(_eventListener, ourNode, theirNode, ancestorNode);
			Assert.IsTrue(result.Contains("val='0'") || result.Contains("val=\"0\""));
			_eventListener.AssertExpectedConflictCount(0);
		}

		private static XmlNode CreateNodes(string commonAncestor, string ourContent, string theirContent, out XmlNode theirNode, out XmlNode ancestorNode)
		{
			var ancestorDoc = new XmlDocument();
			ancestorDoc.LoadXml(commonAncestor);
			ancestorNode = ancestorDoc.DocumentElement.FirstChild;

			var ourDoc = new XmlDocument();
			ourDoc.LoadXml(ourContent);
			var ourNode = ourDoc.DocumentElement.FirstChild;

			var theirDoc = new XmlDocument();
			theirDoc.LoadXml(theirContent);
			theirNode = theirDoc.DocumentElement.FirstChild;
			return ourNode;
		}
	}
}