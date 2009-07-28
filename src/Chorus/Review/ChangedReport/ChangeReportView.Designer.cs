namespace Baton.Review.ChangedReport
{
	partial class ChangeReportView
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
			this._normalChangeDescriptionRenderer = new System.Windows.Forms.WebBrowser();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPageRaw = new System.Windows.Forms.TabPage();
			this._rawChangeDescriptionRenderer = new System.Windows.Forms.WebBrowser();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPageRaw.SuspendLayout();
			this.SuspendLayout();
			//
			// _normalChangeDescriptionRenderer
			//
			this._normalChangeDescriptionRenderer.AllowWebBrowserDrop = false;
			this._normalChangeDescriptionRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._normalChangeDescriptionRenderer.Location = new System.Drawing.Point(3, 3);
			this._normalChangeDescriptionRenderer.MinimumSize = new System.Drawing.Size(20, 20);
			this._normalChangeDescriptionRenderer.Name = "_normalChangeDescriptionRenderer";
			this._normalChangeDescriptionRenderer.Size = new System.Drawing.Size(136, 118);
			this._normalChangeDescriptionRenderer.TabIndex = 0;
			this._normalChangeDescriptionRenderer.WebBrowserShortcutsEnabled = false;
			this._normalChangeDescriptionRenderer.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this._normalChangeDescriptionRenderer_Navigating);
			//
			// tabControl1
			//
			this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPageRaw);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(150, 150);
			this.tabControl1.TabIndex = 1;
			//
			// tabPage1
			//
			this.tabPage1.Controls.Add(this._normalChangeDescriptionRenderer);
			this.tabPage1.Location = new System.Drawing.Point(4, 4);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(142, 124);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Normal";
			this.tabPage1.UseVisualStyleBackColor = true;
			//
			// tabPageRaw
			//
			this.tabPageRaw.Controls.Add(this._rawChangeDescriptionRenderer);
			this.tabPageRaw.Location = new System.Drawing.Point(4, 4);
			this.tabPageRaw.Name = "tabPageRaw";
			this.tabPageRaw.Size = new System.Drawing.Size(142, 124);
			this.tabPageRaw.TabIndex = 1;
			this.tabPageRaw.Text = "Raw";
			this.tabPageRaw.UseVisualStyleBackColor = true;
			//
			// _rawChangeDescriptionRenderer
			//
			this._rawChangeDescriptionRenderer.AllowWebBrowserDrop = false;
			this._rawChangeDescriptionRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._rawChangeDescriptionRenderer.Location = new System.Drawing.Point(0, 0);
			this._rawChangeDescriptionRenderer.MinimumSize = new System.Drawing.Size(20, 20);
			this._rawChangeDescriptionRenderer.Name = "_rawChangeDescriptionRenderer";
			this._rawChangeDescriptionRenderer.Size = new System.Drawing.Size(142, 124);
			this._rawChangeDescriptionRenderer.TabIndex = 1;
			this._rawChangeDescriptionRenderer.WebBrowserShortcutsEnabled = false;
			//
			// ChangeReportView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabControl1);
			this.Name = "ChangeReportView";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPageRaw.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser _normalChangeDescriptionRenderer;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPageRaw;
		private System.Windows.Forms.WebBrowser _rawChangeDescriptionRenderer;

	}
}