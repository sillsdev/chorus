using System;
using System.Windows.Forms;

namespace Chorus.notes
{
	public partial class ConflictDetailsForm : Form
	{
		public ConflictDetailsForm()
		{
			InitializeComponent();
		}

		public void SetDocumentText(string text)
		{
			_conflictDisplay.IsWebBrowserContextMenuEnabled = false;
			_conflictDisplay.DocumentText = text;
			_conflictDisplay.WebBrowserShortcutsEnabled = true;
		}

		public string TechnicalDetails { get; set; }

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
#if MONO
			//GECKOFX: what to do?
#else
			_conflictDisplay.Document.ExecCommand(@"Copy", false, null);
#endif
		}

		private void technicalDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_conflictDisplay.DocumentText = TechnicalDetails;
		}
	}
}
