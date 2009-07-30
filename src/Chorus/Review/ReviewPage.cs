using System.Windows.Forms;
using Chorus.Review.ChangedReport;
using Chorus.Review.ChangesInRevision;
using Chorus.Review.RevisionsInRepository;

namespace Chorus.Review
{
	public partial class ReviewPage : UserControl
	{

		public ReviewPage(RevisionsInRepositoryView revisionsInRepositoryView, ChangesInRevisionView changesInRevisionView, ChangeReportView changeReportView)
		{
			InitializeComponent();

			SuspendLayout();
			this.Padding = new Padding(20, 20,20,20);
			var lowerContainer = new SplitContainer();
			lowerContainer.Orientation = Orientation.Vertical;
			lowerContainer.Dock = DockStyle.Fill;
			changesInRevisionView.Dock = DockStyle.Fill;
			changeReportView.Dock = DockStyle.Fill;

//             var group = new GroupBox();
//             group.Text = "Changes in Revision";
//             group.Controls.Add(lowerContainer);
//             group.Dock = DockStyle.Fill;

			lowerContainer.Panel1.Controls.Add(changesInRevisionView);
			lowerContainer.Panel2.Controls.Add(changeReportView);


			var verticalContainer = new SplitContainer();
			verticalContainer.Orientation = Orientation.Horizontal;
			revisionsInRepositoryView.Dock = DockStyle.Fill;
			verticalContainer.Panel1.Controls.Add(revisionsInRepositoryView);
			verticalContainer.Panel2.Controls.Add(lowerContainer);
			verticalContainer.Dock = DockStyle.Fill;
			Controls.Add(verticalContainer);
			ResumeLayout();
		}
	}
}