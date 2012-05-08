using System;

namespace Chorus.UI.Sync
{
	partial class SyncDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncDialog));
			this._closeWhenDoneTimer = new System.Windows.Forms.Timer(this.components);
			this._syncStartControl = new Chorus.UI.Sync.SyncStartControl();
			this._syncControl = new Chorus.UI.Sync.SyncControl();
			this.SuspendLayout();
			//
			// _closeWhenDoneTimer
			//
			this._closeWhenDoneTimer.Interval = 500;
			this._closeWhenDoneTimer.Tick += new System.EventHandler(this._closeWhenDoneTimer_Tick);
			//
			// _syncStartControl
			//
			this._syncStartControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._syncStartControl.Location = new System.Drawing.Point(0, 0);
			this._syncStartControl.Name = "_syncStartControl";
			this._syncStartControl.Size = new System.Drawing.Size(496, 440);
			this._syncStartControl.TabIndex = 1;
			this._syncStartControl.Visible = false;
			this._syncStartControl.RepositoryChosen += new System.EventHandler<Chorus.UI.Sync.SyncStartArgs>(this._syncStartControl1_RepositoryChosen);
			//
			// _syncControl
			//
			this._syncControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._syncControl.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._syncControl.DesiredHeight = 560;
			this._syncControl.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._syncControl.Location = new System.Drawing.Point(0, 10);
			this._syncControl.Margin = new System.Windows.Forms.Padding(48, 22, 48, 22);
			this._syncControl.Model = null;
			this._syncControl.Name = "_syncControl";
			this._syncControl.Size = new System.Drawing.Size(494, 431);
			this._syncControl.TabIndex = 0;
			this._syncControl.Visible = false;
			this._syncControl.CloseButtonClicked += new System.EventHandler(this._syncControl_CloseButtonClicked);
			//
			// SyncDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(496, 440);
			this.Controls.Add(this._syncStartControl);
			this.Controls.Add(this._syncControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(48, 22, 48, 22);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SyncDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Send/Receive";
			this.Load += new System.EventHandler(this.SyncDialog_Load);
			this.Shown += new System.EventHandler(this.SyncDialog_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private SyncControl _syncControl;
		private System.Windows.Forms.Timer _closeWhenDoneTimer;
		private SyncStartControl _syncStartControl;

	}
}