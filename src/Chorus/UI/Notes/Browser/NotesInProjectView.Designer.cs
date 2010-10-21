using Chorus.UI.Review.RevisionsInRepository;

namespace Chorus.UI.Notes.Browser
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
			this.label = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.date = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._stateImageList = new System.Windows.Forms.ImageList(this.components);
			this._refreshTimer = new System.Windows.Forms.Timer(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.searchBox1 = new Chorus.UI.Notes.Browser.SearchBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.showClosedNotesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showClosedNotesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			//
			// _messageListView
			//
			this._messageListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._messageListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.label,
			this.author,
			this.date});
			this._messageListView.FullRowSelect = true;
			this._messageListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._messageListView.HideSelection = false;
			this._messageListView.Location = new System.Drawing.Point(6, 27);
			this._messageListView.MultiSelect = false;
			this._messageListView.Name = "_messageListView";
			this._messageListView.ShowItemToolTips = true;
			this._messageListView.Size = new System.Drawing.Size(462, 321);
			this._messageListView.TabIndex = 2;
			this._messageListView.UseCompatibleStateImageBehavior = false;
			this._messageListView.View = System.Windows.Forms.View.Details;
			this._messageListView.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
			//
			// label
			//
			this.label.Text = "Label";
			this.label.Width = 118;
			//
			// author
			//
			this.author.Text = "Author";
			this.author.Width = 110;
			//
			// date
			//
			this.date.Text = "Date";
			this.date.Width = 86;
			//
			// _stateImageList
			//
			this._stateImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_stateImageList.ImageStream")));
			this._stateImageList.TransparentColor = System.Drawing.Color.Transparent;
			this._stateImageList.Images.SetKeyName(0, "check16x16.png");
			//
			// _refreshTimer
			//
			this._refreshTimer.Enabled = true;
			this._refreshTimer.Interval = 1000;
			this._refreshTimer.Tick += new System.EventHandler(this.OnRefreshTimer_Tick);
			//
			// searchBox1
			//
			this.searchBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox1.BackColor = System.Drawing.Color.White;
			this.searchBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.searchBox1.Location = new System.Drawing.Point(295, 3);
			this.searchBox1.Name = "searchBox1";
			this.searchBox1.Size = new System.Drawing.Size(175, 20);
			this.searchBox1.TabIndex = 3;
			this.searchBox1.SearchTextChanged += new System.EventHandler(this.searchBox1_SearchTextChanged);
			//
			// contextMenuStrip1
			//
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.showClosedNotesToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(177, 26);
			this.contextMenuStrip1.Text = "Filter";
			//
			// showClosedNotesToolStripMenuItem
			//
			this.showClosedNotesToolStripMenuItem.Name = "showClosedNotesToolStripMenuItem";
			this.showClosedNotesToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
			this.showClosedNotesToolStripMenuItem.Text = "Show Closed Notes";
			//
			// menuStrip1
			//
			this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.filterToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(4, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(30, 24);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			//
			// filterToolStripMenuItem
			//
			this.filterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.showClosedNotesToolStripMenuItem1});
			this.filterToolStripMenuItem.Image = global::Chorus.Properties.Resources.Filter;
			this.filterToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
			this.filterToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
			//
			// showClosedNotesToolStripMenuItem1
			//
			this.showClosedNotesToolStripMenuItem1.CheckOnClick = true;
			this.showClosedNotesToolStripMenuItem1.Name = "showClosedNotesToolStripMenuItem1";
			this.showClosedNotesToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
			this.showClosedNotesToolStripMenuItem1.Text = "Show Closed Notes";
			this.showClosedNotesToolStripMenuItem1.Click += new System.EventHandler(this.showClosedNotesToolStripMenuItem1_Click);
			//
			// NotesInProjectView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.searchBox1);
			this.Controls.Add(this._messageListView);
			this.Name = "NotesInProjectView";
			this.Size = new System.Drawing.Size(470, 348);
			this.Load += new System.EventHandler(this.OnLoad);
			this.VisibleChanged += new System.EventHandler(this.NotesInProjectView_VisibleChanged);
			this.contextMenuStrip1.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView _messageListView;
		private System.Windows.Forms.ColumnHeader date;
		private System.Windows.Forms.ColumnHeader label;
		private System.Windows.Forms.Timer _refreshTimer;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ColumnHeader author;
		private SearchBox searchBox1;
		private System.Windows.Forms.ImageList _stateImageList;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem showClosedNotesToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showClosedNotesToolStripMenuItem1;

	}
}