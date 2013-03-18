using System.Windows.Forms;

namespace SampleApp
{
	partial class Form1
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this._frontPage = new System.Windows.Forms.TabPage();
			this._notesPage = new System.Windows.Forms.TabPage();
			this._historyPage = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this._userPicker = new System.Windows.Forms.ComboBox();
			this._viewTestDataDirectory = new System.Windows.Forms.LinkLabel();
			this._syncButton = new System.Windows.Forms.Button();
			this._helpProvider = new Vulcan.Uczniowie.HelpProvider.HelpComponent(this.components);
			this.button1 = new Button();
			this.tabControl1.SuspendLayout();
			this.SuspendLayout();
			//
			// tabControl1
			//
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this._frontPage);
			this.tabControl1.Controls.Add(this._notesPage);
			this.tabControl1.Controls.Add(this._historyPage);
			this.tabControl1.Location = new System.Drawing.Point(0, 41);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(620, 357);
			this.tabControl1.TabIndex = 0;
			//
			// _frontPage
			//
			this._frontPage.Location = new System.Drawing.Point(4, 22);
			this._frontPage.Name = "_frontPage";
			this._frontPage.Padding = new System.Windows.Forms.Padding(3);
			this._frontPage.Size = new System.Drawing.Size(612, 331);
			this._frontPage.TabIndex = 0;
			this._frontPage.Text = "Data";
			this._frontPage.UseVisualStyleBackColor = true;
			//
			// _notesPage
			//
			this._notesPage.Location = new System.Drawing.Point(4, 22);
			this._notesPage.Name = "_notesPage";
			this._notesPage.Padding = new System.Windows.Forms.Padding(3);
			this._notesPage.Size = new System.Drawing.Size(612, 331);
			this._notesPage.TabIndex = 1;
			this._notesPage.Text = "Notes";
			this._notesPage.UseVisualStyleBackColor = true;
			//
			// _historyPage
			//
			this._historyPage.Location = new System.Drawing.Point(4, 22);
			this._historyPage.Name = "_historyPage";
			this._historyPage.Padding = new System.Windows.Forms.Padding(3);
			this._historyPage.Size = new System.Drawing.Size(612, 331);
			this._historyPage.TabIndex = 2;
			this._historyPage.Text = "History";
			this._historyPage.UseVisualStyleBackColor = true;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "User:";
			//
			// _userPicker
			//
			this._userPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._userPicker.FormattingEnabled = true;
			this._userPicker.Items.AddRange(new object[] {
			"Bob",
			"Sue"});
			this._userPicker.Location = new System.Drawing.Point(60, 6);
			this._userPicker.Name = "_userPicker";
			this._userPicker.Size = new System.Drawing.Size(181, 21);
			this._userPicker.TabIndex = 2;
			this._userPicker.SelectedIndexChanged += new System.EventHandler(this._userPicker_SelectedIndexChanged);
			//
			// _viewTestDataDirectory
			//
			this._viewTestDataDirectory.AutoSize = true;
			this._viewTestDataDirectory.Location = new System.Drawing.Point(290, 13);
			this._viewTestDataDirectory.Name = "_viewTestDataDirectory";
			this._viewTestDataDirectory.Size = new System.Drawing.Size(125, 13);
			this._viewTestDataDirectory.TabIndex = 3;
			this._viewTestDataDirectory.TabStop = true;
			this._viewTestDataDirectory.Text = "View Test Data Directory";
			this._viewTestDataDirectory.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._viewTestDataDirectory_LinkClicked);
			//
			// _syncButton
			//
			this._syncButton.Location = new System.Drawing.Point(511, 9);
			this._syncButton.Name = "_syncButton";
			this._syncButton.Size = new System.Drawing.Size(105, 31);
			this._syncButton.TabIndex = 4;
			this._syncButton.Text = "Send/Receive";
			this._syncButton.UseVisualStyleBackColor = true;
			this._syncButton.Click += new System.EventHandler(this.OnSendReceiveClick);
			//
			// button1
			//
			this.button1.Location = new System.Drawing.Point(632, 9);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(105, 48);
			this.button1.TabIndex = 6;
			this.button1.Text = "Quiet background checkin";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(620, 398);
			this.Controls.Add(this.button1);
			this.Controls.Add(this._syncButton);
			this.Controls.Add(this._viewTestDataDirectory);
			this.Controls.Add(this._userPicker);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tabControl1);
			this.Name = "Form1";
			this.Text = "Chorus Sample App";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.tabControl1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage _frontPage;
		private System.Windows.Forms.TabPage _notesPage;
		private System.Windows.Forms.TabPage _historyPage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _userPicker;
		private System.Windows.Forms.LinkLabel _viewTestDataDirectory;
		private System.Windows.Forms.Button _syncButton;
		private System.Windows.Forms.Button button1;
		private Vulcan.Uczniowie.HelpProvider.HelpComponent _helpProvider;
	}
}
