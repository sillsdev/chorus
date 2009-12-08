using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Chorus.Utilities;

namespace Chorus.annotations
{
	public class Annotation
	{
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		internal readonly XElement _element;
		private AnnotationClass _class;

		public Annotation(XElement element)
		{
			_element = element;
			_class = AnnotationClassFactory.GetClassOrDefault(ClassName);
		}

		public Annotation(string annotationClass, string refUrl, string path)
			:this(XElement.Parse(string.Format("<annotation class='{0}' ref='{1}' guid='{2}'/>", annotationClass,refUrl, System.Guid.NewGuid().ToString())))
		{
			AnnotationFilePath = path; //TODO: this awkward, and not avail in the XElement constructor
		}


		public string ClassName
		{
			get { return _element.GetAttributeValue("class"); }
		}

		public string Guid
		{
			get { return _element.GetAttributeValue("guid"); }
		}

		/// <summary>
		/// Gets the ref with any reserved characters (e.g. &, <, >) till escaped to be safe in the xml
		/// </summary>
		public string RefStillEscaped
		{
			get { return _element.GetAttributeValue("ref"); }
		}


		public string RefUnEscaped
		{
			get
			{
				var value = _element.GetAttributeValue("ref");
				return Annotation.GetUnEscapedString(value);
			}
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

		public void SetStatus(string author, string status)
		{
			if(status!=Status)
			{
				AddMessage(author, status, string.Empty);
			}
		}

		public bool CanResolve
		{
			get { return _class.UserCanResolve; }
		}

		public bool IsClosed
		{
			get { return Status.ToLower() == "closed"; }
		}

		public Message AddMessage(string author, string status, string contents)
		{
			if(status==null)
			{
				status = Status;
			}
			var m = new Message(author, status, contents);
			_element.Add(m.Element);
			return m;
		}
		public string LabelOfThingAnnotated
		{
			get { return GetLabelFromRef("?"); }
		}
		public string GetLabelFromRef(string defaultIfCannotGetIt)
		{
			string name = "label";
			return GetValueFromQueryStringOfRef(name, defaultIfCannotGetIt);
		}

		/// <summary>
		/// get at the value in a URL, which are listed the collection of name=value pairs after the ?
		/// </summary>
		/// <example>GetValueFromQueryStringOfRef("id", ""lift://blah.lift?id=fooid") returns "foo"</example>
		public string GetValueFromQueryStringOfRef(string name, string defaultIfCannotGetIt)
		{
			try
			{
				Uri uri;
				if (!Uri.TryCreate(RefStillEscaped, UriKind.Absolute, out uri))
				{
					throw new ApplicationException("Could not parse the url " + RefStillEscaped);
				}

				var parse = System.Web.HttpUtility.ParseQueryString(uri.Query);

				var r = parse.GetValues(name);
				var label = r==null? defaultIfCannotGetIt : r.First();
				return string.IsNullOrEmpty(label) ? defaultIfCannotGetIt : label;
			}
			catch (Exception)
			{
				return defaultIfCannotGetIt;
			}
		}

//        public string GetImageUrl(int pixels)
//        {
//            var dir = Path.Combine(Path.GetTempPath(), "chorusIcons");
//            if (!Directory.Exists(dir))
//                Directory.CreateDirectory(dir);
//
//            var bmapPath = Path.Combine(dir, "question.bmp");
//            if (!File.Exists(bmapPath))
//            {
//                using (var bmap = Chorus.Properties.TagIcons.Question.ToBitmap())
//                {
//                    bmap.Save(bmapPath);
//                }
//            }
//            return "file://"+bmapPath;
//        }

		public Image GetImage(int pixels)
		{
			return _class.GetImage(pixels);
		}

		public string GetDiagnosticDump()
		{
			{
				StringBuilder b = new StringBuilder();
				b.AppendLine(this.AnnotationFilePath);
			 //   b.AppendLine(RefStillEscaped);
				using (XmlWriter x = XmlWriter.Create(b))
				{
					this._element.WriteTo(x);
				}
				return b.ToString();
			}
		}

		public string AnnotationFilePath { get; set; }

		public void SetStatusToClosed(string userName)
		{
			SetStatus(userName, "closed");
		}


		public static string GetEscapedString(string s)
		{
			//review: is this different from URI.EscapeDataString?
			string x = HttpUtility.UrlEncode(s);
			x = x.Replace("'", "%27");
			return x;
		}

		public static string GetUnEscapedString(string s)
		{
			var x = s.Replace("%27", "'");
			return HttpUtility.UrlDecode(x);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(Annotation))
			{
				return false;
			}
			return Equals((Annotation)obj);
		}

		public bool Equals(Annotation other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other._element, _element);
		}

		public override int GetHashCode()
		{
			return (_element != null ? _element.GetHashCode() : 0);
		}

		public string GetTextForToolTip()
		{
			var b = new StringBuilder();
			b.AppendLine("--"+ClassName+"--");
			foreach (var message in Messages)
			{
				if (message.Text.Trim().Length > 0)
				{
					b.AppendLine(message.Author + ": " + message.Text);
				}
			}
			if (IsClosed)
			{
				b.AppendLine("This note is closed.");
			}
			return b.ToString();
		}
	}
}