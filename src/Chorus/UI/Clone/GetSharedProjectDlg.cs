using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Chorus.ChorusHub;

namespace Chorus.UI.Clone
{
	internal partial class GetSharedProjectDlg : Form
	{
		private GetSharedProjectModel _model;

		internal GetSharedProjectDlg()
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
			//disable all initially
			_useUSBButton.Enabled = _useInternetButton.Enabled = _useChorusHubButton.Enabled = false;
			Shown += GetSharedProjectDlgShown;
		}

		void GetSharedProjectDlgShown(object sender, EventArgs e)
		{
			Shown -= GetSharedProjectDlgShown;
			SetButtonStates();
		}

		private void SetButtonStates()
		{
			_useUSBButton.Enabled = new CloneFromUsb().GetHaveOneOrMoreUsbDrives();
			_useInternetButton.Enabled = NetworkInterface.GetIsNetworkAvailable();
			var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
			_useChorusHubButton.Enabled = ((chorusHubServerInfo != null) && (chorusHubServerInfo.ServerIsCompatibleWithThisClient));
		}

		internal void InitFromModel(GetSharedProjectModel model)
		{
			_model = model;
		}

		private void BtnUsbClicked(object sender, EventArgs e)
		{
			_model.RepositorySource = ExtantRepoSource.Usb;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void BtnInternetClicked(object sender, EventArgs e)
		{
			_model.RepositorySource = ExtantRepoSource.Internet;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void BtnChorusHubClicked(object sender, EventArgs e)
		{
			_model.RepositorySource = ExtantRepoSource.ChorusHub;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
