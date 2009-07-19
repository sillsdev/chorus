using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Chorus.Utilities;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Note, the conflict log is kept in xml, but that doesn't mean this is only for merging xml documents.
	/// </summary>
	public class XmlLogMergeEventListener : IMergeEventListener, IDisposable
	{
		private XmlWriter _writer;
		private XmlDocument _xmlDoc;
		private string _path;
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";

		/// <summary>
		/// used for finding the context in the orginal file of any conflicts which may occur inside the element
		/// </summary>
		private ContextDescriptor _context;

		static public string GetXmlConflictFilePath(string baseXmlFile)
		{
			return baseXmlFile + ".conflicts";
		}

		public XmlLogMergeEventListener(string path)
		{
			_path = path;

			try
			{
				if (!File.Exists(path))
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml("<conflicts/>");
					doc.Save(path);
				}
			}
			catch (Exception error)
			{
				Debug.Fail("Something went wrong trying to create a blank onflict file :"+error.Message);
				//todo log that the xml was the wrong format
			}

			_xmlDoc = new XmlDocument();
			_xmlDoc.Load(path);
			_writer = _xmlDoc.CreateNavigator().SelectSingleNode("conflicts").AppendChild();
		}
		public void ConflictOccurred(IConflict conflict)
		{
//            //this hacky business is about using a serializer when it doesn't own the whole document...
//            //we're just using it to make xml for one element, which it doesn't like to do.
//            StringBuilder builder = new StringBuilder();
//            using (StringWriter sw = new StringWriter(builder))
//            {
//                var x = new XmlSerializer(conflict.GetType());
//                var fragmentWriter = new XmlFragmentWriter(sw);
//                x.Serialize(fragmentWriter, conflict);
//                fragmentWriter.Close();
//            }
//            var doc = new XmlDocument();
//            doc.LoadXml(builder.ToString());
//            _writer.WriteElementString(conflict.GetType().Name.ToString(), doc.FirstChild.InnerXml);

			conflict.Context = this._context;
			conflict.WriteAsXml(_writer);
		}

		public void ChangeOccurred(IChangeReport change)
		{
			/*
			 * at this time, we aren't using these, and they mess with our simple-minded
			 * "conflicting merge" detector, which just sees if the conflicts file was updated.
			 */
/*            _writer.WriteStartElement("change");
			_writer.WriteAttributeString("type", string.Empty, change.ActionLabel);
			_writer.WriteAttributeString("guid", string.Empty, change.Guid.ToString());
			_writer.WriteAttributeString("date", string.Empty, DateTime.UtcNow.ToString(TimeFormatNoTimeZone));
			if (_context != null)
			{
				_context.WriteAttributes(_writer);
			}
			_writer.WriteString(change.GetFullHumanReadableDescription());
			_writer.WriteEndElement();
			*/
		}

		public void EnteringContext(ContextDescriptor context)
		{
			_context = context;
		}

		public void Dispose()
		{
			_writer.Close();
			_xmlDoc.Save(_path);
		}
	}

	class XmlFragmentWriter : XmlTextWriter
	{
		public XmlFragmentWriter(TextWriter w) : base(w) { }
		bool _skip = false;

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			// STEP 1 - Omits XSD and XSI declarations.

			// From Kzu - http://weblogs.asp.net/cazzu/archive/2004/01/23/62141.aspx

			if (prefix == "xmlns" && (localName == "xsd" || localName == "xsi"))
			{
				_skip = true;
				return;
			}
			base.WriteStartAttribute(prefix, localName, ns);
		}

		public override void WriteString(string text)
		{
			if (!_skip)
				base.WriteString(text);
		}

		public override void WriteEndAttribute()
		{

			if (_skip)
			{
				// Reset the flag, so we keep writing.
				_skip = false;
				return;
			}
			base.WriteEndAttribute();
		}

		public override void WriteStartDocument()
		{
			// STEP 2: Do nothing so we omit the xml declaration.
		}
	}
}