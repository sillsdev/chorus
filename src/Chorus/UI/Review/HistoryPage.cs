using System;
using System.Windows.Forms;
using Chorus.Review;
using Chorus.UI.Review.ChangedReport;
using Chorus.UI.Review.ChangesInRevision;
using Chorus.UI.Review.RevisionsInRepository;

namespace Chorus.UI.Review
{
	public partial class HistoryPage : UserControl
	{
		public delegate HistoryPage Factory(HistoryPageOptions options);//used by autofac
		public event EventHandler<RevisionEventArgs> RevisionSelectionChanged;

		public HistoryPage(RevisionInRepositoryModel.Factory revisionsInRepositoryModelFactory,
			ChangesInRevisionView changesInRevisionView,
			ChangeReportView changeReportView,
			HistoryPageOptions options)
		{
			InitializeComponent();

			SuspendLayout();
			this.Padding = new Padding(20, 20,20,20);
			var lowerContainer = new SplitContainer();
			lowerContainer.Orientation = Orientation.Vertical;

//             var group = new GroupBox();
//             group.Text = "Changes in Revision";
//             group.Controls.Add(lowerContainer);
//             group.Dock = DockStyle.Fill;

			lowerContainer.Panel1.Controls.Add(changesInRevisionView);
			lowerContainer.Panel2.Controls.Add(changeReportView);

			var revisionListModel = revisionsInRepositoryModelFactory(options.RevisionListOptions);
			var revisionsInRepositoryView = new RevisionsInRepositoryView(revisionListModel);
			revisionsInRepositoryView.RevisionSelectionChanged += HandleRevisionSelectionChanged;

			var mainContainer = new SplitContainer();
			mainContainer.Orientation = Orientation.Horizontal;

			mainContainer.Panel1.Controls.Add(revisionsInRepositoryView);
			mainContainer.Panel2.Controls.Add(lowerContainer);
			mainContainer.Dock = DockStyle.Fill;

			lowerContainer.Dock = DockStyle.Fill;
			changesInRevisionView.Dock = DockStyle.Fill;
			changeReportView.Dock = DockStyle.Fill;
			revisionsInRepositoryView.Dock = DockStyle.Fill;

			Controls.Add(mainContainer);
			ResumeLayout();
		}

		private void HandleRevisionSelectionChanged(object sender, RevisionEventArgs e)
		{
			if (RevisionSelectionChanged != null)
				RevisionSelectionChanged(this, e);
		}
	}

}