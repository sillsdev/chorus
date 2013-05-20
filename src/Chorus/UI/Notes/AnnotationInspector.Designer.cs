namespace Chorus.UI.Notes
{
	partial class AnnotationInspector
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
#if MONO
			this.webBrowser1 = new Gecko.GeckoWebBrowser();
#else
			this.webBrowser1 = new System.Windows.Forms.WebBrowser();
#endif
			this._pathLabel = new System.Windows.Forms.Label();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.SuspendLayout();
			//
			// webBrowser1
			//
			this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.webBrowser1.Location = new System.Drawing.Point(1, 42);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size(463, 348);
			this.webBrowser1.TabIndex = 0;
			//
			// _pathLabel
			//
			this._pathLabel.AutoSize = true;
			this._pathLabel.Location = new System.Drawing.Point(13, 13);
			this._pathLabel.Name = "_pathLabel";
			this._pathLabel.Size = new System.Drawing.Size(35, 13);
			this._pathLabel.TabIndex = 1;
			this._pathLabel.Text = "label1";
			//
			// AnnotationInspector
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(467, 402);
			this.Controls.Add(this._pathLabel);
			this.Controls.Add(this.webBrowser1);
			this.Name = "AnnotationInspector";
			this.Text = "AnnotationInspector";
			this.Load += new System.EventHandler(this.AnnotationInspector_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

#if MONO
		private Gecko.GeckoWebBrowser webBrowser1;
#else
		private System.Windows.Forms.WebBrowser webBrowser1;
#endif
		private System.Windows.Forms.Label _pathLabel;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
	}
}
