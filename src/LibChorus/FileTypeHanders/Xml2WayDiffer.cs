using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.generic.xmldiff;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders
{
	public class Xml2WayDiffer
	{
		private readonly IMergeEventListener m_eventListener;
		private readonly FileInRevision m_parentFileInRevision;
		private readonly FileInRevision m_childFileInRevision;
		private byte[] m_parentBytes;
		private byte[] m_childBytes;
		private readonly string m_startTag;
		private readonly string m_fileClosingTag;
		private readonly string m_identfierAttribute;

		public static Xml2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child,
			IMergeEventListener eventListener, HgRepository repository,
			string startTag, string fileClosingTag, string identfierAttribute)
		{
			return new Xml2WayDiffer(parent.GetFileContentsAsBytes(repository), child.GetFileContentsAsBytes(repository),
				eventListener, parent, child,
				startTag, fileClosingTag, identfierAttribute);
		}
		/// <summary>Used by unit tests only.</summary>
		public static Xml2WayDiffer CreateFromStrings(string parentXml, string childXml,
			IMergeEventListener eventListener,
			string startTag, string fileClosingTag, string identfierAttribute)
		{
			var enc = Encoding.UTF8;
			return new Xml2WayDiffer(enc.GetBytes(parentXml), enc.GetBytes(childXml),
				eventListener, null, null,
				startTag, fileClosingTag, identfierAttribute);
		}

		private Xml2WayDiffer(byte[] parentBytes, byte[] childBytes, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child, string startTag, string fileClosingTag, string identfierAttribute)
		{
			m_parentFileInRevision = parent;
			m_childFileInRevision = child;
			m_startTag = startTag;
			m_fileClosingTag = fileClosingTag;
			m_identfierAttribute = identfierAttribute;
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
			using (var prepper = new DifferDictionaryPrepper(parentIndex, m_parentBytes, m_startTag, m_fileClosingTag, m_identfierAttribute))
			{
				prepper.Run();
			}
			m_parentBytes = null;
			var childIndex = new Dictionary<string, byte[]>(m_childBytes.Length / estimatedObjectCount);
			using (var prepper = new DifferDictionaryPrepper(childIndex, m_childBytes, m_startTag, m_fileClosingTag, m_identfierAttribute))
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

					var childStr = enc.GetString(childValue);
					// May have added dateDeleted' attr, in which case treat it as deleted, not changed.
					// NB: This is only for Lift diffing, not FW diffing,
					// so figure a way to have the client do this kind of check.
					if (childStr.Contains("dateDeleted="))
					{
						m_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
														m_parentFileInRevision,
														XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
														XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
					}
					else
					{
						var parentStr = enc.GetString(parentValue);
						try
						{
							var parentInput = new XmlInput(parentStr);
							var childInput = new XmlInput(childStr);
							if (XmlUtilities.AreXmlElementsEqual(childInput, parentInput))
								continue;
						}
						catch (Exception error)
						{
							m_eventListener.ChangeOccurred(new ErrorDeterminingChangeReport(
								m_parentFileInRevision,
								m_childFileInRevision,
								XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
								XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc),
								error));
							continue;
						}
						m_eventListener.ChangeOccurred(new XmlChangedRecordReport(
														m_parentFileInRevision,
														m_childFileInRevision,
														XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
														XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
					}
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
