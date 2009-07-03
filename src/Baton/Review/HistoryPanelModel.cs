using System;
using System.Collections.Generic;
using Baton.Review;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Misc;

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

		public List<RevisionDescriptor> GetHistoryItems()
		{
			Guard.AgainstNull(ProgressDisplay, "ProgressDisplay");
			if (!RepositoryManager.CheckEnvironmentAndShowMessageIfAppropriate("en"))
				return new List<RevisionDescriptor>();
			return _repositoryManager.GetHistoryItems(ProgressDisplay);
		}

		public void SelectedRevisionChanged(RevisionDescriptor descriptor)
		{
			if (_revisionSelectedEvent!=null)
				_revisionSelectedEvent.Raise(descriptor);
		}
	}
}