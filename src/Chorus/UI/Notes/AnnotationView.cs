using System;
using System.Drawing;
using System.Windows.Forms;

namespace Chorus.UI.Notes
{
	public partial class AnnotationView : UserControl
	{
		private readonly AnnotationViewModel _model;

		public AnnotationView(AnnotationViewModel model)
		{
			_model = model;
			_model.UpdateContent += OnUpdateContent;
			_model.UpdateStates += OnUpdateStates;
			InitializeComponent();
			this.Visible = false;//wait for an annotation to be selected
		}

		void OnUpdateContent(object sender, EventArgs e)
		{
			_annotationLogo.Image = _model.GetAnnotationLogoImage();
			_annotationClassLabel.Text = _model.ClassLabel;

			this._existingMessagesHtmlView.DocumentText = _model.GetExistingMessagesHtml();

			_newMessage.Text = _model.NewMessageText;
			OnUpdateStates(sender,e);

			Visible = true;
		}

		void OnUpdateStates(object sender, EventArgs e)
		{
			_closedCheckBox.Checked = _model.IsClosed;
			_closedCheckBox.Visible = _model.ResolvedControlShouldBeVisible;
			_addButton.Enabled = _model.AddButtonEnabled;
			_addButton.Visible = _model.ShowNewMessageControls;
			_newMessage.Visible = _model.ShowNewMessageControls;
			_addNewMessageLabel.Visible = _model.ShowNewMessageControls;
		}

		private void AnnotationView_Load(object sender, EventArgs e)
		{

		}

		private void _existingMessagesHtmlView_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			_existingMessagesHtmlView.Document.BackColor = this.BackColor;
		}

		private void OnClosedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			_model.IsClosed = (_closedCheckBox.Checked);
		}

		private void _addButton_Click(object sender, EventArgs e)
		{
			_model.AddNewMessage();
			_newMessage.Text = _model.NewMessageText;
		}

		private void _newMessage_TextChanged(object sender, EventArgs e)
		{
			_model.NewMessageText = _newMessage.Text;
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

	}
}