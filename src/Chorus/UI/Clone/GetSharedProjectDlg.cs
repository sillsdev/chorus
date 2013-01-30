using System;
using System.Windows.Forms;

namespace Chorus.UI.Clone
{
	internal partial class GetSharedProjectDlg : Form
	{
		private GetSharedProjectModel _model;

		internal GetSharedProjectDlg()
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
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
