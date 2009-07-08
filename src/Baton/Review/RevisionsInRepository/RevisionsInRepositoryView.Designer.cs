namespace Baton.HistoryPanel
{
	partial class HistoryPanel
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
			this._loadButton = new System.Windows.Forms.Button();
			this._historyList = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			//
			// _loadButton
			//
			this._loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._loadButton.Location = new System.Drawing.Point(392, 3);
			this._loadButton.Name = "_loadButton";
			this._loadButton.Size = new System.Drawing.Size(75, 23);
			this._loadButton.TabIndex = 1;
			this._loadButton.Text = "Get History";
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
			this._historyList.HideSelection = false;
			this._historyList.Location = new System.Drawing.Point(3, 35);
			this._historyList.MultiSelect = false;
			this._historyList.Name = "_historyList";
			this._historyList.Size = new System.Drawing.Size(464, 324);
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
			// HistoryPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._historyList);
			this.Controls.Add(this._loadButton);
			this.Name = "HistoryPanel";
			this.Size = new System.Drawing.Size(470, 362);
			this.VisibleChanged += new System.EventHandler(this.HistoryPanel_VisibleChanged);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _loadButton;
		private System.Windows.Forms.ListView _historyList;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;

	}
}