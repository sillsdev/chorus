using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

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
			var repository = HgRepository.CreateOrLocate(repositoryLocation, new NullProgress());
			_model = new SettingsModel(repository);
			userNameTextBox.Text = _model.GetUserName(new NullProgress());

			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl.Model = _internetModel;

			_internetButtonEnabledCheckBox.Checked = Properties.Settings.Default.InternetEnabled;
//			_serverSettingsControl = new ServerSettingsControl(_internetModel);
//			_serverSettingsControl.Dock = DockStyle.Fill;
//			_internetButtonEnabledCB = new CheckBox { Text = "Show Internet as Send/Receive option",
//													  Checked = Properties.Settings.Default.InternetEnabled,
//													  CheckAlign = ContentAlignment.TopLeft,
//													  TextAlign = ContentAlignment.BottomLeft };
//			var internetPanel = new FlowLayoutPanel {AutoSize = true, FlowDirection = FlowDirection.TopDown};
//			internetPanel.Controls.Add(_internetButtonEnabledCB);
//			internetPanel.Controls.Add(_serverSettingsControl);
//			internetTab.Controls.Add(internetPanel);

			_sharedFolderModel = new NetworkFolderSettingsModel();
			_sharedFolderModel.InitFromProjectPath(repositoryLocation);
			_sharedFolderSettingsControl.Model = _sharedFolderModel;
			_sharedFolderButtonEnabledCheckBox.Checked = Properties.Settings.Default.SharedFolderEnabled;
//			_sharedFolderSettingsControl = new NetworkFolderSettingsControl(_sharedFolderModel);
//			_sharedFolderButtonEnabledCheckBox = new CheckBox { Text = "Show Network Folder as Send/Receive option",
//														  Checked = Properties.Settings.Default.SharedFolderEnabled,
//														  CheckAlign = ContentAlignment.TopLeft,
//														  TextAlign = ContentAlignment.BottomLeft };
//			var folderPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown };
//			folderPanel.Controls.Add(_sharedFolderButtonEnabledCheckBox);
//			folderPanel.Controls.Add(_sharedFolderSettingsControl);
//			networkFolderTab.Controls.Add(folderPanel);
			settingsTabs.SelectedIndexChanged += new EventHandler(settingsTabSelectionChanged);
		}

		void settingsTabSelectionChanged(object sender, EventArgs e)
		{
//			if (settingsTabs.SelectedTab == networkFolderTab)
//			{
//				if (DialogResult.Cancel ==
//				    MessageBox.Show(
//				    	"Sharing repositories over a local network may sometimes cause a repository to become corrupted. This can be repaired by copying one of the good copies of the repository, but it may require expert help. If you have a good internet connection or a small enough group to pass a USB key around, we recommend one of the other Send/Receive options.",
//				    	"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning))
//				{
//					_sharedFolderSettingsControl.Enabled = false;
//				}
//				else
//				{
//					_sharedFolderSettingsControl.Enabled = true;
//				}
//
//			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			_internetModel.SaveSettings();
			_sharedFolderModel.SaveSettings();
			_model.SaveSettings();
			Properties.Settings.Default.InternetEnabled = _internetButtonEnabledCheckBox.Checked;
			Properties.Settings.Default.SharedFolderEnabled = _sharedFolderButtonEnabledCheckBox.Checked;
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

		private void nameLabel_Click(object sender, EventArgs e)
		{

		}
	}
}
