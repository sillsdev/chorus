using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHanders.lift;
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

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			DispatchingMergeEventListener listenerDispatcher = new DispatchingMergeEventListener();

			//Debug.Fail("hello");
			//review: where should these really go?
			string dir = Path.GetDirectoryName(mergeOrder.pathToOurs);
			using (HumanLogMergeEventListener humanListener = new HumanLogMergeEventListener(mergeOrder.pathToOurs + ".conflicts.txt"))
			using (XmlLogMergeEventListener xmlListener = new XmlLogMergeEventListener(mergeOrder.pathToOurs + ".conflicts"))
			{
				listenerDispatcher.AddEventListener(humanListener);
				listenerDispatcher.AddEventListener(xmlListener);
				mergeOrder.EventListener = listenerDispatcher;

				//;  Debug.Fail("hello");
				LiftMerger merger;
				switch (mergeOrder.ConflictHandlingMode)
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
			}
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision fileInRevision, string pathToParent, string pathToChild)
		{
			var listener = new ChangeAndConflictAccumulator();
			var strat = new LiftEntryMergingStrategy(new NullMergeSituation());
			var differ = Lift2WayDiffer.CreateFromFiles(strat, pathToParent, pathToChild, listener);
			differ.ReportDifferencesToListener();
			return listener.Changes;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new LiftChangePresenter(report as IXmlChangeReport);
			}
			else
			{
				return new DefaultChangePresenter(report);
			}
		}


		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision.RelativePath, "Initial") };
		}

	}
}