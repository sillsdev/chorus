using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Utilities;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Notes.Browser
{
	public partial class NotesInProjectView : UserControl
	{
		public delegate NotesInProjectView Factory(IProgress progress);//autofac uses this
		private NotesInProjectViewModel _model;

		public NotesInProjectView(NotesInProjectViewModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			model.ReloadMessages += new EventHandler(OnReloadMessages);
			_model = model;
			//       _model.ProgressDisplay = new NullProgress();
			InitializeComponent();
			_messageListView.SmallImageList = AnnotationClassFactory.CreateImageListContainingAnnotationImages();
			showClosedNotesToolStripMenuItem1.Checked = _model.ShowClosedNotes;
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
			if (_messageListView.SelectedIndices.Count > 0)
			{
				previousItem = _messageListView.SelectedItems[0].Tag as ListMessage;
			}

			Cursor.Current = Cursors.WaitCursor;
			_messageListView.SuspendLayout();
			_messageListView.Items.Clear();
			List<ListViewItem> rows = new List<ListViewItem>();

			foreach (var item in _model.GetMessages())
			{
				rows.Add(item.GetListViewItem());
			}
			_messageListView.Items.AddRange(rows.ToArray());
			_messageListView.ResumeLayout();
			Cursor.Current = Cursors.Default;

			//restore the previous selection
			if (previousItem !=null)
			{
				foreach (ListViewItem listViewItem in _messageListView.Items)
				{
					if (((ListMessage)(listViewItem.Tag)).Message.Guid == previousItem.Message.Guid)
					{
						listViewItem.Selected = true;
						break;
					}
				}
			}
			//enhance...we could, if the message is not found, go looking for the owning annotation. But since
			//you can't currently delete a message, that wouldn't have any advantage yet.

			//this leads to hiding the annotationview when nothing is actually selected anymore (because of searching)
			OnSelectedIndexChanged(null, null);
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
		}

		private void _filterCombo_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void showClosedNotesToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			_model.ShowClosedNotes = ((ToolStripMenuItem)sender).Checked;
		}
	}
}