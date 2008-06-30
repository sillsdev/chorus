using System;
using System.Collections.Generic;
using System.Text;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	internal class SyncPanelModel
	{
		private string _logText;
		private SyncManager _syncManager;
		private readonly ApplicationSyncContext _syncContext;
		private readonly IProgress _progress;
		private SyncResults _syncResults;

		public  SyncPanelModel(ApplicationSyncContext syncContext, IProgress progress)
		{
			_syncContext = syncContext;
			_progress = progress;
		}

		public void Sync()
		{
			RepositoryManager manager = RepositoryManager.FromAppContext(_syncContext);

			SyncOptions options = new SyncOptions();
			options.DoPullFromOthers = false;
			options.DoMergeWithOthers = false;

			manager.SyncNow(options, _progress);
		}

	}
}
