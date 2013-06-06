namespace Chorus.notes
{
	partial class ConflictDetailsForm
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
			this._okButton = new System.Windows.Forms.Button();
			this._conflictDisplay = new System.Windows.Forms.WebBrowser();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._okButton, "Common.OK");
			this._okButton.Location = new System.Drawing.Point(851, 533);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 0;
			this._okButton.Text = "OK";
			this._okButton.UseVisualStyleBackColor = true;
			//
			// _conflictDisplay
			//
			this._conflictDisplay.AllowWebBrowserDrop = false;
			this._conflictDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._conflictDisplay, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._conflictDisplay, null);
			this.l10NSharpExtender1.SetLocalizingId(this._conflictDisplay, "ConflictDetailsForm.ConflictDetailsForm._conflictDisplay");
			this._conflictDisplay.Location = new System.Drawing.Point(0, 27);
			this._conflictDisplay.MinimumSize = new System.Drawing.Size(20, 20);
			this._conflictDisplay.Name = "_conflictDisplay";
			this._conflictDisplay.Size = new System.Drawing.Size(948, 487);
			this._conflictDisplay.TabIndex = 10;
			this._conflictDisplay.WebBrowserShortcutsEnabled = false;
			//
			// menuStrip1
			//
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.editToolStripMenuItem});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.menuStrip1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.menuStrip1, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this.menuStrip1, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this.menuStrip1, "ConflictDetailsForm.ConflictDetailsForm.menuStrip1");
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(950, 24);
			this.menuStrip1.TabIndex = 11;
			this.menuStrip1.Text = "menuStrip1";
			//
			// editToolStripMenuItem
			//
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.copyToolStripMenuItem});
			this.l10NSharpExtender1.SetLocalizableToolTip(this.editToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.editToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.editToolStripMenuItem, "ConflictDetailsForm.EditMenu");
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			//
			// copyToolStripMenuItem
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.copyToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.copyToolStripMenuItem, null);
			this.l10NSharpExtender1.SetLocalizingId(this.copyToolStripMenuItem, "ConflictDetailsForm.CopyMenuItem");
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "ConflictDetailsForm";
			//
			// ConflictDetailsForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(950, 568);
			this.Controls.Add(this._conflictDisplay);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.menuStrip1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "ConflictDetailsForm.WindowTitle");
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ConflictDetailsForm";
			this.ShowIcon = false;
			this.Text = "Conflict Details";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.WebBrowser _conflictDisplay;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}