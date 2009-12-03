using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chorus.annotations;

namespace Chorus.UI.Notes.Bar
{
	public partial class NoteDetailDialog : Form
	{
		public NoteDetailDialog(Annotation annotation, AnnotationViewModel.Factory viewModelFactory)
		{
			InitializeComponent();
			Text = String.Format("{0} on {1}", annotation.ClassName, annotation.LabelOfThingAnnotated);
			var model = viewModelFactory(annotation);
			var view = new AnnotationView(model);
			view.Dock = DockStyle.Fill;
			Controls.Add(view);
		}

		private void NoteDetailDialog_Load(object sender, EventArgs e)
		{

		}
	}
}