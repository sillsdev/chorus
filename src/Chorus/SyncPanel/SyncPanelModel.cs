using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;
using System.Linq;
using Chorus.VcsDrivers;

namespace Chorus.SyncPanel
{
	public class SyncPanelModel
	{
		private readonly Synchronizer _synchronizer;
		public IProgress ProgressDisplay{get; set;}
		private readonly BackgroundWorker _backgroundWorker;

		public SyncPanelModel(ProjectFolderConfiguration projectFolderConfiguration)
		{
			_synchronizer = Synchronizer.FromProjectConfiguration(projectFolderConfiguration, new NullProgress());
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.DoWork += worker_DoWork;
		}

		public bool EnableSync
		{
			get { return !_backgroundWorker.IsBusy; }
		}

		public List<RepositoryAddress> GetRepositoriesToList()
		{
			//nb: at the moment, we can't just get it new each time, because it stores the
			//enabled state of the check boxes
			return _synchronizer.GetPotentialSynchronizationSources(new NullProgress());
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

			_backgroundWorker.RunWorkerAsync(new object[] { _synchronizer, options, ProgressDisplay });
		}
		static void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			object[] args = e.Argument as object[];
			Synchronizer synchronizer = args[0] as Synchronizer;
			e.Result =  synchronizer.SyncNow(args[1] as SyncOptions, args[2] as IProgress);
			SoundPlayer player = new SoundPlayer(Properties.Resources.finished);
			player.Play();
		}

		public void PathEnabledChanged(RepositoryAddress address, CheckState state)
		{
			address.Enabled = (state == CheckState.Checked);

			//NB: we may someday decide to distinguish between this chorus-app context of "what
			//repos I used last time" and the hgrc default which effect applications (e.g. wesay)
			_synchronizer.SetIsOneOfDefaultSyncAddresses(address, address.Enabled);
		}
	}
}