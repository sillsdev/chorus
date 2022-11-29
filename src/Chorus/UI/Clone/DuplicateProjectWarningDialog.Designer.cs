namespace Chorus.UI.Clone
{
	partial class DuplicateProjectWarningDialog
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
			this._mainLabel = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonHelp = new System.Windows.Forms.Button();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _mainLabel
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this._mainLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._mainLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._mainLabel, "DuplicateProjectWarningDialog.DuplicateProjectWarningDialog._mainLabel");
			this._mainLabel.Location = new System.Drawing.Point(12, 9);
			this._mainLabel.Name = "_mainLabel";
			this._mainLabel.Size = new System.Drawing.Size(510, 55);
			this._mainLabel.TabIndex = 0;
			this._mainLabel.Text = "Warning";
			//
			// buttonOK
			//
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.buttonOK, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.buttonOK, null);
			this.l10NSharpExtender1.SetLocalizingId(this.buttonOK, "Common.OK");
			this.buttonOK.Location = new System.Drawing.Point(327, 80);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			//
			// buttonHelp
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.buttonHelp, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.buttonHelp, null);
			this.l10NSharpExtender1.SetLocalizingId(this.buttonHelp, "Common.Help");
			this.buttonHelp.Location = new System.Drawing.Point(440, 80);
			this.buttonHelp.Name = "buttonHelp";
			this.buttonHelp.Size = new System.Drawing.Size(82, 23);
			this.buttonHelp.TabIndex = 2;
			this.buttonHelp.Text = "&Help";
			this.buttonHelp.UseVisualStyleBackColor = true;
			this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "DuplicateProjectWarningDialog";
			//
			// DuplicateProjectWarningDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(540, 117);
			this.ControlBox = false;
			this.Controls.Add(this.buttonHelp);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this._mainLabel);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "DuplicateProjectWarningDialog.WindowTitle");
			this.MinimizeBox = false;
			this.Name = "DuplicateProjectWarningDialog";
			this.Text = "Duplicate Project--Get operation cancelled";
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label _mainLabel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonHelp;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}