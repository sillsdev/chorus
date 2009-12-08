using System.Drawing;
using System.Windows.Forms;


namespace Chorus.UI.Notes
{
	public partial class NotesPage : UserControl
	{

		public NotesPage(NotesInProjectView notesInProjectView, AnnotationView annotationView)
		{
			InitializeComponent();
			this.Font = SystemFonts.MessageBoxFont;

			SuspendLayout();
			annotationView.ModalDialogMode = false;
			annotationView.Dock = DockStyle.Fill;
			annotationView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			splitContainer1.Panel2.Padding = new Padding(3,34,3,3);//drop it below the search box of the other pain

			notesInProjectView.Dock = DockStyle.Fill;
			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(annotationView);
			ResumeLayout();
		}
	}
}