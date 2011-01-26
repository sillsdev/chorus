using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Chorus.sync;
using Chorus.UI.Review;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Progress.LogBox;

namespace Chorus.UI.Review.RevisionsInRepository
{
	public class RevisionInRepositoryModel
	{
		private readonly HgRepository _repository;
		private readonly RevisionSelectedEvent _revisionSelectedEvent;
		private string _currentTipRev;
		public IProgress ProgressDisplay { get; set; }

		public RevisionInRepositoryModel(HgRepository repository, RevisionSelectedEvent revisionSelectedEvent)
		{
			Guard.AgainstNull(repository, "repository");
			_repository = repository;
			_revisionSelectedEvent = revisionSelectedEvent;
		}

		public List<Revision> GetHistoryItems()
		{
			Guard.AgainstNull(ProgressDisplay, "ProgressDisplay");
			var msg = HgRepository.GetEnvironmentReadinessMessage("en");
			if (!string.IsNullOrEmpty(msg))
			{
				MessageBox.Show(msg, "Chorus", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return new List<Revision>();
			}
			var tip = _repository.GetTip();
			if (tip != null)
			{
				_currentTipRev = tip.Number.LocalRevisionNumber;
			}
			return _repository.GetAllRevisions();
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
				var tip = _repository.GetTip();
				if (tip == null)
					return false;

				var s = tip.Number.LocalRevisionNumber;
				return s != _currentTipRev;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}