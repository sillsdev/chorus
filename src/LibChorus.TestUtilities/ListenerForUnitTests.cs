using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace LibChorus.TestUtilities
{
	public class ListenerForUnitTests : ChangeAndConflictAccumulator
	{
//        public List<IConflict> Conflicts = new List<IConflict>();
//        public List<IConflict> Warnings = new List<IConflict>();
//        public List<IChangeReport> Changes = new List<IChangeReport>();
//        public List<ContextDescriptor> Contexts = new List<ContextDescriptor>();
//
//        /// <summary>
//        /// Historically, this class's implementation of ConflictOccurred (before it was split into two
//        /// interface members) did not push any context.
//        /// To keep the behavior the same, RecordContextInConflict does nothing.
//        /// </summary>
//        /// <param name="conflict"></param>
//        public void RecordContextInConflict(IConflict conflict)
//        {
//        }
//        public void ConflictOccurred(IConflict conflict)
//        {
//            Conflicts.Add(conflict);
//        }
//
//        public void WarningOccurred(IConflict warning)
//        {
//            Warnings.Add(warning);
//        }
//
//        public void ChangeOccurred(IChangeReport change)
//        {
//            Changes.Add(change);
//        }
//
//        public void EnteringContext(ContextDescriptor context)
//        {
//            Contexts.Add(context);
//        }
		private ContextDescriptor _currentContext = new NullContextDescriptor();
		public override void EnteringContext(ContextDescriptor context)
		{
			_currentContext = context;
			base.EnteringContext(context);
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
					Debug.WriteLine("    "+conflict.ToString());
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
					Debug.WriteLine("    "+change.ToString());
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