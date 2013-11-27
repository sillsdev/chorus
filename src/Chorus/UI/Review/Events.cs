using System;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Review
{
	public class RevisionSelectedEvent : Event<Revision>
	{ }

	public class ChangedRecordSelectedEvent : Event<IChangeReport>
	{ }

	public class NavigateToRecordEvent : Event<string>
	{

	}

	public class RevisionEventArgs : EventArgs
	{
		public RevisionEventArgs(Revision revision)
		{
			Revision = revision;
		}

		public Revision Revision { get; private set; }
	}
}