using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.notes;

namespace Chorus.UI.Notes.Bar
{
	public partial class NoteDetailDialog : Form
	{
		public NoteDetailDialog(Annotation annotation, AnnotationEditorModel.Factory viewModelFactory)
		{
			InitializeComponent();
			Text = String.Format("{0} on {1}", annotation.ClassName, annotation.LabelOfThingAnnotated);
			var model = viewModelFactory(annotation);
			var view = new AnnotationEditorView(model);
			view.ModalDialogMode = true;
			view.Dock = DockStyle.Fill;
			//view.Size  = new Size(Width, Height - 50);
			Controls.Add(view);
			AcceptButton = view.CloseButton;
			view.OnClose += (CloseButton_Click);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			//this relies on us being the second receiver of this message, after the view itself
			this.Close();
		}

		private void NoteDetailDialog_Load(object sender, EventArgs e)
		{

		}
	}
}