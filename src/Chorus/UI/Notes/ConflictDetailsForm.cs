using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
			// Using _existingMessagesDisplay.DocumentText =  causes an exception on mono
#if MONO
			// Todo Linux (JohnT): is this the only thing that needs to be different?
			text = text.Replace("'", "\'");
			_conflictDisplay.Navigate("javascript:{document.body.outerHTML = '" + text + "';}");
#else
			_conflictDisplay.DocumentText = text;
			_conflictDisplay.WebBrowserShortcutsEnabled = true;
#endif
		}

		public string TechnicalDetails { get; set; }

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_conflictDisplay.Document.ExecCommand(@"Copy", false, null);
		}

		private void technicalDetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			_conflictDisplay.DocumentText = TechnicalDetails;
		}
	}
}
