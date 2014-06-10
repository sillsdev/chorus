using Palaso.UI.WindowsForms.HtmlBrowser;

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
			if (disposing)
			{
				if (webBrowser1 != null)
					webBrowser1.Dispose();
				if (components != null)
					components.Dispose();
			}
			webBrowser1 = null;
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
			this.webBrowser1 = new XWebBrowser();
			this._pathLabel = new System.Windows.Forms.Label();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// webBrowser1
			//
			this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this.webBrowser1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.webBrowser1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.webBrowser1, "AnnotationInspector.AnnotationInspector.webBrowser1");
			this.webBrowser1.Location = new System.Drawing.Point(1, 42);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size(463, 348);
			this.webBrowser1.TabIndex = 0;
			//
			// _pathLabel
			//
			this._pathLabel.AutoSize = true;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._pathLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._pathLabel, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._pathLabel, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._pathLabel, "AnnotationInspector.AnnotationInspector._pathLabel");
			this._pathLabel.Location = new System.Drawing.Point(13, 13);
			this._pathLabel.Name = "_pathLabel";
			this._pathLabel.Size = new System.Drawing.Size(35, 13);
			this._pathLabel.TabIndex = 1;
			this._pathLabel.Text = "label1";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "AnnotationInspector";
			//
			// AnnotationInspector
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(467, 402);
			this.Controls.Add(this._pathLabel);
			this.Controls.Add(this.webBrowser1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this, "AnnotationInspector.WindowTitle");
			this.Name = "AnnotationInspector";
			this.Text = "AnnotationInspector";
			this.Load += new System.EventHandler(this.AnnotationInspector_Load);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private XWebBrowser webBrowser1;
		private System.Windows.Forms.Label _pathLabel;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
