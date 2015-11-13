using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using SIL.Lift;
using SIL.Xml;
using SIL.Progress;

namespace Chorus.FileTypeHandlers.lift
{
	public class LiftSynchronizerAdjunct : ISychronizerAdjunct
	{
		private readonly string _branchName = @"default";
		private readonly string _liftPathName;

		/// <summary>
		/// Synchronizer Adjunct for use processing LIFT files
		/// </summary>
		/// <param name="liftFileFullPathName">Please provide a full pathname to the LIFT file.</param>
		public LiftSynchronizerAdjunct(string liftFileFullPathName)
		{
			_liftPathName = liftFileFullPathName;
			_branchName = GetBranchNameFromLiftFile();
		}

		private string GetBranchNameFromLiftFile()
		{
			const string LIFT = @"LIFT";
			using (var reader = XmlReader.Create(_liftPathName, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				reader.MoveToContent();
				reader.MoveToAttribute(@"version");
				var liftVersionString = reader.Value;
				if (liftVersionString == @"0.13")
				{
					return @"default";
				}
				return LIFT + reader.Value;
			}
		}

		private void PutFilesInFixedOrder()
		{
			LiftSorter.SortLiftFile(_liftPathName);
			LiftSorter.SortLiftRangesFiles(Path.ChangeExtension(_liftPathName, "lift-ranges"));
		}

		#region Implementation of ISychronizerAdjunct

		/// <summary>
		/// Allow the client to do something right before the initial local commit.
		/// </summary>
		public void PrepareForInitialCommit(IProgress progress)
		{
			PutFilesInFixedOrder();
		}

		/// <summary>
		/// Allow the client to do something in one of two cases:
		///		1. User A had no new changes, but User B (from afar) did have them. No merge was done.
		///		2. There was a merge failure, so a rollback is being done.
		/// In both cases, the client may need to do something.
		/// </summary>
		///<param name="progress">A progress mechanism.</param>
		/// <param name="isRollback">"True" if there was a merge failure, and the repo is being rolled back to an earlier state. Otherwise "False".</param>
		public void SimpleUpdate(IProgress progress, bool isRollback)
		{
			WasUpdated = true;
		}

		/// <summary>
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method not be called at all, if there was no merging.</remarks>
		public void PrepareForPostMergeCommit(IProgress progress)
		{
			WasUpdated = true;
			PutFilesInFixedOrder();
		}

		/// <summary>
		/// Get the branch name the client wants to use. This might be (for example) a current version label
		/// of the client's data model. Used to create a version branch in the repository.
		/// </summary>
		public string BranchName
		{
			get { return _branchName; }
		}

		public bool WasUpdated { get; private set; }

		/// <summary>
		/// During a Send/Receive when Chorus has completed a pull and there is more than one branch on the repository
		/// it will pass the revision of the head of each branch to the client.
		/// The client can use this to display messages to the users when other branches are active other than their own.
		/// i.e. "Someone else has a new version you should update"
		/// or "Your colleague needs to update, you won't see their changes until they do."
		/// Note: partly because it modifies the global Settings file, I haven't made a unit test for this method.
		/// Most of the functionality is deliberately in the GetRepositoryBranchCheckData, which is tested.
		/// </summary>
		/// <param name="branches">A list (IEnumerable really) of all the open branches in this repo.</param>
		/// <param name="progress">Where we will write a warning if changes in other branches</param>
		public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
		{
			string savedSettings = Properties.Settings.Default.OtherBranchRevisions;
			string conflictingUser = GetRepositoryBranchCheckData(branches, BranchName, ref savedSettings);
			if (!string.IsNullOrEmpty(conflictingUser))
				progress.WriteWarning(string.Format("Other users of this LIFT repository (most recently {0}) are using a different version of FieldWorks or WeSay. Changes from users with more recent versions will not be merged into projects using older versions. We strongly recommend that all users upgrade to the same version as soon as possible.", conflictingUser));
			Properties.Settings.Default.OtherBranchRevisions = savedSettings;
			Properties.Settings.Default.Save();
		}


		/// <summary>
		/// This method (also used by FlexBridge) is passed a set of revision branches, as made available to CheckRepositoryBranches,
		/// and the name of the branch that we currently use, and a string of the form branch:revision number;branch:revision number
		/// which records any other branches which were notified in a previous call to CheckRepositoryBranches.
		/// It detects whether there are new changes to a branch other than ours, and if so, returns the ID of (one of) the users who
		/// has made a change. Otherwise it returns null.
		/// It also updates the savedSettings string to match the incoming information about current branches.
		/// </summary>
		/// <param name="branches"></param>
		/// <param name="currentBranch"></param>
		/// <param name="savedSettings"></param>
		/// <returns></returns>
		public static string GetRepositoryBranchCheckData(IEnumerable<Revision> branches, string currentBranch, ref string savedSettings)
		{
			var sb = new StringBuilder();
			string result = null;
			foreach (var rev in branches)
			{
				// Due to Windows and Linux differences with mercurial we can't be sure if branch is going to be
				// an empty string or 'default' for the default branch.
				// Force both our currentBranch and the revision branch to say "default" in both conditions.
				var revisionBranch = rev.Branch == string.Empty ? @"default" : rev.Branch;
				currentBranch = currentBranch == string.Empty ? @"default" : currentBranch;
				if (revisionBranch == currentBranch)
					continue; // Changes on our own branch aren't a problem
				int revision;
				if (!int.TryParse(rev.Number.LocalRevisionNumber, out revision))
					continue; // or crash?
				if (sb.Length > 0)
					sb.Append(";");
				sb.Append(revisionBranch);
				sb.Append(":");
				sb.Append(rev.Number.LocalRevisionNumber);
				foreach (var otherRev in (savedSettings ?? "").Split(';'))
				{
					var parts = otherRev.Split(':');
					if (parts.Length != 2)
						continue; // weird, ignore
					if (parts[0] != revisionBranch)
						continue;
					int otherRevision;
					if (!int.TryParse(parts[1], out otherRevision))
						continue;
					if (revision > otherRevision)
					{
						result = rev.UserId;
						// Loop must continue to build the complete new OtherBranchRevisions string and write it out.
					}
				}
			}
			savedSettings = sb.ToString();
			return result;
		}


		#endregion
	}
}
