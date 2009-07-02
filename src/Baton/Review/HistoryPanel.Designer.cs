namespace Baton.HistoryPanel
{
	partial class HistoryPanel
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
			this._historyText = new System.Windows.Forms.TextBox();
			this._loadButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// _historyText
			//
			this._historyText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._historyText.Location = new System.Drawing.Point(3, 32);
			this._historyText.Multiline = true;
			this._historyText.Name = "_historyText";
			this._historyText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._historyText.Size = new System.Drawing.Size(470, 327);
			this._historyText.TabIndex = 0;
			//
			// _loadButton
			//
			this._loadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._loadButton.Location = new System.Drawing.Point(392, 3);
			this._loadButton.Name = "_loadButton";
			this._loadButton.Size = new System.Drawing.Size(75, 23);
			this._loadButton.TabIndex = 1;
			this._loadButton.Text = "Get History";
			this._loadButton.UseVisualStyleBackColor = true;
			this._loadButton.Click += new System.EventHandler(this._loadButton_Click);
			//
			// HistoryPanel
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._loadButton);
			this.Controls.Add(this._historyText);
			this.Name = "HistoryPanel";
			this.Size = new System.Drawing.Size(470, 362);
			this.Load += new System.EventHandler(this.HistoryPanel_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _historyText;
		private System.Windows.Forms.Button _loadButton;

	}
}