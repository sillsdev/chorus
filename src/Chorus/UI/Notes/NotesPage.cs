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
			this.Padding = new Padding(20, 20,20,20);
			var lowerContainer = new SplitContainer();
			lowerContainer.Orientation = Orientation.Vertical;
			lowerContainer.Dock = DockStyle.Fill;
			annotationView.Dock = DockStyle.Fill;

	  //todo: don't need this splitter
			lowerContainer.Panel1.Controls.Add(annotationView);

			var verticalContainer = new SplitContainer();
			verticalContainer.Orientation = Orientation.Horizontal;
			notesInProjectView.Dock = DockStyle.Fill;
			verticalContainer.Panel1.Controls.Add(notesInProjectView);
			verticalContainer.Panel2.Controls.Add(lowerContainer);
			verticalContainer.Dock = DockStyle.Fill;
			Controls.Add(verticalContainer);
			ResumeLayout();
		}
	}
}