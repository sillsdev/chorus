namespace Chorus.UI.Misc
{
	partial class ServerSettingsControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this._projectIdLabel = new System.Windows.Forms.Label();
			this._accountLabel = new System.Windows.Forms.Label();
			this._passwordLabel = new System.Windows.Forms.Label();
			this._projectId = new System.Windows.Forms.TextBox();
			this._accountName = new System.Windows.Forms.TextBox();
			this._password = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._showCharacters = new System.Windows.Forms.CheckBox();
			this._customUrlLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._serverCombo = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this._customUrl = new System.Windows.Forms.TextBox();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _projectIdLabel
			//
			this._projectIdLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._projectIdLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._projectIdLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._projectIdLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._projectIdLabel, "ServerSettingsControl.ProjectId");
			this._projectIdLabel.Location = new System.Drawing.Point(23, 78);
			this._projectIdLabel.Name = "_projectIdLabel";
			this._projectIdLabel.Size = new System.Drawing.Size(54, 13);
			this._projectIdLabel.TabIndex = 26;
			this._projectIdLabel.Text = "Project ID";
			//
			// _accountLabel
			//
			this._accountLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._accountLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._accountLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._accountLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._accountLabel, "ServerSettingsControl.Login");
			this._accountLabel.Location = new System.Drawing.Point(44, 112);
			this._accountLabel.Name = "_accountLabel";
			this._accountLabel.Size = new System.Drawing.Size(33, 13);
			this._accountLabel.TabIndex = 26;
			this._accountLabel.Text = "Login";
			//
			// _passwordLabel
			//
			this._passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._passwordLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._passwordLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._passwordLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._passwordLabel, "ServerSettingsControl.Password");
			this._passwordLabel.Location = new System.Drawing.Point(24, 146);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(53, 13);
			this._passwordLabel.TabIndex = 26;
			this._passwordLabel.Text = "Password";
			//
			// _projectId
			//
			this._projectId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._projectId, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._projectId, null);
			this.l10NSharpExtender1.SetLocalizingId(this._projectId, "ServerSettingsControl.ServerSettingsControl._projectId");
			this._projectId.Location = new System.Drawing.Point(83, 75);
			this._projectId.Name = "_projectId";
			this._projectId.Size = new System.Drawing.Size(263, 20);
			this._projectId.TabIndex = 0;
			this.toolTip1.SetToolTip(this._projectId, "Usually the Ethnologue code, e.g. \'tpi\'");
			this._projectId.TextChanged += new System.EventHandler(this._projectId_TextChanged);
			this._projectId.KeyDown += new System.Windows.Forms.KeyEventHandler(this._textbox_KeyDown);
			this._projectId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._textbox_KeyPress);
			//
			// _accountName
			//
			this._accountName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._accountName, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._accountName, null);
			this.l10NSharpExtender1.SetLocalizingId(this._accountName, "ServerSettingsControl.ServerSettingsControl._accountName");
			this._accountName.Location = new System.Drawing.Point(83, 109);
			this._accountName.Name = "_accountName";
			this._accountName.Size = new System.Drawing.Size(263, 20);
			this._accountName.TabIndex = 1;
			this.toolTip1.SetToolTip(this._accountName, "This is your account on the server, which must already be set up.");
			this._accountName.TextChanged += new System.EventHandler(this._accountName_TextChanged);
			this._accountName.KeyDown += new System.Windows.Forms.KeyEventHandler(this._textbox_KeyDown);
			this._accountName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._textbox_KeyPress);
			//
			// _password
			//
			this._password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._password, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._password, null);
			this.l10NSharpExtender1.SetLocalizingId(this._password, "ServerSettingsControl.ServerSettingsControl._password");
			this._password.Location = new System.Drawing.Point(83, 143);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(263, 20);
			this._password.TabIndex = 2;
			this.toolTip1.SetToolTip(this._password, "This is the password belonging to this account, as it was set up on the server.");
			this._password.TextChanged += new System.EventHandler(this._password_TextChanged);
			this._password.KeyDown += new System.Windows.Forms.KeyEventHandler(this._textbox_KeyDown);
			this._password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._textbox_KeyPress);
			//
			// _showCharacters
			//
			this._showCharacters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._showCharacters.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._showCharacters, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._showCharacters, null);
			this.l10NSharpExtender1.SetLocalizingId(this._showCharacters, "ServerSettingsControl.ShowCharacters");
			this._showCharacters.Location = new System.Drawing.Point(83, 173);
			this._showCharacters.Name = "_showCharacters";
			this._showCharacters.Size = new System.Drawing.Size(263, 17);
			this._showCharacters.TabIndex = 30;
			this._showCharacters.Text = "Show characters";
			this.toolTip1.SetToolTip(this._showCharacters, "Select this box to display the password.");
			this._showCharacters.UseVisualStyleBackColor = true;
			this._showCharacters.CheckedChanged += new System.EventHandler(this._showCharacters_CheckedChanged);
			//
			// _customUrlLabel
			//
			this._customUrlLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._customUrlLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._customUrlLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._customUrlLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._customUrlLabel, "ServerSettingsControl.URL");
			this._customUrlLabel.Location = new System.Drawing.Point(48, 44);
			this._customUrlLabel.Name = "_customUrlLabel";
			this._customUrlLabel.Size = new System.Drawing.Size(29, 13);
			this._customUrlLabel.TabIndex = 29;
			this._customUrlLabel.Text = "URL";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this._serverCombo, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this._passwordLabel, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this._customUrlLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._accountLabel, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._projectIdLabel, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._customUrl, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this._projectId, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this._accountName, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this._password, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this._showCharacters, 1, 5);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(349, 208);
			this.tableLayoutPanel1.TabIndex = 30;
			//
			// _serverCombo
			//
			this._serverCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._serverCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._serverCombo.FormattingEnabled = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._serverCombo, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._serverCombo, null);
			this.l10NSharpExtender1.SetLocalizingId(this._serverCombo, "ServerSettingsControl.ServerSettingsControl._serverCombo");
			this._serverCombo.Location = new System.Drawing.Point(83, 6);
			this._serverCombo.Name = "_serverCombo";
			this._serverCombo.Size = new System.Drawing.Size(263, 21);
			this._serverCombo.TabIndex = 6;
			//
			// label1
			//
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label1.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.label1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.label1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.label1, "ServerSettingsControl.Server");
			this.label1.Location = new System.Drawing.Point(39, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 27;
			this.label1.Text = "Server";
			//
			// _customUrl
			//
			this._customUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._customUrl, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._customUrl, null);
			this.l10NSharpExtender1.SetLocalizingId(this._customUrl, "ServerSettingsControl.ServerSettingsControl._customUrl");
			this._customUrl.Location = new System.Drawing.Point(83, 41);
			this._customUrl.Name = "_customUrl";
			this._customUrl.Size = new System.Drawing.Size(263, 20);
			this._customUrl.TabIndex = 28;
			this._customUrl.TextChanged += new System.EventHandler(this._customUrl_TextChanged);
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "ServerSettingsControl";
			//
			// ServerSettingsControl
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "ServerSettingsControl.ServerSettingsControl.ServerSettingsControl");
			this.MinimumSize = new System.Drawing.Size(363, 200);
			this.Name = "ServerSettingsControl";
			this.Size = new System.Drawing.Size(363, 208);
			this.Load += new System.EventHandler(this.OnLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label _projectIdLabel;
		private System.Windows.Forms.Label _accountLabel;
		private System.Windows.Forms.Label _passwordLabel;
		private System.Windows.Forms.TextBox _projectId;
		private System.Windows.Forms.TextBox _accountName;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label _customUrlLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ComboBox _serverCombo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _customUrl;
		private System.Windows.Forms.CheckBox _showCharacters;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
