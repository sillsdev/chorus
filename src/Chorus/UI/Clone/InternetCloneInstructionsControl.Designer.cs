namespace Chorus.UI.Clone
{
	partial class InternetCloneInstructionsControl
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._urlBox = new System.Windows.Forms.TextBox();
			this._localFolderName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._downloadButton = new System.Windows.Forms.Button();
			this._targetInfoLabel = new System.Windows.Forms.TextBox();
			this._targetWarningImage = new System.Windows.Forms.PictureBox();
			this._sourcetWarningImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._sourcetWarningImage)).BeginInit();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(182, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Internet address of the project (URL):";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label2.Location = new System.Drawing.Point(48, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(235, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Example: http://hg-public.languagedepot.org/tpi";
			//
			// _urlBox
			//
			this._urlBox.Location = new System.Drawing.Point(48, 39);
			this._urlBox.Name = "_urlBox";
			this._urlBox.Size = new System.Drawing.Size(307, 20);
			this._urlBox.TabIndex = 0;
			this._urlBox.Text = "http://";
			this._urlBox.TextChanged += new System.EventHandler(this._urlBox_TextChanged);
			//
			// _localFolderName
			//
			this._localFolderName.Location = new System.Drawing.Point(51, 122);
			this._localFolderName.Name = "_localFolderName";
			this._localFolderName.Size = new System.Drawing.Size(144, 20);
			this._localFolderName.TabIndex = 1;
			this._localFolderName.TextChanged += new System.EventHandler(this.OnLocalNameChanged);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 101);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(248, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Name for the project when saved on this computer:";
			//
			// _downloadButton
			//
			this._downloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._downloadButton.Location = new System.Drawing.Point(16, 239);
			this._downloadButton.Name = "_downloadButton";
			this._downloadButton.Size = new System.Drawing.Size(75, 23);
			this._downloadButton.TabIndex = 2;
			this._downloadButton.Text = "&Download";
			this._downloadButton.UseVisualStyleBackColor = true;
			this._downloadButton.Click += new System.EventHandler(this._downloadButton_Click);
			//
			// _targetInfoLabel
			//
			this._targetInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._targetInfoLabel.BackColor = System.Drawing.SystemColors.Control;
			this._targetInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._targetInfoLabel.Location = new System.Drawing.Point(51, 148);
			this._targetInfoLabel.Multiline = true;
			this._targetInfoLabel.Name = "_targetInfoLabel";
			this._targetInfoLabel.ReadOnly = true;
			this._targetInfoLabel.Size = new System.Drawing.Size(304, 85);
			this._targetInfoLabel.TabIndex = 19;
			this._targetInfoLabel.Text = "Info about target";
			//
			// _targetWarningImage
			//
			this._targetWarningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._targetWarningImage.Location = new System.Drawing.Point(16, 117);
			this._targetWarningImage.Name = "_targetWarningImage";
			this._targetWarningImage.Size = new System.Drawing.Size(26, 33);
			this._targetWarningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._targetWarningImage.TabIndex = 20;
			this._targetWarningImage.TabStop = false;
			this._targetWarningImage.Visible = false;
			//
			// _sourcetWarningImage
			//
			this._sourcetWarningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._sourcetWarningImage.Location = new System.Drawing.Point(16, 35);
			this._sourcetWarningImage.Name = "_sourcetWarningImage";
			this._sourcetWarningImage.Size = new System.Drawing.Size(26, 31);
			this._sourcetWarningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._sourcetWarningImage.TabIndex = 21;
			this._sourcetWarningImage.TabStop = false;
			this._sourcetWarningImage.Visible = false;
			//
			// InternetCloneInstructionsControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._downloadButton);
			this.Controls.Add(this._sourcetWarningImage);
			this.Controls.Add(this._targetWarningImage);
			this.Controls.Add(this._targetInfoLabel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._localFolderName);
			this.Controls.Add(this._urlBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "InternetCloneInstructionsControl";
			this.Size = new System.Drawing.Size(370, 278);
			this.Load += new System.EventHandler(this.AccountInfo_Load);
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._sourcetWarningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _urlBox;
		private System.Windows.Forms.TextBox _localFolderName;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.Button _downloadButton;
		private System.Windows.Forms.TextBox _targetInfoLabel;
		private System.Windows.Forms.PictureBox _targetWarningImage;
		private System.Windows.Forms.PictureBox _sourcetWarningImage;
	}
}
