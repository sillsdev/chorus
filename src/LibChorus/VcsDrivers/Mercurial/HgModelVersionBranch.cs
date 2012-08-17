using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Progress.LogBox;

namespace Chorus.VcsDrivers.Mercurial
{
	internal class HgModelVersionBranch
	{
		private string _userName;
		private HgRepository _repo;

		public HgModelVersionBranch(HgRepository repo, string userName)
		{
			_repo = repo;
			_userName = userName;
		}

		public string ClientVersion { get; set; }

		internal List<Revision> GetBranches(IProgress progress)
		{
			string what = "branches";
			progress.WriteVerbose("Getting {0} of {1}", what, _userName);
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

		internal void CreateNewBranch(string versionNumber)
		{
			var cmdString = "hg branch";
			var paramArray = new string[] { versionNumber };
			var result = _repo.Execute(_repo.SecondsBeforeTimeoutOnLocalOperation, cmdString, paramArray);
			// TODO: This is a stub!
			if (result.ExitCode != 0)
			{
				throw new ApplicationException(string.Format("Failed to create new branch: exit code = {0} Message: {1}",
					result.ExitCode, result.StandardError));
			}
		}
	}
}
