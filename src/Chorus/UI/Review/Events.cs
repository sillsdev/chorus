using System.Text;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Review
{
	public class RevisionSelectedEvent : Event<Revision>
	{ }

	public class ChangedRecordSelectedEvent : Event<IChangeReport>
	{ }

	public class NavigateToRecordEvent : Event<string>
	{ }
}