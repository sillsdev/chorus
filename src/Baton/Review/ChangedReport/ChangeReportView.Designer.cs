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
			this._changeDescriptionRenderer = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			//
			// _changeDescriptionRenderer
			//
			this._changeDescriptionRenderer.AllowWebBrowserDrop = false;
			this._changeDescriptionRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._changeDescriptionRenderer.Location = new System.Drawing.Point(0, 0);
			this._changeDescriptionRenderer.MinimumSize = new System.Drawing.Size(20, 20);
			this._changeDescriptionRenderer.Name = "_changeDescriptionRenderer";
			this._changeDescriptionRenderer.Size = new System.Drawing.Size(150, 150);
			this._changeDescriptionRenderer.TabIndex = 0;
			this._changeDescriptionRenderer.WebBrowserShortcutsEnabled = false;
			//
			// ChangeReportView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._changeDescriptionRenderer);
			this.Name = "ChangeReportView";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser _changeDescriptionRenderer;

	}
}