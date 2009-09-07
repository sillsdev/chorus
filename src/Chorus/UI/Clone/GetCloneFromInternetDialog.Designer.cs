namespace Chorus.UI.Clone
{
	partial class GetCloneFromInternetDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetCloneFromInternetDialog));
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._progressLog = new System.Windows.Forms.RichTextBox();
			this._statusImages = new System.Windows.Forms.ImageList(this.components);
			this._statusImage = new System.Windows.Forms.Button();
			this._statusLabel = new System.Windows.Forms.TextBox();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._cancelTaskButton = new System.Windows.Forms.Button();
			this._progressLogVerbose = new System.Windows.Forms.RichTextBox();
			this._showVerboseLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(306, 227);
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
			this._okButton.Location = new System.Drawing.Point(215, 227);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _progressLog
			//
			this._progressLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._progressLog.Location = new System.Drawing.Point(15, 78);
			this._progressLog.Name = "_progressLog";
			this._progressLog.Size = new System.Drawing.Size(366, 120);
			this._progressLog.TabIndex = 4;
			this._progressLog.Text = "";
			//
			// _statusImages
			//
			this._statusImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_statusImages.ImageStream")));
			this._statusImages.TransparentColor = System.Drawing.Color.Transparent;
			this._statusImages.Images.SetKeyName(0, "Success");
			this._statusImages.Images.SetKeyName(1, "Error");
			//
			// _statusImage
			//
			this._statusImage.FlatAppearance.BorderSize = 0;
			this._statusImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._statusImage.ImageIndex = 0;
			this._statusImage.ImageList = this._statusImages;
			this._statusImage.Location = new System.Drawing.Point(8, 7);
			this._statusImage.Name = "_statusImage";
			this._statusImage.Size = new System.Drawing.Size(50, 36);
			this._statusImage.TabIndex = 17;
			this._statusImage.UseVisualStyleBackColor = true;
			//
			// _statusLabel
			//
			this._statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._statusLabel.BackColor = System.Drawing.SystemColors.Control;
			this._statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._statusLabel.Location = new System.Drawing.Point(58, 12);
			this._statusLabel.Multiline = true;
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.ReadOnly = true;
			this._statusLabel.Size = new System.Drawing.Size(316, 60);
			this._statusLabel.TabIndex = 18;
			this._statusLabel.Text = "Status text";
			//
			// _progressBar
			//
			this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._progressBar.Location = new System.Drawing.Point(15, 48);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(287, 13);
			this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this._progressBar.TabIndex = 21;
			//
			// _cancelTaskButton
			//
			this._cancelTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelTaskButton.Location = new System.Drawing.Point(306, 43);
			this._cancelTaskButton.Name = "_cancelTaskButton";
			this._cancelTaskButton.Size = new System.Drawing.Size(75, 23);
			this._cancelTaskButton.TabIndex = 23;
			this._cancelTaskButton.Text = "Cancel";
			this._cancelTaskButton.UseVisualStyleBackColor = true;
			this._cancelTaskButton.Click += new System.EventHandler(this.button2_Click);
			//
			// _progressLogVerbose
			//
			this._progressLogVerbose.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._progressLogVerbose.Location = new System.Drawing.Point(26, 72);
			this._progressLogVerbose.Name = "_progressLogVerbose";
			this._progressLogVerbose.Size = new System.Drawing.Size(366, 120);
			this._progressLogVerbose.TabIndex = 24;
			this._progressLogVerbose.Text = "";
			//
			// _showVerboseLink
			//
			this._showVerboseLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._showVerboseLink.AutoSize = true;
			this._showVerboseLink.Location = new System.Drawing.Point(12, 227);
			this._showVerboseLink.Name = "_showVerboseLink";
			this._showVerboseLink.Size = new System.Drawing.Size(69, 13);
			this._showVerboseLink.TabIndex = 25;
			this._showVerboseLink.TabStop = true;
			this._showVerboseLink.Text = "Show Details";
			this._showVerboseLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._showVerboseLink_LinkClicked);
			//
			// GetCloneFromInternetDialog
			//
			this.AcceptButton = this._okButton;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(394, 262);
			this.Controls.Add(this._showVerboseLink);
			this.Controls.Add(this._progressLogVerbose);
			this.Controls.Add(this._cancelTaskButton);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._statusImage);
			this.Controls.Add(this._progressLog);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._statusLabel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(322, 300);
			this.Name = "GetCloneFromInternetDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Get Project From Internet";
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.RichTextBox _progressLog;
		private System.Windows.Forms.ImageList _statusImages;
		private System.Windows.Forms.Button _statusImage;
		private System.Windows.Forms.TextBox _statusLabel;
		private System.Windows.Forms.ProgressBar _progressBar;
		private System.Windows.Forms.Button _cancelTaskButton;
		private System.Windows.Forms.RichTextBox _progressLogVerbose;
		private System.Windows.Forms.LinkLabel _showVerboseLink;
	}
}