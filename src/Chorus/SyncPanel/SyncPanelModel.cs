using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using System.Linq;

namespace Chorus.UI
{
	public class SyncPanelModel
	{
		private readonly RepositoryManager _repositoryManager;
		public IProgress ProgressDisplay{get; set;}
		private BackgroundWorker _backgroundWorker;

		public SyncPanelModel(RepositoryManager repositoryManager)
		{
			_repositoryManager = repositoryManager;
			 _backgroundWorker = new BackgroundWorker();
			_backgroundWorker.DoWork += new DoWorkEventHandler(worker_DoWork);
			//_backgroundWorker.RunWorkerCompleted +=(()=>this.EnableSync = true);

		}

		public bool EnableSync
		{
			get { return !_backgroundWorker.IsBusy; }
		}

		public List<RepositoryAddress> GetRepositoriesToList()
		{
			//nb: at the moment, we can't just get it new each time, because it stores the
			//enabled state of the check boxes
		   return _repositoryManager.GetPotentialSources(new NullProgress());
//            return _repositorySources;
		}

		public void Sync()
		{
			if(_backgroundWorker.IsBusy)
				return;

			SyncOptions options = new SyncOptions();
			options.CheckinDescription = "[chorus] sync";
			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = true;
			options.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r=>r.Enabled));


		   _backgroundWorker.RunWorkerAsync(new object[] { _repositoryManager, options, ProgressDisplay });


			//_repositoryManager.SyncNow(options, ProgressDisplay);
			//SoundPlayer player = new SoundPlayer(@"C:\chorus\src\sounds\finished.wav");
			//player.Play();
		}
		 static void worker_DoWork(object sender, DoWorkEventArgs e)
		 {
			 object[] args = e.Argument as object[];
			 RepositoryManager repoManager = args[0] as RepositoryManager;
			 e.Result =  repoManager.SyncNow(args[1] as SyncOptions, args[2] as IProgress);
		 }

		public void PathEnabledChanged(RepositoryAddress address, CheckState state)
		{
			address.Enabled = (state == CheckState.Checked);

			//NB: we may someday decide to distinguish between this chorus-app context of "what
			//I did last time and the hgrc default which effect applications (e.g. wesay)
			_repositoryManager.GetRepository(new NullProgress()).SetIsOneDefaultSyncAddresses(address, address.Enabled);
		}
	}
}
