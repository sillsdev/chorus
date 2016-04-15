// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace Chorus.sync
{
	/// <summary>
	/// Results of a sync
	/// </summary>
	public class SyncResults
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Chorus.sync.SyncResults"/> class.
		/// </summary>
		public SyncResults()
		{
			Succeeded = true;
			DidGetChangesFromOthers = false;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the sync succeeded.>
		/// </summary>
		public bool Succeeded { get; set; }

		/// <summary>
		/// If if this is true, the client app needs to restart or read in the new stuff
		/// </summary>
		public bool DidGetChangesFromOthers { get; set; }

		/// <summary>
		/// Gets or sets the encountered error.
		/// </summary>
		public Exception ErrorEncountered { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the sync got cancelled.
		/// </summary>
		public bool Cancelled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the sync updated the data.
		/// </summary>
		public bool WasUpdated { get; set; }
	}
}
