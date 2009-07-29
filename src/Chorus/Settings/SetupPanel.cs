using System;
using System.IO;
using System.Windows.Forms;
using Other=Chorus.Utilities.Other;

namespace Chorus.Settings
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