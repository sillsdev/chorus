using System.Text;
using Chorus.merge;
using Chorus.notes;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.UI.Notes
{
	public class MessageSelectedEvent : Event<Annotation, Message>
	{ }
}