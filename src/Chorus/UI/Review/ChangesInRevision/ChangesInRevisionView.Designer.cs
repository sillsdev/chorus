namespace Chorus.UI.Review.ChangesInRevision
{
	partial class ChangesInRevisionView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangesInRevisionView));
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// listView1
			//
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader3,
			this.columnHeader1,
			this.columnHeader2});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(327, 150);
			this.listView1.SmallImageList = this.imageList1;
			this.listView1.StateImageList = this.imageList1;
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			//
			// columnHeader3
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.columnHeader3, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.columnHeader3, null);
			this.l10NSharpExtender1.SetLocalizingId(this.columnHeader3, "ChangesInRevisionView.Type");
			this.columnHeader3.Text = "Type";
			this.columnHeader3.Width = 111;
			//
			// columnHeader1
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.columnHeader1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.columnHeader1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.columnHeader1, "ChangesInRevisionView.Item");
			this.columnHeader1.Text = "Item";
			this.columnHeader1.Width = 98;
			//
			// columnHeader2
			//
			this.l10NSharpExtender1.SetLocalizableToolTip(this.columnHeader2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.columnHeader2, null);
			this.l10NSharpExtender1.SetLocalizingId(this.columnHeader2, "ChangesInRevisionView.Action");
			this.columnHeader2.Text = "Action";
			this.columnHeader2.Width = 112;
			//
			// imageList1
			//
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "image");
			this.imageList1.Images.SetKeyName(1, "sound");
			this.imageList1.Images.SetKeyName(2, "file");
			this.imageList1.Images.SetKeyName(3, "WesayConfig");
			this.imageList1.Images.SetKeyName(4, "WeSay");
			this.imageList1.Images.SetKeyName(5, "mergeConflict");
			this.imageList1.Images.SetKeyName(6, "error");
			this.imageList1.Images.SetKeyName(7, "question");
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "ChangesInRevisionView";
			//
			// ChangesInRevisionView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listView1);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "ChangesInRevisionView.ChangesInRevisionView.ChangesInRevisionView");
			this.Name = "ChangesInRevisionView";
			this.Size = new System.Drawing.Size(327, 150);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ImageList imageList1;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;

	}
}