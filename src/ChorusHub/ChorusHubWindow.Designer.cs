using Palaso.UI.WindowsForms.Progress;

namespace ChorusHub
{
	partial class ChorusHubWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChorusHubWindow));
			this._logBox = new Palaso.UI.WindowsForms.Progress.LogBox();
			this._stopChorusHub = new System.Windows.Forms.LinkLabel();
			this._serviceTimer = new System.Windows.Forms.Timer(this.components);
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.SuspendLayout();
			//
			// _logBox
			//
			this._logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
			this._logBox.CancelRequested = false;
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this._logBox.Location = new System.Drawing.Point(0, 1);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(426, 238);
			this._logBox.TabIndex = 0;
			//
			// _stopChorusHub
			//
			this._stopChorusHub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._stopChorusHub.AutoSize = true;
			this._stopChorusHub.Location = new System.Drawing.Point(11, 261);
			this._stopChorusHub.Name = "_stopChorusHub";
			this._stopChorusHub.Size = new System.Drawing.Size(88, 13);
			this._stopChorusHub.TabIndex = 1;
			this._stopChorusHub.TabStop = true;
			this._stopChorusHub.Text = "Stop Chorus Hub";
			this._stopChorusHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._stopChorusHub_LinkClicked);
			//
			// _serviceTimer
			//
			this._serviceTimer.Tick += new System.EventHandler(this.timer1_Tick);
			//
			// ChorusHubWindow
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(428, 288);
			this.Controls.Add(this._stopChorusHub);
			this.Controls.Add(this._logBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "ChorusHubWindow";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Chorus Hub";
			this.Load += new System.EventHandler(this.ChorusHubWindow_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.Progress.LogBox _logBox;
		private System.Windows.Forms.LinkLabel _stopChorusHub;
		private System.Windows.Forms.Timer _serviceTimer;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
	}
}