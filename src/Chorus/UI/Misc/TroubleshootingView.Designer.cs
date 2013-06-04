namespace Chorus.UI.Misc
{
	partial class TroubleshootingView
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
			this._runDiagnosticsButton = new System.Windows.Forms.Button();
			this._copyLink = new System.Windows.Forms.LinkLabel();
			this._outputBox = new System.Windows.Forms.RichTextBox();
			this._statusLabel = new System.Windows.Forms.Label();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _runDiagnosticsButton
			//
			this._runDiagnosticsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._runDiagnosticsButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._runDiagnosticsButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._runDiagnosticsButton, "TroubleshootingView.RunDiagnosticsButton");
			this._runDiagnosticsButton.Location = new System.Drawing.Point(371, 22);
			this._runDiagnosticsButton.Name = "_runDiagnosticsButton";
			this._runDiagnosticsButton.Size = new System.Drawing.Size(102, 23);
			this._runDiagnosticsButton.TabIndex = 1;
			this._runDiagnosticsButton.Text = "Run Diagnostics";
			this._runDiagnosticsButton.UseVisualStyleBackColor = true;
			this._runDiagnosticsButton.Click += new System.EventHandler(this._runDiagnosticsButton_Click);
			//
			// _copyLink
			//
			this._copyLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._copyLink.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._copyLink, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._copyLink, null);
			this.l10NSharpExtender1.SetLocalizingId(this._copyLink, "TroubleshootingView.CopyToClipboard");
			this._copyLink.Location = new System.Drawing.Point(12, 412);
			this._copyLink.Name = "_copyLink";
			this._copyLink.Size = new System.Drawing.Size(88, 13);
			this._copyLink.TabIndex = 2;
			this._copyLink.TabStop = true;
			this._copyLink.Text = "copy to clipboard";
			this._copyLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._copyLink_LinkClicked);
			//
			// _outputBox
			//
			this._outputBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._outputBox.Location = new System.Drawing.Point(15, 70);
			this._outputBox.Name = "_outputBox";
			this._outputBox.ReadOnly = true;
			this._outputBox.Size = new System.Drawing.Size(458, 319);
			this._outputBox.TabIndex = 3;
			this._outputBox.TabStop = false;
			this._outputBox.Text = "";
			//
			// _statusLabel
			//
			this._statusLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._statusLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._statusLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._statusLabel, "TroubleshootingView.TroubleshootingView._statusLabel");
			this._statusLabel.Location = new System.Drawing.Point(12, 22);
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.Size = new System.Drawing.Size(35, 13);
			this._statusLabel.TabIndex = 4;
			this._statusLabel.Text = "label1";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "TroubleshootingView";
			//
			// TroubleshootingView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._statusLabel);
			this.Controls.Add(this._outputBox);
			this.Controls.Add(this._copyLink);
			this.Controls.Add(this._runDiagnosticsButton);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "TroubleshootingView.TroubleshootingView.TroubleshootingView");
			this.Name = "TroubleshootingView";
			this.Size = new System.Drawing.Size(489, 444);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _runDiagnosticsButton;
		private System.Windows.Forms.LinkLabel _copyLink;
		private System.Windows.Forms.RichTextBox _outputBox;
		private System.Windows.Forms.Label _statusLabel;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
