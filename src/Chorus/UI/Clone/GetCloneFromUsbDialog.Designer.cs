using System;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;


namespace Chorus.UI.Clone
{
	partial class GetCloneFromUsbDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetCloneFromUsbDialog));
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._copyToComputerButton = new System.Windows.Forms.Button();
			this._statusImages = new System.Windows.Forms.ImageList(this.components);
			this._statusImage = new System.Windows.Forms.Button();
			this._lookingForUsbTimer = new System.Windows.Forms.Timer(this.components);
			this._statusLabel = new System.Windows.Forms.TextBox();
			this._logBox = new Palaso.UI.WindowsForms.Progress.LogBox();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.SuspendLayout();
			//
			// listView1
			//
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2});
			this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listView1.LargeImageList = this.imageList1;
			this.listView1.Location = new System.Drawing.Point(12, 50);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(281, 162);
			this.listView1.SmallImageList = this.imageList1;
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			this.listView1.DoubleClick += new System.EventHandler(this.OnMakeCloneClick);
			//
			// columnHeader1
			//
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 170;
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Modified Date";
			this.columnHeader2.Width = 120;
			//
			// imageList1
			//
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList1.Images.SetKeyName(0, "Project");
			this.imageList1.Images.SetKeyName(1, "ProjectSelected.png");
			this.imageList1.Images.SetKeyName(2, "Folder_Disabled.png");
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(218, 227);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 1;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(137, 227);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _copyToComputerButton
			//
			this._copyToComputerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._copyToComputerButton.Location = new System.Drawing.Point(15, 227);
			this._copyToComputerButton.Name = "_copyToComputerButton";
			this._copyToComputerButton.Size = new System.Drawing.Size(116, 23);
			this._copyToComputerButton.TabIndex = 5;
			this._copyToComputerButton.Text = "&Copy To Computer";
			this._copyToComputerButton.UseVisualStyleBackColor = true;
			this._copyToComputerButton.Click += new System.EventHandler(this.OnMakeCloneClick);
			//
			// _statusImages
			//
			this._statusImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_statusImages.ImageStream")));
			this._statusImages.TransparentColor = System.Drawing.Color.Transparent;
			this._statusImages.Images.SetKeyName(0, "UsbDriveNotFound");
			this._statusImages.Images.SetKeyName(1, "Success");
			this._statusImages.Images.SetKeyName(2, "Error");
			//
			// _statusImage
			//
			this._statusImage.FlatAppearance.BorderSize = 0;
			this._statusImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._statusImage.ImageKey = "UsbDriveNotFound";
			this._statusImage.ImageList = this._statusImages;
			this._statusImage.Location = new System.Drawing.Point(8, 7);
			this._statusImage.Name = "_statusImage";
			this._statusImage.Size = new System.Drawing.Size(50, 36);
			this._statusImage.TabIndex = 17;
			this._statusImage.UseVisualStyleBackColor = true;
			//
			// _lookingForUsbTimer
			//
			this._lookingForUsbTimer.Interval = 500;
			this._lookingForUsbTimer.Tick += new System.EventHandler(this._lookingForUsbTimer_Tick);
			//
			// _statusLabel
			//
			this._statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._statusLabel.BackColor = System.Drawing.SystemColors.Control;
			this._statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._statusLabel.Location = new System.Drawing.Point(65, 7);
			this._statusLabel.Multiline = true;
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.ReadOnly = true;
			this._statusLabel.Size = new System.Drawing.Size(228, 205);
			this._statusLabel.TabIndex = 18;
			this._statusLabel.Text = "Status text";
			//
			// _logBox
			//
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this._logBox.CancelRequested = false;
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(78, 29);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(231, 150);
			this._logBox.TabIndex = 19;
			//
			// GetCloneFromUsbDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(306, 262);
			this.Controls.Add(this._logBox);
			this.Controls.Add(this._statusImage);
			this.Controls.Add(this._copyToComputerButton);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this._statusLabel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(322, 300);
			this.Name = "GetCloneFromUsbDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Get Project From USB Drive";
			this.Load += new System.EventHandler(this.GetCloneFromUsbDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button _copyToComputerButton;
		private System.Windows.Forms.ImageList _statusImages;
		private System.Windows.Forms.Button _statusImage;
		private System.Windows.Forms.Timer _lookingForUsbTimer;
		private System.Windows.Forms.TextBox _statusLabel;
		private LogBox _logBox;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;

	}
}