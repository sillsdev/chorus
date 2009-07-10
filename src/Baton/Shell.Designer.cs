using Baton.Settings;
using Chorus.UI;

namespace Baton
{
	partial class Shell
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Shell));
			this._tabControl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.OpenRepositoryButton = new System.Windows.Forms.ToolStripButton();
			this._tabControl.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			//
			// _tabControl
			//
			this._tabControl.Controls.Add(this.tabPage1);
			this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tabControl.Location = new System.Drawing.Point(0, 25);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(856, 495);
			this._tabControl.TabIndex = 0;
			//
			// tabPage1
			//
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(848, 469);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "dummy";
			this.tabPage1.UseVisualStyleBackColor = true;
			//
			// toolStrip1
			//
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.OpenRepositoryButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(856, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			//
			// OpenRepositoryButton
			//
			this.OpenRepositoryButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.OpenRepositoryButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenRepositoryButton.Image")));
			this.OpenRepositoryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.OpenRepositoryButton.Name = "OpenRepositoryButton";
			this.OpenRepositoryButton.Size = new System.Drawing.Size(23, 22);
			this.OpenRepositoryButton.Text = "Open Different Repository...";
			this.OpenRepositoryButton.ToolTipText = "Open Different Repository...";
			this.OpenRepositoryButton.Click += new System.EventHandler(this.OpenRepositoryButton_Click);
			//
			// Shell
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(856, 520);
			this.Controls.Add(this._tabControl);
			this.Controls.Add(this.toolStrip1);
			this.Name = "Shell";
			this.ShowIcon = false;
			this.Text = "Chorus";
			this._tabControl.ResumeLayout(false);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl _tabControl;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton OpenRepositoryButton;

	}
}