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
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
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
			this._conflictDisplay.Location = new System.Drawing.Point(0, 2);
			this._conflictDisplay.MinimumSize = new System.Drawing.Size(20, 20);
			this._conflictDisplay.Name = "_conflictDisplay";
			this._conflictDisplay.Size = new System.Drawing.Size(948, 512);
			this._conflictDisplay.TabIndex = 10;
			this._conflictDisplay.WebBrowserShortcutsEnabled = false;
			//
			// ConflictDetailsForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(950, 568);
			this.Controls.Add(this._conflictDisplay);
			this.Controls.Add(this._okButton);
			this.Name = "ConflictDetailsForm";
			this.ShowIcon = false;
			this.Text = "Conflict Details";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.WebBrowser _conflictDisplay;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
	}
}