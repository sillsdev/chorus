using System;
using System.Windows.Forms;
using Chorus.UI.Review.ChangedReport;
using Chorus.UI.Review.ChangesInRevision;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Review
{
	public partial class HistoryPage : UserControl
	{
		public delegate HistoryPage Factory(HistoryPageOptions options);//used by autofac

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
			lowerContainer.Dock = DockStyle.Fill;
			changesInRevisionView.Dock = DockStyle.Fill;
			changeReportView.Dock = DockStyle.Fill;

//             var group = new GroupBox();
//             group.Text = "Changes in Revision";
//             group.Controls.Add(lowerContainer);
//             group.Dock = DockStyle.Fill;

			lowerContainer.Panel1.Controls.Add(changesInRevisionView);
			lowerContainer.Panel2.Controls.Add(changeReportView);

			var revisionListModel = revisionsInRepositoryModelFactory(options.RevisionListOptions);
			var revisionsInRepositoryView = new RevisionsInRepositoryView(revisionListModel);

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