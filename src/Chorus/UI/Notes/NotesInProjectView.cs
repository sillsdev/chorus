using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Chorus.UI.Notes
{
	public partial class NotesInProjectView : UserControl
	{
		private NotesInProjectModel _model;

		public NotesInProjectView(NotesInProjectModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_model = model;
			//       _model.ProgressDisplay = new NullProgress();
			InitializeComponent();
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
			foreach (var item in _model.GetMessages())
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

		}

		private void OnLoad(object sender, EventArgs e)
		{
			ReloadMessages();
		}
	}
}