using System.Threading;
using Palaso.UI.WindowsForms.SettingProtection;

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
			this._useSharedFolderButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useUSBButton = new System.Windows.Forms.Button();
			this._updateDisplayTimer = new System.Windows.Forms.Timer(this.components);
			this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this._commitMessageText = new System.Windows.Forms.TextBox();
			this._internetStatusLabel = new System.Windows.Forms.LinkLabel();
			this._useSharedFolderStatusLabel = new System.Windows.Forms.LinkLabel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this._settingsButton = new Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherButton();
			this._internetDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this._sharedNetworkDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.commitMessageLabel = new Chorus.UI.BetterLabel();
			this._usbStatusLabel = new Chorus.UI.BetterLabel();
			this.usbDriveLocator = new Chorus.UI.UsbDriveLocator(this.components);
			this._tableLayoutPanel.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).BeginInit();
			this.SuspendLayout();
			//
			// _useSharedFolderButton
			//
			this._useSharedFolderButton.BackColor = System.Drawing.Color.White;
			this._useSharedFolderButton.Enabled = false;
			this._useSharedFolderButton.Image = global::Chorus.Properties.Resources.networkFolder29x32;
			this._useSharedFolderButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useSharedFolderButton.Location = new System.Drawing.Point(3, 228);
			this._useSharedFolderButton.Name = "_useSharedFolderButton";
			this._useSharedFolderButton.Size = new System.Drawing.Size(167, 38);
			this._useSharedFolderButton.TabIndex = 0;
			this._useSharedFolderButton.Text = "&Shared Network Folder";
			this._useSharedFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useSharedFolderButton.UseVisualStyleBackColor = false;
			this._useSharedFolderButton.Click += new System.EventHandler(this._useSharedFolderButton_Click);
			//
			// _useInternetButton
			//
			this._useInternetButton.BackColor = System.Drawing.Color.White;
			this._useInternetButton.Image = global::Chorus.Properties.Resources.internet29x32;
			this._useInternetButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useInternetButton.Location = new System.Drawing.Point(3, 143);
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(167, 38);
			this._useInternetButton.TabIndex = 0;
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
			this._useUSBButton.Location = new System.Drawing.Point(3, 58);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(167, 38);
			this._useUSBButton.TabIndex = 0;
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
			this._tableLayoutPanel.Controls.Add(this._useInternetButton, 0, 5);
			this._tableLayoutPanel.Controls.Add(this._internetStatusLabel, 0, 7);
			this._tableLayoutPanel.Controls.Add(this._useSharedFolderButton, 0, 8);
			this._tableLayoutPanel.Controls.Add(this._useSharedFolderStatusLabel, 0, 9);
			this._tableLayoutPanel.Controls.Add(this.flowLayoutPanel1, 0, 11);
			this._tableLayoutPanel.Controls.Add(this._internetDiagnosticsLink, 1, 5);
			this._tableLayoutPanel.Controls.Add(this._sharedNetworkDiagnosticsLink, 1, 8);
			this._tableLayoutPanel.Location = new System.Drawing.Point(22, 13);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 12;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.Size = new System.Drawing.Size(342, 351);
			this._tableLayoutPanel.TabIndex = 2;
			//
			// _commitMessageText
			//
			this._commitMessageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._commitMessageText, 2);
			this._commitMessageText.Location = new System.Drawing.Point(3, 23);
			this._commitMessageText.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
			this._commitMessageText.Name = "_commitMessageText";
			this._commitMessageText.Size = new System.Drawing.Size(319, 20);
			this._commitMessageText.TabIndex = 4;
			//
			// _internetStatusLabel
			//
			this._internetStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._internetStatusLabel.AutoSize = true;
			this._tableLayoutPanel.SetColumnSpan(this._internetStatusLabel, 2);
			this._internetStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._internetStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this._internetStatusLabel.Location = new System.Drawing.Point(3, 205);
			this._internetStatusLabel.Name = "_internetStatusLabel";
			this._internetStatusLabel.Size = new System.Drawing.Size(336, 20);
			this._internetStatusLabel.TabIndex = 5;
			this._internetStatusLabel.TabStop = true;
			this._internetStatusLabel.Text = "A nice message with launcher";
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
			this._useSharedFolderStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this._useSharedFolderStatusLabel.Location = new System.Drawing.Point(3, 270);
			this._useSharedFolderStatusLabel.Name = "_useSharedFolderStatusLabel";
			this._useSharedFolderStatusLabel.Size = new System.Drawing.Size(336, 20);
			this._useSharedFolderStatusLabel.TabIndex = 7;
			this._useSharedFolderStatusLabel.TabStop = true;
			this._useSharedFolderStatusLabel.Text = "A nice message with launcher";
			this._useSharedFolderStatusLabel.UseCompatibleTextRendering = true;
			//
			// flowLayoutPanel1
			//
			this._tableLayoutPanel.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this._settingsButton);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 313);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(339, 35);
			this.flowLayoutPanel1.TabIndex = 8;
			//
			// _settingsButton
			//
			this._settingsButton.LaunchSettingsCallback = null;
			this._settingsButton.Location = new System.Drawing.Point(0, 0);
			this._settingsButton.Margin = new System.Windows.Forms.Padding(0);
			this._settingsButton.Name = "_settingsButton";
			this._settingsButton.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this._settingsButton.Size = new System.Drawing.Size(131, 22);
			this._settingsButton.TabIndex = 0;
			//
			// _internetDiagnosticsLink
			//
			this._internetDiagnosticsLink.AccessibleName = "InternetDiagnosticsLink";
			this._internetDiagnosticsLink.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._internetDiagnosticsLink.AutoSize = true;
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
			this._sharedNetworkDiagnosticsLink.Location = new System.Drawing.Point(277, 241);
			this._sharedNetworkDiagnosticsLink.Name = "_sharedNetworkDiagnosticsLink";
			this._sharedNetworkDiagnosticsLink.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._sharedNetworkDiagnosticsLink.Size = new System.Drawing.Size(62, 13);
			this._sharedNetworkDiagnosticsLink.TabIndex = 9;
			this._sharedNetworkDiagnosticsLink.TabStop = true;
			this._sharedNetworkDiagnosticsLink.Text = "Diagnostics";
			this._sharedNetworkDiagnosticsLink.Visible = false;
			this._sharedNetworkDiagnosticsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._sharedNetworkDiagnosticsLink_LinkClicked);
			//
			// commitMessageLabel
			//
			this.commitMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.commitMessageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._tableLayoutPanel.SetColumnSpan(this.commitMessageLabel, 2);
			this.commitMessageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.commitMessageLabel.Location = new System.Drawing.Point(3, 3);
			this.commitMessageLabel.Multiline = true;
			this.commitMessageLabel.Name = "commitMessageLabel";
			this.commitMessageLabel.ReadOnly = true;
			this.commitMessageLabel.Size = new System.Drawing.Size(336, 14);
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
			this._usbStatusLabel.Location = new System.Drawing.Point(3, 103);
			this._usbStatusLabel.Multiline = true;
			this._usbStatusLabel.Name = "_usbStatusLabel";
			this._usbStatusLabel.ReadOnly = true;
			this._usbStatusLabel.Size = new System.Drawing.Size(336, 14);
			this._usbStatusLabel.TabIndex = 1;
			this._usbStatusLabel.TabStop = false;
			this._usbStatusLabel.Text = "Checking...";
			//
			// SyncStartControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this._tableLayoutPanel);
			this.Name = "SyncStartControl";
			this.Size = new System.Drawing.Size(384, 367);
			this._tableLayoutPanel.ResumeLayout(false);
			this._tableLayoutPanel.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _useUSBButton;
		private System.Windows.Forms.Button _useInternetButton;
		private System.Windows.Forms.Button _useSharedFolderButton;
		private System.Windows.Forms.Timer _updateDisplayTimer;
		private BetterLabel _usbStatusLabel;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
		private System.Windows.Forms.ToolTip toolTip1;
		private BetterLabel commitMessageLabel;
		private System.Windows.Forms.TextBox _commitMessageText;
		private UsbDriveLocator usbDriveLocator;
		private System.Windows.Forms.LinkLabel _internetStatusLabel;
		private System.Windows.Forms.LinkLabel _useSharedFolderStatusLabel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.LinkLabel _sharedNetworkDiagnosticsLink;
		private System.Windows.Forms.LinkLabel _internetDiagnosticsLink;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherButton _settingsButton;
	}
}
