using SIL.Progress;
using SIL.Windows.Forms.Progress;

namespace Chorus.UI.Clone
{
	partial class GetCloneFromInternetDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetCloneFromInternetDialog));
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._statusImages = new System.Windows.Forms.ImageList(this.components);
			this._statusImage = new System.Windows.Forms.Button();
			this._statusLabel = new System.Windows.Forms.TextBox();
			this._progressBar = new SIL.Windows.Forms.Progress.SimpleProgressIndicator();
			this._cancelTaskButton = new System.Windows.Forms.Button();
			this._fixSettingsButton = new System.Windows.Forms.Button();
			this._logBox = new SIL.Windows.Forms.Progress.LogBox();
			this._statusProgress = new SIL.Windows.Forms.Progress.SimpleStatusProgress();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._helpButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// _cancelButton
			// 
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._cancelButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._cancelButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._cancelButton, "Common.Cancel");
			this._cancelButton.Location = new System.Drawing.Point(295, 240);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 1;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			// 
			// _okButton
			// 
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._okButton, "Common.OK");
			this._okButton.Location = new System.Drawing.Point(206, 240);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			// 
			// _statusImages
			// 
			this._statusImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_statusImages.ImageStream")));
			this._statusImages.TransparentColor = System.Drawing.Color.Transparent;
			this._statusImages.Images.SetKeyName(0, "Success");
			this._statusImages.Images.SetKeyName(1, "Error");
			// 
			// _statusImage
			// 
			this._statusImage.FlatAppearance.BorderSize = 0;
			this._statusImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._statusImage.ImageIndex = 0;
			this._statusImage.ImageList = this._statusImages;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._statusImage, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._statusImage, null);
			this.l10NSharpExtender1.SetLocalizingId(this._statusImage, "GetCloneFromInternetDialog.GetCloneFromInternetDialog._statusImage");
			this._statusImage.Location = new System.Drawing.Point(8, 7);
			this._statusImage.Name = "_statusImage";
			this._statusImage.Size = new System.Drawing.Size(50, 36);
			this._statusImage.TabIndex = 17;
			this._statusImage.UseVisualStyleBackColor = true;
			// 
			// _statusLabel
			// 
			this._statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._statusLabel.BackColor = System.Drawing.SystemColors.Control;
			this._statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._statusLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._statusLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._statusLabel, "GetCloneFromInternetDialog.GetCloneFromInternetDialog._statusLabel");
			this._statusLabel.Location = new System.Drawing.Point(58, 7);
			this._statusLabel.MaximumSize = new System.Drawing.Size(316, 71);
			this._statusLabel.Multiline = true;
			this._statusLabel.Name = "_statusLabel";
			this._statusLabel.ReadOnly = true;
			this._statusLabel.Size = new System.Drawing.Size(316, 25);
			this._statusLabel.TabIndex = 18;
			this._statusLabel.Text = "Status label";
			// 
			// _progressBar
			// 
			this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._progressBar, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._progressBar, null);
			this.l10NSharpExtender1.SetLocalizingId(this._progressBar, "GetCloneFromInternetDialog.GetCloneFromInternetDialog._progressBar");
			this._progressBar.Location = new System.Drawing.Point(15, 54);
			this._progressBar.MarqueeAnimationSpeed = 50;
			this._progressBar.Name = "_progressBar";
			this._progressBar.PercentCompleted = 0;
			this._progressBar.Size = new System.Drawing.Size(275, 10);
			this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this._progressBar.SyncContext = null;
			this._progressBar.TabIndex = 0;
			// 
			// _cancelTaskButton
			// 
			this._cancelTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._cancelTaskButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._cancelTaskButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._cancelTaskButton, "Common.Cancel");
			this._cancelTaskButton.Location = new System.Drawing.Point(295, 45);
			this._cancelTaskButton.Name = "_cancelTaskButton";
			this._cancelTaskButton.Size = new System.Drawing.Size(75, 23);
			this._cancelTaskButton.TabIndex = 23;
			this._cancelTaskButton.Text = "Cancel";
			this._cancelTaskButton.UseVisualStyleBackColor = true;
			this._cancelTaskButton.Click += new System.EventHandler(this.button2_Click);
			// 
			// _fixSettingsButton
			// 
			this._fixSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._fixSettingsButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._fixSettingsButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._fixSettingsButton, "GetCloneFromInternetDialog.FixSettings");
			this._fixSettingsButton.Location = new System.Drawing.Point(113, 240);
			this._fixSettingsButton.Name = "_fixSettingsButton";
			this._fixSettingsButton.Size = new System.Drawing.Size(75, 23);
			this._fixSettingsButton.TabIndex = 1;
			this._fixSettingsButton.Text = "&Fix Settings";
			this._fixSettingsButton.UseVisualStyleBackColor = true;
			this._fixSettingsButton.Click += new System.EventHandler(this._fixSettingsButton_Click);
			// 
			// _logBox
			// 
			this._logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._logBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._logBox.BackColor = System.Drawing.Color.Transparent;
			this._logBox.CancelRequested = false;
			this._logBox.ErrorEncountered = false;
			this._logBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._logBox.GetDiagnosticsMethod = null;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._logBox, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._logBox, null);
			this.l10NSharpExtender1.SetLocalizingId(this._logBox, "GetCloneFromInternetDialog.GetCloneFromInternetDialog.LogBox");
			this._logBox.Location = new System.Drawing.Point(15, 80);
			this._logBox.Name = "_logBox";
			this._logBox.ProgressIndicator = null;
			this._logBox.ShowCopyToClipboardMenuItem = false;
			this._logBox.ShowDetailsMenuItem = false;
			this._logBox.ShowDiagnosticsMenuItem = false;
			this._logBox.ShowFontMenuItem = false;
			this._logBox.ShowMenu = true;
			this._logBox.Size = new System.Drawing.Size(359, 172);
			this._logBox.TabIndex = 0;
			this._logBox.Load += new System.EventHandler(this._logBox_Load);
			// 
			// _statusProgress
			// 
			this._statusProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._statusProgress.AutoSize = true;
			this._statusProgress.CancelRequested = false;
			this._statusProgress.ErrorEncountered = false;
			this._statusProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._statusProgress.LastException = null;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._statusProgress, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._statusProgress, null);
			this.l10NSharpExtender1.SetLocalizingId(this._statusProgress, "GetCloneFromInternetDialog.GetCloneFromInternetDialog._statusProgress");
			this._statusProgress.Location = new System.Drawing.Point(58, 36);
			this._statusProgress.Name = "_statusProgress";
			this._statusProgress.ProgressIndicator = null;
			this._statusProgress.Size = new System.Drawing.Size(61, 15);
			this._statusProgress.SyncContext = null;
			this._statusProgress.TabIndex = 24;
			this._statusProgress.Text = "status text";
			this._statusProgress.WarningEncountered = false;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "GetCloneFromInternetDialog";
			// 
			// _helpButton
			// 
			this._helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._helpButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._helpButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._helpButton, "Common.Help");
			this._helpButton.Location = new System.Drawing.Point(32, 240);
			this._helpButton.Name = "_helpButton";
			this._helpButton.Size = new System.Drawing.Size(75, 23);
			this._helpButton.TabIndex = 1;
			this._helpButton.Text = "&Help";
			this._helpButton.UseVisualStyleBackColor = true;
			this._helpButton.Click += new System.EventHandler(this._helpButton_Click);
			// 
			// GetCloneFromInternetDialog
			// 
			this.AcceptButton = this._okButton;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(394, 275);
			this.Controls.Add(this._statusProgress);
			this.Controls.Add(this._cancelTaskButton);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._statusImage);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._helpButton);
			this.Controls.Add(this._fixSettingsButton);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._logBox);
			this.Controls.Add(this._statusLabel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "GetCloneFromInternetDialog.WindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(322, 300);
			this.Name = "GetCloneFromInternetDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Get Project From Internet";
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResizeBegin += new System.EventHandler(this.GetCloneFromInternetDialog_ResizeBegin);
			this.ResizeEnd += new System.EventHandler(this.GetCloneFromInternetDialog_ResizeEnd);
			this.BackColorChanged += new System.EventHandler(this.GetCloneFromInternetDialog_BackColorChanged);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.GetCloneFromInternetDialog_Paint);
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.ImageList _statusImages;
		private System.Windows.Forms.Button _statusImage;
		private System.Windows.Forms.TextBox _statusLabel;
		private SimpleProgressIndicator _progressBar;
		private System.Windows.Forms.Button _cancelTaskButton;
		private LogBox _logBox;
		private System.Windows.Forms.Button _fixSettingsButton;
		private SimpleStatusProgress _statusProgress;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
		private System.Windows.Forms.Button _helpButton;
	}
}