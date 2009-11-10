using System.Windows.Forms;
using Chorus.UI.Review.ChangedReport;


namespace Chorus.UI.Notes
{
	public partial class NotesPage : UserControl
	{

		public NotesPage(NotesInProjectView notesInProjectView, AnnotationView annotationView)
		{
			InitializeComponent();

			SuspendLayout();
//            this.Padding = new Padding(20, 20,20,20);
//            annotationView.Padding=new Padding(0,50,0,0);
			annotationView.Dock = DockStyle.Fill;
	   //     notesInProjectView.Width = 300;

			notesInProjectView.Dock = DockStyle.Fill;
			splitContainer1.Panel1.Controls.Add(notesInProjectView);
			splitContainer1.Panel2.Controls.Add(annotationView);
			ResumeLayout();
		}
	}
}