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
			path = Path.GetFullPath(path);
			// GECKOFX: check that this does load
			// Not used within chorus or wesay anywhere
			// path of file now seems to be UI/Settings/SetupPanel.htm
			System.Console.WriteLine ("SetupPanel opening UI - uri="+"file://" + path);
			webBrowser1.Navigate("file://" + path);
			//webBrowser1.Document.Write(File.ReadAllText(path));
			// webBrowser1.DocumentText = File.ReadAllText(path);
			//    webBrowser1.Refresh();
		}
	}
}