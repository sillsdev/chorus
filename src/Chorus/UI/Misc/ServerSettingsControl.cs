using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Chorus.Model;


namespace Chorus.UI.Misc
{
	///<summary>
	/// This control lets the user identify the server to use with send/receive,
	/// including account information. Normally used with Either ServerSEetingsDialog,
	/// or in conjunction with TargetFolderControl in GetCloneFromInterentDialog
	///</summary>
	public partial class ServerSettingsControl : UserControl
	{
		public event EventHandler DisplayUpdated;

		private ServerSettingsModel _model;

		public ServerSettingsControl()
		{
			InitializeComponent();
			SynchronizePasswordControls();
		}

		public ServerSettingsModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				if (value == null)
					return;
				foreach (KeyValuePair<string, string> pair in Model.Servers)
				{
					_serverCombo.Items.Add(pair.Key);
				}
				_serverCombo.SelectedIndexChanged += OnSelectedIndexChanged;
			}
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (Model.SelectedServerLabel != (string)_serverCombo.SelectedItem)
			{
				Model.SelectedServerLabel = (string)_serverCombo.SelectedItem;

				UpdateDisplay();
			}
		}

		private void UpdateDisplay()
		{
			_serverCombo.SelectedItem = Model.SelectedServerLabel;

			_customUrl.Text = Model.URL;
			_customUrl.Visible = Model.CustomUrlSelected;
			_customUrlLabel.Visible = Model.CustomUrlSelected;

			_accountName.Text = Model.AccountName;
			_password.Text = Model.Password;
			_projectId.Text = Model.ProjectId;


			_accountName.Visible = Model.NeedProjectDetails;
			_projectId.Visible = Model.NeedProjectDetails;
			_password.Visible = Model.NeedProjectDetails;
			_showCharacters.Visible = Model.NeedProjectDetails;
			_accountLabel.Visible = Model.NeedProjectDetails;
			_projectIdLabel.Visible = Model.NeedProjectDetails;
			_passwordLabel.Visible = Model.NeedProjectDetails;

			if (DisplayUpdated != null)
				DisplayUpdated.Invoke(this, null);
		}

		private void _customUrl_TextChanged(object sender, EventArgs e)
		{
			Model.CustomUrl = _customUrl.Text;
			UpdateDisplay();
		}

		private void _projectId_TextChanged(object sender, EventArgs e)
		{
			Model.ProjectId = _projectId.Text;
			UpdateDisplay();
		}

		private void _accountName_TextChanged(object sender, EventArgs e)
		{
			Model.AccountName = _accountName.Text;
			UpdateDisplay();
		}

		bool _spaceForTextBox;

		/// <summary>
		/// Record whether the incoming character was from the space bar key.
		/// </summary>
		private void _textbox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				_spaceForTextBox = true;
		}

		/// <summary>
		/// If the incoming character is a space, ignore it.
		/// </summary>
		private void _textbox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (_spaceForTextBox)
			{
				e.Handled = true;
				_spaceForTextBox = false;
			}
			else if (Char.IsWhiteSpace(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		private void _password_TextChanged(object sender, EventArgs e)
		{
			Model.Password = _password.Text.Trim();
			UpdateDisplay();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			UpdateDisplay();
		}

		private void _showCharacters_CheckedChanged(object sender, EventArgs e)
		{
			SynchronizePasswordControls();
		}

		private void SynchronizePasswordControls()
		{
			_password.UseSystemPasswordChar = !_showCharacters.Checked;
		}
	}
}
