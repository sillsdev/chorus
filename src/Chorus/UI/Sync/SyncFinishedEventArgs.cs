using System;
using Chorus.sync;

namespace Chorus.UI.Sync
{
	public class SyncFinishedEventArgs : EventArgs
	{
		public SyncResults Results { get; private set; }

		public SyncFinishedEventArgs(SyncResults syncResults)
		{
			if (syncResults == null) throw new ArgumentNullException("syncResults");
			Results = syncResults;
		}
	}
}