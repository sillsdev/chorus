// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Chorus.VcsDrivers;
using Palaso.Code;
using Palaso.Extensions;
using Palaso.Progress;

namespace Chorus.sync
{
	/// <summary>
	/// Sync control model
	/// </summary>
	public class SyncControlModelSimple
	{
		private static object _lockToken = new object();
		private readonly IChorusUser _user;
		/// <summary>
		/// The synchronizer.
		/// </summary>
		protected readonly Synchronizer _synchronizer;
		/// <summary>
		/// The background worker.
		/// </summary>
		protected readonly BackgroundWorker _backgroundWorker;
		/// <summary>
		/// Occurs when synchronize is finished.
		/// </summary>
		public event EventHandler SynchronizeOver;

		/// <summary>
		/// The multi progress.
		/// </summary>
		protected readonly MultiProgress _progress;
		private BackgroundWorker _asyncLocalCheckInWorker;

		/// <summary>
		/// Gets or sets the status progress.
		/// </summary>
		public IProgress StatusProgress { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Chorus.UI.Sync.SyncControlModelSimple"/> class.
		/// </summary>
		public SyncControlModelSimple(ProjectFolderConfiguration projectFolderConfiguration,
			IChorusUser user, IProgress progress)
		{
			_user = user;
			_progress = new MultiProgress();
			StatusProgress = progress;
			_progress.Add(StatusProgress);
			_synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, _progress);
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.WorkerSupportsCancellation = true;
			_backgroundWorker.RunWorkerCompleted += OnBackgroundWorkerRunWorkerCompleted;
			_backgroundWorker.DoWork += worker_DoWork;

			//clients will normally change these
			SyncOptions = new SyncOptions();
			SyncOptions.CheckinDescription = "[] sync";
			SyncOptions.DoPullFromOthers = true;
			SyncOptions.DoMergeWithOthers = true;
			SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled));
		}

		/// <summary>
		/// Gets called when the background worker completed work
		/// </summary>
		protected virtual void OnBackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (SynchronizeOver != null)
			{
				if (e.Cancelled)
				{
					var r = new SyncResults();
					r.Succeeded = false;
					r.Cancelled = true;
					SynchronizeOver.Invoke(r, null);
				}
				else //checking e.Result if there was a cancellation causes an InvalidOperationException
				{
					SynchronizeOver.Invoke(e.Result, null);
				}
			}
		}

		/// <summary>
		/// Gets the name of the user.
		/// </summary>
		public string UserName
		{
			get
			{
				return _user == null ? "anonymous" : _user.Name;
			}
		}

		/// <summary>
		/// Gets a value indicating whether we're currently synchronizing.
		/// </summary>
		public bool SynchronizingNow
		{
			get { return _backgroundWorker.IsBusy; }
		}

		/// <summary>
		/// Gets or sets the sync options.
		/// </summary>
		public SyncOptions SyncOptions { get; set; }

		/// <summary>
		/// Gets a value indicating whether cancellation is pending.
		/// </summary>
		public bool CancellationPending
		{
			get { return _backgroundWorker.CancellationPending;  }
		}

		/// <summary>
		/// Gets a value indicating whether errors or warnings were encountered.
		/// </summary>
		/// <value><c>true</c> if errors or warnings encountered; otherwise, <c>false</c>.</value>
		public bool ErrorsOrWarningsEncountered
		{
			get { return _progress.ErrorEncountered || _progress.WarningsEncountered; }
		}

		/// <summary>
		/// Gets a list of the repositories
		/// </summary>
		public List<RepositoryAddress> GetRepositoriesToList()
		{
			//nb: at the moment, we can't just get it new each time, because it stores the
			//enabled state of the check boxes
			return _synchronizer.GetPotentialSynchronizationSources();
		}

		/// <summary>
		/// Sync
		/// </summary>
		/// <param name="useTargetsAsSpecifiedInSyncOptions">This is a bit of a hack
		/// until I figure out something better... it will be true for cases where
		/// the app is just doing a backup..., false were we want to sync to whatever
		/// sites the user has indicated</param>
		public void Sync(bool useTargetsAsSpecifiedInSyncOptions)
		{
			_progress.WriteStatus("Syncing...");
			lock (this)
			{
				if(_backgroundWorker.IsBusy)
					return;
				if (!useTargetsAsSpecifiedInSyncOptions)
				{
//                    foreach (var address in GetRepositoriesToList().Where(r => !r.Enabled))
//                    {
//                        SyncOptions.RepositorySourcesToTry.RemoveAll(x => x.URI == address.URI);
//                    }
				   // SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled && !SyncOptions.RepositorySourcesToTry.Any(x=>x.URI ==r.URI)));
					SyncOptions.RepositorySourcesToTry.Clear();
					SyncOptions.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r => r.Enabled ));

				}
				_backgroundWorker.RunWorkerAsync(new object[] {_synchronizer, SyncOptions, _progress});
			}
		}

		static void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			object[] args = e.Argument as object[];
			Synchronizer synchronizer = args[0] as Synchronizer;
			// We need to make sure that only one thread at a time goes through SyncNow()
			// to avoid race conditions accessing the repository.
			lock (_lockToken)
			{
				e.Result = synchronizer.SyncNow(sender as BackgroundWorker, e, args[1] as SyncOptions);
			}
		}

		/// <summary>
		/// Gets called after a path gets enabled/disabled.
		/// </summary>
		public void PathEnabledChanged(RepositoryAddress address, bool enabled)
		{
			address.Enabled = enabled;

			//NB: we may someday decide to distinguish between this chorus-app context of "what
			//repos I used last time" and the hgrc default which effect applications (e.g. wesay)
			_synchronizer.SetIsOneOfDefaultSyncAddresses(address, address.Enabled);
		}

		/// <summary>
		/// Check in, to the local disk repository, any changes to this point.
		/// </summary>
		/// <param name="checkinDescription">A description of what work was done that you're
		/// wanting to checkin. E.g. "Delete a Book"</param>
		/// <param name="callbackWhenFinished">Action to call after finishing</param>
		public void AsyncLocalCheckIn(string checkinDescription, Action<SyncResults> callbackWhenFinished)
		{
			var repoPath = this._synchronizer.Repository.PathToRepo.CombineForPath(".hg");
			Require.That(Directory.Exists(repoPath), "The repository should already exist before " +
				"calling AsyncLocalCheckIn(). Expected to find the hg folder at " + repoPath);

			// NB: if someone were to call this fast and repeatedly, I won't vouch for any kind of
			// safety here. This is just designed for checking in occasionally, like as users do
			// some major new thing, or finish some task.
			if (_asyncLocalCheckInWorker != null && !_asyncLocalCheckInWorker.IsBusy)
			{
				_asyncLocalCheckInWorker.Dispose(); //timidly avoid a leak
			}
			_asyncLocalCheckInWorker = new BackgroundWorker();
			_asyncLocalCheckInWorker.DoWork += (o, args) =>
			{
				var options = new SyncOptions {
					CheckinDescription = checkinDescription,
					DoMergeWithOthers = false,
					DoPullFromOthers = false,
					DoSendToOthers = false
				};
				// We need to make sure that only one thread at a time goes through SyncNow()
				// to avoid race conditions accessing the repository.
				SyncResults result;
				lock (_lockToken)
				{
					result = _synchronizer.SyncNow(options);
				}
				if (callbackWhenFinished != null)
				{
					callbackWhenFinished(result);
				}
			};
			_asyncLocalCheckInWorker.RunWorkerAsync();
		}
	}
}
