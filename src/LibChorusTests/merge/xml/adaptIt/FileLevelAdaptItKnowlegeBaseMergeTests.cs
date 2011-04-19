using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.adaptIt;
using Chorus.merge;
using Chorus.Utilities;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace LibChorus.Tests.merge.xml.adaptIt
{
	[TestFixture]
	public class FileLevelAdaptItKnowlegeBaseMergeTests
	{
		private string _ancestor;
		private ListenerForUnitTests _eventListener;

		[SetUp]
		public void Setup()
		{
			_ancestor = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<!--
	 Note: Using Microsoft WORD 2003 or later is not a good way to edit this xml file.
	 Instead, use NotePad, WordPad or Windows Explorer.
	 Please note: the order of the TU elements in this xml file might differ each time the knowledge base is saved. This is not an error. -->
<KB docVersion='4' srcName='Ngaing' tgtName='English' max='4'>
	<MAP mn='1'>
		<TU f='0' k='foo'>
			<RS n='1' a='boo'/>
		</TU>
		<TU f='0' k='fa'>
			<RS n='1' a='ba'/>
		</TU>
</MAP>
	<MAP mn='4'>
		<TU f='0' k='adi kai abaing iri'>
			<RS n='1' a='seven'/>
		</TU>
	</MAP>
</KB>";


			//TODO: remove this once we figure out a way around it

			_ancestor= _ancestor.Replace("xmlns='http://www.sil.org/computing/schemas/AdaptIt KB.xsd'", "");
		}


		[Test]
		public void CanMergeFile_IsKB_returnsTrue()
		{
			var path = Path.Combine(Path.GetTempPath(), "adaptitKb.xml");
			File.WriteAllText(path, _ancestor);
			using (var f = TempFile.TrackExisting(path))
			{
				var handler = new AdaptItFileHandler();
				Assert.IsTrue(handler.CanMergeFile(path));
			}
		}

		[Test]
		public void CanMergeFile_FileEmpty_ReturnsFalse()
		{
			var path = Path.Combine(Path.GetTempPath(), "adaptitKb.xml");
			File.WriteAllText(path,"");
			using (var f =  TempFile.TrackExisting(path))
			{
				var handler = new AdaptItFileHandler();
				Assert.IsFalse(handler.CanMergeFile(path));
			}
		}

		[Test]
		public void CanMergeFile_FileHasDifferentXml_ReturnsFalse()
		{
			var path = Path.Combine(Path.GetTempPath(), "adaptitKb.xml");
			File.WriteAllText(path, @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?><foo/>");
			using (var f = TempFile.TrackExisting(path))
			{
				var handler = new AdaptItFileHandler();
				Assert.IsFalse(handler.CanMergeFile(path));
			}
		}

//        [Test]
//        public void TEMP_namespaceIssue()
//        {
//            var dom = new XmlDocument();
//            dom.LoadXml(_ancestor);
//
//            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(dom.NameTable);
//            var def = namespaceManager.GetNamespacesInScope(XmlNamespaceScope.Local);
//            dom.SelectNodes("MAP", namespaceManager);
//
//        }

		[Test]
		public void Do3WayMerge_NoChanges_NothingDuplicated()
		{
			using (var file = new TempFile(_ancestor))
			{
				var handler = new AdaptItFileHandler();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(file.Path, file.Path, file.Path, situation);

				handler.Do3WayMerge(mergeOrder);
				var result = File.ReadAllText(file.Path);

				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("AdaptItKnowledgeBase/KB/MAP", 2);
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//TU", 3);
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//RS", 3);
			}
		}

		[Test] public void Do3WayMerge_BothAddedSameMap_HaveAllMaps()
		{
			var ourContent = _ancestor.Replace("<MAP mn='4'>",
											   @"<MAP mn='3'>
													<TU f='0' k='waik bering bering'>
														<RS n='1' a='dragonfly'/>
													</TU>
												</MAP>
												<MAP mn='4'>");
			var theirContent = _ancestor.Replace("<MAP mn='4'>",
											   @"<MAP mn='2'>
													<TU f='0' k='bering bering'>
														<RS n='1' a='fly'/>
													</TU>
												</MAP>
												<MAP mn='3'>
													<TU f='0' k='foo bar bar'>
														<RS n='1' a='dragon'/>
													</TU>
												</MAP>
												<MAP mn='4'>");
			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//MAP[@mn='1']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//MAP[@mn='2']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//MAP[@mn='3']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//MAP[@mn='4']");
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='2']/TU", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='3']/TU", 2);
		}

		[Test]
		public void Do3WayMerge_BothAddedDifferentPhrases_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("<MAP mn='1'>",
											   @"<MAP mn='1'>
													<TU f='0' k='hello'>
														<RS n='1' a='bonjour'/>
													</TU>");
			var theirContent = _ancestor.Replace("<MAP mn='1'>",
											   @"<MAP mn='1'>
													<TU f='0' k='goodbye'>
														<RS n='1' a='au revoir'/>
													</TU>");

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='1']/TU[@k='hello']", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='1']/TU[@k='goodbye']", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='1']/TU", 4);//this one is fragile, will break if the setup changes
		}

		[Test]
		public void Do3WayMerge_BothIncreasedTheCountOnAnRS_PickOneAndNoConflictsReported()
		{
			var ourContent = _ancestor.Replace("<RS n='1' a='boo'/>",
											  "<RS n='3' a='boo'/>");
			var theirContent = _ancestor.Replace("<RS n='1' a='boo'/>",
											   "<RS n='5' a='boo'/>");

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//RS[@a='boo']", 1);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//RS[@a='boo' and @n='3']", 1);
			_eventListener.AssertExpectedConflictCount(0);
		}

		[Test]
		public void Do3WayMerge_OneAddedAPhraseTheOtherRemovedADifferentPhrase_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("<MAP mn='1'>",
											   @"<MAP mn='1'>
													<TU f='0' k='hello'>
														<RS n='1' a='bonjour'/>
													</TU>");

			AssertThatXmlIn.String(_ancestor).HasSpecifiedNumberOfMatchesForXpath("//TU[@k='foo']", 1);//otherwise the test is invalid
			var dom = new XmlDocument();
			dom.LoadXml(_ancestor);
			var node = dom.SelectSingleNode("//TU[@k='foo']");
			node.ParentNode.RemoveChild(node);
			var theirContent = dom.OuterXml;

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//MAP[@mn='1']/TU[@k='hello']", 1);
			AssertThatXmlIn.String(result).HasNoMatchForXpath("//TU[@k='foo']");
		}

		[Test]
	public void Do3WayMerge_BothAddedTheSameRS_ButWithDifferentNAttrValues_PickOneAndNoConflictsReported()
	{
		var ourContent = _ancestor.Replace("<RS n='1' a='boo'/>",
										  @"<RS n='1' a='boo'/>
											<RS n='1' a='bar'/>");
		var theirContent = _ancestor.Replace("<RS n='1' a='boo'/>",
											@"<RS n='1' a='boo'/>
											  <RS n='2' a='bar'/>");

		var result = DoMerge(ourContent, theirContent);
		AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//RS[@a='bar']", 1);
		AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//RS[@a='bar' and @n='1']", 1);
		_eventListener.AssertExpectedConflictCount(0);
	}

		private string DoMerge(string ourContent, string theirContent)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(_ancestor))
			{
				var handler = new AdaptItFileHandler();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				_eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = _eventListener;
				handler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
			}
			return result;
		}
	}
}