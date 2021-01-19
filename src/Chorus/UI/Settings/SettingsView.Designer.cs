using System;

namespace Chorus.UI.Settings
{
	partial class SettingsView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsView));
			this._repositoryAliases = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this._userName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.instructionsLabel = new Chorus.UI.BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.SuspendLayout();
			// 
			// _repositoryAliases
			// 
			this._repositoryAliases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._repositoryAliases.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._repositoryAliases.Location = new System.Drawing.Point(61, 153);
			this._repositoryAliases.MaximumSize = new System.Drawing.Size(700, 90);
			this._repositoryAliases.Multiline = true;
			this._repositoryAliases.Name = "_repositoryAliases";
			this._repositoryAliases.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._repositoryAliases.Size = new System.Drawing.Size(426, 67);
			this._repositoryAliases.TabIndex = 3;
			this._repositoryAliases.Leave += new System.EventHandler(this._repositoryAliases_Leave);
			this._repositoryAliases.Validating += new System.ComponentModel.CancelEventHandler(this.OnRepositoryAliases_Validating);
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(58, 67);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(74, 17);
			this.label5.TabIndex = 9;
			this.label5.Text = "User Name";
			//
			// _userName
			//
			this._userName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._userName.Location = new System.Drawing.Point(139, 67);
			this._userName.Name = "_userName";
			this._userName.Size = new System.Drawing.Size(147, 23);
			this._userName.TabIndex = 8;
			this._userName.Validating += new System.ComponentModel.CancelEventHandler(this.OnUserName_Validating);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(58, 124);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(310, 17);
			this.label3.TabIndex = 10;
			this.label3.Text = "Chorus repositories on servers and users\' machines";
			//
			// pictureBox2
			//
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(15, 124);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(40, 36);
			this.pictureBox2.TabIndex = 12;
			this.pictureBox2.TabStop = false;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(11, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(183, 20);
			this.label1.TabIndex = 13;
			this.label1.Text = "Change Chorus Settings";
			// 
			// instructionsLabel
			// 
			this.instructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.instructionsLabel.BackColor = System.Drawing.Color.DarkRed;
			this.instructionsLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.instructionsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.instructionsLabel.Location = new System.Drawing.Point(61, 226);
			this.instructionsLabel.Multiline = true;
			this.instructionsLabel.Name = "instructionsLabel";
			this.instructionsLabel.ReadOnly = true;
			this.instructionsLabel.Size = new System.Drawing.Size(426, 175);
			this.instructionsLabel.TabIndex = 14;
			this.instructionsLabel.TabStop = false;
			this.instructionsLabel.Text = resources.GetString("instructionsLabel.Text");
			// 
			// SettingsView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.instructionsLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this._userName);
			this.Controls.Add(this._repositoryAliases);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SettingsView";
			this.Size = new System.Drawing.Size(505, 473);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _repositoryAliases;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox _userName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label label1;
		private BetterLabel instructionsLabel;
	}
}