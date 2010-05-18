using System.Windows.Forms;

namespace Chorus.UI.Misc
{
	///<summary>
	/// This dialog lets the user identify the server to use with send/receive,
	/// including account information
	///</summary>
	public partial class ServerSettingsDialog : Form
	{
		private readonly ServerSettingsModel _model;
		private ServerSettingsControl _serverSettingsControl;

		public ServerSettingsDialog(ServerSettingsModel model)
		{
			_model = model;
			InitializeComponent();
			_serverSettingsControl = new ServerSettingsControl(model);
			_serverSettingsControl.TabIndex = 0;
			_serverSettingsControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.Controls.Add(_serverSettingsControl);
		}

		private void _closeButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void _okButton_Click(object sender, System.EventArgs e)
		{
			_model.SaveSettings();
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
