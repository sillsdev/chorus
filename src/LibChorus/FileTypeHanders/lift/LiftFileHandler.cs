using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	public class LiftFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile) == ".lift");
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CanDiffFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanDiffFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
//            DispatchingMergeEventListener listenerDispatcher = new DispatchingMergeEventListener();
//            using (HumanLogMergeEventListener humanListener = new HumanLogMergeEventListener(mergeOrder.pathToOurs + ".conflicts.txt"))
//            using (XmlLogMergeEventListener xmlListener = new XmlLogMergeEventListener(mergeOrder.pathToOurs + ".conflicts"))
//            {
//                listenerDispatcher.AddEventListener(humanListener);
//                listenerDispatcher.AddEventListener(xmlListener);
//                mergeOrder.EventListener = listenerDispatcher;

				//;  Debug.Fail("hello");
				LiftMerger merger;
				switch (mergeOrder.MergeSituation.ConflictHandlingMode)
				{
					default:
						throw new ArgumentException("The Lift merger cannot handle the requested conflict handling mode");
					case MergeOrder.ConflictHandlingModeChoices.WeWin:

						merger = new LiftMerger(new LiftEntryMergingStrategy(mergeOrder.MergeSituation), mergeOrder.pathToOurs, mergeOrder.pathToTheirs,
												mergeOrder.pathToCommonAncestor);
						break;
					case MergeOrder.ConflictHandlingModeChoices.TheyWin:
						merger = new LiftMerger(new LiftEntryMergingStrategy(mergeOrder.MergeSituation), mergeOrder.pathToTheirs, mergeOrder.pathToOurs,
												mergeOrder.pathToCommonAncestor);
						break;
				}
				merger.EventListener = mergeOrder.EventListener;

				string newContents = merger.GetMergedLift();
				File.WriteAllText(mergeOrder.pathToOurs, newContents);
//            }
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision child, FileInRevision parent, HgRepository repository)
		{
			var listener = new ChangeAndConflictAccumulator();
			var strat = new LiftEntryMergingStrategy(new NullMergeSituation());

			//pull the files out of the repository so we can read them
				var differ = Lift2WayDiffer.CreateFromFileInRevision(strat, parent, child, listener, repository);
				differ.ReportDifferencesToListener();
				return listener.Changes;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new LiftChangePresenter(report as IXmlChangeReport);
			}
			else
			{
				return new DefaultChangePresenter(report, repository);
			}
		}


		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

	}
}