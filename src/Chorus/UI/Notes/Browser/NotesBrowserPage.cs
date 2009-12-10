using System.Drawing;
using System.Windows.Forms;
using Chorus.Utilities;


namespace Chorus.UI.Notes.Browser
{
	public partial class NotesBrowserPage : UserControl
	{
		public delegate NotesBrowserPage Factory();//autofac uses this

		public NotesBrowserPage(NotesInProjectViewModel.Factory notesInProjectViewModelFactory, AnnotationEditorView annotationView)
		{
			InitializeComponent();
			this.Font = SystemFonts.MessageBoxFont;

			SuspendLayout();
			annotationView.ModalDialogMode = false;
			annotationView.Dock = DockStyle.Fill;
			annotationView.ModalDialogMode = false;
			annotationView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			splitContainer1.Panel2.Padding = new Padding(3,34,3,3);//drop it below the search box of the other pain

			var notesInProjectModel = notesInProjectViewModelFactory(new NullProgress());
			var notesInProjectView = new NotesInProjectView(notesInProjectModel);
			notesInProjectView.Dock = DockStyle.Fill;
			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(annotationView);
			ResumeLayout();
		}
	}
}