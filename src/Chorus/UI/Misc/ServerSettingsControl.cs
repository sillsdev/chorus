using System;
using System.ComponentModel;
using System.Windows.Forms;
using Chorus.Model;


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

		private ServerSettingsModel _model;

		public ServerSettingsControl()
		{
			InitializeComponent();

			_bandwidth.Items.AddRange(ServerSettingsModel.Bandwidths);
		}

		public ServerSettingsModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				if (value == null)
					return;
				// TODO (Hasso)
			}
		}

		private void UpdateDisplay()
		{
			_accountName.Text = Model.Username;
			_password.Text = Model.Password;

			_customUrl.Text = Model.URL;
			_customUrl.Visible = Model.CustomUrlSelected;

			_buttonLogIn.Visible = !Model.CustomUrlSelected;
			_buttonLogIn.Enabled = Model.CanLogIn;

			_bandwidth.SelectedItem = Model.Bandwidth;
			_bandwidthLabel.Visible = _bandwidth.Visible = !Model.CustomUrlSelected && Model.HasLoggedIn;

			_projectId.Text = Model.ProjectId;
			_projectIdLabel.Visible = _projectId.Visible = !Model.CustomUrlSelected && Model.HasLoggedIn;

			DisplayUpdated?.Invoke(this, null);
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
			Model.Username = _accountName.Text;
			UpdateDisplay();
		}

		bool _spaceForTextBox;

		/// <summary>
		/// Record whether the incoming character was from the space bar key.
		/// </summary>
		private void _textbox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				_spaceForTextBox = true;
		}

		/// <summary>
		/// If the incoming character is a space, ignore it.
		/// </summary>
		private void _textbox_KeyPress(object sender, KeyPressEventArgs e)
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

		private void _checkCustomUrl_CheckedChanged(object sender, EventArgs e)
		{
			Model.CustomUrlSelected = _checkCustomUrl.Checked;
			UpdateDisplay();
		}

		private void _bandwidth_SelectedIndexChanged(object sender, EventArgs e)
		{
			Model.Bandwidth = (ServerSettingsModel.BandwidthItem)_bandwidth.SelectedItem;
		}

		private void _buttonLogIn_Click(object sender, EventArgs e)
		{
			Model.HasLoggedIn = !Model.HasLoggedIn; // TODO (Hasso) actually log in
			UpdateDisplay();
		}
	}
}
