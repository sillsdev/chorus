using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities;

namespace Chorus.UI.Clone
{
	public partial class GetCloneDialog : Form
	{
		private GetCloneModel _model;
		private IProgress _progress;

		public GetCloneDialog(string parentDirectoryToPutCloneIn)
		{
			Font = SystemFonts.MessageBoxFont;

			InitializeComponent();
			_model = new GetCloneModel(parentDirectoryToPutCloneIn);
			UpdateDisplay();
			_progress = new TextBoxProgress(_progressLog);
			_progressLog.Visible = false;
			_okButton.Visible = false;
		}

		private void UpdateDisplay()
		{
			_copyToComputerButton.Enabled = listView1.SelectedItems.Count == 1;
		}

		private void GetCloneDialog_Load(object sender, EventArgs e)
		{
			foreach (string  path in _model.GetDirectoriesWithMecurialRepos())
			{
				var item = new ListViewItem(System.IO.Path.GetFileName(path));
				item.Tag = path;
				var last = File.GetLastWriteTime(path);
				item.SubItems.Add(last.ToShortDateString()+" "+last.ToShortTimeString());
				item.ToolTipText = path;
				item.ImageIndex = 0;
				listView1.Items.Add(item);
			}
			_statusLabel.Text = "Select one of the following:";
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDisplay();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
		   DialogResult = DialogResult.OK;
			Close();
		}

		protected string SelectedPath
		{
			get
			{
				if(listView1.SelectedItems == null)
					return null;
				if(listView1.SelectedItems.Count == 0)
					return null;
				return listView1.SelectedItems[0].Tag as string;
			}
		}

		public string PathToNewProject { get; private set; }

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void _copyToComputerButton_Click(object sender, EventArgs e)
		{
			try
			{
				_copyToComputerButton.Visible = false;

				_statusLabel.Text = "Copying project to this computer...";
				listView1.Visible = false;

				_progressLog.Location = listView1.Location;
				_progressLog.Bounds = listView1.Bounds;
				_progressLog.Visible = true;
				_progress.ShowVerbose = true;
				PathToNewProject = _model.GetClone(SelectedPath, _progress);
				_statusLabel.Text = "Done.";
				_okButton.Visible = true;
				_cancelButton.Enabled = false;
			}
			catch (Exception error)
			{
				_statusLabel.Text = "Failed.";
			}
		}
	}
}
