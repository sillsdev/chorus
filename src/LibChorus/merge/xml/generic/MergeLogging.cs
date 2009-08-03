using System;
using System.Collections.Generic;

namespace Chorus.merge.xml.generic
{
	public interface IMergeEventListener
	{
		void ConflictOccurred(IConflict conflict);
		void WarningOccurred(IConflict warning);

		void ChangeOccurred(IChangeReport change);

		/// <summary>
		/// In order to be able to store in the conflict enough information to later retrieve the conflicting
		/// data, someone must call this when new element levels were reached.
		/// Then when a conflict occurs, the listener pushes this context into the conflict and (at least
		/// in the case of the xmllistener as of june2009) writes out the conflict with this context in the
		/// xml record of the conflict.  Later, a UI handling conflicts can retrieve this info in order
		/// to reconstruct exact what and where the conflict was.
		/// </summary>
		/// <param name="context">an xpath, line number, whatever works for reconstructing the situation at a later date</param>
		void EnteringContext(ContextDescriptor context);
	}

	public class NullMergeEventListener : IMergeEventListener
	{
		public void ConflictOccurred(IConflict conflict)
		{

		}

		public void WarningOccurred(IConflict conflict)
		{

		}

		public void ChangeOccurred(IChangeReport change)
		{

		}

		public void EnteringContext(ContextDescriptor context)
		{

		}
	}

	public class DispatchingMergeEventListener : IMergeEventListener
	{
		private List<IMergeEventListener> _listeners = new List<IMergeEventListener>();

		public void AddEventListener(IMergeEventListener listener)
		{
			_listeners.Add(listener);
		}

		public void ConflictOccurred(IConflict conflict)
		{
			foreach (IMergeEventListener listener in _listeners)
			{
				listener.ConflictOccurred(conflict);
			}
		}

		public void WarningOccurred(IConflict conflict)
		{
			 foreach (IMergeEventListener listener in _listeners)
			{
				listener.WarningOccurred(conflict);
			}
		}

		public void ChangeOccurred(IChangeReport change)
		{
			 foreach (IMergeEventListener listener in _listeners)
			{
				listener.ChangeOccurred(change);
			}
		}

		public void EnteringContext(ContextDescriptor context)
		{
			 foreach (IMergeEventListener listener in _listeners)
			{
				listener.EnteringContext(context);
			}
		}
	}


//    public class MergeReport : IMergeEventListener
//    {
//        private List<IConflict> _conflicts=new List<IConflict>();
//        //private string _result;
//        public void ConflictOccurred(IConflict conflict)
//        {
//            _conflicts.Add(conflict);
//        }
//    }

//    public interface IMergeReportMaker
//    {
//        MergeReport GetReport();
//    }

//    public class DefaultMergeReportMaker : IMergeReportMaker
//    {
//
//        public MergeReport GetReport()
//        {
//            return new MergeReport();
//        }
//
//    }
}