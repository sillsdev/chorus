using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Palaso.Progress;

namespace Chorus.UI.Notes.Browser
{
	public partial class NotesInProjectView : UserControl
	{
		public delegate NotesInProjectView Factory(IProgress progress);//autofac uses this
		private NotesInProjectViewModel _model;

		public NotesInProjectView(NotesInProjectViewModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_model = model;
			_model.ReloadMessages += new EventHandler(OnReloadMessages);
			_model.CancelSelectedMessageChanged += SuspendLayout;
			//       _model.ProgressDisplay = new NullProgress();
			InitializeComponent();
			_messageListView.SmallImageList = AnnotationClassFactoryUI.CreateImageListContainingAnnotationImages();
			showResolvedNotesMenuItem.Checked = _model.ShowClosedNotes;
			showQuestionsMenuItem.Checked = !_model.HideQuestions;
			showMergeNotifcationsMenuItem.Checked = !_model.HideNotifications;
			showMergeConflictsMenuItem.Checked = !_model.HideCriticalConflicts;
			timer1.Interval = 1000;
			timer1.Tick += new EventHandler(timer1_Tick);
			timer1.Enabled = true;
		}

		void timer1_Tick(object sender, EventArgs e)
		{
			if(Visible)
			{
				_model.CheckIfWeNeedToReload();
			}
		}


		void OnReloadMessages(object sender, EventArgs e)
		{
			ListMessage previousItem = null;
			int previousIndex = -1;
			if (_messageListView.SelectedIndices.Count > 0)
			{
				previousItem = _messageListView.SelectedItems[0].Tag as ListMessage;
				previousIndex = _messageListView.SelectedIndices[0];
			}

			Cursor.Current = Cursors.WaitCursor;
			_messageListView.SuspendLayout();

			List<ListViewItem> rows = new List<ListViewItem>();
			foreach (var item in _model.GetMessages())
			{
				rows.Add(item.GetListViewItem(_model.DisplaySettings));
			}
			_messageListView.Items.Clear(); // Don't even think of moving this before the loop, as the items are doubled for reasons unknown.
			_messageListView.Items.AddRange(rows.ToArray());

			//restore the previous selection
			if (_model.SelectedMessageGuid != null)
			{
				if (_messageListView.Items.Count > 0)
				{
					// Enhance pH: if the message was not found, check if it was resolved and no longer met the filter,
					// or if the user changed the filter to exclude the message.  Reselect only if the message was
					// [un]resolved
					if (!SilentSelectByGuid(_model.SelectedMessageGuid))
					{
						// Likely we hid the item that was previously selected.
						// Select something, preferably the item at the same position.
						if (previousIndex < 0)
							_messageListView.Items[0].Selected = true;
						else if (_messageListView.Items.Count > previousIndex) // usual case, if we deleted one thing and not the last
							_messageListView.Items[previousIndex].Selected = true;
						else
							_messageListView.Items[_messageListView.Items.Count - 1].Selected = true; // closest to original index
					}
				}
				else
				{
					// hides the annotationview when there was a message selected but are no more available (due to searching)
					OnSelectedIndexChanged(null, null);
				}
			}
			//enhance...we could, if the message is not found, go looking for the owning annotation. But since
			//you can't currently delete a message, that wouldn't have any advantage yet.
			filterStateLabel.Text = _model.FilterStateMessage;

			_messageListView.ResumeLayout();
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Selects the item with the specified GUID, if available, and temporarily disables
		/// OnSelectedIndexChanged events while selecting
		/// </summary>
		/// <param name="guid"></param>
		/// <returns>true iff the specified item is available to be selected</returns>
		private bool SilentSelectByGuid(string guid)
		{
			foreach (ListViewItem listViewItem in _messageListView.Items)
			{
				if (((ListMessage)(listViewItem.Tag)).ParentAnnotation.Guid == guid)
				{
					var temporarilyDisabledEventHandlers = _model.EventToRaiseForChangedMessage;
					_model.EventToRaiseForChangedMessage = null;
					listViewItem.Selected = true;
					_model.EventToRaiseForChangedMessage = temporarilyDisabledEventHandlers;
					return true;
				}
			}
			return false;
		}

		private void SilentSelectAndUnselectByGuid(object guid, CancelEventArgs e)
		{
			e.Cancel = true;

			var temporarilyDisabledEventHandlers = _model.EventToRaiseForChangedMessage;
			_model.EventToRaiseForChangedMessage = null;

			foreach (ListViewItem listViewItem in _messageListView.Items)
			{
				if (((ListMessage)(listViewItem.Tag)).ParentAnnotation.Guid == (string)guid)
				{
					listViewItem.Selected = true;
					e.Cancel = false;
				}
				else
				{
					listViewItem.Selected = false;
				}
			}

			_model.EventToRaiseForChangedMessage = temporarilyDisabledEventHandlers;
		}

		private void SuspendLayout(object sender, CancelEventArgs e)
		{
			_messageListView.SuspendLayout();
		}

		// TODO pH 2013.08: make this event preventable
		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_messageListView.SelectedItems.Count == 0)
			{
				_model.SelectedMessageChanged(null);
			}
			else
			{
				_model.SelectedMessageChanged(_messageListView.SelectedItems[0].Tag as ListMessage);
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			//OnReloadMessages(null,null);
			_model.CheckIfWeNeedToReload();
		}

		private void searchBox1_SearchTextChanged(object sender, EventArgs e)
		{
			_model.SearchTextChanged(sender as string);
		}

		private void NotesInProjectView_VisibleChanged(object sender, EventArgs e)
		{
			if (this.Visible)
				_model.CheckIfWeNeedToReload();
			else
			{
				//Enhance: this is never called (e.g. we get a call when we become visible, but not invisible)
				_model.GoodTimeToSave();
			}
		}

		private void _filterCombo_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void showClosedNotesMenuItem_Click(object sender, EventArgs e)
		{
			_model.ShowClosedNotes = ((ToolStripMenuItem)sender).Checked;
		}

		private void showQuestionsMenuItem_Click(object sender, EventArgs e)
		{
			_model.HideQuestions = !((ToolStripMenuItem)sender).Checked;
		}

		private void showMergeNotificationsMenuItem_Click(object sender, EventArgs e)
		{
			_model.HideNotifications = !((ToolStripMenuItem)sender).Checked;
		}

		private void showMergeConflictsMenuItem_Click(object sender, EventArgs e)
		{
			_model.HideCriticalConflicts = !((ToolStripMenuItem)sender).Checked;
		}
	}
}