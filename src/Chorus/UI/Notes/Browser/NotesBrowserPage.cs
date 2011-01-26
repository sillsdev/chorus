using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Chorus.Utilities;
using Palaso.Progress.LogBox;


namespace Chorus.UI.Notes.Browser
{
	public partial class NotesBrowserPage : UserControl
	{
		public delegate NotesBrowserPage Factory(IEnumerable<AnnotationRepository> repositories);//autofac uses this

		internal NotesInProjectViewModel _notesInProjectModel;

		public NotesBrowserPage(NotesInProjectViewModel.Factory notesInProjectViewModelFactory, IEnumerable<AnnotationRepository> repositories, AnnotationEditorView annotationView)
		{
			InitializeComponent();
			this.Font = SystemFonts.MessageBoxFont;

			SuspendLayout();
			annotationView.ModalDialogMode = false;
			annotationView.Dock = DockStyle.Fill;
			annotationView.ModalDialogMode = false;
			annotationView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			splitContainer1.Panel2.Padding = new Padding(3,34,3,3);//drop it below the search box of the other pain

			_notesInProjectModel = notesInProjectViewModelFactory(repositories, new NullProgress());
			var notesInProjectView = new NotesInProjectView(_notesInProjectModel);
			notesInProjectView.Dock = DockStyle.Fill;
			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(annotationView);
			ResumeLayout();
		}
	}
}