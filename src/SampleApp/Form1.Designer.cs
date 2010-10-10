namespace SampleApp
{
	partial class Form1
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this._frontPage = new System.Windows.Forms.TabPage();
			this._notesPage = new System.Windows.Forms.TabPage();
			this._historyPage = new System.Windows.Forms.TabPage();
			this.tabControl1.SuspendLayout();
			this.SuspendLayout();
			//
			// tabControl1
			//
			this.tabControl1.Controls.Add(this._frontPage);
			this.tabControl1.Controls.Add(this._notesPage);
			this.tabControl1.Controls.Add(this._historyPage);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(466, 357);
			this.tabControl1.TabIndex = 0;
			//
			// _frontPage
			//
			this._frontPage.Location = new System.Drawing.Point(4, 22);
			this._frontPage.Name = "_frontPage";
			this._frontPage.Padding = new System.Windows.Forms.Padding(3);
			this._frontPage.Size = new System.Drawing.Size(458, 331);
			this._frontPage.TabIndex = 0;
			this._frontPage.Text = "Data";
			this._frontPage.UseVisualStyleBackColor = true;
			//
			// _notesPage
			//
			this._notesPage.Location = new System.Drawing.Point(4, 22);
			this._notesPage.Name = "_notesPage";
			this._notesPage.Padding = new System.Windows.Forms.Padding(3);
			this._notesPage.Size = new System.Drawing.Size(458, 331);
			this._notesPage.TabIndex = 1;
			this._notesPage.Text = "Notes";
			this._notesPage.UseVisualStyleBackColor = true;
			//
			// _historyPage
			//
			this._historyPage.Location = new System.Drawing.Point(4, 22);
			this._historyPage.Name = "_historyPage";
			this._historyPage.Padding = new System.Windows.Forms.Padding(3);
			this._historyPage.Size = new System.Drawing.Size(458, 331);
			this._historyPage.TabIndex = 2;
			this._historyPage.Text = "History";
			this._historyPage.UseVisualStyleBackColor = true;
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(466, 357);
			this.Controls.Add(this.tabControl1);
			this.Name = "Form1";
			this.Text = "Chorus Sample App";
			this.tabControl1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage _frontPage;
		private System.Windows.Forms.TabPage _notesPage;
		private System.Windows.Forms.TabPage _historyPage;
	}
}
