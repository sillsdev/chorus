using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Chorus.Utilities;

namespace Chorus.annotations
{
	public class Annotation
	{
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		private readonly XElement _element;
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
		public string Label
		{
			get { return GetLabelFromRef("?"); }
		}
		public string GetLabelFromRef(string defaultIfCannotGetIt)
		{
			try
			{
				Uri uri;
				if (!Uri.TryCreate(Ref, UriKind.Absolute, out uri))
				{
					throw new ApplicationException("Could not parse the url " + Ref);
				}

				var parse = System.Web.HttpUtility.ParseQueryString(uri.Query);
				var label = parse.GetValues("label").FirstOrDefault();
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
			 //   b.AppendLine(Ref);
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
	}
}