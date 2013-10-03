using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Chorus.notes;

namespace Chorus.UI.Notes
{
	public partial class AnnotationEditorView : UserControl
	{
		private readonly AnnotationEditorModel _model;
		private bool _waitingOnBrowserToBeReady;
		public EventHandler OnClose;
		public AnnotationEditorView(AnnotationEditorModel model)
		{
			_model = model;
			_model.UpdateContent += OnUpdateContent;
			_model.UpdateStates += OnUpdateStates;
			InitializeComponent();
			Visible = model.IsVisible;
			ModalDialogMode = true;
			//needs to be primed this way
			SetDocumentText(@"<html><head></head><body></body></html>");
			_newMessage.Font = model.FontForNewMessage;

			//a signal to keep palaso localiztion helper from messing with our font
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
#if MONO
			_existingMessagesDisplay.LoadHtml(text);
#else
			_existingMessagesDisplay.DocumentText = text;
#endif
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
			else
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
			_waitingOnBrowserToBeReady = true;
		}

		private void OnBrower_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			WebBrowser x = sender as WebBrowser;
			x.Document.BackColor = this.BackColor;
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
			var dlg = new AnnotationInspector(_model.Annotation);
			dlg.ShowDialog();
			Cursor.Current = Cursors.Default;
		}

#if MONO
		private void _existingMessagesHandleLinkClick(object sender, Gecko.GeckoDomEventArgs e)
		{
			Gecko.GeckoHtmlElement clicked = e.Target;
			if(clicked != null && clicked.TagName == "A")
			{
				e.Handled = true;
				_model.HandleLinkClicked(new Uri(clicked.GetAttribute("href")));
			}
		}

		private void _existingMessagesDisplay_DocumentCompleted(object sender, EventArgs e)
		{
			if (_waitingOnBrowserToBeReady)
			{
				_waitingOnBrowserToBeReady = false;
				OnUpdateContent(null,null);
			}
			// The Windows/.Net code appears to be trying to scroll to the bottom child.
			// This has much the same effect for Gecko.
			_existingMessagesDisplay.Document.Body.ScrollIntoView(false);
		}
#else
		private void _existingMessagesDisplay_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			if (e.Url.Scheme == "about")
				return;
			e.Cancel = true;
			_model.HandleLinkClicked(e.Url);
		}

		private void _existingMessagesDisplay_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if(_waitingOnBrowserToBeReady)
			{
				_waitingOnBrowserToBeReady = false;
				OnUpdateContent(null,null);
			}

			var c = _existingMessagesDisplay.Document.Body.Children.Count;
			if (c > 0)
			{
				_existingMessagesDisplay.Document.Body.Children[c - 1].ScrollIntoView(false);
			}
		}
#endif

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
