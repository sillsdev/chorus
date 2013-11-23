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
#if MONO
			_conflictDisplay.LoadHtml(text);
			// disable right click menu (it would stay up and never go away, and we don't want it)
			_conflictDisplay.NoDefaultContextMenu = true;
#else
			_conflictDisplay.DocumentText = text;
			_conflictDisplay.WebBrowserShortcutsEnabled = true;
#endif
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
#if MONO
			_conflictDisplay.LoadHtml(TechnicalDetails);
#else
			_conflictDisplay.DocumentText = TechnicalDetails;
#endif
		}
	}
}
