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
			this.nameLabel = new System.Windows.Forms.Label();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.settingsTabs = new System.Windows.Forms.TabControl();
			this.internetTab = new System.Windows.Forms.TabPage();
			this.networkFolderTab = new System.Windows.Forms.TabPage();
			this.helpButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.settingsProtectionButton = new Palaso.UI.WindowsForms.SettingProtection.SettingsProtectionLauncherButton();
			this.settingsTabs.SuspendLayout();
			this.SuspendLayout();
			//
			// nameLabel
			//
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(13, 13);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(161, 13);
			this.nameLabel.TabIndex = 0;
			this.nameLabel.Text = "Name to show in change history:";
			//
			// userNameTextBox
			//
			this.userNameTextBox.Location = new System.Drawing.Point(16, 30);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(419, 20);
			this.userNameTextBox.TabIndex = 1;
			this.userNameTextBox.TextChanged += new System.EventHandler(this.userNameTextBox_TextChanged);
			//
			// settingsTabs
			//
			this.settingsTabs.Controls.Add(this.internetTab);
			this.settingsTabs.Controls.Add(this.networkFolderTab);
			this.settingsTabs.Location = new System.Drawing.Point(16, 56);
			this.settingsTabs.Name = "settingsTabs";
			this.settingsTabs.SelectedIndex = 0;
			this.settingsTabs.Size = new System.Drawing.Size(423, 243);
			this.settingsTabs.TabIndex = 2;
			//
			// internetTab
			//
			this.internetTab.Location = new System.Drawing.Point(4, 22);
			this.internetTab.Name = "internetTab";
			this.internetTab.Padding = new System.Windows.Forms.Padding(3);
			this.internetTab.Size = new System.Drawing.Size(415, 217);
			this.internetTab.TabIndex = 0;
			this.internetTab.Text = "Internet";
			this.internetTab.UseVisualStyleBackColor = true;
			//
			// networkFolderTab
			//
			this.networkFolderTab.Location = new System.Drawing.Point(4, 22);
			this.networkFolderTab.Name = "networkFolderTab";
			this.networkFolderTab.Padding = new System.Windows.Forms.Padding(3);
			this.networkFolderTab.Size = new System.Drawing.Size(415, 217);
			this.networkFolderTab.TabIndex = 1;
			this.networkFolderTab.Text = "Network Folder";
			this.networkFolderTab.UseVisualStyleBackColor = true;
			//
			// helpButton
			//
			this.helpButton.Location = new System.Drawing.Point(360, 316);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new System.Drawing.Size(75, 23);
			this.helpButton.TabIndex = 5;
			this.helpButton.Text = "Help";
			this.helpButton.UseVisualStyleBackColor = true;
			//
			// cancelButton
			//
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(279, 316);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			//
			// okButton
			//
			this.okButton.Location = new System.Drawing.Point(198, 316);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			//
			// settingsProtectionButton
			//
			this.settingsProtectionButton.Location = new System.Drawing.Point(17, 302);
			this.settingsProtectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.settingsProtectionButton.Name = "settingsProtectionButton";
			this.settingsProtectionButton.Size = new System.Drawing.Size(257, 37);
			this.settingsProtectionButton.TabIndex = 0;
			//
			// SendReceiveSettings
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(447, 350);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.helpButton);
			this.Controls.Add(this.settingsTabs);
			this.Controls.Add(this.userNameTextBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.settingsProtectionButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SendReceiveSettings";
			this.ShowIcon = false;
			this.Text = "Send/Receive Settings";
			this.settingsTabs.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox userNameTextBox;
		private System.Windows.Forms.TabControl settingsTabs;
		private System.Windows.Forms.TabPage internetTab;
		private System.Windows.Forms.TabPage networkFolderTab;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsProtectionLauncherButton settingsProtectionButton;
	}
}