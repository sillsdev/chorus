using System;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.Help;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Code;
using Palaso.Progress;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private SettingsModel _model;

		private ServerSettingsModel _internetModel;

		private NetworkFolderSettingsModel _sharedFolderModel;

		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();

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

			_sharedFolderModel = new NetworkFolderSettingsModel();
			_sharedFolderModel.InitFromProjectPath(repositoryLocation);
			_sharedFolderSettingsControl.Model = _sharedFolderModel;

			_showSharedFolderInSendReceive.Checked = Properties.Settings.Default.SharedFolderEnabled;
			_showSharedFolderInSendReceive.CheckedChanged += networkFolderCheckChanged;
			_sharedFolderSettingsControl.Enabled = _showSharedFolderInSendReceive.Checked;

			_showChorusHubInSendReceive.Checked = Properties.Settings.Default.ShowChorusHubInSendReceive;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			if(_internetButtonEnabledCheckBox.Checked)
			{
				_internetModel.SaveSettings();
			}
			if (_showSharedFolderInSendReceive.Checked)
			{
				_sharedFolderModel.SaveSettings();
			}
			_model.SaveSettings();
			Properties.Settings.Default.InternetEnabled = _internetButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.SharedFolderEnabled = _showSharedFolderInSendReceive.Checked;
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

		private void networkFolderCheckChanged(object sender, EventArgs e)
		{
			_sharedFolderSettingsControl.Enabled = _showSharedFolderInSendReceive.Checked;
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
			else if (selectedTab == networkFolderTab)
			{
				Help.ShowHelp(this, helpFile,
					"Tasks/Network_Folder_tab.htm");
			}
			else if (selectedTab == chorusHubTab)
			{
				Help.ShowHelp(this, helpFile,
					"/Tasks/Chorus_Hub_tab.htm");
			}
		}
	}
}
