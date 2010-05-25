namespace Chorus.UI.Clone
{
	partial class TargetFolderControl
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
			this._localFolderName = new System.Windows.Forms.TextBox();
			this._targetWarningImage = new System.Windows.Forms.PictureBox();
			this._localFolderLabel = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._downloadButton = new System.Windows.Forms.Button();
			this._targetInfoLabel = new Chorus.UI.BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).BeginInit();
			this.SuspendLayout();
			//
			// _localFolderName
			//
			this._localFolderName.Location = new System.Drawing.Point(108, 35);
			this._localFolderName.Name = "_localFolderName";
			this._localFolderName.Size = new System.Drawing.Size(166, 20);
			this._localFolderName.TabIndex = 3;
			this.toolTip1.SetToolTip(this._localFolderName, "What to call this project");
			this._localFolderName.TextChanged += new System.EventHandler(this._localName_TextChanged);
			//
			// _targetWarningImage
			//
			this._targetWarningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._targetWarningImage.Location = new System.Drawing.Point(280, 29);
			this._targetWarningImage.Name = "_targetWarningImage";
			this._targetWarningImage.Size = new System.Drawing.Size(26, 33);
			this._targetWarningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._targetWarningImage.TabIndex = 20;
			this._targetWarningImage.TabStop = false;
			this._targetWarningImage.Visible = false;
			//
			// _localFolderLabel
			//
			this._localFolderLabel.AutoSize = true;
			this._localFolderLabel.Location = new System.Drawing.Point(26, 13);
			this._localFolderLabel.Name = "_localFolderLabel";
			this._localFolderLabel.Size = new System.Drawing.Size(182, 13);
			this._localFolderLabel.TabIndex = 26;
			this._localFolderLabel.Text = "Name for the folder on your computer";
			//
			// _downloadButton
			//
			this._downloadButton.Location = new System.Drawing.Point(3, 13);
			this._downloadButton.Name = "_downloadButton";
			this._downloadButton.Size = new System.Drawing.Size(94, 23);
			this._downloadButton.TabIndex = 4;
			this._downloadButton.Text = "&Download";
			this._downloadButton.UseVisualStyleBackColor = true;
			//
			// _targetInfoLabel
			//
			this._targetInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._targetInfoLabel.BackColor = System.Drawing.SystemColors.Control;
			this._targetInfoLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._targetInfoLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._targetInfoLabel.ForeColor = System.Drawing.Color.DimGray;
			this._targetInfoLabel.Location = new System.Drawing.Point(28, 61);
			this._targetInfoLabel.Multiline = true;
			this._targetInfoLabel.Name = "_targetInfoLabel";
			this._targetInfoLabel.ReadOnly = true;
			this._targetInfoLabel.Size = new System.Drawing.Size(370, 54);
			this._targetInfoLabel.TabIndex = 25;
			this._targetInfoLabel.TabStop = false;
			this._targetInfoLabel.Text = "runtime info";
			//
			// TargetFolderControl
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this._downloadButton);
			this.Controls.Add(this._localFolderLabel);
			this.Controls.Add(this._targetInfoLabel);
			this.Controls.Add(this._targetWarningImage);
			this.Controls.Add(this._localFolderName);
			this.MinimumSize = new System.Drawing.Size(430, 167);
			this.Name = "TargetFolderControl";
			this.Size = new System.Drawing.Size(430, 167);
			this.Load += new System.EventHandler(this.InternetCloneInstructionsControl_Load);
			((System.ComponentModel.ISupportInitialize)(this._targetWarningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _localFolderName;
		private System.Windows.Forms.PictureBox _targetWarningImage;
		private BetterLabel _targetInfoLabel;
		private System.Windows.Forms.Label _localFolderLabel;
		private System.Windows.Forms.ToolTip toolTip1;
		public System.Windows.Forms.Button _downloadButton;
	}
}
