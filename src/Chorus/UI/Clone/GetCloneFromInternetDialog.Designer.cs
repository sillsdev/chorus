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
			this._showVerboseLog = new System.Windows.Forms.CheckBox();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._cancelTaskButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
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
			this._okButton.Location = new System.Drawing.Point(127, 227);
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
			this._progressLog.Size = new System.Drawing.Size(278, 120);
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
			this._statusLabel.Location = new System.Drawing.Point(65, 7);
			this._statusLabel.Multiline = true;
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.ReadOnly = true;
			this._statusLabel.Size = new System.Drawing.Size(228, 65);
			this._statusLabel.TabIndex = 18;
			this._statusLabel.Text = "Status text";
			//
			// _showVerboseLog
			//
			this._showVerboseLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._showVerboseLog.AutoSize = true;
			this._showVerboseLog.Location = new System.Drawing.Point(15, 204);
			this._showVerboseLog.Name = "_showVerboseLog";
			this._showVerboseLog.Size = new System.Drawing.Size(158, 17);
			this._showVerboseLog.TabIndex = 20;
			this._showVerboseLog.Text = "Show diagnostic information";
			this._showVerboseLog.UseVisualStyleBackColor = true;
			//
			// _progressBar
			//
			this._progressBar.Location = new System.Drawing.Point(15, 49);
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(278, 13);
			this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this._progressBar.TabIndex = 21;
			//
			// _cancelTaskButton
			//
			this._cancelTaskButton.Location = new System.Drawing.Point(219, 12);
			this._cancelTaskButton.Name = "_cancelTaskButton";
			this._cancelTaskButton.Size = new System.Drawing.Size(75, 23);
			this._cancelTaskButton.TabIndex = 23;
			this._cancelTaskButton.Text = "Cancel";
			this._cancelTaskButton.UseVisualStyleBackColor = true;
			this._cancelTaskButton.Click += new System.EventHandler(this.button2_Click);
			//
			// GetCloneFromInternetDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(306, 262);
			this.Controls.Add(this._cancelTaskButton);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._showVerboseLog);
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
		private System.Windows.Forms.CheckBox _showVerboseLog;
		private System.Windows.Forms.ProgressBar _progressBar;
		private System.Windows.Forms.Button _cancelTaskButton;
	}
}