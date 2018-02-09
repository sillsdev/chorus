using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;
using Chorus.Utilities;
using Palaso.Providers;

namespace Chorus.notes
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
				SecurityElement.Escape(author), SecurityElement.Escape(status), DateTimeProvider.Current.Now.ToString(Annotation.TimeFormatNoTimeZone),
				GuidProvider.Current.NewGuid(), SecurityElement.Escape(contents));
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
			get
			{
				var date = _element.GetAttributeValue("date");
				DateTime dt;
				if (DateTime.TryParse(date, out dt))
				{
					return dt;
				}
				return default(DateTime);
			}
		}

		public string Status
		{
			get { return _element.GetAttributeValue("status"); }
		}

		public string GetSimpleHtmlText()
		{
			return GetHtmlText(null);
		}

		public string Text
		{
			get
			{
				var t = _element.Nodes().OfType<XText>().FirstOrDefault();
				if (t == null)
					return string.Empty;
				return t.Value;
			}
		}

		public string GetHtmlText(EmbeddedMessageContentHandlerRepository embeddedMessageContentHandlerRepository)
		{
			var b = new StringBuilder();
			b.Append(Text);

			if (embeddedMessageContentHandlerRepository != null)
			{
				XCData cdata = _element.Nodes().OfType<XCData>().FirstOrDefault();

				if (cdata != null)
				{
					string content = cdata.Value;
					var handler = embeddedMessageContentHandlerRepository.GetHandlerOrDefaultForCData(content);
					b.AppendLine(handler.GetHyperLink(content));
				}
			}

			return b.ToString();

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