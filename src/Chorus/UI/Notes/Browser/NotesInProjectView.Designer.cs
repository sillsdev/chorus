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
			this.label = new System.Windows.Forms.ColumnHeader();
			this.author = new System.Windows.Forms.ColumnHeader();
			this.date = new System.Windows.Forms.ColumnHeader();
			this._stateImageList = new System.Windows.Forms.ImageList(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.searchBox1 = new Chorus.UI.Notes.Browser.SearchBox();
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
			this._messageListView.Location = new System.Drawing.Point(0, 27);
			this._messageListView.MultiSelect = false;
			this._messageListView.Name = "_messageListView";
			this._messageListView.ShowItemToolTips = true;
			this._messageListView.Size = new System.Drawing.Size(470, 321);
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
			// timer1
			//
			this.timer1.Interval = 500;
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
			// NotesInProjectView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.searchBox1);
			this.Controls.Add(this._messageListView);
			this.Name = "NotesInProjectView";
			this.Size = new System.Drawing.Size(470, 348);
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _messageListView;
		private System.Windows.Forms.ColumnHeader date;
		private System.Windows.Forms.ColumnHeader label;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ColumnHeader author;
		private SearchBox searchBox1;
		private System.Windows.Forms.ImageList _stateImageList;

	}
}