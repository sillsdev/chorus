namespace Chorus.UI.Clone
{
	partial class GetCloneFromNetworkFolderDlg
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
				updateProgress.Stop();
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.panel = new System.Windows.Forms.Panel();
			this.chooseRepositoryLabel = new System.Windows.Forms.Label();
			this.projectRepositoryListView = new System.Windows.Forms.ListView();
			this.projectHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.dateHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lookInLabel = new System.Windows.Forms.Label();
			this.folderBrowserControl = new Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.statusLabel = new System.Windows.Forms.Label();
			this.helpButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.getButton = new System.Windows.Forms.Button();
			this.updateProgress = new System.Windows.Forms.Timer(this.components);
			this.panel.SuspendLayout();
			this.SuspendLayout();
			//
			// panel
			//
			this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel.Controls.Add(this.chooseRepositoryLabel);
			this.panel.Controls.Add(this.projectRepositoryListView);
			this.panel.Controls.Add(this.lookInLabel);
			this.panel.Controls.Add(this.folderBrowserControl);
			this.panel.Location = new System.Drawing.Point(12, 12);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(540, 289);
			this.panel.TabIndex = 0;
			this.panel.Resize += new System.EventHandler(this.PanelResize);
			//
			// chooseRepositoryLabel
			//
			this.chooseRepositoryLabel.Location = new System.Drawing.Point(277, 0);
			this.chooseRepositoryLabel.Name = "chooseRepositoryLabel";
			this.chooseRepositoryLabel.Size = new System.Drawing.Size(260, 14);
			this.chooseRepositoryLabel.TabIndex = 7;
			this.chooseRepositoryLabel.Text = "Choose a Project Repository:";
			//
			// projectRepositoryListView
			//
			this.projectRepositoryListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.projectRepositoryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.projectHeader,
			this.dateHeader});
			this.projectRepositoryListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.projectRepositoryListView.FullRowSelect = true;
			this.projectRepositoryListView.HideSelection = false;
			this.projectRepositoryListView.Location = new System.Drawing.Point(277, 17);
			this.projectRepositoryListView.MultiSelect = false;
			this.projectRepositoryListView.Name = "projectRepositoryListView";
			this.projectRepositoryListView.ShowItemToolTips = true;
			this.projectRepositoryListView.Size = new System.Drawing.Size(263, 269);
			this.projectRepositoryListView.TabIndex = 1;
			this.projectRepositoryListView.UseCompatibleStateImageBehavior = false;
			this.projectRepositoryListView.View = System.Windows.Forms.View.Details;
			this.projectRepositoryListView.SelectedIndexChanged += new System.EventHandler(this.OnRepositoryListViewSelectionChange);
			this.projectRepositoryListView.DoubleClick += new System.EventHandler(this.OnRepositoryListViewDoubleClick);
			//
			// projectHeader
			//
			this.projectHeader.Text = "Project";
			this.projectHeader.Width = 115;
			//
			// dateHeader
			//
			this.dateHeader.Text = "Modified Date";
			this.dateHeader.Width = 144;
			//
			// lookInLabel
			//
			this.lookInLabel.Location = new System.Drawing.Point(0, 0);
			this.lookInLabel.Name = "lookInLabel";
			this.lookInLabel.Size = new System.Drawing.Size(271, 14);
			this.lookInLabel.TabIndex = 6;
			this.lookInLabel.Text = "Look in:";
			//
			// folderBrowserControl
			//
			this.folderBrowserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.folderBrowserControl.BackColor = System.Drawing.Color.White;
			this.folderBrowserControl.Location = new System.Drawing.Point(0, 17);
			this.folderBrowserControl.Name = "folderBrowserControl";
			this.folderBrowserControl.SelectedPath = "";
			this.folderBrowserControl.ShowAddressbar = true;
			this.folderBrowserControl.ShowGoButton = false;
			this.folderBrowserControl.ShowMyDocuments = true;
			this.folderBrowserControl.ShowMyFavorites = true;
			this.folderBrowserControl.ShowMyNetwork = true;
			this.folderBrowserControl.ShowOnlyMappedDrives = true;
			this.folderBrowserControl.ShowToolbar = false;
			this.folderBrowserControl.Size = new System.Drawing.Size(271, 269);
			this.folderBrowserControl.TabIndex = 0;
			this.folderBrowserControl.PathChanged += new Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl.PathChangedEventHandler(this.FolderBrowserControlPathChanged);
			//
			// progressBar
			//
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(12, 323);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(540, 14);
			this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar.TabIndex = 1;
			//
			// statusLabel
			//
			this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.statusLabel.AutoEllipsis = true;
			this.statusLabel.Location = new System.Drawing.Point(12, 304);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(540, 14);
			this.statusLabel.TabIndex = 2;
			this.statusLabel.Text = "No folder selected";
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			// helpButton
			//
			this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.helpButton.Location = new System.Drawing.Point(477, 347);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new System.Drawing.Size(75, 23);
			this.helpButton.TabIndex = 3;
			this.helpButton.Text = "Help";
			this.helpButton.UseVisualStyleBackColor = true;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(396, 347);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// getButton
			//
			this.getButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.getButton.Enabled = false;
			this.getButton.Location = new System.Drawing.Point(315, 347);
			this.getButton.Name = "getButton";
			this.getButton.Size = new System.Drawing.Size(75, 23);
			this.getButton.TabIndex = 5;
			this.getButton.Text = "Get";
			this.getButton.UseVisualStyleBackColor = true;
			this.getButton.Click += new System.EventHandler(this.OnGetButtonClick);
			//
			// updateProgress
			//
			this.updateProgress.Enabled = true;
			this.updateProgress.Tick += new System.EventHandler(this.OnProgressTick);
			//
			// GetCloneFromNetworkFolderDlg
			//
			this.AcceptButton = this.getButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(564, 382);
			this.Controls.Add(this.getButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.helpButton);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.panel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(350, 250);
			this.Name = "GetCloneFromNetworkFolderDlg";
			this.ShowIcon = false;
			this.Text = "Get Project from Shared Network Folder";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel;
		private Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl folderBrowserControl;
		private System.Windows.Forms.ListView projectRepositoryListView;
		private System.Windows.Forms.ColumnHeader projectHeader;
		private System.Windows.Forms.ColumnHeader dateHeader;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Button helpButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button getButton;
		private System.Windows.Forms.Label lookInLabel;
		private System.Windows.Forms.Label chooseRepositoryLabel;
		private System.Windows.Forms.Timer updateProgress;
	}
}