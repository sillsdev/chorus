using System.Collections.Generic;
using Chorus.merge;

namespace Chorus.FileTypeHanders
{
	public interface IChorusFileTypeHandler
	{
		bool CanHandleFile(string pathToFile);
		int Merge(merge.MergeOrder mergeOrder);
		IEnumerable<IChangeReport> Find2WayDifferences(string pathToParent, string pathToChild);
		IChangePresenter GetChangePresenter(IChangeReport report);
	}
}
