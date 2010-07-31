using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return (Path.GetExtension(pathToFile).ToLower() == ".lift");
		}

		public bool CanValidateFile(string pathToFile)
		{
			return CanDiffFile(pathToFile);
		}


		public string ValidateFile(string pathToFile, IProgress progress)
		{
			//todo: decide how we want to use LiftIO validation. For now, just make sure it is well-formed xml
			return XmlValidation.ValidateFile(pathToFile, progress);
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
			LiftMerger merger;
			string winnerId;
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				default:
					throw new ArgumentException("The Lift merger cannot handle the requested conflict handling mode");
				case MergeOrder.ConflictHandlingModeChoices.WeWin:
					winnerId = mergeOrder.MergeSituation.AlphaUserId;
					merger = new LiftMerger(mergeOrder,
						mergeOrder.pathToOurs, mergeOrder.pathToTheirs,
						new LiftEntryMergingStrategy(mergeOrder.MergeSituation), mergeOrder.pathToCommonAncestor, winnerId);
					break;
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					winnerId = mergeOrder.MergeSituation.BetaUserId;
					merger = new LiftMerger(mergeOrder,
						mergeOrder.pathToTheirs, mergeOrder.pathToOurs,
						new LiftEntryMergingStrategy(mergeOrder.MergeSituation), mergeOrder.pathToCommonAncestor, winnerId);
					break;
			}
			merger.EventListener = mergeOrder.EventListener;

			merger.DoMerge(mergeOrder.pathToOurs);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, "entry", "id");
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new LiftChangePresenter(report as IXmlChangeReport);
			}
			else if (report is ErrorDeterminingChangeReport)
			{
				return (IChangePresenter)report;
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

		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "lift";
		}
	}
}