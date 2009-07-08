using System;
using System.Collections.Generic;
using Chorus.merge;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// This is the handler of last resort.
	/// </summary>
	public class DefaultFileTypeHandler : IChorusFileTypeHandler
	{
		public bool CanHandleFile(string pathToFile)
		{
			return false;
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to merge files like '{0}'", mergeOrder.pathToOurs));
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(string pathToParent, string pathToChild)
		{
			throw new ApplicationException(string.Format("Chorus could not find a handler to diff files like '{0}'", pathToChild));
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			return new DefaultChangePresenter(report);
		}
	}
}