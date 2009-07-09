using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Baton.Review.ChangedReport;
using Baton.Review.RevisionChanges;
using Baton.Review.RevisionsInRepository;

namespace Baton.HistoryPanel
{
	public partial class ReviewPage : UserControl
	{

		 public ReviewPage(RevisionsInRepositoryView revisionsInRepositoryView, ChangesInRevisionView changesInRevisionView, ChangeReportView changeReportView)
		{
			InitializeComponent();
			SuspendLayout();
			var lowerContainer = new SplitContainer();
			lowerContainer.Orientation = Orientation.Vertical;
			 lowerContainer.Dock = DockStyle.Fill;
			 changesInRevisionView.Dock = DockStyle.Fill;
			 changeReportView.Dock = DockStyle.Fill;

			 var group = new GroupBox();
			 group.Text = "Changes in Revision";
			 group.Controls.Add(lowerContainer);
			 group.Dock = DockStyle.Fill;

			lowerContainer.Panel1.Controls.Add(changesInRevisionView);
			lowerContainer.Panel2.Controls.Add(changeReportView);

			var verticalContainer = new SplitContainer();
			 verticalContainer.Orientation = Orientation.Horizontal;
			 revisionsInRepositoryView.Dock = DockStyle.Fill;
			verticalContainer.Panel1.Controls.Add(revisionsInRepositoryView);
			verticalContainer.Panel2.Controls.Add(group);
			 verticalContainer.Dock = DockStyle.Fill;
			 Controls.Add(verticalContainer);
			ResumeLayout();
		}
	}
}
