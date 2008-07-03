namespace Chorus.UI
{
	partial class MainWindow
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this._syncPanel = new Chorus.UI.SyncPanel();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.settingsPanel2 = new Chorus.UI.SettingsPanel();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.setupPanel1 = new Chorus.UI.SetupPanel();
			this._historyPanel = new Chorus.UI.HistoryPanel();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			//
			// tabControl1
			//
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(515, 374);
			this.tabControl1.TabIndex = 0;
			//
			// tabPage1
			//
			this.tabPage1.Controls.Add(this._syncPanel);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(507, 348);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Sync";
			this.tabPage1.UseVisualStyleBackColor = true;
			//
			// _syncPanel
			//
			this._syncPanel.BackColor = System.Drawing.SystemColors.Control;
			this._syncPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._syncPanel.Location = new System.Drawing.Point(3, 3);
			this._syncPanel.Name = "_syncPanel";
			this._syncPanel.ProjectFolderConfig = null;
			this._syncPanel.Size = new System.Drawing.Size(501, 342);
			this._syncPanel.TabIndex = 0;
			this._syncPanel.UserName = "anonymous";
			//
			// tabPage2
			//
			this.tabPage2.Controls.Add(this._historyPanel);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(507, 348);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "History";
			this.tabPage2.UseVisualStyleBackColor = true;
			//
			// tabPage4
			//
			this.tabPage4.Controls.Add(this.settingsPanel2);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage4.Size = new System.Drawing.Size(507, 348);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Settings";
			this.tabPage4.UseVisualStyleBackColor = true;
			//
			// settingsPanel2
			//
			this.settingsPanel2.AutoScroll = true;
			this.settingsPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.settingsPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsPanel2.Location = new System.Drawing.Point(3, 3);
			this.settingsPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.settingsPanel2.Name = "settingsPanel2";
			this.settingsPanel2.Size = new System.Drawing.Size(501, 342);
			this.settingsPanel2.TabIndex = 0;
			//
			// tabPage3
			//
			this.tabPage3.Controls.Add(this.setupPanel1);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(507, 348);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Getting Started";
			this.tabPage3.UseVisualStyleBackColor = true;
			//
			// setupPanel1
			//
			this.setupPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.setupPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.setupPanel1.Location = new System.Drawing.Point(0, 0);
			this.setupPanel1.Name = "setupPanel1";
			this.setupPanel1.Size = new System.Drawing.Size(507, 348);
			this.setupPanel1.TabIndex = 0;
			//
			// _historyPanel
			//
			this._historyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._historyPanel.Location = new System.Drawing.Point(3, 3);
			this._historyPanel.Name = "_historyPanel";
			this._historyPanel.ProjectFolderConfig = null;
			this._historyPanel.Size = new System.Drawing.Size(501, 342);
			this._historyPanel.TabIndex = 0;
			this._historyPanel.UserName = "anonymous";
			//
			// MainWindow
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(515, 374);
			this.Controls.Add(this.tabControl1);
			this.Name = "MainWindow";
			this.ShowIcon = false;
			this.Text = "Chorus";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private SyncPanel _syncPanel;
		private SetupPanel setupPanel1;
		private System.Windows.Forms.TabPage tabPage4;
		private SettingsPanel settingsPanel2;
		private HistoryPanel _historyPanel;
	}
}