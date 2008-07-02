namespace Chorus.UI
{
	partial class SyncPanel
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
			this._syncButton = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.label1 = new System.Windows.Forms.Label();
			this._syncTargets = new System.Windows.Forms.CheckedListBox();
			this.label2 = new System.Windows.Forms.Label();
			this._logBox = new System.Windows.Forms.RichTextBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			//
			// _syncButton
			//
			this._syncButton.Location = new System.Drawing.Point(407, 18);
			this._syncButton.Name = "_syncButton";
			this._syncButton.Size = new System.Drawing.Size(75, 23);
			this._syncButton.TabIndex = 0;
			this._syncButton.Text = "Sync Now";
			this._syncButton.UseVisualStyleBackColor = true;
			this._syncButton.Click += new System.EventHandler(this.syncButton_Click);
			//
			// splitContainer1
			//
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(3, 56);
			this.splitContainer1.Name = "splitContainer1";
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this.label1);
			this.splitContainer1.Panel1.Controls.Add(this._syncTargets);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this.label2);
			this.splitContainer1.Panel2.Controls.Add(this._logBox);
			this.splitContainer1.Size = new System.Drawing.Size(485, 270);
			this.splitContainer1.SplitterDistance = 104;
			this.splitContainer1.TabIndex = 1;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(4, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(86, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Try to Sync With";
			//
			// _syncTargets
			//
			this._syncTargets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._syncTargets.FormattingEnabled = true;
			this._syncTargets.Items.AddRange(new object[] {
			"USB Drive"});
			this._syncTargets.Location = new System.Drawing.Point(0, 32);
			this._syncTargets.MinimumSize = new System.Drawing.Size(105, 79);
			this._syncTargets.Name = "_syncTargets";
			this._syncTargets.Size = new System.Drawing.Size(105, 79);
			this._syncTargets.TabIndex = 1;
			this._syncTargets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._syncTargets_ItemCheck);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(25, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Log";
			//
			// _logBox
			//
			this._logBox.Location = new System.Drawing.Point(0, 33);
			this._logBox.Name = "_logBox";
			this._logBox.Size = new System.Drawing.Size(371, 255);
			this._logBox.TabIndex = 0;
			this._logBox.Text = "";
			//
			// SyncPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this._syncButton);
			this.Name = "SyncPanel";
			this.Size = new System.Drawing.Size(491, 329);
			this.Load += new System.EventHandler(this.SyncPanel_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _syncButton;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.RichTextBox _logBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckedListBox _syncTargets;
		private System.Windows.Forms.Label label2;
	}
}
