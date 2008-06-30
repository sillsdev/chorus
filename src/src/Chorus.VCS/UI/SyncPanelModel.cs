using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	internal class SyncPanelModel
	{
		private readonly ApplicationSyncContext _context;
		private readonly IProgress _progress;
		public List<RepositorySource> RepositoriesToTry = new List<RepositorySource>();
		public IList<RepositorySource> RepositoriesToList;

		public  SyncPanelModel(ApplicationSyncContext syncContext, IProgress progress)
		{
			_context = syncContext;
			_progress = progress;

			RepositoryManager manager = RepositoryManager.FromContext(_context);
			RepositoriesToList= manager.KnownRepositories;
			RepositoriesToTry.AddRange(RepositoriesToList);
		}

		public bool EnableSync
		{
			get { return RepositoriesToTry.Count > 0; }
		}

		public void Sync()
		{
			RepositoryManager manager = RepositoryManager.FromContext(_context);

			SyncOptions options = new SyncOptions();
			options.DoPullFromOthers = false;
			options.DoMergeWithOthers = false;
			options.RepositoriesToTry = RepositoriesToTry;

			manager.SyncNow(options, _progress);
		}

	}
}
