using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Adds conflicts and any other things that need to be part of the official history
	/// to the ChorusML file which corresponds to the file being merged (e.g.,  foo.lift has a foo.lift.ChorusML)
	/// </summary>
	public class ChorusMLMergeEventListener : IMergeEventListener, IDisposable
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
			return baseXmlFile + ".ChorusML";
		}

		public ChorusMLMergeEventListener(string path)
		{
			_path = path;

			try
			{
				if (!File.Exists(path))
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml("<markup/>");
					doc.Save(path);
				}
			}
			catch (Exception error)
			{
				Debug.Fail("Something went wrong trying to create a blank ChorusML file :"+error.Message);
				//todo log that the xml was the wrong format
			}

			_xmlDoc = new XmlDocument();
			_xmlDoc.Load(path);
			_writer = _xmlDoc.CreateNavigator().SelectSingleNode("markup").AppendChild();
		}
		public void ConflictOccurred(IConflict conflict)
		{
			conflict.Context = _context;
			conflict.WriteAsXml(_writer);
		}

		public void WarningOccurred(IConflict warning)
		{
			warning.Context = _context;
			warning.WriteAsXml(_writer);
		}

		public void ChangeOccurred(IChangeReport change)
		{
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