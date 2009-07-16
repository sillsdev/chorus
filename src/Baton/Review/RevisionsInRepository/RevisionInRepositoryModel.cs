using System;
using System.Collections.Generic;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Baton.Review.RevisionsInRepository
{
	public class RevisionInRepositoryModel
	{
		private readonly RepositoryManager _repositoryManager;
		private readonly RevisionSelectedEvent _revisionSelectedEvent;
		private string _currentTipRev;
		public IProgress ProgressDisplay { get; set; }

		public RevisionInRepositoryModel(RepositoryManager repositoryManager, RevisionSelectedEvent revisionSelectedEvent)
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

			var tip = _repositoryManager.GetRepository(ProgressDisplay).GetTip();
			if (tip != null)
			{
				_currentTipRev = tip.Number.LocalRevisionNumber;
			}
			return _repositoryManager.GetAllRevisions(ProgressDisplay);
		}

		public void SelectedRevisionChanged(Revision descriptor)
		{
			if (_revisionSelectedEvent!=null)
				_revisionSelectedEvent.Raise(descriptor);
		}

		public bool GetNeedRefresh()
		{
			try
			{
				var s = _repositoryManager.GetRepository(ProgressDisplay).GetTip().Number.LocalRevisionNumber;
				return s != _currentTipRev;
			}
			catch (Exception)
			{
				return false;
				throw;
			}
		}
	}
}