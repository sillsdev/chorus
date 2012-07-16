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
				if (value == null)
					return;
				sharedFolderTextbox.Text = _model.SharedFolder;
				_model.MessageBoxService = new MessageBoxService();
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

		private void _networkWarningButton_Click(object sender, EventArgs e)
		{
			MessageBox.Show(
				"Using a simple shared network folder is somewhat experimental. We don’t yet know if it will be reliable for you, or not… it is difficult for us to test all the different kinds of networks out there. So if you use this feature, please let us know how it goes for you, whether you have success or problems.",
				"Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private class MessageBoxService : NetworkFolderSettingsModel.IMessageBoxService
		{
			public DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
			{
				return MessageBox.Show(message, title, buttons, icon);
			}
		}
	}
}
