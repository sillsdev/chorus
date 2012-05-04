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

		[Obsolete("For designer purposes only.")]
		public NetworkFolderSettingsControl()
		{
			InitializeComponent();
		}

		/// <summary>
		/// The Constructor that should be used when working with this control
		/// </summary>
		/// <param name="sharedFolderModel"></param>
		public NetworkFolderSettingsControl(NetworkFolderSettingsModel sharedFolderModel)
		{
			_model = sharedFolderModel;
			InitializeComponent();
			sharedFolderTextbox.Text = _model.SharedFolder;
		}

		private void sharedFolderTextbox_TextChanged(object sender, EventArgs e)
		{
			_model.SharedFolder = sharedFolderTextbox.Text;
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			var folderBrowser = new FolderBrowserDialog {SelectedPath = _model.SharedFolder, ShowNewFolderButton = true};
			var result = folderBrowser.ShowDialog();
			if(result == DialogResult.OK)
			{
				sharedFolderTextbox.Text = folderBrowser.SelectedPath;
			}
		}
	}
}
