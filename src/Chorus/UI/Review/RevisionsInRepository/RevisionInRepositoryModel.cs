using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Chorus.Review;
using Chorus.VcsDrivers.Mercurial;
using L10NSharp;
using Palaso.Code;
using Palaso.Progress;

namespace Chorus.UI.Review.RevisionsInRepository
{
	public class RevisionInRepositoryModel
	{
		private readonly HgRepository _repository;
		private readonly RevisionSelectedEvent _revisionSelectedEvent;
		private readonly RevisionListOptions _options;
		private string _currentTipRev;
		public IProgress ProgressDisplay { get; set; }

		public delegate RevisionInRepositoryModel Factory(RevisionListOptions options);
		/// <summary>
		/// Revisions take time to gether up, so the UI gets them in chunks,
		/// while we gather up more in the background
		/// </summary>
		public Queue<Revision> DiscoveredRevisionsQueue
		{
			get; set;
		}

		public RevisionInRepositoryModel(HgRepository repository,
										RevisionSelectedEvent revisionSelectedEvent,
										RevisionListOptions options)
		{
			Guard.AgainstNull(repository, "repository");
			_repository = repository;
			_revisionSelectedEvent = revisionSelectedEvent;
			_options = options;
			DiscoveredRevisionsQueue =  new Queue<Revision>();
		}

		/// <summary>
		/// This lets you pick a range of revisions to compare, not just a single one
		/// </summary>
		public bool DoShowRevisionChoiceControls
		{
			get { return _options.ShowRevisionChoiceControls; }
			set { _options.ShowRevisionChoiceControls = value; }
		}

		internal void SelectedRevisionChanged(Revision descriptor)
		{
			// This was a public method, but it was changed to internal, since the whole model instance is effectively internal,
			// since it is not exposed beyond its view, which is also not exposed to anyone.

			// In an ideal world, the RevisionSelectedEvent instance in _revisionSelectedEvent would allow client code to add new subscribers,
			// but that world doesn't exist yet, so the view has added a kludge Windows event and calls it, after callinng this method.
			if (_revisionSelectedEvent!=null)
				_revisionSelectedEvent.Raise(descriptor);
		}

		public bool GetNeedRefresh()
		{
			try
			{
				var tip = _repository.GetTip();
				if (tip == null)
					return false;

				var s = tip.Number.LocalRevisionNumber;
				return s != _currentTipRev;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private BackgroundWorker RevisionGetter;

		/// <summary>
		/// Get the list of optional extra columns to show.
		/// </summary>
		public IEnumerable<HistoryColumnDefinition> ExtraColumns
		{
			get { return _options.ExtraColumns; }
		}
		/// <summary>
		/// This one will not return until every revision has been gathered up...
		/// that can take a long time.  Use BeginGettingRevisions where possible.
		/// </summary>
		public IEnumerable<Revision> GetAllRevisions()
		{
			BeginGettingRevisions();
			while (RevisionGetter!=null)
			{
				//dangerous if used outside of unit tests,
				//but needed to keep a background worker going.
				//we could switch to a normal thread to avoid needing this,
				//then we could just sleep.
				Application.DoEvents();
			}
			var results = new List<Revision>();
			lock (DiscoveredRevisionsQueue)
			{
				while (DiscoveredRevisionsQueue.Count > 0)
				{
					results.Add(DiscoveredRevisionsQueue.Dequeue());
				}
				DiscoveredRevisionsQueue.Clear();
			}
			return results;
		}

		/// <summary>
		/// After call this, you can check DiscoveredRevisionsQueue occasionally (lock on it)
		/// </summary>
		public void BeginGettingRevisions()
		{
			if (RevisionGetter != null)
			{
				// RevisionGetter.Dispose();//review
				return; // Let the poor thing do its job. Otherwise, repeated Refresh attempts will throw a null reference exception, when doing its RunWorkerCompleted event.
			}

			Guard.AgainstNull(ProgressDisplay, "ProgressDisplay");
			SanityCheck();//review: remove?

			RevisionGetter = new BackgroundWorker();
			RevisionGetter.DoWork += RevisionGetter_DoWork;
			RevisionGetter.RunWorkerCompleted += RevisionGetter_RunWorkerCompleted;
			var tip = _repository.GetTip();
			if (tip != null)
			{
				_currentTipRev = tip.Number.LocalRevisionNumber;
			}

			ProgressDisplay.WriteStatus(LocalizationManager.GetString("Messages.GettingHistory", "Getting history..."));
			RevisionGetter.RunWorkerAsync();
		}

		void RevisionGetter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// Make sure those puppies aren't used any more.
			// And, let the GC do its thing on the old thread.
			// Otherwise, we will hang onto it, until we go away.
			RevisionGetter.DoWork -= RevisionGetter_DoWork;
			RevisionGetter.RunWorkerCompleted -= RevisionGetter_RunWorkerCompleted;

			ProgressDisplay.WriteStatus("");
			RevisionGetter.Dispose();
			RevisionGetter = null;
		}

		private void SanityCheck()
		{
			var msg = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(msg))
			{
				Palaso.Reporting.ErrorReport.NotifyUserOfProblem(msg);
				return;
			}
		}

		void RevisionGetter_DoWork(object sender, DoWorkEventArgs e)
		{
			lock (DiscoveredRevisionsQueue)
			{
				DiscoveredRevisionsQueue.Clear();
			}
			//NOte: at this point, we're not really getting revisions little by little,
			//we're getting them all at once.  Is it still worth it?  Maybe not.
			//after all, the UI could just request all items, then use a timer to
			//add them in the background, if needed.  In order to actually get them
			//little by little, we'd have to go all the way down to hg, and see if we
			//can get it to page its results. Or at least we'd have to "page" the making
			//of revision objects.
			//ANother idea: get the first batch of, say 20 fast, queue those up, then get the rest.
			//This could be done by making a GetFirstNRevisions(20), which would use
			//	the --limit command given to hg log.  The trick, then
			//is when we call GetAllRevisions(), we need to not add those first (up to) 20 again.
			foreach (var revision in _repository.GetAllRevisions())
			{
				if (_options.RevisionsToShowFilter(revision))
				{
					lock (DiscoveredRevisionsQueue)
					{
						this.DiscoveredRevisionsQueue.Enqueue(revision);
					}
				}
			}
		}
	}
}