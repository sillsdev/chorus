using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Other=Chorus.Utilities.Other;

namespace Chorus.UI
{
	public partial class SetupPanel : UserControl
	{
		public SetupPanel()
		{
			InitializeComponent();
		}

		private void SetupPanel_Load(object sender, EventArgs e)
		{
			string path = Path.Combine(Other.DirectoryOfExecutingAssembly, "UI/SetupPanel.htm");
			webBrowser1.Document.Write(File.ReadAllText(path));
		   // webBrowser1.DocumentText = File.ReadAllText(path);
		//    webBrowser1.Refresh();
		}
	}
}
