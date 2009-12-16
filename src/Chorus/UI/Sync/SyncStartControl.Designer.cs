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
			this._usbStatusLabel = new Chorus.UI.BetterLabel();
			this._internetStatusLabel = new Chorus.UI.BetterLabel();
			this._sharedFolderLabel = new Chorus.UI.BetterLabel();
			this.betterLabel2 = new Chorus.UI.BetterLabel();
			this._commitMessageText = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.usbDriveLocator = new Chorus.UI.UsbDriveLocator(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).BeginInit();
			this.SuspendLayout();
			//
			// _useSharedFolderButton
			//
			this._useSharedFolderButton.BackColor = System.Drawing.Color.White;
			this._useSharedFolderButton.Enabled = false;
			this._useSharedFolderButton.Image = global::Chorus.Properties.Resources.networkFolder29x32;
			this._useSharedFolderButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useSharedFolderButton.Location = new System.Drawing.Point(3, 153);
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
			this._updateDisplayTimer.Interval = 500;
			this._updateDisplayTimer.Tick += new System.EventHandler(this.OnUpdateDisplayTick);
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._useUSBButton, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._usbStatusLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._internetStatusLabel, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this._sharedFolderLabel, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._useInternetButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._useSharedFolderButton, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.betterLabel2, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this._commitMessageText, 0, 7);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 13);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 8;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(330, 302);
			this.tableLayoutPanel1.TabIndex = 2;
			this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
			//
			// _usbStatusLabel
			//
			this._usbStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._usbStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
			// _internetStatusLabel
			//
			this._internetStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._internetStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._internetStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._internetStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._internetStatusLabel.Location = new System.Drawing.Point(3, 123);
			this._internetStatusLabel.Multiline = true;
			this._internetStatusLabel.Name = "_internetStatusLabel";
			this._internetStatusLabel.ReadOnly = true;
			this._internetStatusLabel.Size = new System.Drawing.Size(324, 24);
			this._internetStatusLabel.TabIndex = 1;
			this._internetStatusLabel.TabStop = false;
			this._internetStatusLabel.Text = "Checking...";
			//
			// _sharedFolderLabel
			//
			this._sharedFolderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sharedFolderLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._sharedFolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._sharedFolderLabel.ForeColor = System.Drawing.Color.DimGray;
			this._sharedFolderLabel.Location = new System.Drawing.Point(3, 198);
			this._sharedFolderLabel.Multiline = true;
			this._sharedFolderLabel.Name = "_sharedFolderLabel";
			this._sharedFolderLabel.ReadOnly = true;
			this._sharedFolderLabel.Size = new System.Drawing.Size(324, 23);
			this._sharedFolderLabel.TabIndex = 1;
			this._sharedFolderLabel.TabStop = false;
			this._sharedFolderLabel.Text = "Checking...";
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.Location = new System.Drawing.Point(3, 228);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(324, 24);
			this.betterLabel2.TabIndex = 3;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "LabelOfThingAnnotated this point in the project history (Optional) :";
			//
			// _commitMessageText
			//
			this._commitMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
			this._commitMessageText.Location = new System.Drawing.Point(3, 258);
			this._commitMessageText.Multiline = true;
			this._commitMessageText.Name = "_commitMessageText";
			this._commitMessageText.Size = new System.Drawing.Size(324, 41);
			this._commitMessageText.TabIndex = 4;
			//
			// SyncStartControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SyncStartControl";
			this.Size = new System.Drawing.Size(330, 383);
			this.Load += new System.EventHandler(this.SyncStartControl_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.usbDriveLocator)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _useUSBButton;
		private System.Windows.Forms.Button _useInternetButton;
		private System.Windows.Forms.Button _useSharedFolderButton;
		private System.Windows.Forms.Timer _updateDisplayTimer;
		private BetterLabel _usbStatusLabel;
		private BetterLabel _internetStatusLabel;
		private BetterLabel _sharedFolderLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ToolTip toolTip1;
		private BetterLabel betterLabel2;
		private System.Windows.Forms.TextBox _commitMessageText;
		private UsbDriveLocator usbDriveLocator;
	}
}
