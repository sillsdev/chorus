using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Chorus.FileTypeHanders;
using Chorus.merge;
using Chorus.retrieval;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.Review.RevisionChanges
{
	public class ChangesInRevisionModel
	{
		private readonly RevisionInspector _revisionInspector;
		private readonly ChangedRecordSelectedEvent _changedRecordSelectedEventToRaise;
		private readonly ChorusFileTypeHandlerCollection _fileHandlers;
		internal event EventHandler UpdateDisplay;
		public IEnumerable<IChangeReport> Changes { get; private set; }

		public ChangesInRevisionModel(RevisionInspector revisionInspector,
			ChangedRecordSelectedEvent changedRecordSelectedEventToRaise,
			RevisionSelectedEvent revisionSelectedEventToSubscribeTo,
			 ChorusFileTypeHandlerCollection fileHandlers)
		{
			_revisionInspector = revisionInspector;
			_changedRecordSelectedEventToRaise = changedRecordSelectedEventToRaise;
			_fileHandlers = fileHandlers;
			revisionSelectedEventToSubscribeTo.Subscribe(SetRevision);

		}

		private void SetRevision(Revision descriptor)
		{
			Cursor.Current = Cursors.WaitCursor;
			if (descriptor != null)
			{
				Changes = _revisionInspector.GetChangeRecords(descriptor);
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

		public IChangePresenter GetChangePresenterForDataType(IChangeReport report)
		{
			var handler = _fileHandlers.GetHandlerForPresentation(report.PathToFile);
			return handler.GetChangePresenter(report);
		 }
	}
}