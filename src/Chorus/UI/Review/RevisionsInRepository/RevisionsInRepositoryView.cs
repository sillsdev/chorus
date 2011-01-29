using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Review.RevisionsInRepository
{
	public partial class RevisionsInRepositoryView : UserControl
	{
		private RevisionInRepositoryModel _model;
		private ProjectFolderConfiguration _project;
		private String _userName="anonymous";

		public RevisionsInRepositoryView(RevisionInRepositoryModel model)
		{
			this.Font = SystemFonts.MessageBoxFont;
			_model = model;
			_model.ProgressDisplay = new NullProgress();
			InitializeComponent();
			UpdateDisplay();
		}

		public ProjectFolderConfiguration ProjectFolderConfig
		{
			get { return _project; }
			set { _project = value; }
		}

		/// <summary>
		/// most client apps won't have anything to put in here, that's ok
		/// </summary>
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; }
		}


		private void UpdateDisplay()
		{
		}

		public void RefreshRevisions()
		{
			Cursor.Current = Cursors.WaitCursor;
			_historyList.Items.Clear();
			List<ListViewItem> rows = new List<ListViewItem>();
			foreach (Revision rev in _model.GetHistoryItems())
			{
				var dateString = rev.DateString;
				DateTime when;

				//TODO: this is all a guess and a mess
				//I haven't figured out how/why hg uses this strange date format,
				//nor if it is going to do it on all machines, or will someday
				//change.
				if (DateTime.TryParseExact(dateString, "ddd MMM dd HH':'mm':'ss yyyy zzz",
										   null, DateTimeStyles.AssumeUniversal, out when))
				{
					when = when.ToLocalTime();
					dateString = when.ToShortDateString()+" "+when.ToShortTimeString();
				}

				var viewItem = new ListViewItem(new string[] {dateString, rev.UserId, GetDescriptionForListView(rev)});
				if (rev.Summary.ToLower().Contains("conflict"))
					viewItem.ImageKey = "Warning";
				else if (rev.Parents.Count > 1)
					viewItem.ImageKey = "Merge";
				else
				{
					var colonLocation = rev.Summary.IndexOf(':');
					string appName = rev.Summary;
					if (colonLocation > 0)
					{
						appName = appName.Substring(0, colonLocation );
					}
					var bracketLocation = appName.IndexOf(']');
					if (bracketLocation > 0)
					{
						appName = appName.Substring(0, bracketLocation);
					}
					appName = appName.Trim(new char[] { '[','+' }); // there was a bug in chorus that introduced the +
					//temp hack... the app has now been fixed to not include this
					appName = appName.Replace("0.5", "");
					viewItem.ImageKey = appName.Trim();

				}
				viewItem.Tag = rev;
				viewItem.ToolTipText = rev.Number.LocalRevisionNumber + ": " + rev.Number.Hash;
				rows.Add(viewItem);
			}
			_historyList.Items.AddRange(rows.ToArray());
			if (_historyList.Items.Count > 0)
			{
			   // no: this can be very slow, so wait until they select one
				//_historyList.Items[0].Selected = true;
			}
			Cursor.Current = Cursors.Default;
		}

		private string GetDescriptionForListView(Revision rev)
		{
			var s = rev.Summary.Substring(rev.Summary.IndexOf(']')+1).Trim();
			if(s=="auto")
				return string.Empty;
			return s;
		}

		private void _historyList_SelectedIndexChanged(object sender, EventArgs e)
		{
			Revision rev = null;
			if( _historyList.SelectedItems.Count == 1)
			{
				rev = _historyList.SelectedItems[0].Tag as Revision;
			}
			_model.SelectedRevisionChanged(rev);
		}

		private void StartRefreshTimer(object sender, EventArgs e)
		{
			timer1.Enabled = this.Visible;
//            if (Visible && _historyList.Items.Count == 0)
//            {
//                Refresh(null, null);
//            }
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (this.Visible)
			{
				if (_model.GetNeedRefresh())
				{
					RefreshRevisions();
					//enhance, check after app itself is reactivated, or work in background.  It  is noticeably slow.
					timer1.Enabled = false;//don't check again so long as we're visible
				}
			}
		}
	   private void OnRefresh(object sender, EventArgs e)
		{
		   RefreshRevisions();
		}


	}
}