using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Chorus.Utilities.code;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Adds conflicts and any other things that need to be part of the official history
	/// to the ChorusNotes file which corresponds to the file being merged (e.g.,  foo.lift has a foo.lift.ChorusNotes)
	/// </summary>
	public class ChorusNotesMergeEventListener : IMergeEventListener, IDisposable
	{
		private XmlWriter _writer;
		private XmlDocument _xmlDoc;
		private string _path;
		static public string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		private const int FormatVersionNumber = 0;

		/// <summary>
		/// used for finding the context in the orginal file of any conflicts which may occur inside the element
		/// </summary>
		private ContextDescriptor _context = new NullContextDescriptor();

		static public string GetChorusNotesFilePath(string baseXmlFile)
		{
			return baseXmlFile + ".ChorusNotes";
		}

		public ChorusNotesMergeEventListener(string path)
		{
			_path = path;

			try
			{
				if (!File.Exists(path))
				{
					var doc = new XmlDocument();
					doc.LoadXml(string.Format("<notes version='{0}'/>", FormatVersionNumber.ToString()));
					using (var fileWriter = XmlWriter.Create(path, CanonicalXmlSettings.CreateXmlWriterSettings()))
					{
						doc.Save(fileWriter);
					}
				}
			}
			catch (Exception error)
			{
				Debug.Fail("Something went wrong trying to create a blank ChorusNotes file :"+error.Message);
				//todo log that the xml was the wrong format
			}

			_xmlDoc = new XmlDocument();
			_xmlDoc.Load(path);
			_writer = _xmlDoc.CreateNavigator().SelectSingleNode("notes").AppendChild();
		}

		public void RecordContextInConflict(IConflict conflict)
		{
			Guard.AgainstNull(_context, "_context");
			conflict.Context = _context;
		}

		public void ConflictOccurred(IConflict conflict)
		{
			conflict.WriteAsChorusNotesAnnotation(_writer);
		}

		public void WarningOccurred(IConflict warning)
		{
			warning.Context = _context;
			warning.WriteAsChorusNotesAnnotation(_writer);
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
			if (_xmlDoc.DocumentElement.ChildNodes.Count == 0 && _xmlDoc.DocumentElement.Attributes["version"].Value == "0")
			{
				// Get rid of empty file.
				File.Delete(_path);
			}
			else
			{
				using (var fileWriter = XmlWriter.Create(_path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					_xmlDoc.Save(fileWriter);
				}
			}
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