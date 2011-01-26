using Palaso.Progress.LogBox;

namespace Chorus.UI.Sync
{
	partial class SyncControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncControl));
			this._tabControl = new System.Windows.Forms.TabControl();
			this._chooseTargetsTab = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this._syncTargets = new System.Windows.Forms.CheckedListBox();
			this._tasksTab = new System.Windows.Forms.TabPage();
			this._tasksListView = new System.Windows.Forms.ListView();
			this._logTab = new System.Windows.Forms.TabPage();
			this._logBox = new LogBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this._cancelButton = new System.Windows.Forms.Button();
			this._updateDisplayTimer = new System.Windows.Forms.Timer(this.components);
			this._closeButton = new System.Windows.Forms.Button();
			this._statusText = new System.Windows.Forms.Label();
			this._showCancelButtonTimer = new System.Windows.Forms.Timer(this.components);
			this._successIcon = new System.Windows.Forms.PictureBox();
			this._warningIcon = new System.Windows.Forms.PictureBox();
			this._sendReceiveButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this._tabControl.SuspendLayout();
			this._chooseTargetsTab.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.tableLayoutPanel4.SuspendLayout();
			this._tasksTab.SuspendLayout();
			this._logTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._successIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._warningIcon)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			//
			// _tabControl
			//
			this._tabControl.Controls.Add(this._chooseTargetsTab);
			this._tabControl.Controls.Add(this._tasksTab);
			this._tabControl.Controls.Add(this._logTab);
			this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tabControl.Location = new System.Drawing.Point(3, 56);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(510, 261);
			this._tabControl.TabIndex = 11;
			//
			// _chooseTargetsTab
			//
			this._chooseTargetsTab.Controls.Add(this.tableLayoutPanel5);
			this._chooseTargetsTab.Location = new System.Drawing.Point(4, 22);
			this._chooseTargetsTab.Name = "_chooseTargetsTab";
			this._chooseTargetsTab.Size = new System.Drawing.Size(502, 235);
			this._chooseTargetsTab.TabIndex = 2;
			this._chooseTargetsTab.Text = "Choose Respositories";
			this._chooseTargetsTab.UseVisualStyleBackColor = true;
			//
			// tableLayoutPanel5
			//
			this.tableLayoutPanel5.ColumnCount = 2;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Controls.Add(this.pictureBox2, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel4, 1, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(502, 235);
			this.tableLayoutPanel5.TabIndex = 12;
			//
			// pictureBox2
			//
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(3, 3);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(40, 36);
			this.pictureBox2.TabIndex = 10;
			this.pictureBox2.TabStop = false;
			//
			// tableLayoutPanel4
			//
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this._syncTargets, 0, 1);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(48, 2);
			this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 2;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(452, 231);
			this.tableLayoutPanel4.TabIndex = 11;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(321, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Attempt to Send/Receive with these people, devices, and servers:";
			//
			// _syncTargets
			//
			this._syncTargets.Dock = System.Windows.Forms.DockStyle.Fill;
			this._syncTargets.FormattingEnabled = true;
			this._syncTargets.Items.AddRange(new object[] {
			"USB Drive"});
			this._syncTargets.Location = new System.Drawing.Point(3, 16);
			this._syncTargets.MinimumSize = new System.Drawing.Size(105, 79);
			this._syncTargets.Name = "_syncTargets";
			this._syncTargets.Size = new System.Drawing.Size(446, 212);
			this._syncTargets.TabIndex = 6;
			this._syncTargets.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._syncTargets_ItemCheck);
			this._syncTargets.VisibleChanged += new System.EventHandler(this.OnRepositoryChoicesVisibleChanged);
			//
			// _tasksTab
			//
			this._tasksTab.Controls.Add(this._tasksListView);
			this._tasksTab.Location = new System.Drawing.Point(4, 22);
			this._tasksTab.Name = "_tasksTab";
			this._tasksTab.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
			this._tasksTab.Size = new System.Drawing.Size(502, 233);
			this._tasksTab.TabIndex = 0;
			this._tasksTab.Text = "Tasks";
			this._tasksTab.UseVisualStyleBackColor = true;
			//
			// _tasksListView
			//
			this._tasksListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tasksListView.Location = new System.Drawing.Point(3, 3);
			this._tasksListView.Name = "_tasksListView";
			this._tasksListView.Size = new System.Drawing.Size(498, 230);
			this._tasksListView.TabIndex = 0;
			this._tasksListView.UseCompatibleStateImageBehavior = false;
			//
			// _logTab
			//
			this._logTab.Controls.Add(this._logBox);
			this._logTab.Location = new System.Drawing.Point(4, 22);
			this._logTab.Name = "_logTab";
			this._logTab.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
			this._logTab.Size = new System.Drawing.Size(502, 235);
			this._logTab.TabIndex = 1;
			this._logTab.Text = "Log";
			this._logTab.UseVisualStyleBackColor = true;
			this._logTab.Resize += new System.EventHandler(this._logTab_Resize);
			//
			// _logBox
			//
			this._logBox.BackColor = System.Drawing.Color.Transparent;
			this._logBox.CancelRequested = false;
			this._logBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(3, 3);
			this._logBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this._logBox.Name = "_logBox";
			this._logBox.Size = new System.Drawing.Size(496, 229);
			this._logBox.TabIndex = 0;
			//
			// progressBar1
			//
			this.progressBar1.Location = new System.Drawing.Point(3, 29);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(302, 10);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar1.TabIndex = 13;
			//
			// _cancelButton
			//
			this._cancelButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._cancelButton.Location = new System.Drawing.Point(3, 3);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(132, 38);
			this._cancelButton.TabIndex = 12;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = false;
			this._cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			//
			// _updateDisplayTimer
			//
			this._updateDisplayTimer.Interval = 300;
			this._updateDisplayTimer.Tick += new System.EventHandler(this.OnUpdateDisplayTimerTick);
			//
			// _closeButton
			//
			this._closeButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._closeButton.Location = new System.Drawing.Point(3, 2);
			this._closeButton.Name = "_closeButton";
			this._closeButton.Size = new System.Drawing.Size(132, 38);
			this._closeButton.TabIndex = 17;
			this._closeButton.Text = "&Close";
			this._closeButton.UseVisualStyleBackColor = false;
			this._closeButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			//
			// _statusText
			//
			this._statusText.AutoSize = true;
			this._statusText.Location = new System.Drawing.Point(3, 0);
			this._statusText.MaximumSize = new System.Drawing.Size(250, 26);
			this._statusText.Name = "_statusText";
			this._statusText.Size = new System.Drawing.Size(248, 26);
			this._statusText.TabIndex = 16;
			this._statusText.Text = "This is very long right now to help me in positioning it.";
			//
			// _showCancelButtonTimer
			//
			this._showCancelButtonTimer.Enabled = true;
			this._showCancelButtonTimer.Interval = 1000;
			this._showCancelButtonTimer.Tick += new System.EventHandler(this._showCancelButtonTimer_Tick);
			//
			// _successIcon
			//
			this._successIcon.Image = ((System.Drawing.Image)(resources.GetObject("_successIcon.Image")));
			this._successIcon.Location = new System.Drawing.Point(3, 3);
			this._successIcon.Name = "_successIcon";
			this._successIcon.Size = new System.Drawing.Size(32, 30);
			this._successIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._successIcon.TabIndex = 15;
			this._successIcon.TabStop = false;
			//
			// _warningIcon
			//
			this._warningIcon.Image = ((System.Drawing.Image)(resources.GetObject("_warningIcon.Image")));
			this._warningIcon.Location = new System.Drawing.Point(3, 3);
			this._warningIcon.Name = "_warningIcon";
			this._warningIcon.Size = new System.Drawing.Size(32, 30);
			this._warningIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._warningIcon.TabIndex = 15;
			this._warningIcon.TabStop = false;
			//
			// _sendReceiveButton
			//
			this._sendReceiveButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this._sendReceiveButton.Image = ((System.Drawing.Image)(resources.GetObject("_sendReceiveButton.Image")));
			this._sendReceiveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._sendReceiveButton.Location = new System.Drawing.Point(3, 3);
			this._sendReceiveButton.Name = "_sendReceiveButton";
			this._sendReceiveButton.Size = new System.Drawing.Size(132, 38);
			this._sendReceiveButton.TabIndex = 14;
			this._sendReceiveButton.Text = "Send/Receive";
			this._sendReceiveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._sendReceiveButton.UseVisualStyleBackColor = false;
			this._sendReceiveButton.Click += new System.EventHandler(this._syncButton_Click);
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._statusText, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(44, 5);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(308, 42);
			this.tableLayoutPanel1.TabIndex = 18;
			//
			// tableLayoutPanel2
			//
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.panel2, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 2);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(512, 49);
			this.tableLayoutPanel2.TabIndex = 19;
			//
			// panel1
			//
			this.panel1.AutoSize = true;
			this.panel1.Controls.Add(this._successIcon);
			this.panel1.Controls.Add(this._warningIcon);
			this.panel1.Location = new System.Drawing.Point(2, 2);
			this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(38, 36);
			this.panel1.TabIndex = 0;
			//
			// panel2
			//
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.Controls.Add(this._sendReceiveButton);
			this.panel2.Controls.Add(this._cancelButton);
			this.panel2.Controls.Add(this._closeButton);
			this.panel2.Location = new System.Drawing.Point(372, 2);
			this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(138, 45);
			this.panel2.TabIndex = 1;
			//
			// tableLayoutPanel3
			//
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this._tabControl, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(516, 320);
			this.tableLayoutPanel3.TabIndex = 20;
			//
			// SyncControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.tableLayoutPanel3);
			this.Name = "SyncControl";
			this.Size = new System.Drawing.Size(516, 320);
			this.Load += new System.EventHandler(this.OnLoad);
			this.Resize += new System.EventHandler(this.SyncControl_Resize);
			this._tabControl.ResumeLayout(false);
			this._chooseTargetsTab.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			this._tasksTab.ResumeLayout(false);
			this._logTab.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._successIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._warningIcon)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl _tabControl;
		private System.Windows.Forms.TabPage _tasksTab;
		private System.Windows.Forms.ListView _tasksListView;
		private System.Windows.Forms.TabPage _logTab;
		private System.Windows.Forms.TabPage _chooseTargetsTab;
		private System.Windows.Forms.CheckedListBox _syncTargets;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button _sendReceiveButton;
		private System.Windows.Forms.Timer _updateDisplayTimer;
		public System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.PictureBox _warningIcon;
		private System.Windows.Forms.PictureBox _successIcon;
		public System.Windows.Forms.Button _closeButton;
		private System.Windows.Forms.Label _statusText;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Timer _showCancelButtonTimer;
		private LogBox _logBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
	}
}