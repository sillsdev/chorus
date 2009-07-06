using System;
using System.Collections.Generic;
using Baton.Review;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.HistoryPanel
{
	public class HistoryPanelModel
	{
		private readonly RepositoryManager _repositoryManager;
		private readonly RevisionSelectedEvent _revisionSelectedEvent;
		public IProgress ProgressDisplay { get; set; }

		public HistoryPanelModel(RepositoryManager repositoryManager, RevisionSelectedEvent revisionSelectedEvent)
		{
			Guard.AgainstNull(repositoryManager, "repositoryManager");
			_repositoryManager = repositoryManager;
			_revisionSelectedEvent = revisionSelectedEvent;
		}

		public List<Revision> GetHistoryItems()
		{
			Guard.AgainstNull(ProgressDisplay, "ProgressDisplay");
			if (!RepositoryManager.CheckEnvironmentAndShowMessageIfAppropriate("en"))
				return new List<Revision>();
			return _repositoryManager.GetAllRevisions(ProgressDisplay);
		}

		public void SelectedRevisionChanged(Revision descriptor)
		{
			if (_revisionSelectedEvent!=null)
				_revisionSelectedEvent.Raise(descriptor);
		}
	}
}