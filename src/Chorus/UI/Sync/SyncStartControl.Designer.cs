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
			if (disposing && (components != null))
			{
				_internetStateWorker.RequestStop();
				_networkStateWorker.RequestStop();
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
			this._useSharedFolderButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useUSBButton = new System.Windows.Forms.Button();
			this._updateDisplayTimer = new System.Windows.Forms.Timer(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._useSharedFolderStatusLabel = new System.Windows.Forms.LinkLabel();
			this._usbStatusLabel = new Chorus.UI.BetterLabel();
			this.betterLabel2 = new Chorus.UI.BetterLabel();
			this._commitMessageText = new System.Windows.Forms.TextBox();
			this._internetStatusLabel = new System.Windows.Forms.LinkLabel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.betterLabel1 = new Chorus.UI.BetterLabel();
			this._userName = new System.Windows.Forms.TextBox();
			this._sharedNetworkDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this._internetDiagnosticsLink = new System.Windows.Forms.LinkLabel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.usbDriveLocator = new Chorus.UI.UsbDriveLocator(this.components);
			this.tableLayoutPanel1.SuspendLayout();
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
			this._useSharedFolderButton.Location = new System.Drawing.Point(3, 157);
			this._useSharedFolderButton.Name = "_useSharedFolderButton";
			this._useSharedFolderButton.Size = new System.Drawing.Size(167, 39);
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
			this._useInternetButton.Location = new System.Drawing.Point(3, 78);
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(167, 39);
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
			this._useUSBButton.Location = new System.Drawing.Point(3, 3);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(167, 39);
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
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.tableLayoutPanel1.Controls.Add(this._useSharedFolderStatusLabel, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._useUSBButton, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._usbStatusLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._useInternetButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._useSharedFolderButton, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.betterLabel2, 0, 7);
			this.tableLayoutPanel1.Controls.Add(this._commitMessageText, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this._internetStatusLabel, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this._sharedNetworkDiagnosticsLink, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this._internetDiagnosticsLink, 1, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 13);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(330, 323);
			this.tableLayoutPanel1.TabIndex = 2;
			//
			// _useSharedFolderStatusLabel
			//
			this._useSharedFolderStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._useSharedFolderStatusLabel.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._useSharedFolderStatusLabel, 2);
			this._useSharedFolderStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._useSharedFolderStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this._useSharedFolderStatusLabel.Location = new System.Drawing.Point(3, 200);
			this._useSharedFolderStatusLabel.Name = "_useSharedFolderStatusLabel";
			this._useSharedFolderStatusLabel.Size = new System.Drawing.Size(324, 21);
			this._useSharedFolderStatusLabel.TabIndex = 7;
			this._useSharedFolderStatusLabel.TabStop = true;
			this._useSharedFolderStatusLabel.Text = "A nice message with launcher";
			this._useSharedFolderStatusLabel.UseCompatibleTextRendering = true;
			this._useSharedFolderStatusLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._sharedFolderStatusLabel_LinkClicked);
			//
			// _usbStatusLabel
			//
			this._usbStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._usbStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel1.SetColumnSpan(this._usbStatusLabel, 2);
			this._usbStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._usbStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._usbStatusLabel.Location = new System.Drawing.Point(3, 48);
			this._usbStatusLabel.Multiline = true;
			this._usbStatusLabel.Name = "_usbStatusLabel";
			this._usbStatusLabel.ReadOnly = true;
			this._usbStatusLabel.Size = new System.Drawing.Size(324, 24);
			this._usbStatusLabel.TabIndex = 1;
			this._usbStatusLabel.TabStop = false;
			this._usbStatusLabel.Text = "Checking...";
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel1.SetColumnSpan(this.betterLabel2, 2);
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.Location = new System.Drawing.Point(3, 267);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(324, 14);
			this.betterLabel2.TabIndex = 3;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Label this point in the project history (Optional) :";
			//
			// _commitMessageText
			//
			this.tableLayoutPanel1.SetColumnSpan(this._commitMessageText, 2);
			this._commitMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
			this._commitMessageText.Location = new System.Drawing.Point(3, 287);
			this._commitMessageText.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
			this._commitMessageText.Multiline = true;
			this._commitMessageText.Name = "_commitMessageText";
			this._commitMessageText.Size = new System.Drawing.Size(307, 37);
			this._commitMessageText.TabIndex = 4;
			//
			// _internetStatusLabel
			//
			this._internetStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._internetStatusLabel.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this._internetStatusLabel, 2);
			this._internetStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._internetStatusLabel.LinkArea = new System.Windows.Forms.LinkArea(20, 8);
			this._internetStatusLabel.Location = new System.Drawing.Point(3, 123);
			this._internetStatusLabel.Name = "_internetStatusLabel";
			this._internetStatusLabel.Size = new System.Drawing.Size(324, 21);
			this._internetStatusLabel.TabIndex = 5;
			this._internetStatusLabel.TabStop = true;
			this._internetStatusLabel.Text = "A nice message with launcher";
			this._internetStatusLabel.UseCompatibleTextRendering = true;
			this._internetStatusLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._internetStatusLabel_LinkClicked);
			//
			// flowLayoutPanel1
			//
			this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
			this.flowLayoutPanel1.Controls.Add(this.betterLabel1);
			this.flowLayoutPanel1.Controls.Add(this._userName);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 233);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(327, 28);
			this.flowLayoutPanel1.TabIndex = 8;
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(3, 3);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(100, 20);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Your Name:";
			//
			// _userName
			//
			this._userName.Location = new System.Drawing.Point(109, 3);
			this._userName.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this._userName.Name = "_userName";
			this._userName.Size = new System.Drawing.Size(200, 20);
			this._userName.TabIndex = 1;
			//
			// _sharedNetworkDiagnosticsLink
			//
			this._sharedNetworkDiagnosticsLink.AccessibleName = "SharedFolderDiagnosticsLink";
			this._sharedNetworkDiagnosticsLink.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._sharedNetworkDiagnosticsLink.AutoSize = true;
			this._sharedNetworkDiagnosticsLink.Enabled = false;
			this._sharedNetworkDiagnosticsLink.Location = new System.Drawing.Point(265, 170);
			this._sharedNetworkDiagnosticsLink.Name = "_sharedNetworkDiagnosticsLink";
			this._sharedNetworkDiagnosticsLink.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._sharedNetworkDiagnosticsLink.Size = new System.Drawing.Size(62, 13);
			this._sharedNetworkDiagnosticsLink.TabIndex = 9;
			this._sharedNetworkDiagnosticsLink.TabStop = true;
			this._sharedNetworkDiagnosticsLink.Text = "Diagnostics";
			this._sharedNetworkDiagnosticsLink.Visible = false;
			this._sharedNetworkDiagnosticsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._sharedNetworkDiagnosticsLink_LinkClicked);
			//
			// _internetDiagnosticsLink
			//
			this._internetDiagnosticsLink.AccessibleName = "InternetDiagnosticsLink";
			this._internetDiagnosticsLink.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._internetDiagnosticsLink.AutoSize = true;
			this._internetDiagnosticsLink.Location = new System.Drawing.Point(265, 92);
			this._internetDiagnosticsLink.Name = "_internetDiagnosticsLink";
			this._internetDiagnosticsLink.Size = new System.Drawing.Size(62, 13);
			this._internetDiagnosticsLink.TabIndex = 10;
			this._internetDiagnosticsLink.TabStop = true;
			this._internetDiagnosticsLink.Text = "Diagnostics";
			this._internetDiagnosticsLink.Visible = false;
			this._internetDiagnosticsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._internetDiagnosticsLink_LinkClicked);
			//
			// SyncStartControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SyncStartControl";
			this.Size = new System.Drawing.Size(330, 388);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _useUSBButton;
		private System.Windows.Forms.Button _useInternetButton;
		private System.Windows.Forms.Button _useSharedFolderButton;
		private System.Windows.Forms.Timer _updateDisplayTimer;
		private BetterLabel _usbStatusLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ToolTip toolTip1;
		private BetterLabel betterLabel2;
		private System.Windows.Forms.TextBox _commitMessageText;
		private UsbDriveLocator usbDriveLocator;
		private System.Windows.Forms.LinkLabel _internetStatusLabel;
		private System.Windows.Forms.LinkLabel _useSharedFolderStatusLabel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private BetterLabel betterLabel1;
		private System.Windows.Forms.TextBox _userName;
		private System.Windows.Forms.LinkLabel _sharedNetworkDiagnosticsLink;
		private System.Windows.Forms.LinkLabel _internetDiagnosticsLink;
	}
}
