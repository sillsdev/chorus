using System;
using System.ComponentModel;

namespace Chorus.UI.Sync
{
	public class SyncStartingEventArgs : CancelEventArgs
	{
		public string LiftPathname { get; private set; }

		public SyncStartingEventArgs(string liftPathname)
			: base(false)
		{
			if (string.IsNullOrEmpty(liftPathname))
				throw new ArgumentNullException("liftPathname");

			LiftPathname = liftPathname;
		}
	}
}