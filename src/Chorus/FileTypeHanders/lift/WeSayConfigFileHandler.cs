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

namespace Chorus.FileTypeHanders.lift
{
	public class WeSayConfigFileHandler : IChorusFileTypeHandler
	{
		public bool CanDiffFile(string pathToFile)
		{
			return false;
		}

		public bool CanMergeFile(string pathToFile)
		{
			return false;
		}

		public bool CanPresentFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile).ToLower() == ".wesayconfig");

		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision fileInRevision, string pathToParent, string pathToChild)
		{
			return new IChangeReport[] {new DefaultChangeReport(fileInRevision.FullPath,"Editted")};
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			return new DefaultChangePresenter(report);
		}


		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision.FullPath, "Added") };
		}

	}
}