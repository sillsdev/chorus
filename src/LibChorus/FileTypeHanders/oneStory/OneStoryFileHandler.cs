using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

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

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			var merger = new XmlMerger(mergeOrder.MergeSituation);
			SetupElementStrategies(merger);

			merger.EventListener = mergeOrder.EventListener;
			var result = merger.MergeFiles(mergeOrder.pathToOurs, mergeOrder.pathToTheirs, mergeOrder.pathToCommonAncestor);
			File.WriteAllText(mergeOrder.pathToOurs, result.MergedNode.OuterXml);
		}

		private void SetupElementStrategies(XmlMerger merger)
		{
			//this is all you need if people will only edit different stories, and no meta data
			merger.MergeStrategies.SetStrategy("StoryProject", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("stories", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("story", ElementStrategy.CreateForKeyedElement("guid", false));

			//this handles the meta data
			merger.MergeStrategies.SetStrategy("Members", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Member", ElementStrategy.CreateForKeyedElement("memberKey", false));
			merger.MergeStrategies.SetStrategy("Fonts", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("VernacularFont", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("NationalBTFont", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("InternationalBTFont", ElementStrategy.CreateSingletonElement());

			//the rest is used only if the same story was editted by two or more people at the same time
			merger.MergeStrategies.SetStrategy("CraftingInfo", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryCrafter", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("StoryPurpose", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("BackTranslator", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("Tests", ElementStrategy.CreateSingletonElement());

			merger.MergeStrategies.SetStrategy("edits", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("edit", ElementStrategy.CreateForKeyedElement("editKey", false));

			merger.MergeStrategies.SetStrategy("verses", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("verse", ElementStrategy.CreateForKeyedElement("guid", true));
			merger.MergeStrategies.SetStrategy("Vernacular", ElementStrategy.CreateForKeyedElement("lang", false));
			merger.MergeStrategies.SetStrategy("NationalBT", ElementStrategy.CreateForKeyedElement("lang", false));
			merger.MergeStrategies.SetStrategy("InternationalBT", ElementStrategy.CreateForKeyedElement("lang", false));

			//todo anchors, TestQuestions, Retellings,ConsultantNotes, CoachNotes
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
