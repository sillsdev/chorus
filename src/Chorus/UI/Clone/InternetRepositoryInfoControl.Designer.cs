namespace Chorus.UI.Clone
{
	partial class InternetRepositoryInfoControl
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
			this._warningImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).BeginInit();
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
			this.label2.Location = new System.Drawing.Point(70, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(204, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Example: hg-public.languagedepot.org/tpi";
			//
			// _urlBox
			//
			this._urlBox.Location = new System.Drawing.Point(73, 39);
			this._urlBox.Name = "_urlBox";
			this._urlBox.Size = new System.Drawing.Size(219, 20);
			this._urlBox.TabIndex = 0;
			this._urlBox.TextChanged += new System.EventHandler(this._urlBox_TextChanged);
			//
			// _localFolderName
			//
			this._localFolderName.Location = new System.Drawing.Point(73, 122);
			this._localFolderName.Name = "_localFolderName";
			this._localFolderName.Size = new System.Drawing.Size(219, 20);
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
			//
			// _targetInfoLabel
			//
			this._targetInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._targetInfoLabel.BackColor = System.Drawing.SystemColors.Control;
			this._targetInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._targetInfoLabel.Location = new System.Drawing.Point(48, 148);
			this._targetInfoLabel.Multiline = true;
			this._targetInfoLabel.Name = "_targetInfoLabel";
			this._targetInfoLabel.ReadOnly = true;
			this._targetInfoLabel.Size = new System.Drawing.Size(304, 85);
			this._targetInfoLabel.TabIndex = 19;
			this._targetInfoLabel.Text = "Info about target";
			//
			// _warningImage
			//
			this._warningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._warningImage.Location = new System.Drawing.Point(16, 148);
			this._warningImage.Name = "_warningImage";
			this._warningImage.Size = new System.Drawing.Size(26, 31);
			this._warningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._warningImage.TabIndex = 20;
			this._warningImage.TabStop = false;
			//
			// InternetRepositoryInfoControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._warningImage);
			this.Controls.Add(this._targetInfoLabel);
			this.Controls.Add(this._downloadButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._localFolderName);
			this.Controls.Add(this._urlBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "InternetRepositoryInfoControl";
			this.Size = new System.Drawing.Size(370, 278);
			this.Load += new System.EventHandler(this.AccountInfo_Load);
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).EndInit();
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
		private System.Windows.Forms.PictureBox _warningImage;
	}
}
