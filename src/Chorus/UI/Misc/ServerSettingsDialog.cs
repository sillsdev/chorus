using System.Windows.Forms;
using Chorus.Model;

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

		public ServerSettingsDialog(string pathToRepositoryFolder)
		{
			_model = new ServerSettingsModel();
			_model.InitFromProjectPath(pathToRepositoryFolder);
			Init();
		}

		public ServerSettingsDialog(ServerSettingsModel model)
		{
			_model = model;
			Init();
		}

		private void Init()
		{
			InitializeComponent();
			_serverSettingsControl = new ServerSettingsControl() { Model = _model };
			_serverSettingsControl.TabIndex = 0;
			_serverSettingsControl.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			_serverSettingsControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.Width = _serverSettingsControl.Width + 30;
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
