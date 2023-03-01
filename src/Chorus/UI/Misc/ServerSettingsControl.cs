using System;
using System.ComponentModel;
using System.Windows.Forms;
using Chorus.Model;
using L10NSharp;
using SIL.Extensions;

namespace Chorus.UI.Misc
{
	///<summary>
	/// This control lets the user identify the server to use with send/receive,
	/// including account information. Normally used with either ServerSettingsDialog,
	/// or in conjunction with TargetFolderControl in GetCloneFromInternetDialog
	///</summary>
	public partial class ServerSettingsControl : UserControl
	{
		public event EventHandler DisplayUpdated;

		public ServerSettingsControl()
		{
			InitializeComponent();

			// format the string with the server URL. We build our own URL because 'resumable' is not helpful here.
			_serverLabel.Text = string.Format(_serverLabel.Text, $"{(ServerSettingsModel.IsPrivateServer ? "private" : "public")}{ServerSettingsModel.LanguageForgeServer}");

			_bandwidth.Items.AddRange(ServerSettingsModel.Bandwidths);
		}

		public ServerSettingsModel Model { get; set; }

		private void UpdateDisplay()
		{
			_username.Text = Model.Username;
			_password.Text = Model.Password;
			_checkRememberPassword.Checked = Model.RememberPassword;

			_customUrl.Text = Model.URL;
			_customUrl.Visible = _checkCustomUrl.Checked = Model.IsCustomUrl;

			_tlpLogIn.Visible = !Model.IsCustomUrl;
			_buttonLogIn.Enabled = Model.CanLogIn;

			_bandwidth.SelectedItem = Model.Bandwidth;
			_bandwidthLabel.Visible = _bandwidth.Visible = !Model.IsCustomUrl && Model.HasLoggedIn;

			_projectId.Text = Model.ProjectId;
			_projectIdLabel.Visible = _projectId.Visible = !Model.IsCustomUrl && Model.HasLoggedIn;

			DisplayUpdated?.Invoke(this, null);
		}

		private void UpdateProjectIds()
		{
			var currentVal = Model.ProjectId;
			_projectId.Items.Clear();
			_projectId.Items.AddRange(Model.AvailableProjects.ToArray());
			if (!string.IsNullOrEmpty(currentVal) && Model.AvailableProjects.Contains(currentVal, StringComparison.Ordinal))
			{
				_projectId.SelectedItem = currentVal;
			}
			else if (Model.AvailableProjects.Count == 1)
				_projectId.SelectedItem = Model.AvailableProjects[0];

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

		private void _username_TextChanged(object sender, EventArgs e)
		{
			Model.Username = _username.Text;
			UpdateDisplay();
		}

		bool _spaceForTextBox;

		/// <summary>
		/// Record whether the incoming character was from the space bar key.
		/// </summary>
		private void _textBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				_spaceForTextBox = true;
		}

		/// <summary>
		/// If the incoming character is a space, ignore it.
		/// </summary>
		private void _textBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (_spaceForTextBox)
			{
				e.Handled = true;
				_spaceForTextBox = false;
			}
			else if (char.IsWhiteSpace(e.KeyChar))
			{
				e.Handled = true;
			}
		}

		private void _password_TextChanged(object sender, EventArgs e)
		{
			Model.Password = _password.Text;
			UpdateDisplay();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			UpdateDisplay();
		}

		private void _checkCustomUrl_CheckedChanged(object sender, EventArgs e)
		{
			Model.IsCustomUrl = _checkCustomUrl.Checked;
			UpdateDisplay();
		}

		private void _checkRememberPassword_CheckedChanged(object sender, EventArgs e)
		{
			Model.RememberPassword = _checkRememberPassword.Checked;
			UpdateDisplay();
		}

		private void _bandwidth_SelectedIndexChanged(object sender, EventArgs e)
		{
			Model.Bandwidth = (ServerSettingsModel.BandwidthItem)_bandwidth.SelectedItem;
			UpdateDisplay();
		}

		private void _buttonLogIn_Click(object sender, EventArgs e)
		{
			string error;
			Model.LogIn(out error);
			if (string.IsNullOrEmpty(error))
			{
				UpdateProjectIds();
			}
			else
			{
				var caption = LocalizationManager.GetString("ServerSettings.ErrorLoggingIn", "Error logging in");
				MessageBox.Show(error, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			UpdateDisplay();
		}
	}
}
