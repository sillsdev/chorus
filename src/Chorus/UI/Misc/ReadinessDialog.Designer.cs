namespace Chorus.UI.Misc
{
	partial class ReadinessDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReadinessDialog));
			this.readinessPanel1 = new Chorus.UI.Misc.ReadinessPanel();
			this._okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// readinessPanel1
			//
			this.readinessPanel1.Location = new System.Drawing.Point(12, 21);
			this.readinessPanel1.Name = "readinessPanel1";
			this.readinessPanel1.Size = new System.Drawing.Size(439, 198);
			this.readinessPanel1.TabIndex = 0;
			//
			// _okButton
			//
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._okButton.Location = new System.Drawing.Point(386, 225);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			//
			// ReadinessDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._okButton;
			this.ClientSize = new System.Drawing.Size(484, 262);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.readinessPanel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ReadinessDialog";
			this.Text = "Chorus Readiness";
			this.ResumeLayout(false);

		}

		#endregion

		private ReadinessPanel readinessPanel1;
		private System.Windows.Forms.Button _okButton;
	}
}