using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace Chorus.annotations
{
	public class AnnotationRepository : IDisposable
	{
		private XDocument _doc;
		private static int kCurrentVersion=0;
		public static string FileExtension = "ChorusNotes";


		public static AnnotationRepository FromFile(string path)
		{
			try
			{
				var doc = XDocument.Load(path);
				ThrowIfVersionTooHigh(doc, path);
				return new AnnotationRepository(doc);
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty, error);
			}
		}

		private static void ThrowIfVersionTooHigh(XDocument doc, string path)
		{
			var version = doc.Element("notes").Attribute("version").Value;
			if (Int32.Parse(version) > kCurrentVersion)
			{
				throw new AnnotationFormatException(
					"The notes file {0} is of a newer version ({1}) than this version of the program supports ({2}).",
					path, version, kCurrentVersion.ToString());
			}
		}

		public static AnnotationRepository FromString(string contents)
		{
			try
			{
				XDocument doc = XDocument.Parse(contents);
				ThrowIfVersionTooHigh(doc, "unknown");
				return new AnnotationRepository(doc);
			}
			catch (XmlException error)
			{
				throw new AnnotationFormatException(string.Empty,error);
			}
		}

		public AnnotationRepository(XDocument doc)
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

	public class AnnotationFormatException : ApplicationException
	{
		public AnnotationFormatException(string message, Exception exception)
			: base(message, exception)
		{
		}
		public AnnotationFormatException(string message, params object[] args)
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