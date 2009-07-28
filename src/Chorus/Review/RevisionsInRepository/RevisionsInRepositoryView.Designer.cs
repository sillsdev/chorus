namespace Baton.Review.RevisionsInRepository
{
	partial class RevisionsInRepositoryView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionsInRepositoryView));
			this._loadButton = new System.Windows.Forms.Button();
			this._historyList = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label3 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			//
			// _loadButton
			//
			this._loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._loadButton.FlatAppearance.BorderSize = 0;
			this._loadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._loadButton.Image = ((System.Drawing.Image)(resources.GetObject("_loadButton.Image")));
			this._loadButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._loadButton.Location = new System.Drawing.Point(400, 1);
			this._loadButton.Name = "_loadButton";
			this._loadButton.Size = new System.Drawing.Size(65, 29);
			this._loadButton.TabIndex = 1;
			this.toolTip1.SetToolTip(this._loadButton, "Reload the history");
			this._loadButton.UseVisualStyleBackColor = true;
			this._loadButton.Click += new System.EventHandler(this._loadButton_Click);
			//
			// _historyList
			//
			this._historyList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._historyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader1,
			this.columnHeader2,
			this.columnHeader3});
			this._historyList.FullRowSelect = true;
			this._historyList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._historyList.HideSelection = false;
			this._historyList.Location = new System.Drawing.Point(3, 32);
			this._historyList.MultiSelect = false;
			this._historyList.Name = "_historyList";
			this._historyList.Size = new System.Drawing.Size(464, 297);
			this._historyList.SmallImageList = this.imageList1;
			this._historyList.TabIndex = 2;
			this._historyList.UseCompatibleStateImageBehavior = false;
			this._historyList.View = System.Windows.Forms.View.Details;
			this._historyList.SelectedIndexChanged += new System.EventHandler(this._historyList_SelectedIndexChanged);
			//
			// columnHeader1
			//
			this.columnHeader1.Text = "Date";
			this.columnHeader1.Width = 95;
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Person";
			this.columnHeader2.Width = 88;
			//
			// columnHeader3
			//
			this.columnHeader3.Text = "Action";
			this.columnHeader3.Width = 300;
			//
			// imageList1
			//
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Merge");
			this.imageList1.Images.SetKeyName(1, "WeSay");
			this.imageList1.Images.SetKeyName(2, "WeSay Configuration Tool");
			this.imageList1.Images.SetKeyName(3, "Warning");
			//
			// timer1
			//
			this.timer1.Interval = 2000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(3, 5);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(181, 20);
			this.label3.TabIndex = 4;
			this.label3.Text = "Review Project Changes";
			//
			// RevisionsInRepositoryView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.label3);
			this.Controls.Add(this._historyList);
			this.Controls.Add(this._loadButton);
			this.Name = "RevisionsInRepositoryView";
			this.Size = new System.Drawing.Size(470, 348);
			this.VisibleChanged += new System.EventHandler(this.HistoryPanel_VisibleChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _loadButton;
		private System.Windows.Forms.ListView _historyList;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ImageList imageList1;

	}
}