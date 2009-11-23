using System;
using System.Linq;
using System.Xml.Linq;
using Chorus.Utilities;

namespace Chorus.annotations
{
	public class Message
	{
		private readonly XElement _element;

		public Message(XElement element)
		{
			_element = element;
		}

		public Message(string author, string status, string contents)
		{
			var s = String.Format("<message author='{0}' status ='{1}' date='{2}' guid='{3}'>{4}</message>",
								  author, status, DateTime.Now.ToString(Annotation.TimeFormatNoTimeZone), System.Guid.NewGuid(), contents);
			_element = XElement.Parse(s);
		}

		public string Guid
		{
			get { return _element.GetAttributeValue("guid"); }
		}

		public string Author
		{
			get { return _element.GetAttributeValue("author"); }
		}

		public DateTime Date
		{
			get {
				var date = _element.GetAttributeValue("date");
				return DateTime.Parse(date);
			}
		}

		public string Status
		{
			get { return _element.GetAttributeValue("status"); }
		}

		public string HtmlText
		{
			get {
				var text= _element.Nodes().OfType<XText>().FirstOrDefault();
				if(text==null)
					return String.Empty;
				// return HttpUtility.HtmlDecode(text.ToString()); <-- this works too
				return text.Value;
			}
		}

		public XElement Element
		{
			get { return _element; }
		}

		public string GetAuthor(string defaultValue)
		{
			return Author.OrDefault(defaultValue);
		}
	}
}