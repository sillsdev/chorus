using System;
using System.Collections;
using System.Collections.Generic;
using System.Media;
using System.Text;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI
{
	public class SyncPanelModel
	{
		private readonly RepositoryManager _repositoryManager;
		public List<RepositorySource> RepositoriesToTry = new List<RepositorySource>();
		public IList<RepositorySource> RepositoriesToList;
		public IProgress ProgressDisplay{get; set;}

		public SyncPanelModel(RepositoryManager repositoryManager)
		{
			_repositoryManager = repositoryManager;

			RepositoriesToList= _repositoryManager.KnownRepositorySources;
			RepositoriesToTry.AddRange(RepositoriesToList);
		}

		public bool EnableSync
		{
			get {
				return true; //because "checking in" locally is still worth doing
				//return RepositorySourcesToTry.Count > 0;
			}
		}


		public void Sync()
		{
			SyncOptions options = new SyncOptions();
			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = true;
			options.RepositorySourcesToTry = RepositoriesToTry;

			_repositoryManager.SyncNow(options, ProgressDisplay);
			//SoundPlayer player = new SoundPlayer(@"C:\chorus\src\sounds\finished.wav");
			//player.Play();
		}

	}
}
