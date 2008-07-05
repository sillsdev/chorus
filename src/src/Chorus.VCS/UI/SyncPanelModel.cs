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
	internal class SyncPanelModel
	{
		private readonly ProjectFolderConfiguration _project;
		private readonly IProgress _progress;
		public List<RepositorySource> RepositoriesToTry = new List<RepositorySource>();
		public IList<RepositorySource> RepositoriesToList;

		public  SyncPanelModel(ProjectFolderConfiguration project, string userName, IProgress progress)
		{
			_project = project;
			_progress = progress;

			RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(_project);
			RepositoriesToList= manager.KnownRepositorySources;
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
			RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(_project);

			SyncOptions options = new SyncOptions();
			options.DoPullFromOthers = true;
			options.DoMergeWithOthers = true;
			options.RepositorySourcesToTry = RepositoriesToTry;

			manager.SyncNow(options, _progress);
			SoundPlayer player = new SoundPlayer(@"C:\chorus\src\sounds\finished.wav");
			player.Play();
		}

	}
}
