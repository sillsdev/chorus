using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.oneStory;
using Chorus.merge;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace LibChorus.Tests.merge.xml.ourStory
{
	[TestFixture]
	public class FileLevelMergeTests
	{
		private string _ancestor;

		[SetUp]
		public void Setup()
		{
			_ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<StoryProject>
  <stories ProjectLanguage='Foobar'>
	<story name='one' guid='1108CC4B-E0B7-4227-9645-829082B3F611'>
	  <verses>
		<verse guid='8B2A9897-B79E-4b86-ADF9-FC73BD7CAF1E'>
		  <InternationalBT lang='en'>This is all about one.</InternationalBT>
		  </verse>
		</verses>
	</story>
	<story name='two' guid='2208CC4B-E0B7-4227-9645-829082B3F622'>
	  <verses>
		<verse guid='222A9897-B79E-4b86-ADF9-FC73BD7CAF22'>
		  <InternationalBT lang='en'>Mostly about two.</InternationalBT>
		  </verse>
		</verses>
	</story>
  </stories>
</StoryProject>";
		}



		[Test]
		public void Do3WayMerge_NoChanges_NothingDuplicated()
		{
			using (var file = new TempFile(_ancestor))
			{
				var handler = new OneStoryFileHandler();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(file.Path, file.Path, file.Path, situation);

				handler.Do3WayMerge(mergeOrder);
				var result = File.ReadAllText(file.Path);

				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("StoryProject/stories/story", 2);
			}
		}

		[Test] public void Do3WayMerge_BothAddedStories_HaveAllStories()
		{
			  var ourContent = _ancestor.Replace("</stories>",
				@"<story name='three' guid='1CB50E18-A031-4CC5-9DE3-CE50838E5686'></story></stories>");
			var theirContent = _ancestor.Replace("</stories>",
				@"<story name='four' guid='00D6AD1B-CA34-4E58-913A-6FADFE8B0FB3'></story></stories>");

			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='one']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='two']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='three']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='four']");

		}

		[Test]
		public void Do3WayMerge_BothEdittedDifferentStories_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("This is all about one.",
				"new text for one");
			var theirContent = _ancestor.Replace("Mostly about two.",
				"new text for two");

			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='one']/verses/verse/InternationalBT[text()='new text for one']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "StoryProject/stories/story[@name='two']/verses/verse/InternationalBT[text()='new text for two']");
		}
		[Test]
		public void Do3WayMerge_OneEdittedTheOtherDeletedAStory_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("This is all about one.",
				"new text for one");
			var dom = new XmlDocument();
			dom.LoadXml(_ancestor);
			var stories = dom.SelectSingleNode("StoryProject/stories");
			var story = stories.SelectSingleNode("story[@name='two']");
			stories.RemoveChild(story);

			var theirContent = dom.OuterXml;

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("StoryProject/stories/story[@name='one']/verses/verse/InternationalBT[text()='new text for one']",1);

			AssertThatXmlIn.String(result).HasNoMatchForXpath("//story[@name='two']");
		}

		private string DoMerge(string ourContent, string theirContent)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(_ancestor))
			{
				var handler = new OneStoryFileHandler();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);

				handler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
			}
			return result;
		}
	}
}