namespace Chorus.UI.Settings
{
	partial class SetupPanel
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.webBrowser1 = new SIL.Windows.Forms.HtmlBrowser.XWebBrowser();
			this.SuspendLayout();
			//
			// webBrowser1
			//
			this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowser1.Location = new System.Drawing.Point(0, 0);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size(522, 310);
			this.webBrowser1.TabIndex = 1;
			this.webBrowser1.AllowNavigation = false;
			this.webBrowser1.AllowWebBrowserDrop = false;
			this.webBrowser1.Url = new System.Uri("", System.UriKind.Relative);
			//
			// SetupPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.webBrowser1);
			this.Name = "SetupPanel";
			this.Size = new System.Drawing.Size(522, 310);
			this.Load += new System.EventHandler(this.SetupPanel_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private SIL.Windows.Forms.HtmlBrowser.XWebBrowser webBrowser1;
	}
}
