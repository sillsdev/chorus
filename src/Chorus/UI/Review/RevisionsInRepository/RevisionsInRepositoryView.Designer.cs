namespace Chorus.UI.Review.RevisionsInRepository
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
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label3 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._historyGrid = new System.Windows.Forms.DataGridView();
			this.ColumnImage = new System.Windows.Forms.DataGridViewImageColumn();
			this.ColumnParentRevision = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnChildRevision = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnPerson = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this._historyGrid)).BeginInit();
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
			this._loadButton.Click += new System.EventHandler(this.OnRefresh);
			//
			// imageList1
			//
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "Merge");
			this.imageList1.Images.SetKeyName(1, "WeSay");
			this.imageList1.Images.SetKeyName(2, "WeSay Configuration Tool");
			this.imageList1.Images.SetKeyName(3, "Warning");
			this.imageList1.Images.SetKeyName(4, "chorus");
			//
			// timer1
			//
			this.timer1.Interval = 500;
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
			// _historyGrid
			//
			this._historyGrid.AllowUserToAddRows = false;
			this._historyGrid.AllowUserToDeleteRows = false;
			this._historyGrid.AllowUserToResizeRows = false;
			this._historyGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this._historyGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this._historyGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.ColumnImage,
			this.ColumnParentRevision,
			this.ColumnChildRevision,
			this.ColumnDate,
			this.ColumnPerson,
			this.ColumnAction});
			this._historyGrid.Location = new System.Drawing.Point(3, 32);
			this._historyGrid.Name = "_historyGrid";
			this._historyGrid.RowHeadersVisible = false;
			this._historyGrid.Size = new System.Drawing.Size(464, 297);
			this._historyGrid.TabIndex = 5;
			this._historyGrid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this._historyGrid_CellClick);
			//
			// ColumnImage
			//
			this.ColumnImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.ColumnImage.HeaderText = "";
			this.ColumnImage.Name = "ColumnImage";
			this.ColumnImage.Width = 5;
			//
			// ColumnParentRevision
			//
			this.ColumnParentRevision.FillWeight = 35F;
			this.ColumnParentRevision.HeaderText = "Old";
			this.ColumnParentRevision.Name = "ColumnParentRevision";
			this.ColumnParentRevision.ToolTipText = "Older (parent) revision to compare";
			//
			// ColumnChildRevision
			//
			this.ColumnChildRevision.FillWeight = 35F;
			this.ColumnChildRevision.HeaderText = "New";
			this.ColumnChildRevision.Name = "ColumnChildRevision";
			this.ColumnChildRevision.ToolTipText = "Newer (child) revision to compare";
			//
			// ColumnDate
			//
			this.ColumnDate.HeaderText = "Date";
			this.ColumnDate.Name = "ColumnDate";
			//
			// ColumnPerson
			//
			this.ColumnPerson.HeaderText = "Person";
			this.ColumnPerson.Name = "ColumnPerson";
			//
			// ColumnAction
			//
			this.ColumnAction.HeaderText = "Action";
			this.ColumnAction.Name = "ColumnAction";
			//
			// RevisionsInRepositoryView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._historyGrid);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._loadButton);
			this.Name = "RevisionsInRepositoryView";
			this.Size = new System.Drawing.Size(470, 348);
			this.Load += new System.EventHandler(this.StartRefreshTimer);
			this.VisibleChanged += new System.EventHandler(this.StartRefreshTimer);
			((System.ComponentModel.ISupportInitialize)(this._historyGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _loadButton;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.DataGridView _historyGrid;
		private System.Windows.Forms.DataGridViewImageColumn ColumnImage;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnParentRevision;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnChildRevision;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDate;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPerson;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAction;

	}
}