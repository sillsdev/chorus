using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.adaptIt
{
	public class AdaptItFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			if (Path.GetExtension(pathToFile).ToLower() != ".xml")
				return false;

			//inexpensively detect if this is an AdaptItKnowledgeBase
			using (var reader = File.OpenText(pathToFile))
			{
				for (int i = 0; i < 10; i++)
				{
					var line = reader.ReadLine();
					if (line!=null && line.Contains("<AdaptItKnowledgeBase"))
						return true;
				}
			}
			return false;
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
			merger.MergeStrategies.SetStrategy("AdaptItKnowledgeBase", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("KB", ElementStrategy.CreateSingletonElement());
//            merger.MergeStrategies.SetStrategy("KB", ElementStrategy.CreateForKeyedElement("tgtName", false));

			//BOB, please review all these. Are the listed attributes really unique?
			merger.MergeStrategies.SetStrategy("MAP", ElementStrategy.CreateForKeyedElement("mn", false));

			//review: is it ok to ignore @f?
			merger.MergeStrategies.SetStrategy("TU", ElementStrategy.CreateForKeyedElement("k", false));

			//review: is it ok to ignore @n?
			//review: is order relevant?
			merger.MergeStrategies.SetStrategy("RS", ElementStrategy.CreateForKeyedElement("a", false));



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
	}
}