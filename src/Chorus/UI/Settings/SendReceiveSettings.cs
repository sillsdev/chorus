using System;
using System.Windows.Forms;
using Chorus.UI.Misc;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private HgRepository _repository;

		private ServerSettingsModel _internetModel;
		private ServerSettingsControl _serverSettingsControl;

		private NetworkFolderSettingsModel _sharedFolderModel;
		private NetworkFolderSettingsControl _sharedFolderSettingsControl;

		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();

			RequireThat.Directory(repositoryLocation).Exists();
			_repository = HgRepository.CreateOrLocate(repositoryLocation, new NullProgress());
			userNameTextBox.Text = _repository.GetUserIdInUse();

			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl = new ServerSettingsControl(_internetModel);
			internetTab.Controls.Add(_serverSettingsControl);

			_sharedFolderModel = new NetworkFolderSettingsModel();
			_sharedFolderModel.InitFromProjectPath(repositoryLocation);
			_sharedFolderSettingsControl = new NetworkFolderSettingsControl(_sharedFolderModel);
			networkFolderTab.Controls.Add(_sharedFolderSettingsControl);
			networkFolderTab.Click += new EventHandler(networkFolderTab_Click);
		}

		void networkFolderTab_Click(object sender, EventArgs e)
		{
			if (DialogResult.Cancel ==
				MessageBox.Show(
				"Sharing repositories over a local network may sometimes cause a repository to become corrupted. This can be repaired by copying one of the good copies of the repository, but it may require expert help. If you have a good internet connection or a small enough group to pass a USB key around, we recommend one of the other Send/Receive options.",
				"Warning", MessageBoxButtons.OKCancel))
			{
				internetTab.Focus();
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			_internetModel.SaveSettings();
			_sharedFolderModel.SaveSettings();
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
			if (_repository.GetUserIdInUse() != _userName.Text.Trim() && _userName.Text.Trim().Length > 0)
			{
				_repository.SetUserNameInIni(_userName.Text.Trim(), new NullProgress());
			}
		}
	}
}
