namespace Baton.HistoryPanel.ChangedRecordControl
{
	partial class ChangedRecordView
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
			this._changeDescriptionRenderer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._changeDescriptionRenderer.Location = new System.Drawing.Point(0, 0);
			this._changeDescriptionRenderer.MinimumSize = new System.Drawing.Size(20, 20);
			this._changeDescriptionRenderer.Name = "_changeDescriptionRenderer";
			this._changeDescriptionRenderer.Size = new System.Drawing.Size(150, 150);
			this._changeDescriptionRenderer.TabIndex = 0;
			//
			// ChangedRecordView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._changeDescriptionRenderer);
			this.Name = "ChangedRecordView";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser _changeDescriptionRenderer;

	}
}
