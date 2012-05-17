using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.FolderBrowserControl;

namespace Chorus.UI.Clone
{
	partial class GetCloneFromNetworkFolderDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel = new Panel();
			this.projectRepositoryListView = new ListView();
			this.projectHeader = ((ColumnHeader)(new ColumnHeader()));
			this.dateHeader = ((ColumnHeader)(new ColumnHeader()));
			this.folderBrowserControl = new FolderBrowserControl();
			this.progressBar = new ProgressBar();
			this.statusLabel = new Label();
			this.helpButton = new Button();
			this.cancelButton = new Button();
			this.getButton = new Button();
			this.lookInLabel = new Label();
			this.chooseRepositoryLabel = new Label();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			//
			// panel
			//
			this.panel.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.panel.Controls.Add(this.chooseRepositoryLabel);
			this.panel.Controls.Add(this.projectRepositoryListView);
			this.panel.Controls.Add(this.lookInLabel);
			this.panel.Controls.Add(this.folderBrowserControl);
			this.panel.Location = new Point(12, 12);
			this.panel.Name = "panel";
			this.panel.Size = new Size(540, 289);
			this.panel.TabIndex = 0;
			this.panel.Resize += new EventHandler(this.PanelResize);
			//
			// projectRepositoryListView
			//
			this.projectRepositoryListView.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Right)));
			this.projectRepositoryListView.Columns.AddRange(new ColumnHeader[] {
			this.projectHeader,
			this.dateHeader});
			this.projectRepositoryListView.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			this.projectRepositoryListView.Location = new Point(277, 17);
			this.projectRepositoryListView.MultiSelect = false;
			this.projectRepositoryListView.Name = "projectRepositoryListView";
			this.projectRepositoryListView.ShowItemToolTips = true;
			this.projectRepositoryListView.Size = new Size(263, 269);
			this.projectRepositoryListView.TabIndex = 1;
			this.projectRepositoryListView.UseCompatibleStateImageBehavior = false;
			this.projectRepositoryListView.View = View.Details;
			//
			// projectHeader
			//
			this.projectHeader.Text = "Project";
			this.projectHeader.Width = 129;
			//
			// dateHeader
			//
			this.dateHeader.Text = "Modified Date";
			this.dateHeader.Width = 130;
			//
			// folderBrowserControl
			//
			this.folderBrowserControl.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
			| AnchorStyles.Left)));
			this.folderBrowserControl.BackColor = Color.White;
			this.folderBrowserControl.Location = new Point(0, 17);
			this.folderBrowserControl.Name = "folderBrowserControl";
			this.folderBrowserControl.SelectedPath = "C:\\";
			this.folderBrowserControl.ShowAddressbar = true;
			this.folderBrowserControl.ShowGoButton = false;
			this.folderBrowserControl.ShowMyDocuments = true;
			this.folderBrowserControl.ShowMyFavorites = true;
			this.folderBrowserControl.ShowMyNetwork = true;
			this.folderBrowserControl.ShowToolbar = false;
			this.folderBrowserControl.Size = new Size(271, 269);
			this.folderBrowserControl.TabIndex = 0;
			this.folderBrowserControl.PathChanged += new FolderBrowserControl.PathChangedEventHandler(this.FolderBrowserControlPathChanged);
			//
			// progressBar
			//
			this.progressBar.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.progressBar.Location = new Point(12, 323);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new Size(540, 14);
			this.progressBar.TabIndex = 1;
			//
			// statusLabel
			//
			this.statusLabel.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left)
			| AnchorStyles.Right)));
			this.statusLabel.AutoEllipsis = true;
			this.statusLabel.Location = new Point(12, 304);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new Size(540, 14);
			this.statusLabel.TabIndex = 2;
			this.statusLabel.Text = "Put clever message here, programmer.";
			this.statusLabel.TextAlign = ContentAlignment.BottomLeft;
			//
			// helpButton
			//
			this.helpButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.helpButton.Location = new Point(477, 347);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new Size(75, 23);
			this.helpButton.TabIndex = 3;
			this.helpButton.Text = "Help";
			this.helpButton.UseVisualStyleBackColor = true;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(396, 347);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// getButton
			//
			this.getButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.getButton.Location = new Point(315, 347);
			this.getButton.Name = "getButton";
			this.getButton.Size = new Size(75, 23);
			this.getButton.TabIndex = 5;
			this.getButton.Text = "Get";
			this.getButton.UseVisualStyleBackColor = true;
			//
			// lookInLabel
			//
			this.lookInLabel.Location = new Point(0, 0);
			this.lookInLabel.Name = "lookInLabel";
			this.lookInLabel.Size = new Size(271, 14);
			this.lookInLabel.TabIndex = 6;
			this.lookInLabel.Text = "Look in:";
			//
			// chooseRepositoryLabel
			//
			this.chooseRepositoryLabel.Location = new Point(277, 0);
			this.chooseRepositoryLabel.Name = "chooseRepositoryLabel";
			this.chooseRepositoryLabel.Size = new Size(260, 14);
			this.chooseRepositoryLabel.TabIndex = 7;
			this.chooseRepositoryLabel.Text = "Choose a Project Repository:";
			//
			// GetCloneFromNetworkFolderDlg
			//
			this.AcceptButton = this.getButton;
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new Size(564, 382);
			this.Controls.Add(this.getButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.helpButton);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.panel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new Size(350, 250);
			this.Name = "GetCloneFromNetworkFolderDlg";
			this.ShowIcon = false;
			this.Text = "Get Project from Shared Network Folder";
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Panel panel;
		private FolderBrowserControl folderBrowserControl;
		private ListView projectRepositoryListView;
		private ColumnHeader projectHeader;
		private ColumnHeader dateHeader;
		private ProgressBar progressBar;
		private Label statusLabel;
		private Button helpButton;
		private Button cancelButton;
		private Button getButton;
		private Label lookInLabel;
		private Label chooseRepositoryLabel;
	}
}