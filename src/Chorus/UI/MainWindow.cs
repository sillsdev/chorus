using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	public partial class MainWindow : Form
	{
		[STAThread]
		static void Main(string[] args)
		{
			 string settingsPath =null;
			if(args.Length > 0)
			{
				settingsPath = args[0];
			}
//            string s = RepositoryManager.GetEnvironmentReadinessMessage("en");
//            if(!string.IsNullOrEmpty(s))
//            {
//                MessageBox.Show(s, "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
//                return;
//            }
			Application.Run(new MainWindow(settingsPath));
		}

		public MainWindow()
		{
			InitializeComponent();
		}
		public MainWindow(string settingsPath)
		{
			InitializeComponent();
			_syncPanel.ProjectFolderConfig = new ProjectFolderConfiguration(Path.GetDirectoryName(settingsPath));
			_historyPanel.ProjectFolderConfig = _syncPanel.ProjectFolderConfig;
		}

	}
}