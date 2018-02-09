using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Chorus.Properties;
using Chorus.Utilities;
using Chorus.merge.xml.generic;
using Palaso.Providers;

namespace Chorus.notes
{
    public class Annotation
    {
		public static readonly string Open = "open";
		public static readonly string Closed = "closed";
		public static string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
        internal readonly XElement _element;
        private AnnotationClass _class;

        public Annotation(XElement element)
        {
            _element = element;
			_class = GetAnnotationClass();
        }

		public Annotation(string annotationClass, string refUrl, string path)
			: this(annotationClass, refUrl, GuidProvider.Current.NewGuid(), path)
		{
		}

		public Annotation(string annotationClass, string refUrl, Guid guid, string path)
        {
			refUrl = UrlHelper.GetEscapedUrl(refUrl);
			_element = XElement.Parse(String.Format("<annotation class='{0}' ref='{1}' guid='{2}'/>",
				annotationClass, refUrl, guid.ToString()));

			_class = GetAnnotationClass();
			AnnotationFilePath = path; //TODO: this awkward, and not avail in the XElement constructor
        }

		private AnnotationClass GetAnnotationClass()
		{
			return AnnotationClassFactory.GetClassOrDefault(IconClassName);
		}

		public string ClassName
        {
            get { return _element.GetAttributeValue("class"); }
        }

		/// <summary>
		/// The class name that should be used to select an icon.
		/// At one stage two varieties of merge conflict were distinguished.
		/// </summary>
		public string IconClassName
		{
			get
			{
				return ClassName;
			}
		}

        public string Guid
        {
            get { return _element.GetAttributeValue("guid"); }
        }

        /// <summary>
		/// Gets the ref with any reserved characters (e.g. &, <, >) still escaped to be safe in the xml
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
                return UrlHelper.GetUnEscapedUrl(value);
            }
        }

		/// <summary>
		/// Notifications are low-priority annotations.
		/// Typically "conflicts" where both users added something, we aren't quite sure of the order, but no actual data loss
		/// has occurred.
		/// </summary>
		public bool IsNotification
		{
			get {return (ClassName == Conflict.NotificationAnnotationClassName);}
		}

		/// <summary>
		/// This covers all kinds of merge conflicts, including notifications. Use IsNotification to distinguish
		/// the more critical ones. This differs slightly from the UI, where "Show Merge Conflicts" just controls
		/// the critical ones. But the classes all inherit from Conflict, so it seems better in the code to consider
		/// anything that wraps a Conflict to be a conflict annotation.
		/// </summary>
		public bool IsConflict
		{
			get { return IsCriticalConflict || IsNotification; }
		}

		/// <summary>
		/// These are what the user thinks of as merge conflict reports.
		/// </summary>
		public bool IsCriticalConflict
		{
			get { return ClassName == Conflict.ConflictAnnotationClassName; }
		}


		public static string GetStatusOfLastMessage(XElement annotation)
        {
            XElement last = LastMessage(annotation);
			return last == null ? String.Empty : last.Attribute("status").Value;
        }

        private static XElement LastMessage(XElement annotation)
        {
            return annotation.XPathSelectElements("message[@status]").LastOrDefault();
        }
        private  XElement LastMessage()
        {
            return LastMessage(_element);
        }

		// Get the Date of the last (presumably most recent) message. We use this to sort them.
	    public DateTime Date
	    {
		    get
		    {
			    var msgElt = LastMessage();
			    if (msgElt == null)
				    return DateTime.MinValue; // arbitrary, we use this to sort messages, so it should not happen
			    return new Message(msgElt).Date;
		    }
	    }

		private XElement FirstMessage()
		{
			return _element.XPathSelectElement("message[@status]");
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
				return last == null ? String.Empty : last.GetAttributeValue("status");
            }

        }

        public void SetStatus(string author, string status)
        {
            if(status!=Status)
            {
				AddMessage(author, status, String.Empty);
            }
        }

		public string StatusGuid
		{
			get
			{
				var last = LastMessage();
				return last == null ? string.Empty : last.GetAttributeValue("guid");
			}
		}

        public bool CanResolve  
        {
            get { return _class.UserCanResolve; }
        }

        public bool IsClosed
        {
			get { return Status.ToLower() == Closed; }
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
        	return UrlHelper.GetValueFromQueryStringOfRef(RefStillEscaped, "label", defaultIfCannotGetIt);
        }

		public Image GetOpenOrClosedImage(int pixels)
		{
			if (!IsClosed)
				return _class.GetImage(pixels);
			if (pixels <= 16)
				return _class.GetSmallClosedImage();
			return OverlayCheckmarkOnLargeIcon();
		}

		private Image OverlayCheckmarkOnLargeIcon()
		{
			const int pixels = 32; // large size icon
			var baseImage = _class.GetImage(pixels);
			var result = new Bitmap(pixels, pixels);
			using (var canvas = Graphics.FromImage(result))
			{
				canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
				canvas.DrawImage(baseImage, 0, 0, pixels, pixels);
				canvas.DrawImage(AnnotationImages.check16x16, new Rectangle(2, 2, pixels - 2, pixels - 2));
				canvas.Save();
			}
			return result;
		}

		public Image GetImage(int pixels)
        {
            return _class.GetImage(pixels);
        }

        public string GetLongLabel()
        {
            return _class.GetLongLabel(LabelOfThingAnnotated);
        }

        public string GetDiagnosticDump()
        {
            {
                StringBuilder b = new StringBuilder();
                b.AppendLine(this.AnnotationFilePath);
             //   b.AppendLine(RefStillEscaped);
                using (XmlWriter x = XmlWriter.Create(b)) // Destination is not a chorus file, so CanonicalXmlSettings aren't used here.
                {
                    this._element.WriteTo(x);
                }
                return b.ToString();
            }
        }

        public string AnnotationFilePath { get; set; }

        public void SetStatusToClosed(string userName)
        {
			SetStatus(userName, Closed);
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
            b.AppendLine(ClassName+": "+LabelOfThingAnnotated);
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