using System;
using System.Windows.Forms;
using Chorus.notes;

namespace Chorus.UI.Notes.Bar
{
	public partial class NoteDetailDialog : Form
	{
		AnnotationEditorView _view;

		public NoteDetailDialog(Annotation annotation, AnnotationEditorModel.Factory viewModelFactory)
		{
			InitializeComponent();
			var model = viewModelFactory(annotation, false);
			Text = model.GetLongLabel();
			_view = new AnnotationEditorView(model);
			_view.ModalDialogMode = true;
			_view.Dock = DockStyle.Fill;
			//_view.Size  = new Size(Width, Height - 50);
			Controls.Add(_view);
			AcceptButton = _view.OKButton;
			_view.OnClose += (CloseButton_Click);
		}

		/// <summary> Sets the DialogResult and closes the dialog </summary>
		/// <param name="sender">must be a DialogResult</param>
		/// <param name="e"></param>
		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = (DialogResult) sender;
			//this relies on us being the second receiver of this message, after the view itself
			this.Close();
		}

		internal IWritingSystem LabelWritingSystem
		{
			set { _view.LabelWritingSystem = value; }
		}

		internal IWritingSystem MessageWritingSystem
		{
			set { _view.MessageWritingSystem = value; }
		}
	}
}