namespace Chorus.UI.Misc
{
	partial class LogBox
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
			this._box = new System.Windows.Forms.RichTextBox();
			this._showDetails = new System.Windows.Forms.CheckBox();
			this._copyToClipboardLink = new System.Windows.Forms.LinkLabel();
			this._verboseBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			//
			// _box
			//
			this._box.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._box.BackColor = System.Drawing.SystemColors.Window;
			this._box.Location = new System.Drawing.Point(3, 3);
			this._box.Name = "_box";
			this._box.ReadOnly = true;
			this._box.Size = new System.Drawing.Size(225, 99);
			this._box.TabIndex = 0;
			this._box.TabStop = false;
			this._box.Text = "";
			//
			// _showDetails
			//
			this._showDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._showDetails.AutoSize = true;
			this._showDetails.Location = new System.Drawing.Point(0, 130);
			this._showDetails.Name = "_showDetails";
			this._showDetails.Size = new System.Drawing.Size(88, 17);
			this._showDetails.TabIndex = 1;
			this._showDetails.Text = "Show Details";
			this._showDetails.UseVisualStyleBackColor = true;
			this._showDetails.CheckedChanged += new System.EventHandler(this._showDetails_CheckedChanged);
			//
			// _copyToClipboardLink
			//
			this._copyToClipboardLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._copyToClipboardLink.AutoSize = true;
			this._copyToClipboardLink.Location = new System.Drawing.Point(122, 130);
			this._copyToClipboardLink.Name = "_copyToClipboardLink";
			this._copyToClipboardLink.Size = new System.Drawing.Size(89, 13);
			this._copyToClipboardLink.TabIndex = 2;
			this._copyToClipboardLink.TabStop = true;
			this._copyToClipboardLink.Text = "Copy to clipboard";
			this._copyToClipboardLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._copyToClipboardLink_LinkClicked);
			//
			// _verboseBox
			//
			this._verboseBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._verboseBox.Location = new System.Drawing.Point(3, 3);
			this._verboseBox.Name = "_verboseBox";
			this._verboseBox.Size = new System.Drawing.Size(225, 99);
			this._verboseBox.TabIndex = 3;
			this._verboseBox.TabStop = false;
			this._verboseBox.Text = "";
			this._verboseBox.Visible = false;
			//
			// LogBox
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this._verboseBox);
			this.Controls.Add(this._copyToClipboardLink);
			this.Controls.Add(this._showDetails);
			this.Controls.Add(this._box);
			this.Name = "LogBox";
			this.Size = new System.Drawing.Size(231, 150);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox _box;
		private System.Windows.Forms.CheckBox _showDetails;
		private System.Windows.Forms.LinkLabel _copyToClipboardLink;
		private System.Windows.Forms.RichTextBox _verboseBox;
	}
}
