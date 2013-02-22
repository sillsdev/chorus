using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.Utilities.Help;

namespace Chorus.UI.Clone
{
	public partial class DuplicateProjectWarningDialog : Form
	{
		public DuplicateProjectWarningDialog()
		{
			InitializeComponent();
		}

		public void Run(string projectWithExistingRepo, string howToSendReceiveMessageText)
		{
			// Review JohnH: this is probably not generic enough to use for WeSay, Bloom, and other clients.
			// If we need to fix it
			_mainLabel.Text = string.Format(
				"The project \"{0}\" on this computer is already using this repository. {1}",
				projectWithExistingRepo, howToSendReceiveMessageText);
			ShowDialog();
		}


		private void buttonHelp_Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, HelpUtils.GetHelpFile(),
					"Tasks/Internet_tab.htm"); // Todo: replace with real address when topic created.
		}
	}
}
