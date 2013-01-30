using System;
using System.Collections.Generic;
using System.Drawing;

using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Chorus.Utilities;
using Chorus.merge.xml.generic;

namespace Chorus.notes
{
    public class Annotation
    {
        static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
        internal readonly XElement _element;
        private AnnotationClass _class;

        public Annotation(XElement element)
        {
            _element = element;
			_class = GetAnnotationClass();
        }

		public Annotation(string annotationClass, string refUrl, string path)
        {
                refUrl = UrlHelper.GetEscapedUrl(refUrl);
             _element = XElement.Parse(string.Format("<annotation class='{0}' ref='{1}' guid='{2}'/>", annotationClass,refUrl, System.Guid.NewGuid().ToString()));

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
		/// Two varieties of merge conflict are distinguished.
		/// </summary>
		public string IconClassName
		{
			get {
				var result = ClassName;
				if (result == Conflict.ConflictAnnotationClassName && IsNotification)
					result = Conflict.NotificationFakeClassName;
				return result;
			}
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
                return UrlHelper.GetUnEscapedUrl(value);
            }
        }

		/// <summary>
		/// Notifications are low-priority annotations.
		/// Typically "conflicts" where both users added something, we aren't quite sure of the order, but no actual data loss
		/// has occurred.
		/// Optimize JohnT: If this proves annoyingly slow (seems possible, but hasn't with a hundred or so notifications),
		/// consider avoiding fluffing up a Conflict object, and maybe even not parsing the content into an XML document;
		/// instead, just search the content for one of the class names or type guids we know are Notifications.
		/// This would be a rather sad kludge, though, and would require trickery rather than just a method override
		/// if some client wants to implement its own Conflict class which is a Notification.
		/// </summary>
		public bool IsNotification
		{
			get
			{
				if (ClassName != "mergeConflict")
					return false;
				var firstMessage = Messages.FirstOrDefault();
				if (firstMessage == null)
					return false; // paranoia
				var cdata = firstMessage.Element.Nodes().OfType<XCData>().FirstOrDefault();
				if (cdata == null)
					return false; // test case needed
				var content = cdata.Value; // This is the text that represents a Conflict object.
				var doc = new XmlDocument();
				var conflict = Conflict.CreateFromConflictElement(XmlUtilities.GetDocumentNodeFromRawXml(content, doc));
				return conflict.IsNotification;
			}
		}

		/// <summary>
		/// This covers all kinds of merge conflicts, including notifications. Use IsNotification to distinguish
		/// the more critical ones. This differs slightly from the UI, where "Show Merge Conflicts" just controls
		/// the critical ones. But the classes all inherit from Conflict, so it seems better in the code to consider
		/// anything that wraps a Conflict to be a conflict annotation.
		/// </summary>
		public bool IsConflict
		{
			get { return ClassName == "mergeConflict"; }
		}

		/// <summary>
		/// These are what the user thinks of as merge conflict reports.
		/// </summary>
		public bool IsCriticalConflict
		{
			get { return IsConflict && !IsNotification; }
		}


		public static string GetStatusOfLastMessage(XElement annotation)
        {
            XElement last = LastMessage(annotation);
            return last == null ? string.Empty : last.Attribute("status").Value;
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
        	return UrlHelper.GetValueFromQueryStringOfRef(RefStillEscaped, "label", defaultIfCannotGetIt);
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
            SetStatus(userName, "closed");
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