using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI
{
	public partial class HistoryPanel : UserControl
	{
	  private HistoryPanelModel _model;
		private ProjectFolderConfiguration _project;
		private String _userName="anonymous";

		public HistoryPanel()
		{
			InitializeComponent();
			UpdateDisplay();
		}

		public ProjectFolderConfiguration ProjectFolderConfig
		{
			get { return _project; }
			set { _project = value; }
		}

		/// <summary>
		/// most client apps won't have anything to put in here, that's ok
		/// </summary>
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}


		private void UpdateDisplay()
		{
		}

		private void HistoryPanel_Load(object sender, EventArgs e)
		{

			_model = new HistoryPanelModel(_project, null);

		}

		private void _loadButton_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			_historyText.Text = "";
			foreach (RevisionDescriptor item in _model.GetHistoryItems())
			{
				_historyText.Text += String.Format("{0}, {1}, {2}\r\n", item.DateString, item.UserId, item.Summary);
			}
			Cursor.Current = Cursors.Default;
		}
	}
}
