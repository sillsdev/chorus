using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftFileHandler : IChorusFileTypeHandler
	{
		internal LiftFileHandler()
		{}

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
			XmlMergeService.CurrentSuppressIndentingChildren = XmlMergeService.LiftSuppressIndentingChildren;
			try
			{
				XmlMergeService.Do3WayMerge(mergeOrder,
					new LiftEntryMergingStrategy(mergeOrder.MergeSituation),
					false,
					"header",
					"entry", "guid");
			}
			finally
			{
				XmlMergeService.CurrentSuppressIndentingChildren = XmlMergeService.DefaultSuppressIndentingChildren;
			}
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, "header", "entry", "guid");
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new LiftChangePresenter(report as IXmlChangeReport);
			}
			if (report is ErrorDeterminingChangeReport)
			{
				return (IChangePresenter)report;
			}
			return new DefaultChangePresenter(report, repository);
		}



		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return "lift";
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return UInt32.MaxValue; }
		}
	}
}