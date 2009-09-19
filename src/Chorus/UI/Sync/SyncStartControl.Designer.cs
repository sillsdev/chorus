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
			this._usbStatusLabel = new Chorus.UI.BetterLabel();
			this._internetStatusLabel = new Chorus.UI.BetterLabel();
			this._sharedFolderLabel = new Chorus.UI.BetterLabel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			// _useSharedFolderButton
			//
			this._useSharedFolderButton.Enabled = false;
			this._useSharedFolderButton.Image = global::Chorus.Properties.Resources.networkFolder29x32;
			this._useSharedFolderButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useSharedFolderButton.Location = new System.Drawing.Point(3, 195);
			this._useSharedFolderButton.Name = "_useSharedFolderButton";
			this._useSharedFolderButton.Size = new System.Drawing.Size(263, 42);
			this._useSharedFolderButton.TabIndex = 0;
			this._useSharedFolderButton.Text = "Shared Network Folder";
			this._useSharedFolderButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useSharedFolderButton.UseVisualStyleBackColor = true;
			this._useSharedFolderButton.Click += new System.EventHandler(this.button2_Click);
			//
			// _useInternetButton
			//
			this._useInternetButton.Image = global::Chorus.Properties.Resources.internet29x32;
			this._useInternetButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useInternetButton.Location = new System.Drawing.Point(3, 99);
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(263, 42);
			this._useInternetButton.TabIndex = 0;
			this._useInternetButton.Text = "Internet";
			this._useInternetButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useInternetButton.UseVisualStyleBackColor = true;
			this._useInternetButton.Click += new System.EventHandler(this.button2_Click);
			//
			// _useUSBButton
			//
			this._useUSBButton.Image = global::Chorus.Properties.Resources.Usb32x28;
			this._useUSBButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useUSBButton.Location = new System.Drawing.Point(3, 3);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(263, 42);
			this._useUSBButton.TabIndex = 0;
			this._useUSBButton.Text = "USB Flash Drive";
			this._useUSBButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useUSBButton.UseVisualStyleBackColor = true;
			//
			// _updateDisplayTimer
			//
			this._updateDisplayTimer.Enabled = true;
			this._updateDisplayTimer.Interval = 500;
			this._updateDisplayTimer.Tick += new System.EventHandler(this.OnUpdateDisplayTick);
			//
			// _usbStatusLabel
			//
			this._usbStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._usbStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._usbStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._usbStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._usbStatusLabel.Location = new System.Drawing.Point(3, 51);
			this._usbStatusLabel.Multiline = true;
			this._usbStatusLabel.Name = "_usbStatusLabel";
			this._usbStatusLabel.ReadOnly = true;
			this._usbStatusLabel.Size = new System.Drawing.Size(324, 42);
			this._usbStatusLabel.TabIndex = 1;
			this._usbStatusLabel.TabStop = false;
			this._usbStatusLabel.Text = "usb";
			//
			// _internetStatusLabel
			//
			this._internetStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._internetStatusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._internetStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._internetStatusLabel.ForeColor = System.Drawing.Color.DimGray;
			this._internetStatusLabel.Location = new System.Drawing.Point(3, 147);
			this._internetStatusLabel.Multiline = true;
			this._internetStatusLabel.Name = "_internetStatusLabel";
			this._internetStatusLabel.ReadOnly = true;
			this._internetStatusLabel.Size = new System.Drawing.Size(324, 42);
			this._internetStatusLabel.TabIndex = 1;
			this._internetStatusLabel.TabStop = false;
			this._internetStatusLabel.Text = "internet";
			//
			// _sharedFolderLabel
			//
			this._sharedFolderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sharedFolderLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._sharedFolderLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._sharedFolderLabel.ForeColor = System.Drawing.Color.DimGray;
			this._sharedFolderLabel.Location = new System.Drawing.Point(3, 243);
			this._sharedFolderLabel.Multiline = true;
			this._sharedFolderLabel.Name = "_sharedFolderLabel";
			this._sharedFolderLabel.ReadOnly = true;
			this._sharedFolderLabel.Size = new System.Drawing.Size(324, 46);
			this._sharedFolderLabel.TabIndex = 1;
			this._sharedFolderLabel.TabStop = false;
			this._sharedFolderLabel.Text = "shared";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._useUSBButton, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._usbStatusLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this._internetStatusLabel, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this._sharedFolderLabel, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this._useInternetButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this._useSharedFolderButton, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(330, 292);
			this.tableLayoutPanel1.TabIndex = 2;
			//
			// SyncStartControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SyncStartControl";
			this.Size = new System.Drawing.Size(330, 292);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
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
	}
}
