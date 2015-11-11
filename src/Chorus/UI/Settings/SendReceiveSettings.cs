using System;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.Help;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using SIL.Code;
using SIL.Progress;
using Chorus.Model;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private SettingsModel _model;

		private ServerSettingsModel _internetModel;

		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Make sure that the html content is displayed properly on both linux and windows by using Navigate on the chorusHubSetup control after the dialog is shown
		/// </summary>
		private void SendReceiveSettingsShown(object sender, EventArgs e)
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendReceiveSettings));
			var hubSetupText = LocalizationManager.GetString(@"Chorus_ChorusHubSetupInstructionsHtml", resources.GetString(@"ChorusHubSetupInstructionsHTML"),
				@"Instructions shown before first Send/Receive. Please keep HTML tags as-is to preserve formatting, and use HTML escapes");
			var tempFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".html"));
			File.WriteAllText(tempFile, hubSetupText);
			var uri = new Uri(tempFile);
			chorusHubSetup.Navigate(uri.AbsoluteUri);
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();

			Shown += SendReceiveSettingsShown;
			RequireThat.Directory(repositoryLocation).Exists();
			var repository = HgRepository.CreateOrUseExisting(repositoryLocation, new NullProgress());
			_model = new SettingsModel(repository);
			userNameTextBox.Text = _model.GetUserName(new NullProgress());

			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl.Model = _internetModel;

			_internetButtonEnabledCheckBox.CheckedChanged += internetCheckChanged;
			_internetButtonEnabledCheckBox.Checked = Properties.Settings.Default.InternetEnabled;
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;

			_showChorusHubInSendReceive.Checked = Properties.Settings.Default.ShowChorusHubInSendReceive;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			if(_internetButtonEnabledCheckBox.Checked)
			{
				_internetModel.SaveSettings();
			}
			_model.SaveSettings();
			Properties.Settings.Default.InternetEnabled = _internetButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.ShowChorusHubInSendReceive = _showChorusHubInSendReceive.Checked;
			Properties.Settings.Default.Save();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void userNameTextBox_TextChanged(object sender, EventArgs e)
		{
			var _userName = userNameTextBox;
			if (_model.GetUserName(new NullProgress()) != _userName.Text.Trim() && _userName.Text.Trim().Length > 0)
			{
				_model.SetUserName(_userName.Text.Trim(), new NullProgress());
			}
		}

		private void internetCheckChanged(object sender, EventArgs e)
		{
			_serverSettingsControl.Enabled = _internetButtonEnabledCheckBox.Checked;
		}

		private void _helpButton_Click(object sender, EventArgs e)
		{
			string helpFile = HelpUtils.GetHelpFile();

			var selectedTab = settingsTabs.SelectedTab;
			if (selectedTab == internetTab)
			{
				Help.ShowHelp(this, helpFile,
					"Tasks/Internet_tab.htm");
			}
			else if (selectedTab == chorusHubTab)
			{
				Help.ShowHelp(this, helpFile,
					"/Tasks/Chorus_Hub_tab.htm");
			}
		}
	}
}
