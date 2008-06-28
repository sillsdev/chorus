using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.generic
{
	public interface IMergeLogger
	{
		void RegisterConflict(IConflict conflict);
	}

	public class MergeLogger : IMergeLogger
	{
		private readonly IList<IConflict> _conflicts;

		public MergeLogger(IList<IConflict> conflicts)
		{
			_conflicts = conflicts;
		}

		public void RegisterConflict(IConflict conflict)
		{
			_conflicts.Add(conflict);
		}
	}

	public class MergeReport
	{
		public string _result;
	}

	public interface IMergeReportMaker
	{
		MergeReport GetReport();
	}

	public class DefaultMergeReportMaker : IMergeReportMaker
	{

		public MergeReport GetReport()
		{
			return new MergeReport();
		}

	}
}