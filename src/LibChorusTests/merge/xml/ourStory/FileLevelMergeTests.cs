using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.oneStory;
using Chorus.merge;
using NUnit.Framework;
using Palaso.IO;
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
			_ancestor = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StoryProject ProjectName=""Foobar"" PanoramaFrontMatter="""">
  <Members>
	<Member name=""Browser"" memberType=""JustLooking"" memberKey=""mem-2f8b34f9-3098-4ad4-b611-5685a4565845"" />
  </Members>
  <Languages>
	<VernacularLang name=""Kangri"" code=""xnr"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Maroon"" SentenceFinalPunct=""।"" Keyboard=""DevRom"" />
	<NationalBTLang name=""Hindi"" code=""hin"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Green"" SentenceFinalPunct=""।"" Keyboard=""DevRom"" />
	<InternationalBTLang name=""English"" code=""en"" FontName=""Times New Roman"" FontSize=""10"" FontColor=""Blue"" SentenceFinalPunct=""."" />
  </Languages>
  <stories SetName=""Stories"">
	<story name=""one"" guid=""1108CC4B-E0B7-4227-9645-829082B3F611"">
	  <verses>
		<verse guid=""8B2A9897-B79E-4b86-ADF9-FC73BD7CAF1E"">
		  <InternationalBT lang=""en"">new text for one</InternationalBT>
		</verse>
	  </verses>
	</story>
	<story name=""two"" guid=""2208CC4B-E0B7-4227-9645-829082B3F622"">
	  <verses>
		<verse guid=""222A9897-B79E-4b86-ADF9-FC73BD7CAF22"">
		  <InternationalBT lang=""en"">new text for two</InternationalBT>
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
				@"<story name=""three"" guid=""1CB50E18-A031-4CC5-9DE3-CE50838E5686""></story></stories>");
			var theirContent = _ancestor.Replace("</stories>",
				@"<story name=""four"" guid=""00D6AD1B-CA34-4E58-913A-6FADFE8B0FB3""></story></stories>");

			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""one""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""two""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""three""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""four""]");

		}

		[Test]
		public void Do3WayMerge_BothEditedDifferentStories_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("This is all about one.",
				"new text for one");
			var theirContent = _ancestor.Replace("Mostly about two.",
				"new text for two");

			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""one""]/verses/verse/InternationalBT[text()=""new text for one""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/stories/story[@name=""two""]/verses/verse/InternationalBT[text()=""new text for two""]");
		}

		[Test]
		public void Do3WayMerge_BothAddMember_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("</Members>",
				@"<Member name=""Crafter"" memberType=""Crafter"" memberKey=""mem-0699a445-75ba-4faf-900e-c7c3d6bc6b1a"" /></Members>");
			var theirContent = _ancestor.Replace("</Members>",
				@"<Member name=""UNS"" memberType=""UNS"" memberKey=""mem-5ffc70a1-a482-461c-b6cc-7283a2a32960"" /></Members>");

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("StoryProject/Members/Member", 3);

			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/Members/Member[@name=""Crafter""]");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/Members/Member[@name=""UNS""]");
		}

		[Test, Ignore("Bob says this never worked")]
		public void Do3WayMerge_BothAddSameMember_ThrowAwayOther()
		{
			var ourContent = _ancestor.Replace("</Members>",
				@"<Member name=""Bill"" memberType=""Crafter"" memberKey=""mem-0699a445-75ba-4faf-900e-c7c3d6bc6b1a"" /></Members>");
			var theirContent = _ancestor.Replace("</Members>",
				@"<Member name=""Bill"" memberType=""UNS"" memberKey=""mem-5ffc70a1-a482-461c-b6cc-7283a2a32960"" /></Members>");

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"StoryProject/Members/Member[@name=""Bill""]", 1);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"StoryProject/Members/Member[@name=""Bill""][@memberType=""Crafter""]");
		}

		[Test]
		public void Do3WayMerge_ChangeMemberName_ThrowAwayOther()
		{
			var ourContent = _ancestor;
			var theirContent = _ancestor.Replace("</Members>",
				@"<Member name=""Bill"" memberType=""UNS"" memberKey=""mem-5ffc70a1-a482-461c-b6cc-7283a2a32960"" /></Members>");

			var result = DoMerge(ourContent, theirContent);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"/StoryProject/Members/Member[@name=""Bill""][@memberKey=""mem-5ffc70a1-a482-461c-b6cc-7283a2a32960""]");
		}

		[Test]
		public void Do3WayMerge_BothChangeKeyboard_ThrowAwayOther()
		{
			var ourContent = _ancestor.Replace(@"<VernacularLang name=""Kangri"" code=""xnr"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Maroon"" SentenceFinalPunct=""।"" Keyboard=""DevRom"" />",
				@"<VernacularLang name=""Kangri"" code=""xnr"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Maroon"" SentenceFinalPunct=""।"" Keyboard=""Keyman DevRom"" />");
			var theirContent = _ancestor.Replace(@"<VernacularLang name=""Kangri"" code=""xnr"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Maroon"" SentenceFinalPunct=""।"" Keyboard=""DevRom"" />",
				@"<VernacularLang name=""Kangri"" code=""xnr"" FontName=""Arial Unicode MS"" FontSize=""12"" FontColor=""Maroon"" SentenceFinalPunct=""।"" Keyboard=""InKey DevRom"" />");

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath("//VernacularLang", 1);
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, @"//VernacularLang[@Keyboard=""Keyman DevRom""]");
		}

		[Test, Ignore("This issue and what to do about it are in process")]
		public void Do3WayMerge_ForceMergeAndMakeSureWeDontLoseIndentation()
		{
			var ourContent = _ancestor.Replace("This is all about one.",
				"new text for one");
			var theirContent = _ancestor.Replace("Mostly about two.",
				"new text for two");
			var result = DoMerge(ourContent, theirContent);

			// I don""t know a better way to do this than to do a string identity match
			var whatTheMergeShouldBe = _ancestor.Replace("This is all about one.",
				"new text for one").Replace("Mostly about two.",
				"new text for two");

			if (whatTheMergeShouldBe != result)
				Assert.Fail(@"Strings don""t match, so we must have trimmed some indentation! This is what the merge result was:{0}{1}{0}{0}This is what it should have been:{0}{2}",
					System.Environment.NewLine, result, whatTheMergeShouldBe);
		}

		[Test]
		public void Do3WayMerge_OneEditedTheOtherDeletedAStory_GotAllChanges()
		{
			var ourContent = _ancestor.Replace("This is all about one.",
				"new text for one");
			var dom = new XmlDocument();
			dom.LoadXml(_ancestor);
			var stories = dom.SelectSingleNode("StoryProject/stories");
			var story = stories.SelectSingleNode(@"story[@name=""two""]");
			stories.RemoveChild(story);

			var theirContent = dom.OuterXml;

			var result = DoMerge(ourContent, theirContent);
			AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"StoryProject/stories/story[@name=""one""]/verses/verse/InternationalBT[text()=""new text for one""]",1);

			AssertThatXmlIn.String(result).HasNoMatchForXpath(@"//story[@name=""two""]");
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