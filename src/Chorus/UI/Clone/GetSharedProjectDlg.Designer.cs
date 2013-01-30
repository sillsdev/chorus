namespace Chorus.UI.Clone
{
	partial class GetSharedProjectDlg
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetSharedProjectDlg));
			this._useUSBButton = new System.Windows.Forms.Button();
			this._useInternetButton = new System.Windows.Forms.Button();
			this._useLocalNetworkButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _useUSBButton
			//
			this._useUSBButton.BackColor = System.Drawing.Color.White;
			this._useUSBButton.Enabled = false;
			this._useUSBButton.Image = ((System.Drawing.Image)(resources.GetObject("_useUSBButton.Image")));
			this._useUSBButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useUSBButton.Location = new System.Drawing.Point(28, 21);
			this._useUSBButton.Name = "_useUSBButton";
			this._useUSBButton.Size = new System.Drawing.Size(167, 39);
			this._useUSBButton.TabIndex = 6;
			this._useUSBButton.Text = "&USB Flash Drive";
			this._useUSBButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useUSBButton.UseVisualStyleBackColor = false;
			this._useUSBButton.Click += new System.EventHandler(this.BtnUsbClicked);
			//
			// _useInternetButton
			//
			this._useInternetButton.BackColor = System.Drawing.Color.White;
			this._useInternetButton.Image = ((System.Drawing.Image)(resources.GetObject("_useInternetButton.Image")));
			this._useInternetButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useInternetButton.Location = new System.Drawing.Point(28, 96);
			this._useInternetButton.Name = "_useInternetButton";
			this._useInternetButton.Size = new System.Drawing.Size(167, 39);
			this._useInternetButton.TabIndex = 5;
			this._useInternetButton.Text = "&Internet";
			this._useInternetButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useInternetButton.UseVisualStyleBackColor = false;
			this._useInternetButton.Click += new System.EventHandler(this.BtnInternetClicked);
			//
			// _useLocalNetworkButton
			//
			this._useLocalNetworkButton.BackColor = System.Drawing.Color.White;
			this._useLocalNetworkButton.Enabled = false;
			this._useLocalNetworkButton.Image = global::Chorus.Properties.Resources.chorusHubMedium;
			this._useLocalNetworkButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._useLocalNetworkButton.Location = new System.Drawing.Point(28, 171);
			this._useLocalNetworkButton.Name = "_useLocalNetworkButton";
			this._useLocalNetworkButton.Size = new System.Drawing.Size(167, 38);
			this._useLocalNetworkButton.TabIndex = 7;
			this._useLocalNetworkButton.Text = "&Chorus Hub";
			this._useLocalNetworkButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._useLocalNetworkButton.UseVisualStyleBackColor = false;
			this._useLocalNetworkButton.Click += new System.EventHandler(this.BtnChorusHubClicked);
			//
			// GetSharedProjectDlg
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(223, 245);
			this.Controls.Add(this._useLocalNetworkButton);
			this.Controls.Add(this._useUSBButton);
			this.Controls.Add(this._useInternetButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GetSharedProjectDlg";
			this.ShowInTaskbar = false;
			this.Text = "Receive project";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _useUSBButton;
		private System.Windows.Forms.Button _useInternetButton;
		private System.Windows.Forms.Button _useLocalNetworkButton;
	}
}