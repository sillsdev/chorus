using System.Windows.Forms;
using Chorus.annotations;
using Message=Chorus.annotations.Message;

namespace Chorus.UI.Notes
{
	/// <summary>
	/// Just helps get <message/>'s  in a ListView
	/// </summary>
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
			var i = new ListViewItem(ParentAnnotation.GetLabelFromRef(""));
			i.Tag = this;
			i.SubItems.Add(Message.GetAuthor("?"));
			i.SubItems.Add(Message.Date.ToShortDateString());
			i.ImageKey = ParentAnnotation.ClassName.ToLower();
			if(ParentAnnotation.IsClosed)
			{
				i.ImageKey += "Closed";
				//i.StateImageIndex = 0;
			}
			return i;
		}
	}
}