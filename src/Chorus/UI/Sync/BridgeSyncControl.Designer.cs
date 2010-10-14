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
			this._syncControl = new Chorus.UI.Sync.SyncControl();
			this._syncStartControl1 = new Chorus.UI.Sync.SyncStartControl();
			this.SuspendLayout();
			//
			// _syncControl
			//
			this._syncControl.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._syncControl.DesiredHeight = 320;
			this._syncControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._syncControl.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._syncControl.Location = new System.Drawing.Point(0, 0);
			this._syncControl.Model = null;
			this._syncControl.Name = "_syncControl";
			this._syncControl.Size = new System.Drawing.Size(575, 365);
			this._syncControl.TabIndex = 0;
			//
			// _syncStartControl1
			//
			this._syncStartControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._syncStartControl1.Location = new System.Drawing.Point(0, 0);
			this._syncStartControl1.Name = "_syncStartControl1";
			this._syncStartControl1.Size = new System.Drawing.Size(302, 302);
			this._syncStartControl1.TabIndex = 2;
			this._syncStartControl1.Visible = false;
			this._syncStartControl1.RepositoryChosen += new System.EventHandler<Chorus.UI.Sync.SyncStartArgs>(this.SelectedRepository);
			//
			// BridgeSyncControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this._syncStartControl1);
			this.Controls.Add(this._syncControl);
			this.Name = "BridgeSyncControl";
			this.Size = new System.Drawing.Size(575, 365);
			this.ResumeLayout(false);

		}

		#endregion

		private SyncControl _syncControl;
		private SyncStartControl _syncStartControl1;
	}
}
