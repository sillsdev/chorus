using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders
{
	public class ConflictFileTypeHandler : IChorusFileTypeHandler
	{
		public bool CanHandleFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile) == ".conflicts");
		}

		public void Do3WayMerge(MergeOrder order)
		{
			XmlMerger merger  = new XmlMerger(order.MergeSituation);
			NodeMergeResult r = merger.MergeFiles(order.pathToOurs, order.pathToTheirs, order.pathToCommonAncestor);
			File.WriteAllText(order.pathToOurs, r.MergedNode.OuterXml);
		}
		public IEnumerable<IChangeReport> Find2WayDifferences(string pathToParent, string pathToChild)
		{
			throw new NotImplementedException(string.Format("The ConflictFileTypeHandler does not yet do diffs"));
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			return new DefaultChangePresenter(report);
		}
	}
}