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
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method is not be called at all, if there was no merging.</remarks>
		void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge);
	}
}