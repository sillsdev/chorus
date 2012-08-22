using System;
using System.Collections.Generic;
using Chorus.VcsDrivers.Mercurial;
using Chorus.sync;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.sync
{
	class ProgrammableSynchronizerAdjunct : ISychronizerAdjunct
	{
		private string _modelVersion;

		public ProgrammableSynchronizerAdjunct(string modelVersion)
		{
			_modelVersion = modelVersion;
		}

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
		{ /* Do nothing at all. */ }

		/// <summary>
		/// Allow the client to do something right after a merge, but before the merge is committed.
		/// </summary>
		/// <remarks>This method not be called at all, if there was no merging.</remarks>
		public void PrepareForPostMergeCommit(IProgress progress)
		{ /* Do nothing at all. */ }

		public string ModelVersion
		{
			get { return _modelVersion ?? ""; } // Hg default branch name
		}

		public void CheckRepositoryBranches(IEnumerable<Revision> branches)
		{ /* Do nothing at all. */ }

		#endregion
	}
}
