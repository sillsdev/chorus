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
		private List<RepositorySource> _repositorySources;

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

		public List<RepositorySource> GetRepositoriesToList()
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

	}
}
