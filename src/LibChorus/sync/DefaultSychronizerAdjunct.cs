using Palaso.Progress.LogBox;

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
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method not be called at all, if there was no merging.</remarks>
		public void PrepareForPostMergeCommit(IProgress progress, int totalNumberOfMerges, int currentMerge)
		{ /* Do nothing at all. */ }

		#endregion
	}
}