using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Class that handles diffing of two versions for FieldWorks 7.0 xml data.
	/// </summary>
	public class FieldWorks2WayDiffer
	{
		private readonly IMergeEventListener m_eventListener;
		private readonly FileInRevision m_parentFileInRevision;
		private readonly FileInRevision m_childFileInRevision;
		private string m_parentXml;
		private string m_childXml;

		public static FieldWorks2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child, IMergeEventListener changeAndConflictAccumulator, HgRepository repository)
		{
			return new FieldWorks2WayDiffer(parent.GetFileContents(repository), child.GetFileContents(repository), changeAndConflictAccumulator, parent, child);
		}
		/// <summary>Used by unit tests only.</summary>
		public static FieldWorks2WayDiffer CreateFromStrings(string parentXml, string childXml, IMergeEventListener eventListener)
		{
			return new FieldWorks2WayDiffer(parentXml, childXml, eventListener);
		}

		private FieldWorks2WayDiffer(string parentXml, string childXml,IMergeEventListener eventListener)
		{
			m_parentFileInRevision = null;
			m_childFileInRevision = null;
			m_parentXml = parentXml;
			m_childXml = childXml;
			m_eventListener = eventListener;
		}

		private FieldWorks2WayDiffer(string parentXml, string childXml, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child)
			: this(parentXml, childXml, eventListener)
		{
			m_parentFileInRevision = parent;
			m_childFileInRevision = child;
		}

		public void ReportDifferencesToListener()
		{
			// This arbitrary length (400) is based on two large databases,
			// one 360M with 474 bytes/object, and one 180M with 541.
			// It's probably not perfect, but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int arbitraryObjectLength = 400;
			var parentIndex = new Dictionary<string, string>(m_parentXml.Length / arbitraryObjectLength);
			PrepareIndex(parentIndex, m_parentXml);
			m_parentXml = null;
			var childIndex = new Dictionary<string, string>(m_childXml.Length / arbitraryObjectLength);
			PrepareIndex(childIndex, m_childXml);
			m_childXml = null;

			var parentDoc = new XmlDocument();
			var childDoc = new XmlDocument();
			foreach (var kvpParent in parentIndex)
			{
				var parentKey = kvpParent.Key;
				var parentValue = kvpParent.Value;
				string childValue;
				if (childIndex.TryGetValue(parentKey, out childValue))
				{
					childIndex.Remove(parentKey);
					if (parentValue == childValue)
						continue;

					var parentInput = new XmlInput(parentValue);
					var childInput = new XmlInput(childValue);
					if (XmlUtilities.AreXmlElementsEqual(childInput, parentInput))
						continue;

					m_eventListener.ChangeOccurred(new XmlChangedRecordReport(
													m_parentFileInRevision,
													m_childFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(parentValue, parentDoc),
													XmlUtilities.GetDocumentNodeFromRawXml(childValue, childDoc)));
				}
				else
				{
					m_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
													m_parentFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(kvpParent.Value, parentDoc),
													null)); // Child Node? How can we put it in, if it was deleted?
				}
			}
			foreach (var child in childIndex.Values)
			{
				m_eventListener.ChangeOccurred(new XmlAdditionChangeReport(
												m_childFileInRevision,
												XmlUtilities.GetDocumentNodeFromRawXml(child, childDoc)));
			}
		}

		private static void PrepareIndex(IDictionary dictionary, string fwData)
		{
#if USEXMLREADER
			// Try using an XmlReader.
			var settings = new XmlReaderSettings { ValidationType = ValidationType.None };
			using (var reader = XmlReader.Create(new StringReader(fwData), settings))
			{
				reader.MoveToContent();
				while (reader.Read())
				{
					if (reader.LocalName != "rt")
						continue;
					var key = reader.GetAttribute("guid");
					var value = reader.ReadOuterXml();
					dictionary.Add(key, value);
				}
			}
#else
			// Try working through the string, directly.
			// NB: If the FW xml ever has rt elements like:  <rt ... />,
			// then this won't work.
			const string guidAttr = "guid=";
			const string openRt = "<rt";
			var startOfRtElementOffset = fwData.IndexOf(openRt);
			const string closeRt = "</rt>";
			while (startOfRtElementOffset > 0)
			{
				var endOfRtElementOffset = fwData.IndexOf(closeRt, startOfRtElementOffset + 3);
				var lengthToCopy = endOfRtElementOffset - startOfRtElementOffset + 5;
				var rtElement = fwData.Substring(startOfRtElementOffset, lengthToCopy);
				var guidStartOffset = rtElement.IndexOf(guidAttr) + 6;
				var guidAsString = rtElement.Substring(guidStartOffset, 36);
				dictionary.Add(guidAsString, rtElement);
				startOfRtElementOffset = fwData.IndexOf(openRt, endOfRtElementOffset);
			}
#endif
		}
	}
}
