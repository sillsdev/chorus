using System;
using System.Collections.Generic;
using System.Diagnostics;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Utilities;

namespace Chorus.FileTypeHanders
{
	public class LiftFileHandler : IChorusFileTypeHandler, IMergeEventListener
	{
		public bool CanHandleFile(string pathToFile)
		{
			return (System.IO.Path.GetExtension(pathToFile) == ".lift");
		}

		public int Merge(MergeOrder mergeOrder)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(string pathToParent, string pathToChild)
		{
			Changes.Clear();
			var strat = new LiftEntryMergingStrategy(new NullMergeSituation());
			var differ = Lift2WayDiffer.CreateFromFiles(strat, pathToParent, pathToChild, this);
			differ.ReportDifferencesToListener();
			return Changes;
		}

		public IChangePresenter GetChangePresenter(IChangeReport report)
		{
			Guard.Against(report.GetType() == typeof(IXmlChangeReport), "Expecting a IXmlChangeReport");
			return new LiftChangePresenter(report as IXmlChangeReport);
		}

		#region IMergeEventListener

		public List<IConflict> Conflicts = new List<IConflict>();
		public List<IChangeReport> Changes = new List<IChangeReport>();
		public List<string> Contexts = new List<string>();

		public void ConflictOccurred(IConflict conflict)
		{
			Conflicts.Add(conflict);
		}

		public void ChangeOccurred(IChangeReport change)
		{
			//Debug.WriteLine(change);
			Changes.Add(change);
		}

		public void EnteringContext(string context)
		{
			Contexts.Add(context);
		}
		#endregion
	}
}