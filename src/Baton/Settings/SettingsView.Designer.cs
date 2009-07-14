namespace Baton.Settings
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
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this._repositoryAliases = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this._userName = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			// checkBox1
			//
			this.checkBox1.AutoSize = true;
			this.checkBox1.Enabled = false;
			this.checkBox1.Location = new System.Drawing.Point(13, 45);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(195, 1);
			this.checkBox1.TabIndex = 1;
			this.checkBox1.Text = "This is a protected master repository";
			this.checkBox1.UseVisualStyleBackColor = true;
			//
			// checkBox2
			//
			this.checkBox2.AutoSize = true;
			this.checkBox2.Enabled = false;
			this.checkBox2.Location = new System.Drawing.Point(13, 45);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.checkBox2.Size = new System.Drawing.Size(233, 1);
			this.checkBox2.TabIndex = 1;
			this.checkBox2.Text = "Assert this user in case of incoming conflicts";
			this.checkBox2.UseVisualStyleBackColor = true;
			//
			// _repositoryAliases
			//
			this._repositoryAliases.Location = new System.Drawing.Point(13, 94);
			this._repositoryAliases.Multiline = true;
			this._repositoryAliases.Name = "_repositoryAliases";
			this._repositoryAliases.Size = new System.Drawing.Size(239, 78);
			this._repositoryAliases.TabIndex = 3;
			this._repositoryAliases.Validating += new System.ComponentModel.CancelEventHandler(this.OnRepositoryAliases_Validating);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(13, 42);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.label1.Size = new System.Drawing.Size(312, 49);
			this.label1.TabIndex = 4;
			this.label1.Text = "Repositories you sometimes synchronize with";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.AutoScroll = true;
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.checkBox2, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this._repositoryAliases, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.tableLayoutPanel1.RowCount = 9;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(505, 473);
			this.tableLayoutPanel1.TabIndex = 7;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(13, 175);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.label2.Size = new System.Drawing.Size(209, 69);
			this.label2.TabIndex = 7;
			this.label2.Text = "The pattern is \"name = address\", e.g.:\r\nsuzie = g:/LanguageProjects/TokPisin\r\nsil" +
				" = http://languageforge.org/TokPisin";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 42);
			this.label3.Name = "label3";
			this.label3.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.label3.Size = new System.Drawing.Size(479, 1);
			this.label3.TabIndex = 4;
			this.label3.Text = resources.GetString("label3.Text");
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 42);
			this.label4.Name = "label4";
			this.label4.Padding = new System.Windows.Forms.Padding(0, 30, 0, 10);
			this.label4.Size = new System.Drawing.Size(476, 1);
			this.label4.TabIndex = 4;
			this.label4.Text = resources.GetString("label4.Text");
			//
			// flowLayoutPanel1
			//
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.Controls.Add(this.label5);
			this.flowLayoutPanel1.Controls.Add(this._userName);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 13);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(479, 26);
			this.flowLayoutPanel1.TabIndex = 6;
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(75, 17);
			this.label5.TabIndex = 0;
			this.label5.Text = "User Name";
			//
			// _userName
			//
			this._userName.Location = new System.Drawing.Point(84, 3);
			this._userName.Name = "_userName";
			this._userName.Size = new System.Drawing.Size(100, 20);
			this._userName.TabIndex = 0;
			this._userName.Validating += new System.ComponentModel.CancelEventHandler(this.OnUserName_Validating);
			//
			// SettingsView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoScroll = true;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SettingsView";
			this.Size = new System.Drawing.Size(505, 473);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.TextBox _repositoryAliases;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox _userName;
		private System.Windows.Forms.Label label2;
	}
}