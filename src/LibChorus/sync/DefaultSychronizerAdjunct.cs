using SIL.Progress;
using System.Collections.Generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.sync
{
	/// <summary>
	/// Default implementation of the ISychronizerAdjunct interface that is used,
	/// when the client does not provide another implementation.
	///
	/// This implementation does nothing at all for either method.
	/// </summary>
	internal class DefaultSychronizerAdjunct : ISychronizerAdjunct
	{
		#region Implementation of ISychronizerAdjunct

		/// <summary>
		/// Allow the client to do something right before the initial local commit.
		/// </summary>
		public void PrepareForInitialCommit(IProgress progress)
		{ /* Do nothing at all. */ }

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
		}

		/// <summary>
		/// Get the branch name the client wants to use. This might be (for example) a current version label
		/// of the client's data model. Used to create a version branch in the repository. This one
		/// returns the default Hg branch.
		/// </summary>
		public string BranchName
		{
			get { return "default"; } // Hg default branch name
		}

		public bool WasUpdated { get; private set; }

		/// <summary>
		/// During a Send/Receive when Chorus has completed a pull and there is more than one branch on the repository
		/// it will pass the revision of the head of each branch to the client.
		/// The client can use this to display messages to the users when other branches are active other than their own.
		/// i.e. "Someone else has a new version you should update"
		/// or "Your colleague needs to update, you won't see their changes until they do."
		/// </summary>
		/// <param name="branches">A list (IEnumerable really) of all the open branches in this repo.</param>
		/// <param name="progress"></param>
		public void CheckRepositoryBranches(IEnumerable<Revision> branches, IProgress progress)
		{ /* Do nothing at all. */ }

		#endregion
	}
}