using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Utilities;

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
			else
			{
				//Enhance: this is never called (e.g. we get a call when we become visible, but not invisible)
				_model.GoodTimeToSave();
			}
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