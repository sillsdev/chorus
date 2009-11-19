using System;
using System.Collections.Generic;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Chorus.notes
{
	public class NotesRepository : IDisposable
	{
		private XDocument _doc;
		private static int kCurrentVersion=0;
		public static string FileExtension = "ChorusNotes";


		public static NotesRepository FromFile(string path)
		{
			try
			{
				var doc = XDocument.Load(path);
				ThrowIfVersionTooHigh(doc, path);
				return new NotesRepository(doc);
			}
			catch (XmlException error)
			{
				throw new NotesFormatException(string.Empty, error);
			}
		}

		private static void ThrowIfVersionTooHigh(XDocument doc, string path)
		{
			var version = doc.Element("notes").Attribute("version").Value;
			if (Int32.Parse(version) > kCurrentVersion)
			{
				throw new NotesFormatException(
					"The notes file {0} is of a newer version ({1}) than this version of the program supports ({2}).",
					path, version, kCurrentVersion.ToString());
			}
		}

		public static NotesRepository FromString(string contents)
		{
			try
			{
				XDocument doc = XDocument.Parse(contents);
				ThrowIfVersionTooHigh(doc, "unknown");
				return new NotesRepository(doc);
			}
			catch (XmlException error)
			{
				throw new NotesFormatException(string.Empty,error);
			}
		}

		public NotesRepository(XDocument doc)
		{
			_doc = doc;

		}

		public void Dispose()
		{

		}

		public IEnumerable<Annotation> GetAllAnnotations()
		{
			return from a in _doc.Root.Elements() select new Annotation(a);
		}

		public IEnumerable<Annotation> GetByCurrentStatus(string status)
		{
			return from a in _doc.Root.Elements()
				   where Annotation.GetStatusOfLastMessage(a) == status
				   select new Annotation(a);
		}

		public void SaveAs(string path)
		{
			_doc.Save(path);
		}

		public Annotation AddAnnotation(string annotationClss, string refUrl)
		{
			var annotation = new Annotation(annotationClss, refUrl);
			_doc.Root.Add(annotation.Element);
			return annotation;
		}
	}

	public class Annotation
	{
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		private readonly XElement _element;

		public Annotation(XElement element)
		{
			_element = element;
		}

		public Annotation(string annotationClass, string refUrl)
		{
			var s = string.Format("<annotation class='{0}' ref='{1}' guid='{2}'/>", annotationClass,refUrl, System.Guid.NewGuid().ToString());
			_element = XElement.Parse(s);
		}

		public string Class
		{
			get { return _element.GetAttributeValue("class"); }
		}

		public string Guid
		{
			get { return _element.GetAttributeValue("guid"); }
		}

		public string Ref
		{
			get { return _element.GetAttributeValue("ref"); }
		}

		public static string GetStatusOfLastMessage(XElement annotation)
		{
			XElement last = LastMessage(annotation);
			return last == null ? string.Empty : last.Attribute("status").Value;
//            var x = annotation.Elements("message");
//            if (x == null)
//                return string.Empty;
//            var y = x.Last();
//            if (y == null)
//                return string.Empty;
//            var v = y.Attribute("status");
//            return v == null ? string.Empty : v.Value;
		}

		private static XElement LastMessage(XElement annotation)
		{
			return annotation.XPathSelectElements("message[@status]").LastOrDefault();
		}
		private  XElement LastMessage()
		{
			return LastMessage(_element);
		}

		public IEnumerable<Message> Messages
		{
			get
			{
				return from msg in _element.Elements("message") select new Message(msg);
			}
		}

		public XElement Element
		{
			get { return _element; }
		}

		public string Status
		{
			get
			{
				var last = LastMessage();
				return last == null ? string.Empty : last.GetAttributeValue("status");
			}
		}

		public Message AddMessage(string author, string status, string contents)
		{
			var m = new Message(author, status, contents);
			_element.Add(m.Element);
			return m;
		}

		public string GetLabelFromRef(string defaultIfCannotGetIt)
		{
			try
			{
				var parse = System.Web.HttpUtility.ParseQueryString(Ref);
				var label = parse.GetValues("label").FirstOrDefault();
				return string.IsNullOrEmpty(label) ? defaultIfCannotGetIt : label;
			}
			catch (Exception)
			{
				return defaultIfCannotGetIt;
			}
		}
	}

	public class Message
	{
		private readonly XElement _element;

		public Message(XElement element)
		{
			_element = element;
		}

		public Message(string author, string status, string contents)
		{
			var s = String.Format("<message author='{0}' status ='{1}' date='{2}'>{3}</message>",
				author, status, DateTime.Now.ToString(Annotation.TimeFormatNoTimeZone), contents);
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


	public class NotesFormatException : ApplicationException
	{
		public NotesFormatException(string message, Exception exception)
			: base(message, exception)
		{
		}
		public NotesFormatException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

	}



	public static class XElementExtensions
	{
		#region GetAttributeValue
		/// <summary>
		/// Gets the value of an attribute
		/// </summary>
		/// <param name="xEl">Extends this XElement Type</param>
		/// <param name="attName">An XName that contains the name of the attribute to retrieve.</param>
		/// <param name="defaultReturn">Default return if the attribute doesn't exist</param>
		/// <returns>Attribute value or default if attribute doesn't exist</returns>
		public static string GetAttributeValue(this XElement xEl, XName attName, string defaultReturn)
		{
			XAttribute att = xEl.Attribute(attName);
			if (att == null) return defaultReturn;
			return att.Value;
		}

		/// <summary>
		/// Gets the value of an attribute
		/// </summary>
		/// <param name="xEl">Extends this XElement Type</param>
		/// <param name="attName">An XName that contains the name of the attribute to retrieve.</param>
		/// <returns>Attribute value or String.Empty if element doesn't exist</returns>
		public static string GetAttributeValue(this XElement xEl, XName attName)
		{
			return xEl.GetAttributeValue(attName, String.Empty);
		}

		/// <summary>
		/// Gets the value of an attribute
		/// </summary>
		/// <param name="xEl">Extends this XElement Type</param>
		/// <param name="attName">An XName that contains the name of the attribute to retrieve.</param>
		/// <param name="defaultReturn">Default return if the attribute doesn't exist</param>
		/// <returns>Attribute value or default if attribute doesn't exist</returns>
		public static T GetAttributeValue<T>(this XElement xEl, XName attName, T defaultReturn)
		{
			string returnValue = xEl.GetAttributeValue(attName, String.Empty);
			if (returnValue == String.Empty) return defaultReturn;
			return (T)Convert.ChangeType(returnValue, typeof(T));
		}

		/// <summary>
		/// Gets the value of an attribute
		/// </summary>
		/// <param name="xEl">Extends this XElement Type</param>
		/// <param name="attName">An XName that contains the name of the attribute to retrieve.</param>
		/// <returns>Attribute value or default of T if element doesn't exist</returns>
		public static T GetAttributeValue<T>(this XElement xEl, XName attName)
		{
			return xEl.GetAttributeValue<T>(attName, default(T));
		}
		#endregion

	}

	public static class ObjectExtensions
	{
		public static string OrDefault(this object s, string defaultIfNullOrMissing)
		{
			if (s == null)
				return defaultIfNullOrMissing;
			return ((string)s) == string.Empty ? defaultIfNullOrMissing : (string)s;
		}
	}
}
