namespace Chorus.UI.Misc
{
	partial class ProxySettingsView
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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this._host = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._userName = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this._password = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label5 = new System.Windows.Forms.Label();
			this._port = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this._bypassList = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "HTTP Proxy";
			//
			// _host
			//
			this._host.Location = new System.Drawing.Point(85, 20);
			this._host.Name = "_host";
			this._host.Size = new System.Drawing.Size(122, 20);
			this._host.TabIndex = 0;
			this.toolTip1.SetToolTip(this._host, "Host name and (optionally) port of the proxy server, for example \"myproxy:8000\".");
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 58);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "User Name";
			//
			// _userName
			//
			this._userName.Location = new System.Drawing.Point(85, 55);
			this._userName.Name = "_userName";
			this._userName.Size = new System.Drawing.Size(122, 20);
			this._userName.TabIndex = 2;
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 89);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(53, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Password";
			//
			// _password
			//
			this._password.Location = new System.Drawing.Point(85, 85);
			this._password.Name = "_password";
			this._password.PasswordChar = '*';
			this._password.Size = new System.Drawing.Size(122, 20);
			this._password.TabIndex = 3;
			//
			// label5
			//
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(213, 26);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(26, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Port";
			//
			// _port
			//
			this._port.Location = new System.Drawing.Point(247, 21);
			this._port.Name = "_port";
			this._port.Size = new System.Drawing.Size(51, 20);
			this._port.TabIndex = 1;
			this._port.Text = "80";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(209, 60);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(52, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "(Optional)";
			//
			// label6
			//
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(209, 86);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(52, 13);
			this.label6.TabIndex = 0;
			this.label6.Text = "(Optional)";
			//
			// label7
			//
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(8, 131);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(68, 13);
			this.label7.TabIndex = 0;
			this.label7.Text = "No Proxy For";
			//
			// _bypassList
			//
			this._bypassList.Location = new System.Drawing.Point(85, 128);
			this._bypassList.Multiline = true;
			this._bypassList.Name = "_bypassList";
			this._bypassList.Size = new System.Drawing.Size(206, 49);
			this._bypassList.TabIndex = 4;
			//
			// ProxySettingsView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.Controls.Add(this._password);
			this.Controls.Add(this._bypassList);
			this.Controls.Add(this._userName);
			this.Controls.Add(this._port);
			this.Controls.Add(this._host);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label1);
			this.Name = "ProxySettingsView";
			this.Size = new System.Drawing.Size(345, 279);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _host;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _userName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox _password;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox _port;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox _bypassList;
	}
}
