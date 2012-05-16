using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	public partial class GetCloneFromNetworkFolderDlg : Form
	{
		public GetCloneFromNetworkFolderDlg()
		{
			InitializeComponent();
		}

		private void folderBrowserControl_PathChanged(object sender, EventArgs e)
		{
			statusLabel.Text = folderBrowserControl.SelectedPath;
		}

		private void panel_Resize(object sender, EventArgs e)
		{
			const int panelMidGapHalf = 3;
			var newWidth = panel.Width;

			folderBrowserControl.Width = newWidth / 2 - panelMidGapHalf;
			lookInLabel.Width = folderBrowserControl.Width;

			projectRepositoryListView.Location = new Point(newWidth / 2 + panelMidGapHalf, projectRepositoryListView.Location.Y);
			projectRepositoryListView.Width = newWidth / 2 - panelMidGapHalf;
			chooseRepositoryLabel.Location = new Point(projectRepositoryListView.Location.X, chooseRepositoryLabel.Location.Y);
			chooseRepositoryLabel.Width = projectRepositoryListView.Width;
		}
	}
}
