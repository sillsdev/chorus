using System;
using System.IO;
using System.Windows.Forms;
using Chorus.Utilities;

namespace Chorus.UI.Settings
{
	public partial class SetupPanel : UserControl
	{
		public SetupPanel()
		{
			InitializeComponent();
		}

		private void SetupPanel_Load(object sender, EventArgs e)
		{
			string path = Path.Combine(ExecutionEnvironment.DirectoryOfExecutingAssembly, "UI/SetupPanel.htm");
			webBrowser1.Document.Write(File.ReadAllText(path));
			// webBrowser1.DocumentText = File.ReadAllText(path);
			//    webBrowser1.Refresh();
		}
	}
}