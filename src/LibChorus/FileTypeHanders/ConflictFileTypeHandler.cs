using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// The key thing to understand here is that the conflict file only *grows*... it doesn't get editted
	/// normally.  So the only changes we're interested in are *additions* of conflict reports to the file.
	/// </summary>
	public class ConflictFileTypeHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return CanMergeFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile) == ".conflicts");
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanDiffFile(pathToFile);
		}

		public void Do3WayMerge(MergeOrder order)
		{
			XmlMerger merger  = new XmlMerger(order.MergeSituation);
			var r = merger.MergeFiles(order.pathToOurs, order.pathToTheirs, order.pathToCommonAncestor);
			File.WriteAllText(order.pathToOurs, r.MergedNode.OuterXml);
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			var listener = new ChangeAndConflictAccumulator();
						//pull the files out of the repository so we can read them
			using (var childFile = child.CreateTempFile(repository))
			using (var parentFile = parent.CreateTempFile(repository))
			{

				var differ = ConflictDiffer.CreateFromFiles(parentFile.Path, childFile.Path, listener);
				differ.ReportDifferencesToListener();
				return listener.Changes;
			}
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new ConflictPresenter(report as IXmlChangeReport, repository);
			}
			else
			{
				return new DefaultChangePresenter(report, repository);
			}
		}



		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			var dom = new XmlDocument();
			dom.Load(file.Path);


			foreach (XmlNode e in dom.SafeSelectNodes("conflicts/conflict"))
			{
				yield return new XmlAdditionChangeReport(fileInRevision, e);
			}
		}
	}
}