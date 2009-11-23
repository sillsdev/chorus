using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.annotations;

namespace Chorus.UI.Notes
{
	public partial class NotesInProjectView : UserControl
	{
		private NotesInProjectViewModel _viewModel;

		public NotesInProjectView(NotesInProjectViewModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_viewModel = model;
			//       _model.ProgressDisplay = new NullProgress();
			InitializeComponent();
			_messageListView.SmallImageList = AnnotationClassFactory.CreateImageListContainingAnnotationImages();
			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
		}

		public void ReloadMessages()
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
		}


		private void OnRefresh(object sender, EventArgs e)
		{
			ReloadMessages();
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_messageListView.SelectedItems.Count > 0)
			{
				_viewModel.SelectedMessageChanged(_messageListView.SelectedItems[0].Tag as ListMessage);
			}
		}

		private void OnLoad(object sender, EventArgs e)
		{
			ReloadMessages();
		}
	}
}