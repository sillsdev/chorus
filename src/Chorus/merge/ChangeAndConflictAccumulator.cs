using System.Collections.Generic;
using Chorus.merge.xml.generic;

namespace Chorus.merge
{
	public class ChangeAndConflictAccumulator : IMergeEventListener
	{

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
	}
}