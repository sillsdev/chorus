using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class HgModelVersionBranch
	{
		private HgRepository _repo;

		public HgModelVersionBranch(HgRepository repo)
		{
			_repo = repo;
		}

		public string ClientVersion { get; set; }

		public string UserId
		{
			get { return _repo.GetUserIdInUse(); }
		}

		internal List<Revision> GetBranches(IProgress progress)
		{
			string what = "branches";
			progress.WriteVerbose("Getting {0} of {1}", what, UserId);
			string result = _repo.GetTextFromQuery(what);

			string[] lines = result.Split('\n');
			List<Revision> branches = new List<Revision>();
			foreach (string line in lines)
			{
				if (line.Trim() == "")
					continue;

				string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2)
					continue;
				string[] revisionParts = parts[1].Split(':');
				branches.Add(new Revision(_repo, parts[0], "", revisionParts[0], revisionParts[1], "unknown"));
			}
			return branches;
		}

		internal void Branch(IProgress progress, string branchName)
		{
			progress.WriteVerbose("{0} changing working dir to branch: {1}", UserId, branchName);
			_repo.Execute(_repo.SecondsBeforeTimeoutOnLocalOperation, "branch -f ", HgRepository.SurroundWithQuotes(branchName));
		}

		internal void CreateNewBranch(string versionNumber)
		{
			_repo.Branch(versionNumber);
			ClientVersion = versionNumber;
		}

		internal bool IsLatestBranchDifferent(string myVersion, out string revNum)
		{
			var latestRevision = _repo.GetRevisionWorkingSetIsBasedOn();
			revNum = latestRevision.Number.LocalRevisionNumber;
			return latestRevision.Branch != myVersion;
		}
	}
}
