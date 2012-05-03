using System;
using System.Windows.Forms;
using Chorus.UI.Misc;

namespace Chorus.UI.Settings
{
	public partial class SendReceiveSettings : Form
	{
		private ServerSettingsModel _internetModel;
		private ServerSettingsControl _serverSettingsControl;

		[Obsolete("for designer support only")]
		public SendReceiveSettings()
		{
			InitializeComponent();
		}

		public SendReceiveSettings(string repositoryLocation)
		{
			InitializeComponent();
			_internetModel = new ServerSettingsModel();
			_internetModel.InitFromProjectPath(repositoryLocation);
			_serverSettingsControl = new ServerSettingsControl(_internetModel);
			internetTab.Controls.Add(_serverSettingsControl);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			_internetModel.SaveSettings();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
