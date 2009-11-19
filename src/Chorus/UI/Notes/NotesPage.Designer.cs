namespace Chorus.UI.Notes
{
	partial class NotesPage
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
			this.searchBox1 = new Chorus.UI.Notes.SearchBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.label3 = new System.Windows.Forms.Label();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			//
			// searchBox1
			//
			this.searchBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox1.BackColor = System.Drawing.Color.White;
			this.searchBox1.Location = new System.Drawing.Point(471, 12);
			this.searchBox1.Name = "searchBox1";
			this.searchBox1.Size = new System.Drawing.Size(226, 27);
			this.searchBox1.TabIndex = 0;
			//
			// splitContainer1
			//
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(15, 57);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Size = new System.Drawing.Size(682, 198);
			this.splitContainer1.SplitterDistance = 317;
			this.splitContainer1.TabIndex = 1;
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(11, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 20);
			this.label3.TabIndex = 5;
			this.label3.Text = "Project Notes";
			//
			// NotesPage
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.label3);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.searchBox1);
			this.Name = "NotesPage";
			this.Size = new System.Drawing.Size(715, 269);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SearchBox searchBox1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Label label3;
	}
}