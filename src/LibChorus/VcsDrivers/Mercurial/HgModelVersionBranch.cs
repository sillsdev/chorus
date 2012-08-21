using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class HgModelVersionBranch
	{
		private HgRepository _repo;
		private IProgress _progress;

		public HgModelVersionBranch(HgRepository repo, IProgress progress)
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

		internal List<Revision> GetBranches()
		{
			string what = "branches";
			_progress.WriteVerbose("Getting {0} of {1}", what, UserId);
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
			_repo.Execute(_repo.SecondsBeforeTimeoutOnLocalOperation, "branch ", HgRepository.SurroundWithQuotes(branchName));
		}

		/// <summary>
		/// This will create a new branch if no branch exists for version number and return null.
		/// If the version number branch does exist, it will return the revision.
		/// </summary>
		/// <param name="versionNumber"></param>
		/// <returns></returns>
		internal Revision CreateNewBranch(string versionNumber)
		{
			var branches = GetBranches();
			Revision existingBranch = null;
			Revision oldBranch = null;
			foreach (var revision in branches)
			{
				if(revision.Branch == versionNumber)
				{
					existingBranch = revision;
				}
				if(revision.Branch == ClientVersion)
				{
					oldBranch = revision;
				}
				if(oldBranch != null && existingBranch != null)
					break;
			}

			ClientVersion = versionNumber;
			if (existingBranch != null && oldBranch != null)
			{
				return existingBranch; //The branch exists, we need to merge into it, not create a new one
			}
			Branch(_progress, versionNumber);
			return null;
		}

		internal bool IsLatestBranchDifferent(string myVersion, out string revNum)
		{
			var latestRevision = _repo.GetRevisionWorkingSetIsBasedOn();
			revNum = latestRevision.Number.LocalRevisionNumber;
			return latestRevision.Branch != myVersion;
		}
	}
}
