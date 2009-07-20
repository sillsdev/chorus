using System;
using System.Collections;
using System.Collections.Generic;
using System.Media;
using System.Text;
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
		private List<RepositoryAddress> _repositorySources;

		public SyncPanelModel(RepositoryManager repositoryManager)
		{
			_repositoryManager = repositoryManager;
			_repositorySources = _repositoryManager.GetPotentialSources(new NullProgress());
		}

		public bool EnableSync
		{
			get {
				return true; //because "checking in" locally is still worth doing
				//return RepositorySourcesToTry.Count > 0;
			}
		}

		public List<RepositoryAddress> GetRepositoriesToList()
		{
			//nb: at the moment, we can't just get it new each time, because it stores the
			//enabled state of the check boxes
			return _repositorySources;
		}

		public void Sync()
		{
			SyncOptions options = new SyncOptions();
			options.CheckinDescription = "[chorus] sync";
			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = true;
			options.RepositorySourcesToTry.AddRange(GetRepositoriesToList().Where(r=>r.Enabled));

			_repositoryManager.SyncNow(options, ProgressDisplay);
			//SoundPlayer player = new SoundPlayer(@"C:\chorus\src\sounds\finished.wav");
			//player.Play();
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
