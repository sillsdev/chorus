using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.FileTypeHanders.oneStory
{
	public class OneStoryFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			return Path.GetExtension(pathToFile).ToLower() == ".onestory";
		}

		public bool CanPresentFile(string pathToFile)
		{
			return false;
		}

		public bool CanValidateFile(string pathToFile)
		{
			return false;
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			throw new NotImplementedException();
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);

			// use linq to write the merged XML out, so it is as much as possible like the format that the OneStory editor
			//  (which uses linq also) writes out. Do this so we maximally keep indentation the same, so that if you do
			//  "view changesets" in TortoiseHG (a line-by-line differencer) it will highlight bona fide differences as much
			//  as possible.
			XDocument doc = XDocument.Parse(result.MergedNode.OuterXml);
			doc.Save(mergeOrder.pathToOurs);
		}

		private void SetupElementStrategies(XmlMerger merger)
		{
			merger.MergeStrategies.SetStrategy("StoryProject", ElementStrategy.CreateSingletonElement());

			//this handles the meta data
			merger.MergeStrategies.SetStrategy("Members", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Member", ElementStrategy.CreateForKeyedElement("memberKey", false));

			merger.MergeStrategies.SetStrategy("Languages", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("VernacularLang", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("NationalBTLang", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("InternationalBTLang", ElementStrategy.CreateSingletonElement());

			// story sets and stories
			merger.MergeStrategies.SetStrategy("stories", ElementStrategy.CreateForKeyedElement("SetName", false));

			var elementStrategyStory = ElementStrategy.CreateForKeyedElement("guid", false);
			elementStrategyStory.AttributesToIgnoreForMerging.Add("stageDateTimeStamp");
			merger.MergeStrategies.SetStrategy("story", elementStrategyStory);

			// the rest is used only if the same story was edited by two or more people at the same time
			//  not supposed to happen, but let's be safer
			merger.MergeStrategies.SetStrategy("CraftingInfo", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryCrafter", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ProjectFacilitator", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryPurpose", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ResourcesUsed", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("BackTranslator", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Tests", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Test", ElementStrategy.CreateForKeyedElement("memberID", true));

			merger.MergeStrategies.SetStrategy("verses", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("verse", ElementStrategy.CreateForKeyedElement("guid", true));
			merger.MergeStrategies.SetStrategy("Vernacular", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("NationalBT", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("InternationalBT", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("anchors", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("anchor", ElementStrategy.CreateForKeyedElement("jumpTarget", false));
			merger.MergeStrategies.SetStrategy("toolTip", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("exegeticalHelps", ElementStrategy.CreateSingletonElement());
			// there can be multiple exegeticalHelp elements, but a) their order doesn't matter and b) they don't need a key
			//  I think if I left this uncommented, then it would only allow one and if another user added one, it
			//  would just replace the one that's there... (i.e. I think that's what ElementStrategy.CreateSingletonElement
			//  means, so... commenting out):
			// merger.MergeStrategies.SetStrategy("exegeticalHelp", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("TestQuestions", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TestQuestion", ElementStrategy.CreateForKeyedElement("guid", false));
			merger.MergeStrategies.SetStrategy("TQVernacular", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TQNationalBT", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("TQInternationalBT", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("Answers", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("answer", ElementStrategy.CreateForKeyedElement("memberID", true));

			merger.MergeStrategies.SetStrategy("Retellings", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Retelling", ElementStrategy.CreateForKeyedElement("memberID", true));

			merger.MergeStrategies.SetStrategy("ConsultantNotes", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("ConsultantConversation", ElementStrategy.CreateForKeyedElement("guid", true));

			var elementStrategyConNote = ElementStrategy.CreateForKeyedElement("guid", true);
			elementStrategyConNote.AttributesToIgnoreForMerging.Add("timeStamp");
			merger.MergeStrategies.SetStrategy("ConsultantNote", elementStrategyConNote);

			merger.MergeStrategies.SetStrategy("CoachNotes", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("CoachConversation", ElementStrategy.CreateForKeyedElement("guid", true));

			var elementStrategyCoaNote = ElementStrategy.CreateForKeyedElement("guid", true);
			elementStrategyCoaNote.AttributesToIgnoreForMerging.Add("timeStamp");
			merger.MergeStrategies.SetStrategy("CoachNote", elementStrategyCoaNote);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			//this is never called because we said we don't do diffs yet; review is handled some other way
			throw new NotImplementedException();
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			//this is never called because we said we don't present diffs; review is handled some other way
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			//this is never called because we said we don't present diffs; review is handled some other way
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "onestory";
		}
	}
}
