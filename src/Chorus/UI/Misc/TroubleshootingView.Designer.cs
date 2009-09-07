namespace Chorus.UI.Misc
{
	partial class TroubleShootingView
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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this._runDiagnosticsButton = new System.Windows.Forms.Button();
			this._emailLink = new System.Windows.Forms.LinkLabel();
			this._copyLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// textBox1
			//
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.Location = new System.Drawing.Point(15, 61);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(458, 336);
			this.textBox1.TabIndex = 0;
			//
			// _runDiagnosticsButton
			//
			this._runDiagnosticsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._runDiagnosticsButton.Location = new System.Drawing.Point(295, 22);
			this._runDiagnosticsButton.Name = "_runDiagnosticsButton";
			this._runDiagnosticsButton.Size = new System.Drawing.Size(178, 23);
			this._runDiagnosticsButton.TabIndex = 1;
			this._runDiagnosticsButton.Text = "Run Diagnostics";
			this._runDiagnosticsButton.UseVisualStyleBackColor = true;
			//
			// _emailLink
			//
			this._emailLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._emailLink.AutoSize = true;
			this._emailLink.Location = new System.Drawing.Point(12, 414);
			this._emailLink.Name = "_emailLink";
			this._emailLink.Size = new System.Drawing.Size(89, 13);
			this._emailLink.TabIndex = 2;
			this._emailLink.TabStop = true;
			this._emailLink.Text = "email to someone";
			//
			// _copyLink
			//
			this._copyLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._copyLink.AutoSize = true;
			this._copyLink.Location = new System.Drawing.Point(198, 414);
			this._copyLink.Name = "_copyLink";
			this._copyLink.Size = new System.Drawing.Size(88, 13);
			this._copyLink.TabIndex = 2;
			this._copyLink.TabStop = true;
			this._copyLink.Text = "copy to clipboard";
			//
			// DiagnosticsPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._copyLink);
			this.Controls.Add(this._emailLink);
			this.Controls.Add(this._runDiagnosticsButton);
			this.Controls.Add(this.textBox1);
			this.Name = "DiagnosticsPanel";
			this.Size = new System.Drawing.Size(489, 444);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button _runDiagnosticsButton;
		private System.Windows.Forms.LinkLabel _emailLink;
		private System.Windows.Forms.LinkLabel _copyLink;
	}
}
