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
			this._warningImage = new System.Windows.Forms.PictureBox();
			this._showSettingsLink = new System.Windows.Forms.LinkLabel();
			this.betterLabel1 = new Chorus.UI.BetterLabel();
			this._chorusReadinessMessage = new Chorus.UI.BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).BeginInit();
			this.SuspendLayout();
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
			// _showSettingsLink
			//
			this._showSettingsLink.AutoSize = true;
			this._showSettingsLink.Location = new System.Drawing.Point(43, 66);
			this._showSettingsLink.Name = "_showSettingsLink";
			this._showSettingsLink.Size = new System.Drawing.Size(161, 13);
			this._showSettingsLink.TabIndex = 12;
			this._showSettingsLink.TabStop = true;
			this._showSettingsLink.Text = "Send/Receive Server Settings...";
			this._showSettingsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._showSettingsLink_LinkClicked);
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BackColor = System.Drawing.SystemColors.Control;
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(43, 3);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(229, 22);
			this.betterLabel1.TabIndex = 13;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Send/Receive Server Readiness";
			//
			// _chorusReadinessMessage
			//
			this._chorusReadinessMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._chorusReadinessMessage.BackColor = System.Drawing.SystemColors.Control;
			this._chorusReadinessMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._chorusReadinessMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._chorusReadinessMessage.Location = new System.Drawing.Point(41, 22);
			this._chorusReadinessMessage.Multiline = true;
			this._chorusReadinessMessage.Name = "_chorusReadinessMessage";
			this._chorusReadinessMessage.ReadOnly = true;
			this._chorusReadinessMessage.Size = new System.Drawing.Size(384, 41);
			this._chorusReadinessMessage.TabIndex = 11;
			this._chorusReadinessMessage.TabStop = false;
			this._chorusReadinessMessage.Text = "Chorus Readiness Message Will Go Here";
			//
			// ReadinessPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.betterLabel1);
			this.Controls.Add(this._showSettingsLink);
			this.Controls.Add(this._chorusReadinessMessage);
			this.Controls.Add(this._warningImage);
			this.Name = "ReadinessPanel";
			this.Size = new System.Drawing.Size(439, 89);
			this.Load += new System.EventHandler(this.ReadinessPanel_Load);
			this.FontChanged += new System.EventHandler(this.ReadinessPanel_FontChanged);
			this.Resize += new System.EventHandler(this.ReadinessPanel_Resize);
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _warningImage;
		private BetterLabel _chorusReadinessMessage;
		private System.Windows.Forms.LinkLabel _showSettingsLink;
		private BetterLabel betterLabel1;
	}
}
