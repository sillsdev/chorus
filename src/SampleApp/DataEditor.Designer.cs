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
			this._fruits = new System.Windows.Forms.TextBox();
			this._syncButton = new System.Windows.Forms.Button();
			this._vegetables = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _fruits
			//
			this._fruits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._fruits.Location = new System.Drawing.Point(16, 80);
			this._fruits.Multiline = true;
			this._fruits.Name = "_fruits";
			this._fruits.Size = new System.Drawing.Size(405, 85);
			this._fruits.TabIndex = 0;
			this._fruits.Tag = "fruits";
			this._fruits.Text = "Apples, Oranges";
			this._fruits.Enter += new System.EventHandler(this._fruits_Enter);
			//
			// _syncButton
			//
			this._syncButton.Location = new System.Drawing.Point(16, 13);
			this._syncButton.Name = "_syncButton";
			this._syncButton.Size = new System.Drawing.Size(105, 31);
			this._syncButton.TabIndex = 2;
			this._syncButton.Text = "Send/Receive";
			this._syncButton.UseVisualStyleBackColor = true;
			this._syncButton.Click += new System.EventHandler(this.OnSendReceiveClick);
			//
			// _vegetables
			//
			this._vegetables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._vegetables.Location = new System.Drawing.Point(16, 207);
			this._vegetables.Multiline = true;
			this._vegetables.Name = "_vegetables";
			this._vegetables.Size = new System.Drawing.Size(405, 85);
			this._vegetables.TabIndex = 1;
			this._vegetables.Tag = "vegetables";
			this._vegetables.Text = "Carrots, Egg plant";
			this._vegetables.Enter += new System.EventHandler(this._vegetables_Enter);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Fruits";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 191);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Vegetables";
			//
			// DataEditor
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._vegetables);
			this.Controls.Add(this._fruits);
			this.Controls.Add(this._syncButton);
			this.Name = "DataEditor";
			this.Size = new System.Drawing.Size(431, 329);
			this.Load += new System.EventHandler(this.DataEditor_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _fruits;
		private System.Windows.Forms.Button _syncButton;
		private System.Windows.Forms.TextBox _vegetables;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}
