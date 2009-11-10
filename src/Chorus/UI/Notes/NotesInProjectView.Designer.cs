using Chorus.UI.Review.RevisionsInRepository;

namespace Chorus.UI.Notes
{
	partial class NotesInProjectView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotesInProjectView));
			this._messageListView = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			//
			// _messageListView
			//
			this._messageListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader4,
			this.columnHeader1,
			this.columnHeader2});
			this._messageListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._messageListView.FullRowSelect = true;
			this._messageListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._messageListView.HideSelection = false;
			this._messageListView.Location = new System.Drawing.Point(0, 0);
			this._messageListView.MultiSelect = false;
			this._messageListView.Name = "_messageListView";
			this._messageListView.ShowItemToolTips = true;
			this._messageListView.Size = new System.Drawing.Size(470, 348);
			this._messageListView.SmallImageList = this.imageList1;
			this._messageListView.TabIndex = 2;
			this._messageListView.UseCompatibleStateImageBehavior = false;
			this._messageListView.View = System.Windows.Forms.View.Details;
			this._messageListView.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
			//
			// columnHeader4
			//
			this.columnHeader4.Text = "Class";
			//
			// columnHeader1
			//
			this.columnHeader1.Text = "Date";
			this.columnHeader1.Width = 124;
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Label";
			this.columnHeader2.Width = 118;
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
			//
			// NotesInProjectView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._messageListView);
			this.Name = "NotesInProjectView";
			this.Size = new System.Drawing.Size(470, 348);
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _messageListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ColumnHeader columnHeader4;

	}
}