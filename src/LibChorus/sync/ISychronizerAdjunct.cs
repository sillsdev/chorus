using Palaso.Progress.LogBox;

namespace Chorus.sync
{
	/// <summary>
	/// Interface that allows Chorus clients to do something special at one or two points in a Send/Receive operation.
	///
	/// One point is right before the initial local commit.
	/// The other point is after a merge, but before its commit (if needed).
	/// </summary>
	public interface ISychronizerAdjunct
	{
		/// <summary>
		/// Allow the client to do something right before the initial local commit.
		/// </summary>
		void PrepareForInitialCommit(IProgress progress);

		/// <summary>
		/// Allow the client to do something right after a merge, but before the merges are committed.
		/// </summary>
		/// <remarks>This method is not be called at all, if there was no merging.</remarks>
		void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge);
	}
}