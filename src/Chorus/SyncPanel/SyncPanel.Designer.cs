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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncPanel));
			this._syncButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this._logBox = new System.Windows.Forms.RichTextBox();
			this._syncTargets = new System.Windows.Forms.CheckedListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.SuspendLayout();
			//
			// _syncButton
			//
			this._syncButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._syncButton.Image = ((System.Drawing.Image)(resources.GetObject("_syncButton.Image")));
			this._syncButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._syncButton.Location = new System.Drawing.Point(330, 21);
			this._syncButton.Name = "_syncButton";
			this._syncButton.Size = new System.Drawing.Size(108, 31);
			this._syncButton.TabIndex = 0;
			this._syncButton.Text = "Send/Recieve";
			this._syncButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._syncButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._syncButton.UseVisualStyleBackColor = true;
			this._syncButton.Click += new System.EventHandler(this.syncButton_Click);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(21, 21);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(229, 20);
			this.label3.TabIndex = 3;
			this.label3.Text = "Send/Recieve Project Changes";
			//
			// _logBox
			//
			this._logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._logBox.Location = new System.Drawing.Point(72, 209);
			this._logBox.Name = "_logBox";
			this._logBox.Size = new System.Drawing.Size(366, 180);
			this._logBox.TabIndex = 4;
			this._logBox.Text = "";
			//
			// _syncTargets
			//
			this._syncTargets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._syncTargets.FormattingEnabled = true;
			this._syncTargets.Items.AddRange(new object[] {
			"USB Drive"});
			this._syncTargets.Location = new System.Drawing.Point(72, 95);
			this._syncTargets.MinimumSize = new System.Drawing.Size(105, 79);
			this._syncTargets.Name = "_syncTargets";
			this._syncTargets.Size = new System.Drawing.Size(366, 79);
			this._syncTargets.TabIndex = 5;
			this._syncTargets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._syncTargets_ItemCheck);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(69, 73);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(321, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Attempt to Send/Receive with these people, devices, and servers:";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(69, 189);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(25, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Log";
			//
			// pictureBox2
			//
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(25, 73);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(40, 36);
			this.pictureBox2.TabIndex = 8;
			this.pictureBox2.TabStop = false;
			//
			// timer1
			//
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			//
			// SyncPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._syncTargets);
			this.Controls.Add(this._logBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._syncButton);
			this.Name = "SyncPanel";
			this.Size = new System.Drawing.Size(464, 454);
			this.Load += new System.EventHandler(this.SyncPanel_Load);
			this.VisibleChanged += new System.EventHandler(this.SyncPanel_VisibleChanged);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _syncButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.RichTextBox _logBox;
		private System.Windows.Forms.CheckedListBox _syncTargets;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Timer timer1;
	}
}
