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
			this._serverCombo = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._customUrl = new System.Windows.Forms.TextBox();
			this._customUrlLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _projectIdLabel
			//
			this._projectIdLabel.AutoSize = true;
			this._projectIdLabel.Location = new System.Drawing.Point(27, 86);
			this._projectIdLabel.Name = "_projectIdLabel";
			this._projectIdLabel.Size = new System.Drawing.Size(54, 13);
			this._projectIdLabel.TabIndex = 26;
			this._projectIdLabel.Text = "Project ID";
			//
			// _accountLabel
			//
			this._accountLabel.AutoSize = true;
			this._accountLabel.Location = new System.Drawing.Point(27, 121);
			this._accountLabel.Name = "_accountLabel";
			this._accountLabel.Size = new System.Drawing.Size(47, 13);
			this._accountLabel.TabIndex = 26;
			this._accountLabel.Text = "Account";
			//
			// _passwordLabel
			//
			this._passwordLabel.AutoSize = true;
			this._passwordLabel.Location = new System.Drawing.Point(27, 156);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(53, 13);
			this._passwordLabel.TabIndex = 26;
			this._passwordLabel.Text = "Password";
			//
			// _projectId
			//
			this._projectId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._projectId.Location = new System.Drawing.Point(110, 84);
			this._projectId.Name = "_projectId";
			this._projectId.Size = new System.Drawing.Size(45, 20);
			this._projectId.TabIndex = 0;
			this.toolTip1.SetToolTip(this._projectId, "Usually the Ethnologue code, e.g. \'tpi\'");
			this._projectId.TextChanged += new System.EventHandler(this._projectId_TextChanged);
			//
			// _accountName
			//
			this._accountName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._accountName.Location = new System.Drawing.Point(110, 119);
			this._accountName.Name = "_accountName";
			this._accountName.Size = new System.Drawing.Size(166, 20);
			this._accountName.TabIndex = 1;
			this.toolTip1.SetToolTip(this._accountName, "This is your account on the server, which must already be set up.");
			this._accountName.TextChanged += new System.EventHandler(this._accountName_TextChanged);
			//
			// _password
			//
			this._password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._password.Location = new System.Drawing.Point(110, 154);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(166, 20);
			this._password.TabIndex = 2;
			this.toolTip1.SetToolTip(this._password, "This is the password belonging to this account, as it was set up on the server.");
			this._password.TextChanged += new System.EventHandler(this._password_TextChanged);
			//
			// _serverCombo
			//
			this._serverCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._serverCombo.FormattingEnabled = true;
			this._serverCombo.Location = new System.Drawing.Point(110, 23);
			this._serverCombo.Name = "_serverCombo";
			this._serverCombo.Size = new System.Drawing.Size(168, 21);
			this._serverCombo.TabIndex = 5;
			this._serverCombo.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(27, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 26;
			this.label1.Text = "Server";
			//
			// _customUrl
			//
			this._customUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._customUrl.Location = new System.Drawing.Point(111, 54);
			this._customUrl.Name = "_customUrl";
			this._customUrl.Size = new System.Drawing.Size(296, 20);
			this._customUrl.TabIndex = 6;
			this._customUrl.TextChanged += new System.EventHandler(this._customUrl_TextChanged);
			//
			// _customUrlLabel
			//
			this._customUrlLabel.AutoSize = true;
			this._customUrlLabel.Location = new System.Drawing.Point(28, 57);
			this._customUrlLabel.Name = "_customUrlLabel";
			this._customUrlLabel.Size = new System.Drawing.Size(29, 13);
			this._customUrlLabel.TabIndex = 29;
			this._customUrlLabel.Text = "URL";
			//
			// InternetCloneInstructionsControl
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this._customUrlLabel);
			this.Controls.Add(this._customUrl);
			this.Controls.Add(this._serverCombo);
			this.Controls.Add(this._passwordLabel);
			this.Controls.Add(this._accountLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._projectIdLabel);
			this.Controls.Add(this._password);
			this.Controls.Add(this._accountName);
			this.Controls.Add(this._projectId);
			this.MinimumSize = new System.Drawing.Size(430, 200);
			this.Name = "InternetCloneInstructionsControl";
			this.Size = new System.Drawing.Size(430, 200);
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _projectIdLabel;
		private System.Windows.Forms.Label _accountLabel;
		private System.Windows.Forms.Label _passwordLabel;
		private System.Windows.Forms.TextBox _projectId;
		private System.Windows.Forms.TextBox _accountName;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.ComboBox _serverCombo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TextBox _customUrl;
		private System.Windows.Forms.Label _customUrlLabel;
	}
}
