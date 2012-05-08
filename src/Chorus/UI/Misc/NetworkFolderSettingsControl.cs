using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chorus.UI.Misc
{
	public partial class NetworkFolderSettingsControl : UserControl
	{
		private NetworkFolderSettingsModel _model;

		public NetworkFolderSettingsControl()
		{
			InitializeComponent();
		}

		public NetworkFolderSettingsModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				sharedFolderTextbox.Text = _model.SharedFolder;
			}
		}

		private void sharedFolderTextbox_TextChanged(object sender, EventArgs e)
		{
			_model.SharedFolder = sharedFolderTextbox.Text;
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			var folderBrowser = new FolderBrowserDialog {Description = "Select the folder where you want your shared projects to go, or the folder where an existing repository for your project is.", SelectedPath = _model.SharedFolder, ShowNewFolderButton = true};
			var result = folderBrowser.ShowDialog();
			if(result == DialogResult.OK)
			{
				sharedFolderTextbox.Text = folderBrowser.SelectedPath;
			}
		}
	}
}
