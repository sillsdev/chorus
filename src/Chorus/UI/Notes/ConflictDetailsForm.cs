using System;
using System.Text;
using System.Windows.Forms;

namespace Chorus.notes
{
	public partial class ConflictDetailsForm : Form
	{
		string _tempPath;
		private string _documentText;
		public ConflictDetailsForm()
		{
			InitializeComponent();
		}

		public void SetDocumentText(string text)
		{
			_conflictDisplay.IsWebBrowserContextMenuEnabled = false;
			// It would be nice to avoid writing a temp file and then loading it,
			// but the Linux gecko didn't display anything with the code that sent
			// the html text directly to the embedded browser. In the interest of
			// keeping the code the same for all platforms, the approach below is
			// now used.
			if (_tempPath == null)
				_tempPath = SIL.IO.TempFile.WithExtension("htm").Path;
			System.IO.File.WriteAllText(_tempPath, text);
			_documentText = text;
			_conflictDisplay.Url = new Uri(_tempPath);
			_conflictDisplay.WebBrowserShortcutsEnabled = true;
		}

		public string TechnicalDetails { get; set; }

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clipboard.Clear();    //Clear if any old value is there in Clipboard        
			Clipboard.SetText(_documentText); //Copy text to Clipboard
		}

		private void technicalDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_conflictDisplay.DocumentText = TechnicalDetails;
		}
	}
}
