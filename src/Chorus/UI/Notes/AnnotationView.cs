using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace Chorus.UI.Notes
{
	public partial class AnnotationView : UserControl
	{
		private readonly AnnotationEditorModel _model;
		private bool _waitingOnBrowserToBeReady;
		public EventHandler OnClose;
		public AnnotationView(AnnotationEditorModel model, bool modalDialogMode)
		{
			_model = model;
			_model.UpdateContent += OnUpdateContent;
			_model.UpdateStates += OnUpdateStates;
			InitializeComponent();
			Visible = model.IsVisible;
			_closeButton.Visible = modalDialogMode;
			//needs to be primed this way
			_existingMessagesDisplay.DocumentText = "<html></html>";
		}

		public Button CloseButton
		{
			get { return _closeButton; }
		}

		void OnUpdateContent(object sender, EventArgs e)
		{
			_annotationLogo.Image = _model.GetAnnotationLogoImage();
			_annotationClassLabel.Text = _model.ClassLabel;

			_existingMessagesDisplay.DocumentText = _model.GetExistingMessagesHtml();

//            _messagesPanel.SuspendLayout();
//            foreach (Control control in _messagesPanel.Controls)
//            {
//                //nb: Clear() doesn't dispose, so we have to go through this
//                _messagesPanel.Controls.RemoveAllItemsWithKey(control);
//                var d = control as IDisposable;
//                if(d!=null)
//                    d.Dispose();
//            }
//
//            foreach (var message in _model.Messages)
//            {
//                _messagesPanel.Controls.Add(_model.GetControlForMessage(message));
//            }

//            _messagesPanel.ResumeLayout();

			_newMessage.Text = _model.NewMessageText;
			OnUpdateStates(sender,e);

		}

		void OnUpdateStates(object sender, EventArgs e)
		{
			_closedCheckBox.Checked = _model.IsClosed;
			_closedCheckBox.Visible = _model.ResolvedControlShouldBeVisible;
			_addButton.Enabled = _model.AddButtonEnabled;
			_addButton.Visible = _model.ShowNewMessageControls;
			_newMessage.Visible = _model.ShowNewMessageControls;
			_addNewMessageLabel.Visible = _model.ShowNewMessageControls;
			Visible = _model.IsVisible;

			_closeButton.Text = _model.CloseButtonText;
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

		private void OnClosedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			_model.IsClosed = (_closedCheckBox.Checked);
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
			if (_model.IsClosed)
			{
				e.Graphics.DrawImage(Properties.Resources.check16x16, new Rectangle(2, 2, 28, 28));
			}
		}

		private void _annotationLogo_DoubleClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			var dlg = new AnnotationInspector(_model.CurrentAnnotation);
			dlg.ShowDialog();
			Cursor.Current = Cursors.Default;
		}

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

	}
}