using System.Collections.Generic;
using Chorus.merge.xml.generic;

namespace Chorus.merge
{
	public class ChangeAndConflictAccumulator : IMergeEventListener
	{

		public List<IConflict> Conflicts = new List<IConflict>();
		public List<IChangeReport> Changes = new List<IChangeReport>();
		public List<IConflict> Warnings = new List<IConflict>();
		public List<ContextDescriptor> Contexts = new List<ContextDescriptor>();

		/// <summary>
		/// Historically, this class's implementation of ConflictOccurred (before it was split into two
		/// interface members) did not push any context.
		/// To keep the behavior the same, RecordContextInConflict does nothing.
		/// </summary>
		/// <param name="conflict"></param>
		public void RecordContextInConflict(IConflict conflict)
		{
		}

		public void ConflictOccurred(IConflict conflict)
		{
			Conflicts.Add(conflict);
		}

		public void WarningOccurred(IConflict warning)
		{
			Warnings.Add(warning);

		}

		public void ChangeOccurred(IChangeReport change)
		{
			//Debug.WriteLine(change);
			Changes.Add(change);
		}

		public void EnteringContext(ContextDescriptor context)
		{
			Contexts.Add(context);
		}
	}
}