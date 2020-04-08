using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Chorus.notes;

namespace Chorus.UI.Notes
{
	public partial class AnnotationEditorView : UserControl
	{
		private readonly AnnotationEditorModel _model;
		private string _tempPath;
		public EventHandler OnClose;
		public AnnotationEditorView(AnnotationEditorModel model)
		{
			_model = model;
			_model.UpdateContent += OnUpdateContent;
			_model.UpdateStates += OnUpdateStates;
			InitializeComponent();
			_existingMessagesDisplay.Navigating += _existingMessagesDisplay_Navigating;
			Visible = model.IsVisible;
			ModalDialogMode = true;
			_newMessage.Font = model.FontForNewMessage;

			//a signal to keep palaso localization helper from messing with our font
			_annotationLabel.UseMnemonic = false;

			_annotationLabel.Font = model.FontForLabel;
		}

		public MessageSelectedEvent EventToRaiseForChangedMessage
		{
			get { return _model.EventToRaiseForChangedMessage; }
		}

		public EmbeddedMessageContentHandlerRepository MesageContentHandlerRepository
		{
			get { return _model.MessageContentHandlerRepository; }
		}


		protected void SetDocumentText(string text)
		{
			// It would be nice to avoid writing a temp file and then loading it,
			// but the Linux gecko didn't display anything with the code that sent
			// the html text directly to the embedded browser. In the interest of
			// keeping the code the same for all platforms, the approach below is
			// now used.  See https://jira.sil.org/browse/WS-139 for details of
			// what was happening.
			if (_tempPath == null)
				_tempPath = SIL.IO.TempFile.WithExtension("htm").Path;
			System.IO.File.WriteAllText(_tempPath, text);
			_existingMessagesDisplay.Url = new Uri(_tempPath);
		}

		public bool ModalDialogMode
		{
			get { return _closeButton.Visible; }
			set { _closeButton.Visible = value; }
		}

		/// <summary>
		/// Allows client code to access the OK button when presenting this control as a modal dialog
		/// </summary>
		public Button OKButton
		{
			get { return _okButton; }
		}

		public bool NewMessageTextEntered
		{
			get { return !String.IsNullOrWhiteSpace(_newMessage.Text); }
		}

		public void AddMessage()
		{
			if (NewMessageTextEntered)
				_model.AddMessage(_newMessage.Text);
			ClearNewMessageText();
		}

		private void UnResolveAndAddMessage()
		{
			if (NewMessageTextEntered)
				_model.UnResolveAndAddMessage(_newMessage.Text);
			else if (_model.Messages.Any()) // don't resolve an empty Note
				_model.IsResolved = !_model.IsResolved;
			ClearNewMessageText();
		}

		public void ClearNewMessageText()
		{
			_newMessage.Text = String.Empty;
		}

		void OnUpdateContent(object sender, EventArgs e)
		{
			if (_model.IsVisible)
			{
				_annotationLogo.Image = _model.GetAnnotationLogoImage();
				_annotationLabel.Text = _model.AnnotationLabel;
				SetDocumentText(_model.GetExistingMessagesHtml());
			}
			OnUpdateStates(sender,e);
		}

		void OnUpdateStates(object sender, EventArgs e)
		{
			Visible = _model.IsVisible;
			if (_model.IsVisible)
			{
				_resolveButton.Visible = _model.ResolvedControlShouldBeVisible;
				_resolveButton.Text = _model.ResolveButtonText;

				_annotationLabel.LinkBehavior = _model.ShowLabelAsHyperlink ?
					LinkBehavior.AlwaysUnderline : LinkBehavior.NeverUnderline;
			}
		}

		private void AnnotationView_Load(object sender, EventArgs e)
		{
			OnUpdateContent(null,null);
			_existingMessagesDisplay.ScrollLastElementIntoView();
		}

		private void OnBrower_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			WebBrowser x = sender as WebBrowser;
			x.Document.BackColor = this.BackColor;
		}

		private void _existingMessagesDisplay_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			if (e.Url.Scheme == "about" || e.Url.Scheme == "file")
				return;
			e.Cancel = true;
			_model.HandleLinkClicked(e.Url);
		}

		private void _closeButton_VisibleChanged(object sender, EventArgs e)
		{
			_okButton.Text = _model.GetOKButtonText(_closeButton.Visible);
			if (!_closeButton.Visible)
			{
				// if the close button isn't visible, move the Add (OK) button over
				_okButton.Location = new Point(
					_closeButton.Location.X + _closeButton.Size.Width - _okButton.Size.Width,
					_closeButton.Location.Y);
			}
			// No need for an else clause to move the button back, b/c _closeButton.Visible is set only once.
		}

		private void _resolveButton_Click(object sender, EventArgs e)
		{
			UnResolveAndAddMessage();

			if (ModalDialogMode)
				_closeButton_Click(DialogResult.OK, e);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			if (ModalDialogMode)
			{
				// We will close the dialog, so we don't need to update the contents of the controls;
				// and doing so on Mono crashes Gecko, apparently because the window is gone before the
				// update completes. We get stack overflow somehow, anyway.
				_model.UpdateContent -= OnUpdateContent;
			}

			AddMessage();

			if (ModalDialogMode)
				_closeButton_Click(DialogResult.OK, e);
		}

		// Close without saving
		private void _closeButton_Click(object sender, EventArgs e)
		{
			if (!(sender is DialogResult))
				sender = DialogResult.Cancel;

			if(OnClose!=null)
				OnClose(sender, e);
		}

		private void _annotationLogo_Paint(object sender, PaintEventArgs e)
		{
			if (_model.IsResolved)
			{
				e.Graphics.DrawImage(Properties.Resources.check16x16, new Rectangle(2, 2, 28, 28));
			}
		}

		private void _annotationLogo_DoubleClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			using (var dlg = new AnnotationInspector(_model.Annotation))
			{
				dlg.ShowDialog();
			}
			Cursor.Current = Cursors.Default;
		}

		private void _annotationLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_model.JumpToAnnotationTarget();
		}

		private void _newMessage_Enter(object sender, EventArgs e)
		{
			_model.ActivateKeyboard();
		}

		internal IWritingSystem LabelWritingSystem
		{
			set
			{
				_model.LabelWritingSystem = value;
				_annotationLabel.Font = _model.FontForLabel;
			}
		}

		internal IWritingSystem MessageWritingSystem
		{
			set
			{
				_model.MessageWritingSystem = value;
				_newMessage.Font = _model.FontForNewMessage;
			}
		}
	}
}
