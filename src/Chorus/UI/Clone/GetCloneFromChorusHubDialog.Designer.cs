namespace Chorus.UI.Clone
{
	partial class GetCloneFromChorusHubDialog
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
			this._projectRepositoryListView = new System.Windows.Forms.ListView();
			this.projectHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cancelButton = new System.Windows.Forms.Button();
			this.getButton = new System.Windows.Forms.Button();
			this.updateProgress = new System.Windows.Forms.Timer(this.components);
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this._getChorusHubInfoBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.panel.SuspendLayout();
			this.SuspendLayout();
			//
			// panel
			//
			this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel.Controls.Add(this._projectRepositoryListView);
			this.panel.Location = new System.Drawing.Point(12, 12);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(540, 289);
			this.panel.TabIndex = 0;
			//
			// _projectRepositoryListView
			//
			this._projectRepositoryListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._projectRepositoryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.projectHeader});
			this._projectRepositoryListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._projectRepositoryListView.FullRowSelect = true;
			this._projectRepositoryListView.HideSelection = false;
			this._projectRepositoryListView.Location = new System.Drawing.Point(0, 0);
			this._projectRepositoryListView.MultiSelect = false;
			this._projectRepositoryListView.Name = "_projectRepositoryListView";
			this._projectRepositoryListView.ShowItemToolTips = true;
			this._projectRepositoryListView.Size = new System.Drawing.Size(537, 286);
			this._projectRepositoryListView.TabIndex = 1;
			this._projectRepositoryListView.UseCompatibleStateImageBehavior = false;
			this._projectRepositoryListView.View = System.Windows.Forms.View.Details;
			this._projectRepositoryListView.SelectedIndexChanged += new System.EventHandler(this.OnRepositoryListViewSelectionChange);
			this._projectRepositoryListView.DoubleClick += new System.EventHandler(this.OnRepositoryListViewDoubleClick);
			//
			// projectHeader
			//
			this.projectHeader.Text = "Project";
			this.projectHeader.Width = 411;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(476, 347);
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
			this.getButton.Location = new System.Drawing.Point(395, 347);
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
			// GetCloneFromChorusHubDialog
			//
			this.AcceptButton = this.getButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(564, 382);
			this.Controls.Add(this.getButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.panel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(350, 250);
			this.Name = "GetCloneFromChorusHubDialog";
			this.ShowIcon = false;
			this.Text = "Get Project from Chorus Hub";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Load += new System.EventHandler(this.OnLoad);
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.ListView _projectRepositoryListView;
		private System.Windows.Forms.ColumnHeader projectHeader;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button getButton;
		private System.Windows.Forms.Timer updateProgress;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.ComponentModel.BackgroundWorker _getChorusHubInfoBackgroundWorker;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
	}
}