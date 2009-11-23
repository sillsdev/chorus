namespace Chorus.UI.Notes
{
	partial class AnnotationView
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
			this._annotationLogo = new System.Windows.Forms.PictureBox();
			this._existingMessagesHtmlView = new System.Windows.Forms.WebBrowser();
			this._resolvedCheckBox = new System.Windows.Forms.CheckBox();
			this._addButton = new System.Windows.Forms.Button();
			this._newMessage = new System.Windows.Forms.TextBox();
			this._annotationClassLabel = new System.Windows.Forms.Label();
			this._annotationDetailsLabel = new System.Windows.Forms.Label();
			this.betterLabel1 = new Chorus.UI.BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this._annotationLogo)).BeginInit();
			this.SuspendLayout();
			//
			// _annotationLogo
			//
			this._annotationLogo.Image = global::Chorus.Properties.Resources.question32x32;
			this._annotationLogo.Location = new System.Drawing.Point(5, 3);
			this._annotationLogo.Name = "_annotationLogo";
			this._annotationLogo.Size = new System.Drawing.Size(32, 32);
			this._annotationLogo.TabIndex = 1;
			this._annotationLogo.TabStop = false;
			//
			// _existingMessagesHtmlView
			//
			this._existingMessagesHtmlView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._existingMessagesHtmlView.Location = new System.Drawing.Point(0, 41);
			this._existingMessagesHtmlView.MinimumSize = new System.Drawing.Size(20, 20);
			this._existingMessagesHtmlView.Name = "_existingMessagesHtmlView";
			this._existingMessagesHtmlView.Size = new System.Drawing.Size(318, 243);
			this._existingMessagesHtmlView.TabIndex = 0;
			this._existingMessagesHtmlView.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this._existingMessagesHtmlView_DocumentCompleted);
			//
			// _resolvedCheckBox
			//
			this._resolvedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._resolvedCheckBox.AutoSize = true;
			this._resolvedCheckBox.Location = new System.Drawing.Point(3, 386);
			this._resolvedCheckBox.Name = "_resolvedCheckBox";
			this._resolvedCheckBox.Size = new System.Drawing.Size(71, 17);
			this._resolvedCheckBox.TabIndex = 3;
			this._resolvedCheckBox.Text = "Resolved";
			this._resolvedCheckBox.UseVisualStyleBackColor = true;
			//
			// _addButton
			//
			this._addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._addButton.Location = new System.Drawing.Point(231, 386);
			this._addButton.Name = "_addButton";
			this._addButton.Size = new System.Drawing.Size(75, 23);
			this._addButton.TabIndex = 6;
			this._addButton.Text = "Add";
			this._addButton.UseVisualStyleBackColor = true;
			//
			// _newMessage
			//
			this._newMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._newMessage.Location = new System.Drawing.Point(4, 325);
			this._newMessage.Multiline = true;
			this._newMessage.Name = "_newMessage";
			this._newMessage.Size = new System.Drawing.Size(302, 45);
			this._newMessage.TabIndex = 7;
			//
			// _annotationClassLabel
			//
			this._annotationClassLabel.AutoSize = true;
			this._annotationClassLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._annotationClassLabel.Location = new System.Drawing.Point(44, -3);
			this._annotationClassLabel.Name = "_annotationClassLabel";
			this._annotationClassLabel.Size = new System.Drawing.Size(86, 25);
			this._annotationClassLabel.TabIndex = 8;
			this._annotationClassLabel.Text = "TheClass";
			//
			// _annotationDetailsLabel
			//
			this._annotationDetailsLabel.AutoSize = true;
			this._annotationDetailsLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._annotationDetailsLabel.ForeColor = System.Drawing.Color.Gray;
			this._annotationDetailsLabel.Location = new System.Drawing.Point(48, 21);
			this._annotationDetailsLabel.Name = "_annotationDetailsLabel";
			this._annotationDetailsLabel.Size = new System.Drawing.Size(41, 15);
			this._annotationDetailsLabel.TabIndex = 9;
			this._annotationDetailsLabel.Text = "details";
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.Location = new System.Drawing.Point(4, 298);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(293, 20);
			this.betterLabel1.TabIndex = 5;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Add new message:";
			//
			// AnnotationView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._annotationDetailsLabel);
			this.Controls.Add(this._annotationClassLabel);
			this.Controls.Add(this._newMessage);
			this.Controls.Add(this._addButton);
			this.Controls.Add(this.betterLabel1);
			this.Controls.Add(this._resolvedCheckBox);
			this.Controls.Add(this._annotationLogo);
			this.Controls.Add(this._existingMessagesHtmlView);
			this.Name = "AnnotationView";
			this.Size = new System.Drawing.Size(321, 415);
			this.Load += new System.EventHandler(this.AnnotationView_Load);
			((System.ComponentModel.ISupportInitialize)(this._annotationLogo)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.WebBrowser _existingMessagesHtmlView;
		private System.Windows.Forms.PictureBox _annotationLogo;
		private System.Windows.Forms.CheckBox _resolvedCheckBox;
		private BetterLabel betterLabel1;
		private System.Windows.Forms.Button _addButton;
		private System.Windows.Forms.TextBox _newMessage;
		private System.Windows.Forms.Label _annotationClassLabel;
		private System.Windows.Forms.Label _annotationDetailsLabel;
	}
}