namespace Chorus.UI.Sync
{
	partial class BridgeSyncControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BridgeSyncControl));
			this._splitContainer = new System.Windows.Forms.SplitContainer();
			this._warningIcon = new System.Windows.Forms.PictureBox();
			this._statusText = new System.Windows.Forms.Label();
			this._successIcon = new System.Windows.Forms.PictureBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this._syncStartControl = new Chorus.UI.Sync.SyncStartControl();
			this._logBox = new Chorus.UI.Misc.LogBox();
			this._splitContainer.Panel1.SuspendLayout();
			this._splitContainer.Panel2.SuspendLayout();
			this._splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._warningIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._successIcon)).BeginInit();
			this.SuspendLayout();
			//
			// _splitContainer
			//
			this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this._splitContainer.IsSplitterFixed = true;
			this._splitContainer.Location = new System.Drawing.Point(0, 0);
			this._splitContainer.Name = "_splitContainer";
			//
			// _splitContainer.Panel1
			//
			this._splitContainer.Panel1.Controls.Add(this._warningIcon);
			this._splitContainer.Panel1.Controls.Add(this._statusText);
			this._splitContainer.Panel1.Controls.Add(this._successIcon);
			this._splitContainer.Panel1.Controls.Add(this.progressBar1);
			this._splitContainer.Panel1.Controls.Add(this._syncStartControl);
			//
			// _splitContainer.Panel2
			//
			this._splitContainer.Panel2.Controls.Add(this._logBox);
			this._splitContainer.Size = new System.Drawing.Size(575, 365);
			this._splitContainer.SplitterDistance = 300;
			this._splitContainer.TabIndex = 3;
			//
			// _warningIcon
			//
			this._warningIcon.Image = ((System.Drawing.Image)(resources.GetObject("_warningIcon.Image")));
			this._warningIcon.Location = new System.Drawing.Point(6, 325);
			this._warningIcon.Name = "_warningIcon";
			this._warningIcon.Size = new System.Drawing.Size(32, 30);
			this._warningIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._warningIcon.TabIndex = 20;
			this._warningIcon.TabStop = false;
			//
			// _statusText
			//
			this._statusText.AutoSize = true;
			this._statusText.Location = new System.Drawing.Point(44, 316);
			this._statusText.MaximumSize = new System.Drawing.Size(250, 26);
			this._statusText.Name = "_statusText";
			this._statusText.Size = new System.Drawing.Size(248, 26);
			this._statusText.TabIndex = 19;
			this._statusText.Text = "This is very long right now to help me in positioning it.";
			//
			// _successIcon
			//
			this._successIcon.Image = ((System.Drawing.Image)(resources.GetObject("_successIcon.Image")));
			this._successIcon.Location = new System.Drawing.Point(6, 320);
			this._successIcon.Name = "_successIcon";
			this._successIcon.Size = new System.Drawing.Size(32, 35);
			this._successIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._successIcon.TabIndex = 18;
			this._successIcon.TabStop = false;
			//
			// progressBar1
			//
			this.progressBar1.Location = new System.Drawing.Point(44, 349);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(248, 10);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.progressBar1.TabIndex = 17;
			//
			// _syncStartControl
			//
			this._syncStartControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._syncStartControl.Location = new System.Drawing.Point(3, 3);
			this._syncStartControl.Name = "_syncStartControl";
			this._syncStartControl.Size = new System.Drawing.Size(294, 306);
			this._syncStartControl.TabIndex = 4;
			this._syncStartControl.RepositoryChosen += new System.EventHandler<Chorus.UI.Sync.SyncStartArgs>(this.SelectedRepository);
			//
			// _logBox
			//
			this._logBox.BackColor = System.Drawing.Color.Transparent;
			this._logBox.CancelRequested = false;
			this._logBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._logBox.Enabled = false;
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(0, 0);
			this._logBox.Name = "_logBox";
			this._logBox.Size = new System.Drawing.Size(271, 365);
			this._logBox.TabIndex = 0;
			//
			// BridgeSyncControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._splitContainer);
			this.MinimumSize = new System.Drawing.Size(500, 365);
			this.Name = "BridgeSyncControl";
			this.Size = new System.Drawing.Size(575, 365);
			this._splitContainer.Panel1.ResumeLayout(false);
			this._splitContainer.Panel1.PerformLayout();
			this._splitContainer.Panel2.ResumeLayout(false);
			this._splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._warningIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._successIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitContainer;
		private SyncStartControl _syncStartControl;
		private Chorus.UI.Misc.LogBox _logBox;
		private System.Windows.Forms.Label _statusText;
		private System.Windows.Forms.PictureBox _successIcon;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.PictureBox _warningIcon;

	}
}
