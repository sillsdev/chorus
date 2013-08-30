using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
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
			InitializeComponent();
			_messageListView.SmallImageList = AnnotationClassFactoryUI.CreateImageListContainingAnnotationImages();
			showResolvedNotesMenuItem.Checked = _model.ShowClosedNotes;
			showQuestionsMenuItem.Checked = _model.ShowQuestions;
			showMergeNotifcationsMenuItem.Checked = _model.ShowNotifications;
			showMergeConflictsMenuItem.Checked = _model.ShowConflicts;
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
			int previousIndex = _messageListView.SelectedIndices.Count > 0 ? _messageListView.SelectedIndices[0] : -1;

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
					// or if the user changed the filter to exclude the message.  Reselect only if the message was resolved
					if (!SelectByGuid(_model.SelectedMessageGuid))
					{
						// Likely we hid the item that was previously selected.
						// Select something, preferably the item at the same position.
						_model.ClearSelectedMessage(); // must be cleared before another is selected
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
		/// Selects the item with the specified GUID, if available; otherwise, returns false
		/// </summary>
		/// <param name="guid"></param>
		/// <returns>true iff the specified item is available to be selected</returns>
		private bool SelectByGuid(string guid)
		{
			foreach (ListViewItem listViewItem in _messageListView.Items)
			{
				if (((ListMessage)(listViewItem.Tag)).ParentAnnotation.Guid == guid)
				{
					listViewItem.Selected = true;
					return true;
				}
			}
			return false;
		}

		private void SuspendLayout(object sender, CancelEventArgs e)
		{
			_messageListView.SuspendLayout();
		}

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
			_model.CheckIfWeNeedToReload();
		}

		private void NotesInProjectView_VisibleChanged(object sender, EventArgs e)
		{
			if (this.Visible)
				_model.CheckIfWeNeedToReload();
			else
			{
				//Enhance: this is never called (i.e. we get a call when we become visible, but not invisible)
				_model.GoodTimeToSave();
			}
		}

		private void showClosedNotesMenuItem_Click(object sender, EventArgs e)
		{
			_model.ShowClosedNotes = ((ToolStripMenuItem)sender).Checked;
			// resync view, in case the event was canceled (due to an unsaved message)
			((ToolStripMenuItem)sender).Checked = _model.ShowClosedNotes;
		}

		private void showQuestionsMenuItem_Click(object sender, EventArgs e)
		{
			_model.ShowQuestions = ((ToolStripMenuItem)sender).Checked;
			// resync view, in case the event was canceled (due to an unsaved message)
			((ToolStripMenuItem)sender).Checked = _model.ShowQuestions;
		}

		private void showMergeConflictsMenuItem_Click(object sender, EventArgs e)
		{
			_model.ShowConflicts = ((ToolStripMenuItem)sender).Checked;
			// resync view, in case the event was canceled (due to an unsaved message)
			((ToolStripMenuItem)sender).Checked = _model.ShowConflicts;
		}

		private void showMergeNotificationsMenuItem_Click(object sender, EventArgs e)
		{
			_model.ShowNotifications = ((ToolStripMenuItem)sender).Checked;
			// resync view, in case the event was canceled (due to an unsaved message)
			((ToolStripMenuItem)sender).Checked = _model.ShowNotifications;
		}

		private void searchBox1_SearchTextChanged(object sender, EventArgs e)
		{
			// If there is no change (the previous change was canceled), don't waste time re-verifying the filter
			if (_model.SearchText == sender as string) return;

			_model.SearchText = sender as string;
			// resync view, in case the event was canceled (due to an unsaved message)
			searchBox1.SearchText = _model.SearchText;
		}
	}
}