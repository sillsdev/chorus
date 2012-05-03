namespace Chorus.UI.Misc
{
	partial class NetworkFolderSettingsControl
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
			this.sharedFolderLabel = new System.Windows.Forms.Label();
			this.sharedFolderTextbox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// sharedFolderLabel
			//
			this.sharedFolderLabel.AutoSize = true;
			this.sharedFolderLabel.Location = new System.Drawing.Point(4, 4);
			this.sharedFolderLabel.Name = "sharedFolderLabel";
			this.sharedFolderLabel.Size = new System.Drawing.Size(119, 13);
			this.sharedFolderLabel.TabIndex = 0;
			this.sharedFolderLabel.Text = "Shared Network Folder:";
			//
			// sharedFolderTextbox
			//
			this.sharedFolderTextbox.Location = new System.Drawing.Point(7, 21);
			this.sharedFolderTextbox.Name = "sharedFolderTextbox";
			this.sharedFolderTextbox.Size = new System.Drawing.Size(198, 20);
			this.sharedFolderTextbox.TabIndex = 1;
			this.sharedFolderTextbox.TextChanged += new System.EventHandler(this.sharedFolderTextbox_TextChanged);
			//
			// browseButton
			//
			this.browseButton.Location = new System.Drawing.Point(221, 21);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(75, 23);
			this.browseButton.TabIndex = 2;
			this.browseButton.Text = "Browse...";
			this.browseButton.UseVisualStyleBackColor = true;
			//
			// NetworkFolderSettingsControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.sharedFolderTextbox);
			this.Controls.Add(this.sharedFolderLabel);
			this.Name = "NetworkFolderSettingsControl";
			this.Size = new System.Drawing.Size(326, 155);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label sharedFolderLabel;
		private System.Windows.Forms.TextBox sharedFolderTextbox;
		private System.Windows.Forms.Button browseButton;
	}
}
