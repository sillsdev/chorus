using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using ChorusHub;

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
			//Handle the button enabling in a background thread to let the dialog appear quicker
			var worker = new BackgroundWorker();
			worker.DoWork += (sender, e) =>
			{
				_useUSBButton.Enabled = new CloneFromUsb().GetHaveOneOrMoreUsbDrives();
				_useInternetButton.Enabled = NetworkInterface.GetIsNetworkAvailable();
				var client = new ChorusHubClient();
				_useChorusHubButton.Enabled = client.FindServer() != null;
			};
			worker.RunWorkerAsync();
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
			MessageBox.Show("Tada!");
			_model.RepositorySource = ExtantRepoSource.ChorusHub;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
