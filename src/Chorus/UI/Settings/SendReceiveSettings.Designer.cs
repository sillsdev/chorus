using System.Windows.Forms;
using SIL.Windows.Forms.HtmlBrowser;
using SIL.Windows.Forms.SettingProtection;

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
				this.nameLabel = new System.Windows.Forms.Label();
				this.userNameTextBox = new System.Windows.Forms.TextBox();
				this.settingsTabs = new System.Windows.Forms.TabControl();
				this.internetTab = new System.Windows.Forms.TabPage();
				this.pictureBox1 = new System.Windows.Forms.PictureBox();
				this._internetSettingsLayout = new System.Windows.Forms.TableLayoutPanel();
				this._internetButtonEnabledCheckBox = new System.Windows.Forms.CheckBox();
				this._serverSettingsControl = new Chorus.UI.Misc.ServerSettingsControl();
				this.chorusHubTab = new System.Windows.Forms.TabPage();
				this._showChorusHubInSendReceive = new System.Windows.Forms.CheckBox();
				this.chorusHubSetup = new SIL.Windows.Forms.HtmlBrowser.XWebBrowser();
				this.pictureBox4 = new System.Windows.Forms.PictureBox();
				this._helpButton = new System.Windows.Forms.Button();
				this._cancelButton = new System.Windows.Forms.Button();
				this._okButton = new System.Windows.Forms.Button();
				this.settingsProtectionButton = new SIL.Windows.Forms.SettingProtection.SettingsProtectionLauncherButton();
				this.pictureBox3 = new System.Windows.Forms.PictureBox();
				this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
				this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
				this.settingsTabs.SuspendLayout();
				this.internetTab.SuspendLayout();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
				this._internetSettingsLayout.SuspendLayout();
				this.chorusHubTab.SuspendLayout();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
				this.SuspendLayout();
				// 
				// nameLabel
				// 
				this.nameLabel.AutoSize = true;
				this.l10NSharpExtender1.SetLocalizableToolTip(this.nameLabel, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.nameLabel, null);
				this.l10NSharpExtender1.SetLocalizingId(this.nameLabel, "SendReceiveSettings.NameToShow");
				this.nameLabel.Location = new System.Drawing.Point(84, 9);
				this.nameLabel.Name = "nameLabel";
				this.nameLabel.Size = new System.Drawing.Size(161, 13);
				this.nameLabel.TabIndex = 0;
				this.nameLabel.Text = "Name to show in change history:";
				// 
				// userNameTextBox
				// 
				this.l10NSharpExtender1.SetLocalizableToolTip(this.userNameTextBox, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.userNameTextBox, null);
				this.l10NSharpExtender1.SetLocalizingId(this.userNameTextBox, "SendReceiveSettings.SendReceiveSettings.userNameTextBox");
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
				this.settingsTabs.Location = new System.Drawing.Point(13, 77);
				this.settingsTabs.Name = "settingsTabs";
				this.settingsTabs.SelectedIndex = 0;
				this.settingsTabs.Size = new System.Drawing.Size(484, 294);
				this.settingsTabs.TabIndex = 2;
				// 
				// internetTab
				// 
				this.internetTab.Controls.Add(this.pictureBox1);
				this.internetTab.Controls.Add(this._internetSettingsLayout);
				this.l10NSharpExtender1.SetLocalizableToolTip(this.internetTab, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.internetTab, null);
				this.l10NSharpExtender1.SetLocalizingId(this.internetTab, "SendReceiveSettings.Internet");
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
				this.l10NSharpExtender1.SetLocalizableToolTip(this.pictureBox1, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.pictureBox1, null);
				this.l10NSharpExtender1.SetLocalizingId(this.pictureBox1, "SendReceiveSettings.SendReceiveSettings.pictureBox1");
				this.pictureBox1.Location = new System.Drawing.Point(6, 14);
				this.pictureBox1.Name = "pictureBox1";
				this.pictureBox1.Size = new System.Drawing.Size(64, 66);
				this.pictureBox1.TabIndex = 1;
				this.pictureBox1.TabStop = false;
				// 
				// _internetSettingsLayout
				// 
				this._internetSettingsLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
				this._internetSettingsLayout.AutoSize = true;
				this._internetSettingsLayout.ColumnCount = 1;
				this._internetSettingsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
				this._internetSettingsLayout.Controls.Add(this._internetButtonEnabledCheckBox, 0, 0);
				this._internetSettingsLayout.Controls.Add(this._serverSettingsControl, 0, 1);
				this._internetSettingsLayout.Location = new System.Drawing.Point(73, 11);
				this._internetSettingsLayout.Name = "_internetSettingsLayout";
				this._internetSettingsLayout.RowCount = 2;
				this._internetSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
				this._internetSettingsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
				this._internetSettingsLayout.Size = new System.Drawing.Size(369, 229);
				this._internetSettingsLayout.TabIndex = 0;
				// 
				// _internetButtonEnabledCheckBox
				// 
				this._internetButtonEnabledCheckBox.AutoSize = true;
				this.l10NSharpExtender1.SetLocalizableToolTip(this._internetButtonEnabledCheckBox, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._internetButtonEnabledCheckBox, null);
				this.l10NSharpExtender1.SetLocalizingId(this._internetButtonEnabledCheckBox, "SendReceiveSettings.ShowInternetOption");
				this._internetButtonEnabledCheckBox.Location = new System.Drawing.Point(88, 3);
				this._internetButtonEnabledCheckBox.Margin = new System.Windows.Forms.Padding(88, 3, 3, 3);
				this._internetButtonEnabledCheckBox.Name = "_internetButtonEnabledCheckBox";
				this._internetButtonEnabledCheckBox.Size = new System.Drawing.Size(211, 16);
				this._internetButtonEnabledCheckBox.TabIndex = 0;
				this._internetButtonEnabledCheckBox.Text = "&Show Internet as Send/Receive option";
				this._internetButtonEnabledCheckBox.UseVisualStyleBackColor = true;
				// 
				// _serverSettingsControl
				// 
				this._serverSettingsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
				this._serverSettingsControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
				this.l10NSharpExtender1.SetLocalizableToolTip(this._serverSettingsControl, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._serverSettingsControl, null);
				this.l10NSharpExtender1.SetLocalizingId(this._serverSettingsControl, "SendReceiveSettings.SendReceiveSettings.ServerSettingsControl");
				this._serverSettingsControl.Location = new System.Drawing.Point(3, 25);
				this._serverSettingsControl.MinimumSize = new System.Drawing.Size(363, 200);
				this._serverSettingsControl.Model = null;
				this._serverSettingsControl.Name = "_serverSettingsControl";
				this._serverSettingsControl.Size = new System.Drawing.Size(363, 201);
				this._serverSettingsControl.TabIndex = 1;
				// 
				// chorusHubTab
				// 
				this.chorusHubTab.Controls.Add(this._showChorusHubInSendReceive);
				this.chorusHubTab.Controls.Add(this.chorusHubSetup);
				this.chorusHubTab.Controls.Add(this.pictureBox4);
				this.l10NSharpExtender1.SetLocalizableToolTip(this.chorusHubTab, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.chorusHubTab, null);
				this.l10NSharpExtender1.SetLocalizingId(this.chorusHubTab, "SendReceiveSettings.ChorusHub");
				this.chorusHubTab.Location = new System.Drawing.Point(4, 22);
				this.chorusHubTab.Name = "chorusHubTab";
				this.chorusHubTab.Padding = new System.Windows.Forms.Padding(3);
				this.chorusHubTab.Size = new System.Drawing.Size(476, 268);
				this.chorusHubTab.TabIndex = 2;
				this.chorusHubTab.Text = "Chorus Hub";
				this.chorusHubTab.UseVisualStyleBackColor = true;
				// 
				// _showChorusHubInSendReceive
				// 
				this._showChorusHubInSendReceive.AutoSize = true;
				this.l10NSharpExtender1.SetLocalizableToolTip(this._showChorusHubInSendReceive, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._showChorusHubInSendReceive, null);
				this.l10NSharpExtender1.SetLocalizingId(this._showChorusHubInSendReceive, "SendReceiveSettings.ShowChorusHub");
				this._showChorusHubInSendReceive.Location = new System.Drawing.Point(85, 17);
				this._showChorusHubInSendReceive.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
				this._showChorusHubInSendReceive.Name = "_showChorusHubInSendReceive";
				this._showChorusHubInSendReceive.Size = new System.Drawing.Size(240, 17);
				this._showChorusHubInSendReceive.TabIndex = 5;
				this._showChorusHubInSendReceive.Text = "&Show Chorus Hub as a Send/Receive option";
				this._showChorusHubInSendReceive.UseVisualStyleBackColor = true;
				// 
				// chorusHubSetup
				// 
				this.chorusHubSetup.AllowWebBrowserDrop = false;
				this.chorusHubSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
				this.chorusHubSetup.BackColor = System.Drawing.Color.White;
				this.chorusHubSetup.Font = new System.Drawing.Font("Segoe UI", 9F);
				this.chorusHubSetup.IsWebBrowserContextMenuEnabled = false;
				this.l10NSharpExtender1.SetLocalizableToolTip(this.chorusHubSetup, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.chorusHubSetup, null);
				this.l10NSharpExtender1.SetLocalizationPriority(this.chorusHubSetup, L10NSharp.LocalizationPriority.MediumLow);
				this.l10NSharpExtender1.SetLocalizingId(this.chorusHubSetup, "SendReceiveSetting.ChorusHubDescription");
				this.chorusHubSetup.Location = new System.Drawing.Point(85, 44);
				this.chorusHubSetup.Name = "chorusHubSetup";
				this.chorusHubSetup.Size = new System.Drawing.Size(0, 0);
				this.chorusHubSetup.TabIndex = 4;
				this.chorusHubSetup.TabStop = false;
				this.chorusHubSetup.Url = new System.Uri("about:blank", System.UriKind.Absolute);
				this.chorusHubSetup.WebBrowserShortcutsEnabled = false;
				// 
				// pictureBox4
				// 
				this.pictureBox4.Image = global::Chorus.Properties.Resources.chorusHubLarge;
				this.l10NSharpExtender1.SetLocalizableToolTip(this.pictureBox4, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.pictureBox4, null);
				this.l10NSharpExtender1.SetLocalizingId(this.pictureBox4, "SendReceiveSettings.SendReceiveSettings.pictureBox4");
				this.pictureBox4.Location = new System.Drawing.Point(6, 17);
				this.pictureBox4.Name = "pictureBox4";
				this.pictureBox4.Size = new System.Drawing.Size(64, 66);
				this.pictureBox4.TabIndex = 3;
				this.pictureBox4.TabStop = false;
				// 
				// _helpButton
				// 
				this._helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.l10NSharpExtender1.SetLocalizableToolTip(this._helpButton, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._helpButton, null);
				this.l10NSharpExtender1.SetLocalizationPriority(this._helpButton, L10NSharp.LocalizationPriority.High);
				this.l10NSharpExtender1.SetLocalizingId(this._helpButton, "Common.Help");
				this._helpButton.Location = new System.Drawing.Point(422, 399);
				this._helpButton.Name = "_helpButton";
				this._helpButton.Size = new System.Drawing.Size(75, 23);
				this._helpButton.TabIndex = 5;
				this._helpButton.Text = "&Help";
				this._helpButton.UseVisualStyleBackColor = true;
				this._helpButton.Click += new System.EventHandler(this._helpButton_Click);
				// 
				// _cancelButton
				// 
				this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.l10NSharpExtender1.SetLocalizableToolTip(this._cancelButton, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._cancelButton, null);
				this.l10NSharpExtender1.SetLocalizingId(this._cancelButton, "Common.Cancel");
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
				this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
				this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
				this.l10NSharpExtender1.SetLocalizingId(this._okButton, "Common.OK");
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
				this.l10NSharpExtender1.SetLocalizableToolTip(this.settingsProtectionButton, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.settingsProtectionButton, null);
				this.l10NSharpExtender1.SetLocalizationPriority(this.settingsProtectionButton, L10NSharp.LocalizationPriority.NotLocalizable);
				this.l10NSharpExtender1.SetLocalizingId(this.settingsProtectionButton, "SendReceiveSettings.SendReceiveSettings.SettingsProtectionLauncherButton");
				this.settingsProtectionButton.Location = new System.Drawing.Point(14, 388);
				this.settingsProtectionButton.Margin = new System.Windows.Forms.Padding(0);
				this.settingsProtectionButton.Name = "settingsProtectionButton";
				this.settingsProtectionButton.Size = new System.Drawing.Size(258, 47);
				this.settingsProtectionButton.TabIndex = 0;
				// 
				// pictureBox3
				// 
				this.pictureBox3.Image = global::Chorus.Properties.Resources.Committer_Person;
				this.l10NSharpExtender1.SetLocalizableToolTip(this.pictureBox3, null);
				this.l10NSharpExtender1.SetLocalizationComment(this.pictureBox3, null);
				this.l10NSharpExtender1.SetLocalizingId(this.pictureBox3, "SendReceiveSettings.SendReceiveSettings.pictureBox3");
				this.pictureBox3.Location = new System.Drawing.Point(17, 9);
				this.pictureBox3.Name = "pictureBox3";
				this.pictureBox3.Size = new System.Drawing.Size(52, 51);
				this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
				this.pictureBox3.TabIndex = 6;
				this.pictureBox3.TabStop = false;
				// 
				// l10NSharpExtender1
				// 
				this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
				this.l10NSharpExtender1.PrefixForNewItems = "SendReceiveSettings";
				// 
				// SendReceiveSettings
				// 
				this.AcceptButton = this._okButton;
				this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.CancelButton = this._cancelButton;
				this.ClientSize = new System.Drawing.Size(508, 439);
				this.Controls.Add(this.pictureBox3);
				this.Controls.Add(this._okButton);
				this.Controls.Add(this._cancelButton);
				this.Controls.Add(this._helpButton);
				this.Controls.Add(this.settingsTabs);
				this.Controls.Add(this.userNameTextBox);
				this.Controls.Add(this.nameLabel);
				this.Controls.Add(this.settingsProtectionButton);
				this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
				this.l10NSharpExtender1.SetLocalizationComment(this, null);
				this.l10NSharpExtender1.SetLocalizingId(this, "SendReceiveSettings.WindowTitle");
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "SendReceiveSettings";
				this.ShowIcon = false;
				this.ShowInTaskbar = false;
				this.Text = "Send/Receive Settings";
				this.settingsTabs.ResumeLayout(false);
				this.internetTab.ResumeLayout(false);
				this.internetTab.PerformLayout();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
				this._internetSettingsLayout.ResumeLayout(false);
				this._internetSettingsLayout.PerformLayout();
				this.chorusHubTab.ResumeLayout(false);
				this.chorusHubTab.PerformLayout();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
				this.ResumeLayout(false);
				this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox userNameTextBox;
		private System.Windows.Forms.TabControl settingsTabs;
		private System.Windows.Forms.TabPage internetTab;
		private System.Windows.Forms.Button _helpButton;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private SIL.Windows.Forms.SettingProtection.SettingsProtectionLauncherButton settingsProtectionButton;
		private System.Windows.Forms.TableLayoutPanel _internetSettingsLayout;
		private System.Windows.Forms.CheckBox _internetButtonEnabledCheckBox;
		private Misc.ServerSettingsControl _serverSettingsControl;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox3;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
		private System.Windows.Forms.TabPage chorusHubTab;
		private SIL.Windows.Forms.HtmlBrowser.XWebBrowser chorusHubSetup;
		private System.Windows.Forms.PictureBox pictureBox4;
		private System.Windows.Forms.CheckBox _showChorusHubInSendReceive;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}