using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using Chorus.Utilities;
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
		private byte[] m_parentBytes;
		private byte[] m_childBytes;

		public static FieldWorks2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child, IMergeEventListener changeAndConflictAccumulator, HgRepository repository)
		{
			return new FieldWorks2WayDiffer(parent.GetFileContentsAsBytes(repository), child.GetFileContentsAsBytes(repository), changeAndConflictAccumulator, parent, child);
		}
		/// <summary>Used by unit tests only.</summary>
		public static FieldWorks2WayDiffer CreateFromStrings(string parentXml, string childXml, IMergeEventListener eventListener)
		{
			var enc = Encoding.UTF8;
			return new FieldWorks2WayDiffer(enc.GetBytes(parentXml), enc.GetBytes(childXml), eventListener, null, null);
		}

		private FieldWorks2WayDiffer(byte[] parentBytes, byte[] childBytes, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child)
		{
			m_parentFileInRevision = parent;
			m_childFileInRevision = child;
			m_parentBytes = parentBytes;
			m_childBytes = childBytes;
			m_eventListener = eventListener;
		}

		public void ReportDifferencesToListener()
		{
			// This arbitrary length (400) is based on two large databases,
			// one 360M with 474 bytes/object, and one 180M with 541.
			// It's probably not perfect, but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int estimatedObjectCount = 400;
			var parentIndex = new Dictionary<string, byte[]>(m_parentBytes.Length / estimatedObjectCount);
			const string startTag = "<rt ";
			const string fileClosingTag = "</languageproject>";
			const string identfierAttribute = "guid";
			using (var prepper = new DifferDictionaryPrepper(parentIndex, m_parentBytes, startTag, fileClosingTag, identfierAttribute))
			{
				prepper.Run();
			}
			m_parentBytes = null;
			var childIndex = new Dictionary<string, byte[]>(m_childBytes.Length / estimatedObjectCount);
			using (var prepper = new DifferDictionaryPrepper(childIndex, m_childBytes, startTag, fileClosingTag, identfierAttribute))
			{
				prepper.Run();
			}
			m_childBytes = null;

			var enc = Encoding.UTF8;
			var parentDoc = new XmlDocument();
			var childDoc = new XmlDocument();
			foreach (var kvpParent in parentIndex)
			{
				var parentKey = kvpParent.Key;
				var parentValue = kvpParent.Value;
				byte[] childValue;
				if (childIndex.TryGetValue(parentKey, out childValue))
				{
					childIndex.Remove(parentKey);
					if (parentValue.Length == childValue.Length)
					{
						if (!parentValue.Where((t, i) => t != childValue[i]).Any())
							continue;
					}

					var parentStr = enc.GetString(parentValue);
					var childStr = enc.GetString(childValue);
					var parentInput = new XmlInput(parentStr);
					var childInput = new XmlInput(childStr);
					if (XmlUtilities.AreXmlElementsEqual(childInput, parentInput))
						continue;

					m_eventListener.ChangeOccurred(new XmlChangedRecordReport(
													m_parentFileInRevision,
													m_childFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
													XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
				}
				else
				{
					m_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
													m_parentFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
													null)); // Child Node? How can we put it in, if it was deleted?
				}
			}
			foreach (var child in childIndex.Values)
			{
				m_eventListener.ChangeOccurred(new XmlAdditionChangeReport(
												m_childFileInRevision,
												XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(child), childDoc)));
			}
		}
	}
}
