using Palaso.UI.WindowsForms.SettingProtection;

namespace Chorus.UI.Settings
{
	partial class SendReceiveSettings
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendReceiveSettings));
			this.nameLabel = new System.Windows.Forms.Label();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.settingsTabs = new System.Windows.Forms.TabControl();
			this.internetTab = new System.Windows.Forms.TabPage();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._internetSettingsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this._internetButtonEnabledCheckBox = new System.Windows.Forms.CheckBox();
			this._serverSettingsControl = new Chorus.UI.Misc.ServerSettingsControl();
			this.chorusHubTab = new System.Windows.Forms.TabPage();
			this.betterLabel1 = new Chorus.UI.BetterLabel();
			this.pictureBox4 = new System.Windows.Forms.PictureBox();
			this.networkFolderTab = new System.Windows.Forms.TabPage();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this._showSharedFolderInSendReceive = new System.Windows.Forms.CheckBox();
			this._sharedFolderSettingsControl = new Chorus.UI.Misc.NetworkFolderSettingsControl();
			this._helpButton = new System.Windows.Forms.Button();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this.settingsProtectionButton = new Palaso.UI.WindowsForms.SettingProtection.SettingsProtectionLauncherButton();
			this.pictureBox3 = new System.Windows.Forms.PictureBox();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this._showChorusHubInSendReceive = new System.Windows.Forms.CheckBox();
			this.settingsTabs.SuspendLayout();
			this.internetTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this._internetSettingsFlow.SuspendLayout();
			this.chorusHubTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
			this.networkFolderTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
			this.SuspendLayout();
			//
			// nameLabel
			//
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(84, 9);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(161, 13);
			this.nameLabel.TabIndex = 0;
			this.nameLabel.Text = "Name to show in change history:";
			//
			// userNameTextBox
			//
			this.userNameTextBox.Location = new System.Drawing.Point(86, 25);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(158, 20);
			this.userNameTextBox.TabIndex = 1;
			this.userNameTextBox.TextChanged += new System.EventHandler(this.userNameTextBox_TextChanged);
			//
			// settingsTabs
			//
			this.settingsTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.settingsTabs.Controls.Add(this.internetTab);
			this.settingsTabs.Controls.Add(this.chorusHubTab);
			this.settingsTabs.Controls.Add(this.networkFolderTab);
			this.settingsTabs.Location = new System.Drawing.Point(13, 77);
			this.settingsTabs.Name = "settingsTabs";
			this.settingsTabs.SelectedIndex = 0;
			this.settingsTabs.Size = new System.Drawing.Size(484, 294);
			this.settingsTabs.TabIndex = 2;
			//
			// internetTab
			//
			this.internetTab.Controls.Add(this.pictureBox1);
			this.internetTab.Controls.Add(this._internetSettingsFlow);
			this.internetTab.Location = new System.Drawing.Point(4, 22);
			this.internetTab.Name = "internetTab";
			this.internetTab.Padding = new System.Windows.Forms.Padding(3);
			this.internetTab.Size = new System.Drawing.Size(476, 268);
			this.internetTab.TabIndex = 0;
			this.internetTab.Text = "Internet";
			this.internetTab.UseVisualStyleBackColor = true;
			//
			// pictureBox1
			//
			this.pictureBox1.Image = global::Chorus.Properties.Resources.internet59x64;
			this.pictureBox1.Location = new System.Drawing.Point(6, 14);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(64, 66);
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			//
			// _internetSettingsFlow
			//
			this._internetSettingsFlow.Controls.Add(this._internetButtonEnabledCheckBox);
			this._internetSettingsFlow.Controls.Add(this._serverSettingsControl);
			this._internetSettingsFlow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this._internetSettingsFlow.Location = new System.Drawing.Point(73, 11);
			this._internetSettingsFlow.Name = "_internetSettingsFlow";
			this._internetSettingsFlow.Size = new System.Drawing.Size(383, 238);
			this._internetSettingsFlow.TabIndex = 0;
			this._internetSettingsFlow.WrapContents = false;
			//
			// _internetButtonEnabledCheckBox
			//
			this._internetButtonEnabledCheckBox.AutoSize = true;
			this._internetButtonEnabledCheckBox.Location = new System.Drawing.Point(88, 3);
			this._internetButtonEnabledCheckBox.Margin = new System.Windows.Forms.Padding(88, 3, 3, 3);
			this._internetButtonEnabledCheckBox.Name = "_internetButtonEnabledCheckBox";
			this._internetButtonEnabledCheckBox.Size = new System.Drawing.Size(211, 17);
			this._internetButtonEnabledCheckBox.TabIndex = 0;
			this._internetButtonEnabledCheckBox.Text = "Show Internet as Send/Receive option";
			this._internetButtonEnabledCheckBox.UseVisualStyleBackColor = true;
			//
			// _serverSettingsControl
			//
			this._serverSettingsControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._serverSettingsControl.Location = new System.Drawing.Point(3, 26);
			this._serverSettingsControl.MinimumSize = new System.Drawing.Size(363, 200);
			this._serverSettingsControl.Model = null;
			this._serverSettingsControl.Name = "_serverSettingsControl";
			this._serverSettingsControl.Size = new System.Drawing.Size(363, 200);
			this._serverSettingsControl.TabIndex = 1;
			//
			// chorusHubTab
			//
			this.chorusHubTab.Controls.Add(this._showChorusHubInSendReceive);
			this.chorusHubTab.Controls.Add(this.betterLabel1);
			this.chorusHubTab.Controls.Add(this.pictureBox4);
			this.chorusHubTab.Location = new System.Drawing.Point(4, 22);
			this.chorusHubTab.Name = "chorusHubTab";
			this.chorusHubTab.Padding = new System.Windows.Forms.Padding(3);
			this.chorusHubTab.Size = new System.Drawing.Size(476, 268);
			this.chorusHubTab.TabIndex = 2;
			this.chorusHubTab.Text = "Chorus Hub";
			this.chorusHubTab.UseVisualStyleBackColor = true;
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BackColor = System.Drawing.Color.White;
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(85, 44);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.betterLabel1.Size = new System.Drawing.Size(365, 201);
			this.betterLabel1.TabIndex = 4;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = resources.GetString("betterLabel1.Text");
			//
			// pictureBox4
			//
			this.pictureBox4.Image = global::Chorus.Properties.Resources.chorusHubLarge;
			this.pictureBox4.Location = new System.Drawing.Point(6, 17);
			this.pictureBox4.Name = "pictureBox4";
			this.pictureBox4.Size = new System.Drawing.Size(64, 66);
			this.pictureBox4.TabIndex = 3;
			this.pictureBox4.TabStop = false;
			//
			// networkFolderTab
			//
			this.networkFolderTab.Controls.Add(this.pictureBox2);
			this.networkFolderTab.Controls.Add(this.flowLayoutPanel1);
			this.networkFolderTab.Location = new System.Drawing.Point(4, 22);
			this.networkFolderTab.Name = "networkFolderTab";
			this.networkFolderTab.Padding = new System.Windows.Forms.Padding(3);
			this.networkFolderTab.Size = new System.Drawing.Size(476, 268);
			this.networkFolderTab.TabIndex = 1;
			this.networkFolderTab.Text = "Network Folder";
			this.networkFolderTab.UseVisualStyleBackColor = true;
			//
			// pictureBox2
			//
			this.pictureBox2.Image = global::Chorus.Properties.Resources.networkFolder58x64;
			this.pictureBox2.Location = new System.Drawing.Point(6, 18);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(64, 66);
			this.pictureBox2.TabIndex = 2;
			this.pictureBox2.TabStop = false;
			//
			// flowLayoutPanel1
			//
			this.flowLayoutPanel1.Controls.Add(this._showSharedFolderInSendReceive);
			this.flowLayoutPanel1.Controls.Add(this._sharedFolderSettingsControl);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(89, 15);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(377, 238);
			this.flowLayoutPanel1.TabIndex = 1;
			this.flowLayoutPanel1.WrapContents = false;
			//
			// _showSharedFolderTargetOption
			//
			this._showSharedFolderInSendReceive.AutoSize = true;
			this._showSharedFolderInSendReceive.Location = new System.Drawing.Point(10, 3);
			this._showSharedFolderInSendReceive.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this._showSharedFolderInSendReceive.Name = "_showSharedFolderInSendReceive";
			this._showSharedFolderInSendReceive.Size = new System.Drawing.Size(256, 17);
			this._showSharedFolderInSendReceive.TabIndex = 0;
			this._showSharedFolderInSendReceive.Text = "Show Network Folder as a Send/Receive option";
			this._showSharedFolderInSendReceive.UseVisualStyleBackColor = true;
			//
			// _sharedFolderSettingsControl
			//
			this._sharedFolderSettingsControl.Location = new System.Drawing.Point(3, 26);
			this._sharedFolderSettingsControl.Model = null;
			this._sharedFolderSettingsControl.Name = "_sharedFolderSettingsControl";
			this._sharedFolderSettingsControl.Size = new System.Drawing.Size(326, 155);
			this._sharedFolderSettingsControl.TabIndex = 1;
			//
			// _helpButton
			//
			this._helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._helpButton.Location = new System.Drawing.Point(422, 399);
			this._helpButton.Name = "_helpButton";
			this._helpButton.Size = new System.Drawing.Size(75, 23);
			this._helpButton.TabIndex = 5;
			this._helpButton.Text = "Help";
			this._helpButton.UseVisualStyleBackColor = true;
			this._helpButton.Click += new System.EventHandler(this._helpButton_Click);
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(341, 399);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 4;
			this._cancelButton.Text = "Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(260, 399);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 3;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this.okButton_Click);
			//
			// settingsProtectionButton
			//
			this.settingsProtectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.settingsProtectionButton.Location = new System.Drawing.Point(14, 388);
			this.settingsProtectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.settingsProtectionButton.Name = "settingsProtectionButton";
			this.settingsProtectionButton.Size = new System.Drawing.Size(258, 37);
			this.settingsProtectionButton.TabIndex = 0;
			//
			// pictureBox3
			//
			this.pictureBox3.Image = global::Chorus.Properties.Resources.Committer_Person;
			this.pictureBox3.Location = new System.Drawing.Point(17, 9);
			this.pictureBox3.Name = "pictureBox3";
			this.pictureBox3.Size = new System.Drawing.Size(52, 51);
			this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox3.TabIndex = 6;
			this.pictureBox3.TabStop = false;
			//
			// _showChorusHubAsTargetOption
			//
			this._showChorusHubInSendReceive.AutoSize = true;
			this._showChorusHubInSendReceive.Location = new System.Drawing.Point(85, 17);
			this._showChorusHubInSendReceive.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this._showChorusHubInSendReceive.Name = "_showChorusHubInSendReceive";
			this._showChorusHubInSendReceive.Size = new System.Drawing.Size(240, 17);
			this._showChorusHubInSendReceive.TabIndex = 5;
			this._showChorusHubInSendReceive.Text = "Show Chorus Hub as a Send/Receive option";
			this._showChorusHubInSendReceive.UseVisualStyleBackColor = true;
			//
			// SendReceiveSettings
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(508, 434);
			this.Controls.Add(this.pictureBox3);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._helpButton);
			this.Controls.Add(this.settingsTabs);
			this.Controls.Add(this.userNameTextBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.settingsProtectionButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SendReceiveSettings";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Send/Receive Settings";
			this.settingsTabs.ResumeLayout(false);
			this.internetTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this._internetSettingsFlow.ResumeLayout(false);
			this._internetSettingsFlow.PerformLayout();
			this.chorusHubTab.ResumeLayout(false);
			this.chorusHubTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
			this.networkFolderTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox userNameTextBox;
		private System.Windows.Forms.TabControl settingsTabs;
		private System.Windows.Forms.TabPage internetTab;
		private System.Windows.Forms.TabPage networkFolderTab;
		private System.Windows.Forms.Button _helpButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsProtectionLauncherButton settingsProtectionButton;
		private System.Windows.Forms.FlowLayoutPanel _internetSettingsFlow;
		private System.Windows.Forms.CheckBox _internetButtonEnabledCheckBox;
		private Misc.ServerSettingsControl _serverSettingsControl;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.CheckBox _showSharedFolderInSendReceive;
		private Misc.NetworkFolderSettingsControl _sharedFolderSettingsControl;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.PictureBox pictureBox3;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
		private System.Windows.Forms.TabPage chorusHubTab;
		private BetterLabel betterLabel1;
		private System.Windows.Forms.PictureBox pictureBox4;
		private System.Windows.Forms.CheckBox _showChorusHubInSendReceive;
	}
}