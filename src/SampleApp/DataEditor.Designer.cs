namespace SampleApp
{
	partial class DataEditor
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
			this._area1Text = new System.Windows.Forms.TextBox();
			this._area1Label = new System.Windows.Forms.Label();
			this._area2Label = new System.Windows.Forms.Label();
			this._area2Text = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// _area1Text
			//
			this._area1Text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._area1Text.Location = new System.Drawing.Point(16, 63);
			this._area1Text.Multiline = true;
			this._area1Text.Name = "_area1Text";
			this._area1Text.Size = new System.Drawing.Size(405, 47);
			this._area1Text.TabIndex = 0;
			this._area1Text.Tag = "fruits";
			this._area1Text.Enter += new System.EventHandler(this.OnEnterBox);
			//
			// _area1Label
			//
			this._area1Label.AutoSize = true;
			this._area1Label.Location = new System.Drawing.Point(13, 47);
			this._area1Label.Name = "_area1Label";
			this._area1Label.Size = new System.Drawing.Size(28, 13);
			this._area1Label.TabIndex = 5;
			this._area1Label.Text = "area";
			//
			// _area2Label
			//
			this._area2Label.AutoSize = true;
			this._area2Label.Location = new System.Drawing.Point(11, 133);
			this._area2Label.Name = "_area2Label";
			this._area2Label.Size = new System.Drawing.Size(28, 13);
			this._area2Label.TabIndex = 7;
			this._area2Label.Text = "area";
			//
			// _area2Text
			//
			this._area2Text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._area2Text.Location = new System.Drawing.Point(14, 149);
			this._area2Text.Multiline = true;
			this._area2Text.Name = "_area2Text";
			this._area2Text.Size = new System.Drawing.Size(405, 47);
			this._area2Text.TabIndex = 6;
			this._area2Text.Tag = "fruits";
			this._area2Text.Enter += new System.EventHandler(this.OnEnterBox);
			//
			// DataEditor
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.Controls.Add(this._area2Label);
			this.Controls.Add(this._area2Text);
			this.Controls.Add(this._area1Label);
			this.Controls.Add(this._area1Text);
			this.Name = "DataEditor";
			this.Size = new System.Drawing.Size(431, 329);
			this.Load += new System.EventHandler(this.DataEditor_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _area1Text;
		private System.Windows.Forms.Label _area1Label;
		private System.Windows.Forms.Label _area2Label;
		private System.Windows.Forms.TextBox _area2Text;
	}
}
