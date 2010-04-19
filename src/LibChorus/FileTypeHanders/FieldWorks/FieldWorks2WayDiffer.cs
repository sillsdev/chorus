using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
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
		private readonly string m_parentXml;
		private readonly string m_childXml;
		private readonly Dictionary<string, string> m_parentIndex = new Dictionary<string, string>();
		private readonly Dictionary<string, string> m_childIndex = new Dictionary<string, string>();

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
			PrepareIndex(m_parentIndex, m_parentXml);
			PrepareIndex(m_childIndex, m_childXml);

			var parentDoc = new XmlDocument();
			parentDoc.AppendChild(parentDoc.CreateNode(XmlNodeType.Element, "root", null));
			var childDoc = new XmlDocument();
			childDoc.AppendChild(childDoc.CreateNode(XmlNodeType.Element, "root", null));
			var keys = new List<string>();
			// Check for new <rt> elements in child.
			foreach (var kvpChild in m_childIndex.Where(kvp => !m_parentIndex.ContainsKey(kvp.Key)))
			{
				m_eventListener.ChangeOccurred(new XmlAdditionChangeReport(
												m_childFileInRevision,
												XmlUtilities.MakeNodeFromString(kvpChild.Value, childDoc),
												null)); // url for final parm, maybe.
				keys.Add(kvpChild.Key);
			}
			// Remove new items from child index.
			foreach (var key in keys)
				m_childIndex.Remove(key);
			keys.Clear();

			// Check for deleted <rt> elements in child.
			foreach (var kvpParent in m_parentIndex.Where(kvp => !m_childIndex.ContainsKey(kvp.Key)))
			{
				m_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
												m_parentFileInRevision,
												XmlUtilities.MakeNodeFromString(kvpParent.Value, parentDoc),
												null)); // url for final parm, maybe.
				keys.Add(kvpParent.Key);
			}
			// Remove deleted items from parent index.
			foreach (var key in keys)
				m_parentIndex.Remove(key);
			keys.Clear();

			// Check for changed <rt> elements in child.
			foreach (var kvpParent in m_parentIndex)
			{
				var parentNode = XmlUtilities.MakeNodeFromString(kvpParent.Value, parentDoc);
				var childNode = XmlUtilities.MakeNodeFromString(m_childIndex[kvpParent.Key], childDoc);
				if (!XmlUtilities.AreXmlElementsEqual(childNode, parentNode))
				{
					// Child has changed.
					m_eventListener.ChangeOccurred(new XmlChangedRecordReport(
													m_parentFileInRevision,
													m_childFileInRevision,
													parentNode,
													childNode,
													null)); // url for final parm, maybe.
				}
			}
		}

		private static void PrepareIndex(IDictionary dictionary, string fwData)
		{
			dictionary.Clear();

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
		}
	}
}
