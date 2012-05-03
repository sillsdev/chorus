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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.settingsTabs = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
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
			// textBox1
			//
			this.textBox1.Location = new System.Drawing.Point(16, 30);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(419, 20);
			this.textBox1.TabIndex = 1;
			//
			// settingsTabs
			//
			this.settingsTabs.Controls.Add(this.tabPage1);
			this.settingsTabs.Controls.Add(this.tabPage2);
			this.settingsTabs.Location = new System.Drawing.Point(16, 56);
			this.settingsTabs.Name = "settingsTabs";
			this.settingsTabs.SelectedIndex = 0;
			this.settingsTabs.Size = new System.Drawing.Size(423, 200);
			this.settingsTabs.TabIndex = 2;
			//
			// tabPage1
			//
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(379, 174);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			//
			// tabPage2
			//
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(415, 174);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "tabPage2";
			this.tabPage2.UseVisualStyleBackColor = true;
			//
			// helpButton
			//
			this.helpButton.Location = new System.Drawing.Point(364, 262);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new System.Drawing.Size(75, 23);
			this.helpButton.TabIndex = 5;
			this.helpButton.Text = "Help";
			this.helpButton.UseVisualStyleBackColor = true;
			//
			// cancelButton
			//
			this.cancelButton.Location = new System.Drawing.Point(283, 262);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// okButton
			//
			this.okButton.Location = new System.Drawing.Point(202, 262);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			//
			// settingsProtectionButton
			//
			this.settingsProtectionButton.Location = new System.Drawing.Point(17, 264);
			this.settingsProtectionButton.Margin = new System.Windows.Forms.Padding(0);
			this.settingsProtectionButton.Name = "settingsProtectionButton";
			this.settingsProtectionButton.Size = new System.Drawing.Size(257, 37);
			this.settingsProtectionButton.TabIndex = 0;
			//
			// SendReceiveSettings
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(447, 324);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.helpButton);
			this.Controls.Add(this.settingsTabs);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.settingsProtectionButton);
			this.Name = "SendReceiveSettings";
			this.Text = "SendReceiveSettings";
			this.settingsTabs.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TabControl settingsTabs;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsProtectionLauncherButton settingsProtectionButton;
	}
}