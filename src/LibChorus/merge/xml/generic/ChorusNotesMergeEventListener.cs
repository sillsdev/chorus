using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SIL.IO;
using SIL.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Adds conflicts and any other things that need to be part of the official history
	/// to the ChorusNotes file which corresponds to the file being merged (e.g.,  foo.lift has a foo.lift.ChorusNotes)
	/// </summary>
	public class ChorusNotesMergeEventListener : IMergeEventListener, IDisposable
	{
		private XmlWriter _writer;
		private XmlReader _reader;
		private FileStream _readerStream;
		private TempFile _tempFile;
		private string _path;
		[Obsolete("Use TimeFormatWithTimeZone instead, as TimeFormatNoTimeZone produces incorrect results when used with DateTime.Now")]
		public static string TimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		public static string TimeFormatWithTimeZone = "yyyy-MM-ddTHH:mm:ssK";
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
				CreateEmptyChorusNotesFile(path);
			}
			catch (Exception error)
			{
				Debug.Fail("Something went wrong trying to create a blank ChorusNotes file :" + error.Message);
				//todo log that the xml was the wrong format
			}

			_tempFile = new TempFile();
			_readerStream = new FileStream(path, FileMode.Open);
			_reader = XmlReader.Create(_readerStream, CanonicalXmlSettings.CreateXmlReaderSettings());
			_writer = XmlWriter.Create(_tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings());
			StreamToInsertionPoint(_reader, _writer);
		}

		private static void CreateEmptyChorusNotesFile(string path)
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

		private void StreamToInsertionPoint(XmlReader reader, XmlWriter writer)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}

			while (reader.Read())
			{
				if (reader.LocalName == "notes" && reader.IsEmptyElement)
				{
					writer.WriteStartElement("notes");
					writer.WriteAttributes(reader, false);
					return;
				}
				if (reader.LocalName == "notes" && !reader.IsStartElement())
				{
					return;
				}
				StreamNode(reader, writer);
			}
		}

		private void StreamNode(XmlReader reader, XmlWriter writer)
		{
			switch (reader.NodeType)
			{
				case XmlNodeType.Element:
					writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
					writer.WriteAttributes(reader, false);
					if (reader.IsEmptyElement)
					{
						writer.WriteEndElement();
					}
					break;
				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					writer.WriteWhitespace(reader.Value);
					break;
				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;
				case XmlNodeType.EntityReference:
					writer.WriteEntityRef(reader.Name);
					break;
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;
				case XmlNodeType.DocumentType:
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
		}

		public void RecordContextInConflict(IConflict conflict)
		{
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
			// N.B.: If you ever decide to write out the change reports,
			// then be prepared to revise XmlMergeService, as it doesn't bother to add them at all,
			// in order to save a ton of memory and avoid 'out of memory' exceptions.
		}

		public void EnteringContext(ContextDescriptor context)
		{
			_context = context ?? new NullContextDescriptor();
		}

		public void Dispose()
		{
			StreamClosingData(_reader, _writer);
			_readerStream.Close();
			_reader.Close();
			_writer.Close();
			bool docIsEmpty;
			using (var fs = new FileStream(_tempFile.Path, FileMode.Open))
			using (var testEmptyDoc = XmlReader.Create(fs))
			{
				testEmptyDoc.MoveToContent();
				docIsEmpty = testEmptyDoc.IsEmptyElement;
			}
			if (!docIsEmpty)
			{
				File.Copy(_tempFile.Path, _path, true);
			}
			else
			{
				File.Delete(_path);
			}
			_tempFile.Dispose();
		}

		private void StreamClosingData(XmlReader xmlDoc, XmlWriter writer)
		{
			writer.WriteEndElement();
			while (xmlDoc.Read())
			{
				StreamNode(xmlDoc, writer);
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