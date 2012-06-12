using Palaso.Progress.LogBox;

namespace Chorus.sync
{
	/// <summary>
	/// Interface that allows Chorus clients to do something special at one or two points in a Send/Receive operation.
	///
	/// One point is right before the initial local commit.
	/// The other point is after an optional merge, but before its commit (cf. remarks and remarks on 'PrepareForPostMergeCommit' method).
	/// <remarks>
	/// NB: A merge is optional in a couple of ways:
	///		1. The client may not have asked for it to be done at all (e.g., WeSay's autosave, does not ask for the pull, merge, or push).
	///		2. Even if the client asks for a merge to be done, a merge may not actually be needed, so isn't done.
	/// </remarks>
	/// </summary>
	public interface ISychronizerAdjunct
	{
		/// <summary>
		/// Allow the client to do something right before the initial local commit.
		/// </summary>
		void PrepareForInitialCommit(IProgress progress);

		/// <summary>
		/// Allow the client to do something in one of two cases:
		///		1. User A had no new changes, but User B (from afar) did have them. No merge was done.
		///		2. There was a merge failure, so a rollback is being done.
		/// In both cases, the client may need to do something.
		/// </summary>
		///<param name="progress">A progress mechanism.</param>
		///<param name="isRollback">"True" if there was a merge failure, and the repo is being rolled back to an earlier state. Otherwise "False".</param>
		void SimpleUpdate(IProgress progress, bool isRollback);

		/// <summary>
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method is not be called at all, if there was no merging.</remarks>
		void PrepareForPostMergeCommit(IProgress progress);
	}
}