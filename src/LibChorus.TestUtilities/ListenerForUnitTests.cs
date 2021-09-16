using System.Diagnostics;
using NUnit.Framework;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace LibChorus.TestUtilities
{
	public class ListenerForUnitTests : ChangeAndConflictAccumulator
	{
		private ContextDescriptor _currentContext = new NullContextDescriptor();

		public override void EnteringContext(ContextDescriptor context)
		{
			_currentContext = context ?? new NullContextDescriptor();
			base.EnteringContext(_currentContext);
		}

		public override void RecordContextInConflict(IConflict conflict)
		{
			base.RecordContextInConflict(conflict);
			conflict.Context = _currentContext; // conforms to an expectation created by ChorusNotesMergeEventListener
		}
		public void AssertExpectedConflictCount(int count)
		{
			if (count != Conflicts.Count)
			{
				Debug.WriteLine("***Got these conflicts:");
				foreach (var conflict in Conflicts)
				{
					Debug.WriteLine($"    {conflict}");
				}
				Assert.AreEqual(count, Conflicts.Count,"Unexpected Conflict Count");
			}
		}

		public void AssertExpectedChangesCount(int count)
		{
			if (count != Changes.Count)
			{
				Debug.WriteLine("***Got these changes:");
				foreach (var change in Changes)
				{
					Debug.WriteLine($"    {change}");
				}
				Assert.AreEqual(count, Changes.Count,"Unexpected Change Count");
			}
		}

		public void AssertFirstChangeType<TExpected>()
		{
			Assert.AreEqual(typeof(TExpected), Changes[0].GetType());
		}
		public void AssertFirstConflictType<TExpected>()
		{
			Assert.AreEqual(typeof(TExpected), Conflicts[0].GetType());
		}
	}
}