using System.Windows.Forms;
using Chorus.notes;
using Message=Chorus.notes.Message;

namespace Chorus.UI.Notes
{
	public class ListMessage
	{
		public Annotation ParentAnnotation { get; private set; }
		public Message Message { get; private set; }

		public ListMessage(Annotation parentAnnotation, Message message)
		{
			ParentAnnotation = parentAnnotation;
			Message = message;
		}

		public ListViewItem GetListViewItem()
		{
			var i = new ListViewItem(ParentAnnotation.Class);
			i.Tag = this;
			i.SubItems.Add(Message.Date.ToShortDateString());
			i.SubItems.Add(ParentAnnotation.GetLabel("unknown"));
			i.SubItems.Add(Message.GetAuthor("unknown"));
			return i;
		}
	}
}