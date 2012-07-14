using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

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
			SetDocumentText("<html><head></head><body></body></html>");
			_newMessage.Font = model.FontForNewMessage;
		}

		protected void SetDocumentText(string text)
		{
			// GECKOFX: is this replace needed or is it already done within geckofx?
			text = text.Replace("'", "\'");
			try
			{
				_existingMessagesDisplay.LoadHtml("javascript:{document.body.outerHTML = '" + text + "';}");
			}
			catch (Exception e)
			{
				System.Console.WriteLine("AnnotationEditorView:SetDocumentText Exception caught: {0}", e.Message);
			}
		}

		public bool ModalDialogMode
		{
			set { _closeButton.Visible = value;}
			get{return _closeButton.Visible;}
		}

		public Button CloseButton
		{
			get { return _closeButton; }
		}

		void OnUpdateContent(object sender, EventArgs e)
		{
			if (_model.IsVisible)
			{
				_annotationLogo.Image = _model.GetAnnotationLogoImage();
				_annotationLabel.Text = _model.AnnotationLabel;
				SetDocumentText(_model.GetExistingMessagesHtml());
				_newMessage.Text = _model.NewMessageText;
			}
			OnUpdateStates(sender,e);
		}

		void OnUpdateStates(object sender, EventArgs e)
		{
			Visible = _model.IsVisible;
			if (_model.IsVisible)
			{
				_resolvedCheckBox.Checked = _model.IsResolved;
				_resolvedCheckBox.Visible = _model.ResolvedControlShouldBeVisible;
				_addButton.Enabled = _model.AddButtonEnabled;
				_addButton.Visible = _model.ShowNewMessageControls;
				_newMessage.Visible = _model.ShowNewMessageControls;
				_addNewMessageLabel.Visible = _model.ShowNewMessageControls;

				_closeButton.Text = _model.CloseButtonText;

				if (_model.ShowLabelAsHyperlink)
				{
					_annotationLabel.LinkBehavior = LinkBehavior.AlwaysUnderline;
				}
				else
				{
				   _annotationLabel.LinkBehavior = LinkBehavior.NeverUnderline;
				}
			}
		}

		private void AnnotationView_Load(object sender, EventArgs e)
		{
			_waitingOnBrowserToBeReady = true;
//            if(_model.IsVisible)
//                OnUpdateContent(null,null);
		}

		private void OnBrower_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			WebBrowser x = sender as WebBrowser;
			x.Document.BackColor = this.BackColor;
		}

		private void OnResolvedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			_model.IsResolved = (_resolvedCheckBox.Checked);
		}

		private void _addButton_Click(object sender, EventArgs e)
		{
			_model.AddButtonClicked();
			_newMessage.Text = _model.NewMessageText;

		}

		private void _newMessage_TextChanged(object sender, EventArgs e)
		{
			_model.NewMessageText = _newMessage.Text;
			OnUpdateStates(null,null);
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

		private void _existingMessagesDisplay_Navigating(object sender, Gecko.GeckoNavigatingEventArgs e)
		{
			if (e.Uri.Scheme == "about")
				return;
			e.Cancel = true;
			_model.HandleLinkClicked(e.Uri);
		}

		private void _existingMessagesDisplay_DocumentCompleted(object sender, EventArgs e)
		{
			if(_waitingOnBrowserToBeReady)
			{
				_waitingOnBrowserToBeReady = false;
				OnUpdateContent(null,null);
			}
			//GECKOFX: looks like previous code is trying to scroll to bottom child
			// does this do it for geckofx?
			_existingMessagesDisplay.Document.Body.ScrollIntoView (false);
/*
			var c = _existingMessagesDisplay.Document.Body.ChildNodes.Count;
			if (c > 0)
			{
				_existingMessagesDisplay.Document.Body.Children[c - 1].ScrollIntoView(false);
			}
			*/
		}

		private void _closeButton_Click(object sender, EventArgs e)
		{
			if(_addButton.Enabled )
			{
				_addButton_Click(sender, e);
			}
			if(OnClose!=null)
			{
				OnClose(sender, e);
			}
		}

		private void _annotationLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_model.JumpToAnnotationTarget();
		}

		private void _newMessage_Enter(object sender, EventArgs e)
		{
			_model.ActivateKeyboard();
		}

	}
}