using System;
using System.Collections.Generic;
using Palaso.Progress;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgModelVersionBranch
	{
		private HgRepository _repo;
		private IProgress _progress;

		internal HgModelVersionBranch(HgRepository repo, IProgress progress)
		{
			_repo = repo;
			_progress = progress;
			SetCurrentClientVersion();
		}

		public string ClientVersion { get; set; }

		public string UserId
		{
			get { return _repo.GetUserIdInUse(); }
		}

		private void SetCurrentClientVersion()
		{
			var currentRevision = _repo.GetRevisionWorkingSetIsBasedOn();
			if (currentRevision != null)
				ClientVersion = currentRevision.Branch;
		}

		/// <summary>
		/// Returns the head revision for each branch in _repo
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<Revision> GetBranches()
		{
			var heads = _repo.GetHeads(); // Heads gets more information than Branches, including summary and userID
			// But now we have to make sure that if there is more than one head for a branch, we only show the latest.
			var branchDict = new Dictionary<string, Revision>();
			foreach (var head in heads)
			{
				Revision previousHeadSameBranch;
				if (branchDict.TryGetValue(head.Branch, out previousHeadSameBranch))
				{
					var prevHeadRevNum = previousHeadSameBranch.Number.LocalRevisionNumber;
					if (Convert.ToInt32(head.Number.LocalRevisionNumber) > Convert.ToInt32(prevHeadRevNum))
					{
						branchDict.Remove(previousHeadSameBranch.Branch);
						branchDict.Add(head.Branch, head);
					}
				}
				else
				{
					branchDict.Add(head.Branch, head);
				}
			}
			return branchDict.Values;
		}

		/// <summary>
		/// For use when updating a model version for the repository,
		/// sets the current branch on the repo and the ClientVersion property to the given branch name
		/// </summary>
		/// <param name="progress"></param>
		/// <param name="branchName"></param>
		public void Branch(IProgress progress, string branchName)
		{
			progress.WriteVerbose("{0} changing working dir to branch: {1}", UserId, branchName);
			_repo.Execute(_repo.SecondsBeforeTimeoutOnLocalOperation, "branch -f ", HgRepository.SurroundWithQuotes(branchName));
			ClientVersion = branchName;
		}

		internal bool IsLatestBranchDifferent(string myVersion, out string revNum)
		{
			var latestRevision = _repo.GetRevisionWorkingSetIsBasedOn();
			revNum = latestRevision.Number.LocalRevisionNumber;
			return latestRevision.Branch != myVersion;
		}
	}
}
