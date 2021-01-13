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
			this._username = new System.Windows.Forms.TextBox();
			this._password = new SIL.Windows.Forms.Widgets.PasswordBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._customUrlLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._bandwidthLabel = new System.Windows.Forms.Label();
			this._tlpCustomUrl = new System.Windows.Forms.TableLayoutPanel();
			this._checkCustomUrl = new System.Windows.Forms.CheckBox();
			this._customUrl = new System.Windows.Forms.TextBox();
			this._buttonLogIn = new System.Windows.Forms.Button();
			this._bandwidth = new System.Windows.Forms.ComboBox();
			this._projectId = new System.Windows.Forms.ComboBox();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this._tlpCustomUrl.SuspendLayout();
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
			this._projectIdLabel.Location = new System.Drawing.Point(23, 182);
			this._projectIdLabel.Name = "_projectIdLabel";
			this._projectIdLabel.Size = new System.Drawing.Size(54, 13);
			this._projectIdLabel.TabIndex = 29;
			this._projectIdLabel.Text = "Project &ID";
			// 
			// _accountLabel
			// 
			this._accountLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._accountLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._accountLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._accountLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._accountLabel, "ServerSettingsControl.Login");
			this._accountLabel.Location = new System.Drawing.Point(44, 10);
			this._accountLabel.Name = "_accountLabel";
			this._accountLabel.Size = new System.Drawing.Size(33, 13);
			this._accountLabel.TabIndex = 2;
			this._accountLabel.Text = "Login";
			// 
			// _passwordLabel
			// 
			this._passwordLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._passwordLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._passwordLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._passwordLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._passwordLabel, "ServerSettingsControl.Password");
			this._passwordLabel.Location = new System.Drawing.Point(24, 44);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(53, 13);
			this._passwordLabel.TabIndex = 8;
			this._passwordLabel.Text = "Password";
			// 
			// _username
			// 
			this._username.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._username, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._username, null);
			this.l10NSharpExtender1.SetLocalizingId(this._username, "ServerSettingsControl.ServerSettingsControl._username");
			this._username.Location = new System.Drawing.Point(83, 7);
			this._username.Name = "_username";
			this._username.Size = new System.Drawing.Size(263, 20);
			this._username.TabIndex = 2;
			this.toolTip1.SetToolTip(this._username, "This is your account on the server, which must already be set up.");
			this._username.TextChanged += new System.EventHandler(this._username_TextChanged);
			this._username.KeyDown += new System.Windows.Forms.KeyEventHandler(this._textBox_KeyDown);
			this._username.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._textBox_KeyPress);
			// 
			// _password
			// 
			this._password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._password, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._password, null);
			this.l10NSharpExtender1.SetLocalizingId(this._password, "ServerSettingsControl.ServerSettingsControl._password");
			this._password.Location = new System.Drawing.Point(83, 41);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(263, 20);
			this._password.TabIndex = 8;
			this.toolTip1.SetToolTip(this._password, "This is the password belonging to this account, as it was set up on the server.");
			this._password.UseSystemPasswordChar = true;
			this._password.TextChanged += new System.EventHandler(this._password_TextChanged);
			this._password.KeyDown += new System.Windows.Forms.KeyEventHandler(this._textBox_KeyDown);
			this._password.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._textBox_KeyPress);
			// 
			// _customUrlLabel
			// 
			this._customUrlLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._customUrlLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._customUrlLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._customUrlLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._customUrlLabel, "ServerSettingsControl.URL");
			this._customUrlLabel.Location = new System.Drawing.Point(10, 78);
			this._customUrlLabel.Name = "_customUrlLabel";
			this._customUrlLabel.Size = new System.Drawing.Size(67, 13);
			this._customUrlLabel.TabIndex = 17;
			this._customUrlLabel.Text = "Custom &URL";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this._projectIdLabel, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._customUrlLabel, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._passwordLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._bandwidthLabel, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this._accountLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._username, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this._tlpCustomUrl, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this._password, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this._buttonLogIn, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this._bandwidth, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this._projectId, 1, 5);
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
			// _bandwidthLabel
			// 
			this._bandwidthLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._bandwidthLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._bandwidthLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._bandwidthLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._bandwidthLabel, "ServerSettingsControl.Server");
			this._bandwidthLabel.Location = new System.Drawing.Point(20, 146);
			this._bandwidthLabel.Name = "_bandwidthLabel";
			this._bandwidthLabel.Size = new System.Drawing.Size(57, 13);
			this._bandwidthLabel.TabIndex = 25;
			this._bandwidthLabel.Text = "&Bandwidth";
			// 
			// _tlpCustomUrl
			// 
			this._tlpCustomUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tlpCustomUrl.ColumnCount = 2;
			this._tlpCustomUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this._tlpCustomUrl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 244F));
			this._tlpCustomUrl.Controls.Add(this._checkCustomUrl, 0, 0);
			this._tlpCustomUrl.Controls.Add(this._customUrl, 1, 0);
			this._tlpCustomUrl.Location = new System.Drawing.Point(83, 71);
			this._tlpCustomUrl.Name = "_tlpCustomUrl";
			this._tlpCustomUrl.RowCount = 1;
			this._tlpCustomUrl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this._tlpCustomUrl.Size = new System.Drawing.Size(263, 28);
			this._tlpCustomUrl.TabIndex = 17;
			// 
			// _checkCustomUrl
			// 
			this._checkCustomUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._checkCustomUrl.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._checkCustomUrl, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._checkCustomUrl, null);
			this.l10NSharpExtender1.SetLocalizingId(this._checkCustomUrl, "ServerSettingsControl.checkBox1");
			this._checkCustomUrl.Location = new System.Drawing.Point(3, 7);
			this._checkCustomUrl.Name = "_checkCustomUrl";
			this._checkCustomUrl.Size = new System.Drawing.Size(13, 14);
			this._checkCustomUrl.TabIndex = 17;
			this._checkCustomUrl.UseVisualStyleBackColor = true;
			this._checkCustomUrl.CheckedChanged += new System.EventHandler(this._checkCustomUrl_CheckedChanged);
			// 
			// _customUrl
			// 
			this._customUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._customUrl, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._customUrl, null);
			this.l10NSharpExtender1.SetLocalizingId(this._customUrl, "ServerSettingsControl.ServerSettingsControl._customUrl");
			this._customUrl.Location = new System.Drawing.Point(22, 4);
			this._customUrl.Name = "_customUrl";
			this._customUrl.Size = new System.Drawing.Size(238, 20);
			this._customUrl.TabIndex = 18;
			this._customUrl.TextChanged += new System.EventHandler(this._customUrl_TextChanged);
			// 
			// _buttonLogIn
			// 
			this.l10NSharpExtender1.SetLocalizableToolTip(this._buttonLogIn, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._buttonLogIn, null);
			this.l10NSharpExtender1.SetLocalizingId(this._buttonLogIn, "ServerSettingsControl.button1");
			this._buttonLogIn.Location = new System.Drawing.Point(83, 105);
			this._buttonLogIn.Name = "_buttonLogIn";
			this._buttonLogIn.Size = new System.Drawing.Size(75, 23);
			this._buttonLogIn.TabIndex = 22;
			this._buttonLogIn.Text = "&Log in";
			this._buttonLogIn.UseVisualStyleBackColor = true;
			this._buttonLogIn.Click += new System.EventHandler(this._buttonLogIn_Click);
			// 
			// _bandwidth
			// 
			this._bandwidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._bandwidth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._bandwidth.FormattingEnabled = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._bandwidth, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._bandwidth, null);
			this.l10NSharpExtender1.SetLocalizingId(this._bandwidth, "ServerSettingsControl.comboBox1");
			this._bandwidth.Location = new System.Drawing.Point(83, 142);
			this._bandwidth.Name = "_bandwidth";
			this._bandwidth.Size = new System.Drawing.Size(263, 21);
			this._bandwidth.TabIndex = 25;
			this._bandwidth.SelectedIndexChanged += new System.EventHandler(this._bandwidth_SelectedIndexChanged);
			// 
			// _projectId
			// 
			this._projectId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._projectId.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this._projectId.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this._projectId.FormattingEnabled = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._projectId, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._projectId, null);
			this.l10NSharpExtender1.SetLocalizingId(this._projectId, "ServerSettingsControl.comboBox2");
			this._projectId.Location = new System.Drawing.Point(83, 178);
			this._projectId.Name = "_projectId";
			this._projectId.Size = new System.Drawing.Size(263, 21);
			this._projectId.TabIndex = 29;
			this._projectId.TextChanged += new System.EventHandler(this._projectId_TextChanged);
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
			this._tlpCustomUrl.ResumeLayout(false);
			this._tlpCustomUrl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label _projectIdLabel;
		private System.Windows.Forms.Label _accountLabel;
		private System.Windows.Forms.Label _passwordLabel;
		private System.Windows.Forms.TextBox _username;
		private SIL.Windows.Forms.Widgets.PasswordBox _password;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label _customUrlLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label _bandwidthLabel;
		private System.Windows.Forms.TextBox _customUrl;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.TableLayoutPanel _tlpCustomUrl;
		private System.Windows.Forms.CheckBox _checkCustomUrl;
		private System.Windows.Forms.Button _buttonLogIn;
		private System.Windows.Forms.ComboBox _bandwidth;
		private System.Windows.Forms.ComboBox _projectId;
	}
}
