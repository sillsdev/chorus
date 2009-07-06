using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Chorus.merge;
using Chorus.retrieval;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.Review.RevisionChanges
{
	public class RevisionChangesModel
	{
		private readonly RevisionInfoProvider _revisionInfoProvider;
		private readonly ChangedRecordSelectedEvent _changedRecordSelectedEventToRaise;
		internal event EventHandler UpdateDisplay;
		public IEnumerable<IChangeReport> Changes { get; private set; }

		public RevisionChangesModel(RevisionInfoProvider revisionInfoProvider,ChangedRecordSelectedEvent changedRecordSelectedEventToRaise, RevisionSelectedEvent revisionSelectedEventToSubscribeTo)
		{
			_revisionInfoProvider = revisionInfoProvider;
			_changedRecordSelectedEventToRaise = changedRecordSelectedEventToRaise;
			revisionSelectedEventToSubscribeTo.Subscribe(SetRevision);

		}

		private void SetRevision(Revision descriptor)
		{
			Cursor.Current = Cursors.WaitCursor;
			if (descriptor != null)
			{
				Changes = _revisionInfoProvider.GetChangeRecords(descriptor);
			}
			else
			{
				Changes = null;
			}
			if(UpdateDisplay !=null)
			{
				UpdateDisplay.Invoke(this, null);
			}
			Cursor.Current = Cursors.Default;
		}

		public IEnumerable<IChangeReport> ChangeReports
		{
			get { return Changes; }
		}

		public void SelectedChangeChanged(IChangeReport report)
		{
			if (_changedRecordSelectedEventToRaise != null)
			{
			   _changedRecordSelectedEventToRaise.Raise(report);
			}
		}
	}
}