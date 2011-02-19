using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress.LogBox;

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
					if (line != null && line.Contains("<KB docVersion"))
						return true;
				}
			}
			return false;
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
			File.WriteAllText(mergeOrder.pathToOurs, result.MergedNode.OuterXml);
		}

		private void SetupElementStrategies(XmlMerger merger)
		{
			// new versions of AI no longer use this element
			//  merger.MergeStrategies.SetStrategy("AdaptItKnowledgeBase", ElementStrategy.CreateSingletonElement());
			merger.MergeStrategies.SetStrategy("KB", ElementStrategy.CreateSingletonElement());

			// Are the listed attributes really unique? rde: yes and their order is probably crucial
			merger.MergeStrategies.SetStrategy("MAP", ElementStrategy.CreateForKeyedElement("mn", true));

			// review: is it ok to ignore @f? rde: I'm not sure what you mean by "ignore", but it seems that
			//  if someone changes @f, it should be changed
			merger.MergeStrategies.SetStrategy("TU", ElementStrategy.CreateForKeyedElement("k", false));

			// ... whereas for RS@a, if there's a conflict, just pick one or the other is fine (if there
			//  were the ability, what we'd want to do is add the differentials from the ancestor--e.g.
			//  if ancestor has 1 and 'mine' is 3 (I've added 2 occurrences of this interpretation) and
			//  theirs is 2 (they've added 1 new occurrence of this interpretation), then make it 4
			//  =1 + 2 + 1. This is what we're really want to do, but otherwise, it isn't a big deal
			//  as far as AI or other users are concerned).
			var elementStrategy = ElementStrategy.CreateForKeyedElement("a", true);
			elementStrategy.AttributesToIgnoreForMerging.Add("n");
			merger.MergeStrategies.SetStrategy("RS", elementStrategy);

			//Banana leaves are great for covering food in a mumu.  But the gorgor leaf is better than that of a banana for cooking manget.
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
			yield return "xml";
		}
	}
}