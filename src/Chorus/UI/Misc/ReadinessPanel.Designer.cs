namespace Chorus.UI.Misc
{
	partial class ReadinessPanel
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
			this._chorusGetHgLink = new System.Windows.Forms.LinkLabel();
			this._chorusGetTortoiseLink = new System.Windows.Forms.LinkLabel();
			this._chorusReadinessMessage = new System.Windows.Forms.Label();
			this._warningImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).BeginInit();
			this.SuspendLayout();
			//
			// _chorusGetHgLink
			//
			this._chorusGetHgLink.AutoSize = true;
			this._chorusGetHgLink.LinkArea = new System.Windows.Forms.LinkArea(4, 28);
			this._chorusGetHgLink.Location = new System.Drawing.Point(3, 56);
			this._chorusGetHgLink.Name = "_chorusGetHgLink";
			this._chorusGetHgLink.Size = new System.Drawing.Size(145, 17);
			this._chorusGetHgLink.TabIndex = 7;
			this._chorusGetHgLink.TabStop = true;
			this._chorusGetHgLink.Text = "Or, get Mercurial alone here";
			this._chorusGetHgLink.UseCompatibleTextRendering = true;
			this._chorusGetHgLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGetMercurialClicked);
			//
			// _chorusGetTortoiseLink
			//
			this._chorusGetTortoiseLink.AutoSize = true;
			this._chorusGetTortoiseLink.Location = new System.Drawing.Point(3, 41);
			this._chorusGetTortoiseLink.Name = "_chorusGetTortoiseLink";
			this._chorusGetTortoiseLink.Size = new System.Drawing.Size(222, 13);
			this._chorusGetTortoiseLink.TabIndex = 6;
			this._chorusGetTortoiseLink.TabStop = true;
			this._chorusGetTortoiseLink.Text = "Get TortoiseHg GUI package at SourceForge";
			this._chorusGetTortoiseLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGetTortoiseHgClicked);
			//
			// _chorusReadinessMessage
			//
			this._chorusReadinessMessage.AutoSize = true;
			this._chorusReadinessMessage.ForeColor = System.Drawing.Color.Black;
			this._chorusReadinessMessage.Location = new System.Drawing.Point(56, 0);
			this._chorusReadinessMessage.MaximumSize = new System.Drawing.Size(500, 0);
			this._chorusReadinessMessage.Name = "_chorusReadinessMessage";
			this._chorusReadinessMessage.Size = new System.Drawing.Size(141, 13);
			this._chorusReadinessMessage.TabIndex = 8;
			this._chorusReadinessMessage.Text = "Chorus Message Goes here.";
			//
			// _warningImage
			//
			this._warningImage.Image = global::Chorus.Properties.Resources.warningImage;
			this._warningImage.Location = new System.Drawing.Point(6, 3);
			this._warningImage.Name = "_warningImage";
			this._warningImage.Size = new System.Drawing.Size(29, 35);
			this._warningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._warningImage.TabIndex = 9;
			this._warningImage.TabStop = false;
			//
			// ReadinessPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._warningImage);
			this.Controls.Add(this._chorusReadinessMessage);
			this.Controls.Add(this._chorusGetHgLink);
			this.Controls.Add(this._chorusGetTortoiseLink);
			this.Name = "ReadinessPanel";
			this.Size = new System.Drawing.Size(439, 90);
			this.Resize += new System.EventHandler(this.ReadinessPanel_Resize);
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel _chorusGetHgLink;
		private System.Windows.Forms.LinkLabel _chorusGetTortoiseLink;
		private System.Windows.Forms.Label _chorusReadinessMessage;
		private System.Windows.Forms.PictureBox _warningImage;
	}
}
