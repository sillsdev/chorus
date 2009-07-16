using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
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
		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision fileInRevision, string pathToParent, string pathToChild)
		{
			var listener = new ChangeAndConflictAccumulator();
			var differ = ConflictDiffer.CreateFromFiles(pathToParent, pathToChild, listener);
			differ.ReportDifferencesToListener();
			return listener.Changes;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			if ((report as IXmlChangeReport) != null)
			{
				return new ConflictPresenter(report as IXmlChangeReport);
			}
			else
			{
				return new DefaultChangePresenter(report);
			}
		}



		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			var dom = new XmlDocument();
			dom.Load(file.Path);


			foreach (XmlNode e in dom.SafeSelectNodes("conflicts/conflict"))
			{
				yield return new XmlAdditionChangeReport(fileInRevision.FullPath, e);
			}
		}
	}

	public class ConflictPresenter : IChangePresenter
	{
		private readonly IXmlChangeReport _report;

		public ConflictPresenter(IXmlChangeReport report)
		{
			_report = report;
		}

		public string GetDataLabel()
		{
			return "conflict";
		}

		public string GetActionLabel()
		{
			return XmlUtilities.GetStringAttribute(_report.ChildNode, "type");
		}

		public string GetHtml()
		{
			var builder = new StringBuilder();
			builder.Append("<html>");
			if (_report is XmlAdditionChangeReport)
			{
				var r = _report as XmlAdditionChangeReport;
				builder.AppendFormat("<p>{0}</p>", XmlUtilities.GetXmlForShowingInHtml(_report.ChildNode.OuterXml));
			}

			builder.Append("</html>");
			return builder.ToString();
		}

		public string GetTypeLabel()
		{
			return "conflict";
		}
	}
}