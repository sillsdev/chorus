namespace Chorus.UI.Clone
{
	partial class InternetCloneInstructionsControl
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
			this._localFolderName = new System.Windows.Forms.TextBox();
			this._targetWarningImage = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this._projectId = new System.Windows.Forms.TextBox();
			this._accountName = new System.Windows.Forms.TextBox();
			this._password = new System.Windows.Forms.TextBox();
			this._serverCombo = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._downloadButton = new System.Windows.Forms.Button();
			this._targetInfoLabel = new Chorus.UI.BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).BeginInit();
			this.SuspendLayout();
			//
			// _localFolderName
			//
			this._localFolderName.Location = new System.Drawing.Point(110, 164);
			this._localFolderName.Name = "_localFolderName";
			this._localFolderName.Size = new System.Drawing.Size(166, 20);
			this._localFolderName.TabIndex = 3;
			this.toolTip1.SetToolTip(this._localFolderName, "What to call this project");
			this._localFolderName.TextChanged += new System.EventHandler(this.OnLocalNameChanged);
			//
			// _targetWarningImage
			//
			this._targetWarningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._targetWarningImage.Location = new System.Drawing.Point(282, 162);
			this._targetWarningImage.Name = "_targetWarningImage";
			this._targetWarningImage.Size = new System.Drawing.Size(26, 33);
			this._targetWarningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._targetWarningImage.TabIndex = 20;
			this._targetWarningImage.TabStop = false;
			this._targetWarningImage.Visible = false;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(27, 61);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(54, 13);
			this.label2.TabIndex = 26;
			this.label2.Text = "Project ID";
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(27, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(47, 13);
			this.label4.TabIndex = 26;
			this.label4.Text = "Account";
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(27, 131);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 13);
			this.label5.TabIndex = 26;
			this.label5.Text = "Password";
			//
			// _projectId
			//
			this._projectId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._projectId.Location = new System.Drawing.Point(110, 59);
			this._projectId.Name = "_projectId";
			this._projectId.Size = new System.Drawing.Size(45, 20);
			this._projectId.TabIndex = 0;
			this.toolTip1.SetToolTip(this._projectId, "Usually the Ethnologue code, e.g. \'tpi\'");
			this._projectId.TextChanged += new System.EventHandler(this.OnAccountInfoTextChanged);
			//
			// _accountName
			//
			this._accountName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._accountName.Location = new System.Drawing.Point(110, 94);
			this._accountName.Name = "_accountName";
			this._accountName.Size = new System.Drawing.Size(166, 20);
			this._accountName.TabIndex = 1;
			this.toolTip1.SetToolTip(this._accountName, "This is your account on the server, which must already be set up.");
			this._accountName.TextChanged += new System.EventHandler(this.OnAccountInfoTextChanged);
			//
			// _password
			//
			this._password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._password.Location = new System.Drawing.Point(110, 129);
			this._password.Name = "_password";
			this._password.Size = new System.Drawing.Size(166, 20);
			this._password.TabIndex = 2;
			this.toolTip1.SetToolTip(this._password, "This is the password belonging to this account, as it was set up on the server.");
			this._password.TextChanged += new System.EventHandler(this.OnAccountInfoTextChanged);
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
			// label6
			//
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(28, 166);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(71, 13);
			this.label6.TabIndex = 26;
			this.label6.Text = "Project Name";
			//
			// _downloadButton
			//
			this._downloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._downloadButton.Location = new System.Drawing.Point(5, 274);
			this._downloadButton.Name = "_downloadButton";
			this._downloadButton.Size = new System.Drawing.Size(94, 23);
			this._downloadButton.TabIndex = 27;
			this._downloadButton.Text = "&Download";
			this._downloadButton.UseVisualStyleBackColor = true;
			//
			// _targetInfoLabel
			//
			this._targetInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._targetInfoLabel.BackColor = System.Drawing.SystemColors.Control;
			this._targetInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._targetInfoLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._targetInfoLabel.ForeColor = System.Drawing.Color.DimGray;
			this._targetInfoLabel.Location = new System.Drawing.Point(30, 190);
			this._targetInfoLabel.Multiline = true;
			this._targetInfoLabel.Name = "_targetInfoLabel";
			this._targetInfoLabel.ReadOnly = true;
			this._targetInfoLabel.Size = new System.Drawing.Size(246, 78);
			this._targetInfoLabel.TabIndex = 25;
			this._targetInfoLabel.TabStop = false;
			this._targetInfoLabel.Text = "runtime info";
			//
			// InternetCloneInstructionsControl
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this._downloadButton);
			this.Controls.Add(this._serverCombo);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._targetInfoLabel);
			this.Controls.Add(this._targetWarningImage);
			this.Controls.Add(this._localFolderName);
			this.Controls.Add(this._password);
			this.Controls.Add(this._accountName);
			this.Controls.Add(this._projectId);
			this.MinimumSize = new System.Drawing.Size(430, 300);
			this.Name = "InternetCloneInstructionsControl";
			this.Size = new System.Drawing.Size(430, 300);
			this.Load += new System.EventHandler(this.AccountInfo_Load);
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _localFolderName;
		private System.Windows.Forms.PictureBox _targetWarningImage;
		private BetterLabel _targetInfoLabel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox _projectId;
		private System.Windows.Forms.TextBox _accountName;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.ComboBox _serverCombo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ToolTip toolTip1;
		public System.Windows.Forms.Button _downloadButton;
	}
}
