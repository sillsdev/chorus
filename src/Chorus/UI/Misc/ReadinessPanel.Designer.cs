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
			this._editServerInfoButton = new System.Windows.Forms.Button();
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
			// _editServerInfoButton
			//
			this._editServerInfoButton.Location = new System.Drawing.Point(46, 52);
			this._editServerInfoButton.Name = "_editServerInfoButton";
			this._editServerInfoButton.Size = new System.Drawing.Size(171, 22);
			this._editServerInfoButton.TabIndex = 10;
			this._editServerInfoButton.Text = "Send/Receive Server Settings...";
			this._editServerInfoButton.UseVisualStyleBackColor = true;
			this._editServerInfoButton.Click += new System.EventHandler(this._editServerInfoButton_Click);
			//
			// _chorusReadinessMessage
			//
			this._chorusReadinessMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._chorusReadinessMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._chorusReadinessMessage.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._chorusReadinessMessage.Location = new System.Drawing.Point(41, 3);
			this._chorusReadinessMessage.Multiline = true;
			this._chorusReadinessMessage.Name = "_chorusReadinessMessage";
			this._chorusReadinessMessage.ReadOnly = true;
			this._chorusReadinessMessage.Size = new System.Drawing.Size(377, 46);
			this._chorusReadinessMessage.TabIndex = 11;
			this._chorusReadinessMessage.TabStop = false;
			this._chorusReadinessMessage.Text = "Chorus Readiness Message Will Go Here";
			//
			// ReadinessPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._chorusReadinessMessage);
			this.Controls.Add(this._editServerInfoButton);
			this.Controls.Add(this._warningImage);
			this.Name = "ReadinessPanel";
			this.Size = new System.Drawing.Size(439, 90);
			this.Load += new System.EventHandler(this.ReadinessPanel_Load);
			this.Resize += new System.EventHandler(this.ReadinessPanel_Resize);
			((System.ComponentModel.ISupportInitialize)(this._warningImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _warningImage;
		private System.Windows.Forms.Button _editServerInfoButton;
		private BetterLabel _chorusReadinessMessage;
	}
}
