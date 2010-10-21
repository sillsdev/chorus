namespace Chorus.UI.Notes.Bar
{
	partial class NotesBarView
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
			this._buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			//
			// _buttonsPanel
			//
			this._buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._buttonsPanel.Location = new System.Drawing.Point(0, 0);
			this._buttonsPanel.Name = "_buttonsPanel";
			this._buttonsPanel.Size = new System.Drawing.Size(226, 49);
			this._buttonsPanel.TabIndex = 0;
			//
			// NotesBarView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._buttonsPanel);
			this.Name = "NotesBarView";
			this.Size = new System.Drawing.Size(226, 49);
			this.Load += new System.EventHandler(this.NotesBarView_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel _buttonsPanel;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}
