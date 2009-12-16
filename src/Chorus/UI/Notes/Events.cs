using System.Text;
using Chorus.notes;
using Chorus.merge;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Notes
{
	public class MessageSelectedEvent : Event<Annotation, Message>
	{ }
}