﻿using System.Threading;
using SIL.Windows.Forms.SettingProtection;

namespace Chorus.UI.Sync
{
	partial class SyncStartControl
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
			Monitor.Enter(this);
			_exiting = true;
			if (disposing && (components != null))
			{
				_updateDisplayTimer.Stop();
				components.Dispose();
			}
			base.Dispose(disposing);
			Monitor.Exit(this);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this._useLocalNetworkButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useUSBButton = new System.Windows.Forms.Button();
			this._updateDisplayTimer = new System.Windows.Forms.Timer(this.components);
			this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this._commitMessageText = new System.Windows.Forms.TextBox();
			this._internetStatusLabel = new System.Windows.Forms.LinkLabel();
			this._useSharedFolderStatusLabel = new System.Windows.Forms.LinkLabel();
			this._internetDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this._sharedNetworkDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this._settingsButton = new SIL.Windows.Forms.SettingProtection.SettingsLauncherButton();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.commitMessageLabel = new Chorus.UI.BetterLabel();
			this._usbStatusLabel = new Chorus.UI.BetterLabel();
			this.usbDriveLocator = new Chorus.UI.UsbDriveLocator(this.components);
			this._tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).BeginInit();
			this.SuspendLayout();
			//
			// _useLocalNetworkButton
			//
			this._useLocalNetworkButton.BackColor = System.Drawing.Color.White;
			this._useLocalNetworkButton.Enabled = false;
			this._useLocalNetworkButton.Image = global::Chorus.Properties.Resources.chorusHubMedium;
			this._useLocalNetworkButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._useLocalNetworkButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._useLocalNetworkButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._useLocalNetworkButton, "SyncStartControl.UseLocalNetworkButton");
			this._useLocalNetworkButton.Location = new System.Drawing.Point(3, 228);
			this._useLocalNetworkButton.Name = "_useLocalNetworkButton";
			this._useLocalNetworkButton.Size = new System.Drawing.Size(167, 38);
			this._useLocalNetworkButton.TabIndex = 3;
			this._useLocalNetworkButton.Text = "&Chorus Hub";
			this._useLocalNetworkButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useLocalNetworkButton.UseVisualStyleBackColor = false;
			this._useLocalNetworkButton.Click += new System.EventHandler(this._useLocalNetworkButton_Click);
			//
			// _useInternetButton
			//
			this._useInternetButton.BackColor = System.Drawing.Color.White;
			this._useInternetButton.Image = global::Chorus.Properties.Resources.internet29x32;
			this._useInternetButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._useInternetButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._useInternetButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._useInternetButton, "SyncStartControl.UseInternetButton");
			this._useInternetButton.Location = new System.Drawing.Point(3, 143);
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(167, 38);
			this._useInternetButton.TabIndex = 2;
			this._useInternetButton.Text = "&Internet";
			this._useInternetButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useInternetButton.UseVisualStyleBackColor = false;
			this._useInternetButton.Click += new System.EventHandler(this._useInternetButton_Click);
			//
			// _useUSBButton
			//
			this._useUSBButton.BackColor = System.Drawing.Color.White;
			this._useUSBButton.Image = global::Chorus.Properties.Resources.Usb32x28;
			this._useUSBButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._useUSBButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._useUSBButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._useUSBButton, "SyncStartControl.UseUSBButton");
			this._useUSBButton.Location = new System.Drawing.Point(3, 58);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(167, 38);
			this._useUSBButton.TabIndex = 1;
			this._useUSBButton.Text = "&USB Flash Drive";
			this._useUSBButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useUSBButton.UseVisualStyleBackColor = false;
			this._useUSBButton.Click += new System.EventHandler(this._useUSBButton_Click);
			//
			// _updateDisplayTimer
			//
			this._updateDisplayTimer.Interval = 2000;
			this._updateDisplayTimer.Tick += new System.EventHandler(this.OnUpdateDisplayTick);
			//
			// _tableLayoutPanel
			//
			this._tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.ColumnCount = 2;
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this._tableLayoutPanel.Controls.Add(this.commitMessageLabel, 0, 0);
			this._tableLayoutPanel.Controls.Add(this._commitMessageText, 0, 1);
			this._tableLayoutPanel.Controls.Add(this._useUSBButton, 0, 2);
			this._tableLayoutPanel.Controls.Add(this._usbStatusLabel, 0, 3);
			this._tableLayoutPanel.Controls.Add(this._useInternetButton, 0, 4);
			this._tableLayoutPanel.Controls.Add(this._internetStatusLabel, 0, 5);
			this._tableLayoutPanel.Controls.Add(this._useLocalNetworkButton, 0, 6);
			this._tableLayoutPanel.Controls.Add(this._useSharedFolderStatusLabel, 0, 7);
			this._tableLayoutPanel.Controls.Add(this._internetDiagnosticsLink, 1, 4);
			this._tableLayoutPanel.Controls.Add(this._sharedNetworkDiagnosticsLink, 1, 6);
			this._tableLayoutPanel.Controls.Add(this._settingsButton, 0, 8);
			this._tableLayoutPanel.Location = new System.Drawing.Point(22, 13);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 9;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // increase V space to fix text clipping issue: WS-46
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F)); // increase V space to fix text clipping issue: WS-46
			this._tableLayoutPanel.Size = new System.Drawing.Size(342, 351);
			this._tableLayoutPanel.TabIndex = 0;
			//
			// _commitMessageText
			//
			this._commitMessageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._commitMessageText, 2);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._commitMessageText, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._commitMessageText, null);
			this.l10NSharpExtender1.SetLocalizingId(this._commitMessageText, "SyncStartControl.SyncStartControl._commitMessageText");
			this._commitMessageText.Location = new System.Drawing.Point(3, 23);
			this._commitMessageText.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
			this._commitMessageText.Name = "_commitMessageText";
			this._commitMessageText.Size = new System.Drawing.Size(319, 20);
			this._commitMessageText.TabIndex = 0;
			//
			// _internetStatusLabel
			//
			this._internetStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._internetStatusLabel.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._internetStatusLabel, 2);
			this._internetStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._internetStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._internetStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._internetStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._internetStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._internetStatusLabel, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._internetStatusLabel, "SyncStartControl._internetStatusLabel");
			this._internetStatusLabel.Location = new System.Drawing.Point(3, 185);
			this._internetStatusLabel.Name = "_internetStatusLabel";
			this._internetStatusLabel.Size = new System.Drawing.Size(336, 21);
			this._internetStatusLabel.TabIndex = 5;
			this._internetStatusLabel.Text = "A nice message";
			this._internetStatusLabel.UseCompatibleTextRendering = true;
			this._internetStatusLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._internetStatusLabel_LinkClicked);
			//
			// _useSharedFolderStatusLabel
			//
			this._useSharedFolderStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._useSharedFolderStatusLabel.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._useSharedFolderStatusLabel, 2);
			this._useSharedFolderStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._useSharedFolderStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._useSharedFolderStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._useSharedFolderStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._useSharedFolderStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._useSharedFolderStatusLabel, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._useSharedFolderStatusLabel, "SyncStartControl._useSharedFolderStatusLabel");
			this._useSharedFolderStatusLabel.Location = new System.Drawing.Point(3, 270);
			this._useSharedFolderStatusLabel.Name = "_useSharedFolderStatusLabel";
			this._useSharedFolderStatusLabel.Size = new System.Drawing.Size(336, 21);
			this._useSharedFolderStatusLabel.TabIndex = 7;
			this._useSharedFolderStatusLabel.Text = "A nice message";
			this._useSharedFolderStatusLabel.UseCompatibleTextRendering = true;
			//
			// _internetDiagnosticsLink
			//
			this._internetDiagnosticsLink.AccessibleName = "InternetDiagnosticsLink";
			this._internetDiagnosticsLink.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._internetDiagnosticsLink.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._internetDiagnosticsLink, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._internetDiagnosticsLink, null);
			this.l10NSharpExtender1.SetLocalizingId(this._internetDiagnosticsLink, "SyncStartControl.Diagnostics");
			this._internetDiagnosticsLink.Location = new System.Drawing.Point(277, 156);
			this._internetDiagnosticsLink.Name = "_internetDiagnosticsLink";
			this._internetDiagnosticsLink.Size = new System.Drawing.Size(62, 13);
			this._internetDiagnosticsLink.TabIndex = 10;
			this._internetDiagnosticsLink.TabStop = true;
			this._internetDiagnosticsLink.Text = "Diagnostics";
			this._internetDiagnosticsLink.Visible = false;
			this._internetDiagnosticsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._internetDiagnosticsLink_LinkClicked);
			//
			// _sharedNetworkDiagnosticsLink
			//
			this._sharedNetworkDiagnosticsLink.AccessibleName = "SharedFolderDiagnosticsLink";
			this._sharedNetworkDiagnosticsLink.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._sharedNetworkDiagnosticsLink.AutoSize = true;
			this._sharedNetworkDiagnosticsLink.Enabled = false;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._sharedNetworkDiagnosticsLink, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._sharedNetworkDiagnosticsLink, null);
			this.l10NSharpExtender1.SetLocalizingId(this._sharedNetworkDiagnosticsLink, "SyncStartControl.Diagnostics");
			this._sharedNetworkDiagnosticsLink.Location = new System.Drawing.Point(277, 241);
			this._sharedNetworkDiagnosticsLink.Name = "_sharedNetworkDiagnosticsLink";
			this._sharedNetworkDiagnosticsLink.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._sharedNetworkDiagnosticsLink.Size = new System.Drawing.Size(62, 13);
			this._sharedNetworkDiagnosticsLink.TabIndex = 4;
			this._sharedNetworkDiagnosticsLink.TabStop = true;
			this._sharedNetworkDiagnosticsLink.Text = "Diagnostics";
			this._sharedNetworkDiagnosticsLink.Visible = false;
			this._sharedNetworkDiagnosticsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._sharedNetworkDiagnosticsLink_LinkClicked);
			//
			// _settingsButton
			//
			this._settingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._settingsButton, 2);
			this._settingsButton.LaunchSettingsCallback = null;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._settingsButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._settingsButton, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._settingsButton, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._settingsButton, "SyncStartControl.SettingsLauncherButton");
			this._settingsButton.Location = new System.Drawing.Point(232, 310);
			this._settingsButton.Margin = new System.Windows.Forms.Padding(0);
			this._settingsButton.Name = "_settingsButton";
			this._settingsButton.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this._settingsButton.Size = new System.Drawing.Size(110, 25);  // increase V space to fix text clipping issue: WS-46
			this._settingsButton.TabIndex = 5;
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "SyncStartControl";
			//
			// commitMessageLabel
			//
			this.commitMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.commitMessageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._tableLayoutPanel.SetColumnSpan(this.commitMessageLabel, 2);
			this.commitMessageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.commitMessageLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.commitMessageLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this.commitMessageLabel, "SyncStartControl.CommitMessage");
			this.commitMessageLabel.Location = new System.Drawing.Point(3, 3);
			this.commitMessageLabel.Multiline = true;
			this.commitMessageLabel.Name = "commitMessageLabel";
			this.commitMessageLabel.ReadOnly = true;
			this.commitMessageLabel.Size = new System.Drawing.Size(336, 25);  // increase V space to fix text clipping issue: WS-46
			this.commitMessageLabel.TabIndex = 3;
			this.commitMessageLabel.TabStop = false;
			this.commitMessageLabel.Text = "Label this point in the project history (Optional) :";
			//
			// _usbStatusLabel
			//
			this._usbStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._usbStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._tableLayoutPanel.SetColumnSpan(this._usbStatusLabel, 2);
			this._usbStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._usbStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._usbStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._usbStatusLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._usbStatusLabel, "SyncStartControl.Checking");
			this._usbStatusLabel.Location = new System.Drawing.Point(3, 103);
			this._usbStatusLabel.Multiline = true;
			this._usbStatusLabel.Name = "_usbStatusLabel";
			this._usbStatusLabel.ReadOnly = true;
			this._usbStatusLabel.Size = new System.Drawing.Size(336, 30);
			this._usbStatusLabel.TabIndex = 1;
			this._usbStatusLabel.TabStop = false;
			this._usbStatusLabel.Text = "Checking...";
			//
			// SyncStartControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this._tableLayoutPanel);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SyncStartControl.SyncStartControl.SyncStartControl");
			this.Name = "SyncStartControl";
			this.Size = new System.Drawing.Size(384, 367);
			this._tableLayoutPanel.ResumeLayout(false);
			this._tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _useUSBButton;
		private System.Windows.Forms.Button _useInternetButton;
		private System.Windows.Forms.Button _useLocalNetworkButton;
		private System.Windows.Forms.Timer _updateDisplayTimer;
		private BetterLabel _usbStatusLabel;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
		private System.Windows.Forms.ToolTip toolTip1;
		private BetterLabel commitMessageLabel;
		private System.Windows.Forms.TextBox _commitMessageText;
		private UsbDriveLocator usbDriveLocator;
		private System.Windows.Forms.LinkLabel _internetStatusLabel;
		private System.Windows.Forms.LinkLabel _useSharedFolderStatusLabel;
		private System.Windows.Forms.LinkLabel _sharedNetworkDiagnosticsLink;
		private System.Windows.Forms.LinkLabel _internetDiagnosticsLink;
		private SettingsLauncherButton _settingsButton;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
