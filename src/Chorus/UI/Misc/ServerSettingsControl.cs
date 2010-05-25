using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Chorus.UI.Misc
{
	///<summary>
	/// This control lets the user identify the server to use with send/receive,
	/// including account information. Normally used with Either ServerSEetingsDialog,
	/// or in conjunction with TargetFolderControl in GetCloneFromInterentDialog
	///</summary>
	public partial class ServerSettingsControl : UserControl
	{
		private readonly ServerSettingsModel _model;

		[Obsolete("for forms designer only")]
		public ServerSettingsControl()
		{
			InitializeComponent();
		}

		public ServerSettingsControl(ServerSettingsModel model)
		{
			_model = model;

			InitializeComponent();

			foreach (KeyValuePair<string, string> pair in _model.Servers)
			{
				_serverCombo.Items.Add(pair.Key);
			}
			_serverCombo.SelectedIndexChanged += OnSelectedIndexChanged;
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_model.SelectedServerLabel != (string)_serverCombo.SelectedItem)
			{
				_model.SelectedServerLabel = (string) _serverCombo.SelectedItem;

				UpdateDisplay();
			}
		}

		private void UpdateDisplay()
		{
			_serverCombo.SelectedItem = _model.SelectedServerLabel;

			_customUrl.Text = _model.URL;
			_customUrl.Visible = _model.CustomUrlSelected;
			_customUrlLabel.Visible = _model.CustomUrlSelected;

			_accountName.Text = _model.AccountName;
			_password.Text = _model.Password;
			_projectId.Text = _model.ProjectId;


			_accountName.Visible = _model.NeedProjectDetails;
			_projectId.Visible = _model.NeedProjectDetails;
			_password.Visible = _model.NeedProjectDetails;
			_accountLabel.Visible = _model.NeedProjectDetails;
			_projectIdLabel.Visible = _model.NeedProjectDetails;
			_passwordLabel.Visible = _model.NeedProjectDetails;
		}

		private void _customUrl_TextChanged(object sender, EventArgs e)
		{
			_model.CustomUrl = _customUrl.Text.Trim();
			UpdateDisplay();
		}

		private void _projectId_TextChanged(object sender, EventArgs e)
		{
			_model.ProjectId = _projectId.Text.Trim();
			UpdateDisplay();
		}

		private void _accountName_TextChanged(object sender, EventArgs e)
		{
			_model.AccountName = _accountName.Text.Trim();
			UpdateDisplay();
		}

		private void _password_TextChanged(object sender, EventArgs e)
		{
			_model.Password = _password.Text.Trim();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
