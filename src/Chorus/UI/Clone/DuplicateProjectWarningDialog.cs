using System;
using System.Windows.Forms;
using Chorus.Utilities.Help;
using L10NSharp;

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
				LocalizationManager.GetString(@"Messages.RepositoryInUseByProject","The project \"{0}\" on this computer is already using this repository. {1}"),
				projectWithExistingRepo, howToSendReceiveMessageText);
			ShowDialog();
		}


		private void buttonHelp_Click(object sender, EventArgs e)
		{
			Help.ShowHelp(this, HelpUtils.GetHelpFile(), @"/Chorus/Duplicate_Project_message.htm");
		}
	}
}
