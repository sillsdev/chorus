using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Baton.Review;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.HistoryPanel.ChangedRecordsList
{
	public partial class ChangedRecordListView : UserControl
	{
		private readonly ChangedRecordSelectedEvent _changedRecordSelectedEventToRaise;

		public ChangedRecordListView(RevisionSelectedEvent revisionSelectedEventToSubscribeTo, ChangedRecordSelectedEvent changedRecordSelectedEventToRaise)
		{
			InitializeComponent();
			_changedRecordSelectedEventToRaise = changedRecordSelectedEventToRaise;
			revisionSelectedEventToSubscribeTo.Subscribe(revision => LoadList(revision));
		}

		private void LoadList(RevisionDescriptor descriptor)
		{
			listView1.Items.Clear();
			AddChangeRow(descriptor);
			AddChangeRow(descriptor);
			AddChangeRow(descriptor);
		}

		private void AddChangeRow(RevisionDescriptor descriptor)
		{
			var dummy = new DummyChangeReport(descriptor.Summary);
			var row = new ListViewItem(dummy.ToString());
			row.Tag = dummy;
			listView1.Items.Add(row);
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_changedRecordSelectedEventToRaise != null)
			{
				if (listView1.SelectedItems.Count == 1)
				{
					_changedRecordSelectedEventToRaise.Raise(listView1.SelectedItems[0].Tag as IChangeReport);
				}
				else
				{
					_changedRecordSelectedEventToRaise.Raise(null);
				}
			}
		}
	}
}
