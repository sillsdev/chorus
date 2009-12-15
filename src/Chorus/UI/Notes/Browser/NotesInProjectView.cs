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
		private NotesInProjectViewModel _viewModel;

		public NotesInProjectView(NotesInProjectViewModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			model.ReloadMessages += new EventHandler(OnReloadMessages);
			_viewModel = model;
			//       _model.ProgressDisplay = new NullProgress();
			InitializeComponent();
			_messageListView.SmallImageList = AnnotationClassFactory.CreateImageListContainingAnnotationImages();
		}


		void OnReloadMessages(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			_messageListView.SuspendLayout();
			_messageListView.Items.Clear();
			List<ListViewItem> rows = new List<ListViewItem>();

			foreach (var item in _viewModel.GetMessages())
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
				_viewModel.SelectedMessageChanged(null);
			}
			else
			{
				_viewModel.SelectedMessageChanged(_messageListView.SelectedItems[0].Tag as ListMessage);
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			//OnReloadMessages(null,null);
			_viewModel.NowVisible();
		}

		private void searchBox1_SearchTextChanged(object sender, EventArgs e)
		{
			_viewModel.SearchTextChanged(sender as string);
		}

		private void NotesInProjectView_VisibleChanged(object sender, EventArgs e)
		{
			if (this.Visible)
				_viewModel.NowVisible();
		}
	}
}