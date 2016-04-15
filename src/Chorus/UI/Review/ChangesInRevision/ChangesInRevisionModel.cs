using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Chorus.FileTypeHandlers;
using Chorus.merge;
using Chorus.retrieval;
using Chorus.Review;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Review.ChangesInRevision
{
	public class ChangesInRevisionModel
	{
		private readonly RevisionInspector _revisionInspector;
		private readonly ChangedRecordSelectedEvent _changedRecordSelectedEventToRaise;
		private readonly NavigateToRecordEvent _navigateToRecordEvent;
		private readonly ChorusFileTypeHandlerCollection _fileHandlers;
		internal event EventHandler UpdateDisplay;
		public IEnumerable<IChangeReport> Changes { get; private set; }

		public ChangesInRevisionModel(RevisionInspector revisionInspector,
									  ChangedRecordSelectedEvent changedRecordSelectedEventToRaise,
									   NavigateToRecordEvent navigateToRecordEventToRaise,
									 RevisionSelectedEvent revisionSelectedEventToSubscribeTo,
									  ChorusFileTypeHandlerCollection fileHandlers)
		{
			_revisionInspector = revisionInspector;
			_changedRecordSelectedEventToRaise = changedRecordSelectedEventToRaise;
			_navigateToRecordEvent = navigateToRecordEventToRaise;
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

		public void NavigationRequested(string url)
		{
			if (_navigateToRecordEvent != null)
			{
				_navigateToRecordEvent.Raise(url);
			}
		}

		public IChangePresenter GetChangePresenterForDataType(IChangeReport report)
		{

			IChorusFileTypeHandler handler;
			if (string.IsNullOrEmpty(report.PathToFile))
			{
				Debug.Fail("Report had empty path (only seeing this because in Debug Mode)");
				handler = new DefaultFileTypeHandler();
			}
			else
			{
				handler = _fileHandlers.GetHandlerForPresentation(report.PathToFile);
			}
			return handler.GetChangePresenter(report, _revisionInspector.Repository);
		}
	}
}