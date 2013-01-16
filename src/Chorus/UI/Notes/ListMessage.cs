using System;
using System.Drawing;
using System.Windows.Forms;
using Chorus.notes;
using Message=Chorus.notes.Message;

namespace Chorus.UI.Notes
{
	/// <summary>
	/// Just helps get <message/>'s  in a ListView
	/// </summary>
	public class ListMessage
	{
		private static Font _sLabelFont;
		public Annotation ParentAnnotation { get; private set; }
		public Message Message { get; private set; }

		public ListMessage(Annotation parentAnnotation, Message message)
		{
			ParentAnnotation = parentAnnotation;
			Message = message;
		}
		public DateTime Date
		{
			get { return Message.Date; }
		}

		/// <summary>
		/// This sort key groups messages about the same annotation (LT-13518), then by the date of individual messages.
		/// Groups are ordered by the last (presumably most recent) message in the group.
		/// </summary>
		public Tuple<DateTime, DateTime> SortKey
		{
			get
			{
				return new Tuple<DateTime, DateTime>(SortDate, Date);
			}
		}

		public DateTime SortDate
		{
			get
			{
				if (ParentAnnotation != null)
					return ParentAnnotation.Date;
				return Date;
			}
		}

		public ListViewItem GetListViewItem(ChorusNotesDisplaySettings displaySettings)
		{
			var i = new ListViewItem(ParentAnnotation.GetLabelFromRef(""));
			i.Tag = this;
			if(_sLabelFont==null)
			{
				//we cache this to save memory
				_sLabelFont = new Font(displaySettings.WritingSystemForNoteLabel.FontName, 10);
			}
			//note: while we would like to just use this font for the label column, this winform ui component
			//doesn't support different fonts.
			i.Font = _sLabelFont;
			var sub = i.SubItems.Add(Message.GetAuthor("?"));
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