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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncDialog));
			this._syncControl = new Chorus.UI.Sync.SyncControl();
			this.SuspendLayout();
			//
			// _syncControl
			//
			this._syncControl.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this._syncControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._syncControl.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._syncControl.Location = new System.Drawing.Point(0, 0);
			this._syncControl.Model = null;
			this._syncControl.Name = "_syncControl";
			this._syncControl.Size = new System.Drawing.Size(470, 325);
			this._syncControl.TabIndex = 0;
			this._syncControl.UserName = "anonymous";
			this._syncControl.CloseButtonClicked += new System.EventHandler(this._syncControl_CloseButtonClicked);
			//
			// SyncDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(470, 325);
			this.Controls.Add(this._syncControl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SyncDialog";
			this.Text = "Send/Receive";
			this.Shown += new System.EventHandler(this.SyncDialog_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private SyncControl _syncControl;

	}
}