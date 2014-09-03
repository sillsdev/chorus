using Palaso.UI.WindowsForms.HtmlBrowser;

namespace Chorus.UI.Notes
{
	partial class AnnotationEditorView
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
			if (disposing)
			{
				if (_existingMessagesDisplay != null)
					_existingMessagesDisplay.Dispose();
				if (components != null)
					components.Dispose();
			}
			_existingMessagesDisplay = null;
			if (_tempPath != null)
				System.IO.File.Delete(_tempPath);
			_tempPath = null;
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
			this._okButton = new System.Windows.Forms.Button();
			this._resolveButton = new System.Windows.Forms.Button();
			this._closeButton = new System.Windows.Forms.Button();
			this._newMessage = new System.Windows.Forms.TextBox();
			this._annotationLogo = new System.Windows.Forms.PictureBox();
			this._existingMessagesDisplay = new XWebBrowser();
			this._annotationLabel = new System.Windows.Forms.LinkLabel();
			this._addNewMessageLabel = new Chorus.UI.BetterLabel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._annotationLogo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._okButton, "AnnotationEditorView.Add");
			this._okButton.Location = new System.Drawing.Point(159, 389);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(90, 23);
			this._okButton.TabIndex = 1;
			this._okButton.Text = "&Add && &OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _resolveButton
			//
			this._resolveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._resolveButton.AutoSize = true;
			this._resolveButton.Image = global::Chorus.Properties.Resources.check12x12;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._resolveButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._resolveButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._resolveButton, "AnnotationEditorView.button1");
			this._resolveButton.Location = new System.Drawing.Point(5, 389);
			this._resolveButton.Name = "_resolveButton";
			this._resolveButton.Size = new System.Drawing.Size(106, 23);
			this._resolveButton.TabIndex = 2;
			this._resolveButton.Text = "&Resolve Note";
			this._resolveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._resolveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._resolveButton.UseVisualStyleBackColor = true;
			this._resolveButton.Click += new System.EventHandler(this._resolveButton_Click);
			//
			// _closeButton
			//
			this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._closeButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._closeButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._closeButton, "AnnotationEditorView.AddAndOk");
			this._closeButton.Location = new System.Drawing.Point(255, 389);
			this._closeButton.Name = "_closeButton";
			this._closeButton.Size = new System.Drawing.Size(63, 23);
			this._closeButton.TabIndex = 3;
			this._closeButton.Text = "&Cancel";
			this._closeButton.UseVisualStyleBackColor = true;
			this._closeButton.VisibleChanged += new System.EventHandler(this._closeButton_VisibleChanged);
			this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
			//
			// _newMessage
			//
			this._newMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._newMessage, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._newMessage, null);
			this.l10NSharpExtender1.SetLocalizingId(this._newMessage, "AnnotationEditorView.AnnotationEditorView._newMessage");
			this._newMessage.Location = new System.Drawing.Point(7, 338);
			this._newMessage.Multiline = true;
			this._newMessage.Name = "_newMessage";
			this._newMessage.Size = new System.Drawing.Size(311, 45);
			this._newMessage.TabIndex = 0;
			this._newMessage.Enter += new System.EventHandler(this._newMessage_Enter);
			//
			// _annotationLogo
			//
			this._annotationLogo.Image = global::Chorus.Properties.Resources.NewNote16x16;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._annotationLogo, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._annotationLogo, null);
			this.l10NSharpExtender1.SetLocalizingId(this._annotationLogo, "AnnotationEditorView.AnnotationEditorView._annotationLogo");
			this._annotationLogo.Location = new System.Drawing.Point(5, 8);
			this._annotationLogo.Name = "_annotationLogo";
			this._annotationLogo.Size = new System.Drawing.Size(32, 32);
			this._annotationLogo.TabIndex = 10;
			this._annotationLogo.TabStop = false;
			this._annotationLogo.Paint += new System.Windows.Forms.PaintEventHandler(this._annotationLogo_Paint);
			this._annotationLogo.DoubleClick += new System.EventHandler(this._annotationLogo_DoubleClick);
			//
			// _existingMessagesDisplay
			//
			this._existingMessagesDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._existingMessagesDisplay, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._existingMessagesDisplay, null);
			this.l10NSharpExtender1.SetLocalizingId(this._existingMessagesDisplay, "AnnotationEditorView.AnnotationEditorView._existingMessagesDisplay");
			this._existingMessagesDisplay.Location = new System.Drawing.Point(5, 47);
			this._existingMessagesDisplay.MinimumSize = new System.Drawing.Size(20, 20);
			this._existingMessagesDisplay.Name = "_existingMessagesDisplay";
			this._existingMessagesDisplay.Size = new System.Drawing.Size(313, 264);
			this._existingMessagesDisplay.TabIndex = 9;
			this._existingMessagesDisplay.AllowWebBrowserDrop = false;
			this._existingMessagesDisplay.WebBrowserShortcutsEnabled = false;
			this._existingMessagesDisplay.IsWebBrowserContextMenuEnabled = false;

			//
			// _annotationLabel
			//
			this._annotationLabel.AutoSize = true;
			this._annotationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._annotationLabel.LinkColor = System.Drawing.Color.Black;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._annotationLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._annotationLabel, null);
			this.l10NSharpExtender1.SetLocalizationPriority(this._annotationLabel, L10NSharp.LocalizationPriority.NotLocalizable);
			this.l10NSharpExtender1.SetLocalizingId(this._annotationLabel, "AnnotationEditorView.AnnotationLabel");
			this._annotationLabel.Location = new System.Drawing.Point(43, 8);
			this._annotationLabel.Name = "_annotationLabel";
			this._annotationLabel.Size = new System.Drawing.Size(129, 24);
			this._annotationLabel.TabIndex = 10;
			this._annotationLabel.TabStop = true;
			this._annotationLabel.Text = "Target of Note";
			this._annotationLabel.UseMnemonic = false;
			this._annotationLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._annotationLabel_LinkClicked);
			//
			// _addNewMessageLabel
			//
			this._addNewMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._addNewMessageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._addNewMessageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.l10NSharpExtender1.SetLocalizableToolTip(this._addNewMessageLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._addNewMessageLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._addNewMessageLabel, "AnnotationEditorView.AddNewMessage");
			this._addNewMessageLabel.Location = new System.Drawing.Point(7, 320);
			this._addNewMessageLabel.Multiline = true;
			this._addNewMessageLabel.Name = "_addNewMessageLabel";
			this._addNewMessageLabel.ReadOnly = true;
			this._addNewMessageLabel.Size = new System.Drawing.Size(293, 15);
			this._addNewMessageLabel.TabIndex = 11;
			this._addNewMessageLabel.TabStop = false;
			this._addNewMessageLabel.Text = "Add new message:";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Chorus";
			this.l10NSharpExtender1.PrefixForNewItems = "AnnotationEditorView";
			//
			// AnnotationEditorView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._resolveButton);
			this.Controls.Add(this._closeButton);
			this.Controls.Add(this._annotationLabel);
			this.Controls.Add(this._existingMessagesDisplay);
			this.Controls.Add(this._newMessage);
			this.Controls.Add(this._addNewMessageLabel);
			this.Controls.Add(this._annotationLogo);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "AnnotationEditorView.AnnotationEditorView.AnnotationEditorView");
			this.Name = "AnnotationEditorView";
			this.Size = new System.Drawing.Size(321, 415);
			this.Load += new System.EventHandler(this.AnnotationView_Load);
			((System.ComponentModel.ISupportInitialize)(this._annotationLogo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _annotationLogo;
		private BetterLabel _addNewMessageLabel;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Button _resolveButton;
		private System.Windows.Forms.Button _closeButton;
		private System.Windows.Forms.TextBox _newMessage;
		private Palaso.UI.WindowsForms.HtmlBrowser.XWebBrowser _existingMessagesDisplay;
		private System.Windows.Forms.LinkLabel _annotationLabel;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}
