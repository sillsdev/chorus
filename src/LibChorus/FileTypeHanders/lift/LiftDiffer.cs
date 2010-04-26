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

namespace Chorus.FileTypeHanders.lift
{

	/// <summary>
	/// Given a parent and child lift file, reports on what changed.
	/// </summary>
	public class Lift2WayDiffer
	{
		private readonly IMergeEventListener _eventListener;
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private byte[] _parentBytes;
		private byte[] _childBytes;

		public static Lift2WayDiffer CreateFromFileInRevision(IMergeStrategy mergeStrategy, FileInRevision parent, FileInRevision child, IMergeEventListener eventListener, HgRepository repository)
		{
			return new Lift2WayDiffer(parent.GetFileContentsAsBytes(repository), child.GetFileContentsAsBytes(repository), eventListener, parent, child);
		}
		public static Lift2WayDiffer CreateFromStrings(IMergeStrategy mergeStrategy, string parentXml, string childXml, IMergeEventListener eventListener)
		{
			var enc = Encoding.UTF8;
			return new Lift2WayDiffer(enc.GetBytes(parentXml), enc.GetBytes(childXml), eventListener, null, null);
		}

		private Lift2WayDiffer(byte[] parentBytes, byte[] childBytes, IMergeEventListener eventListener, FileInRevision parent, FileInRevision child)
		{
			_parentBytes = parentBytes;
			_childBytes = childBytes;
			_eventListener = eventListener;
			_parentFileInRevision = parent;
			_childFileInRevision = child;
		}

		public void ReportDifferencesToListener()
		{
			// This arbitrary length (200) is based on a large database,
			// with over 200 bytes per object. It's probably not perfect,
			// but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int estimatedObjectCount = 200;
			var parentIndex = new Dictionary<string, byte[]>(_parentBytes.Length / estimatedObjectCount);
			const string startTag = "<entry ";
			const string fileClosingTag = "</lift>";
			const string identfierAttribute = "id";
			using (var prepper = new DifferDictionaryPrepper(parentIndex, _parentBytes, startTag, fileClosingTag, identfierAttribute))
			{
				prepper.Run();
			}
			_parentBytes = null;
			var childIndex = new Dictionary<string, byte[]>(_childBytes.Length / estimatedObjectCount);
			using (var prepper = new DifferDictionaryPrepper(childIndex, _childBytes, startTag, fileClosingTag, identfierAttribute))
			{
				prepper.Run();
			}
			_childBytes = null;

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
							continue; // Exact same data in byte array. Ergo, a no change deal.
					}

					var childStr = enc.GetString(childValue);
					// May have added dateDeleted' attr, in which case treat it as deleted, not changed.
					if (childStr.Contains("dateDeleted="))
					{
						_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
														_parentFileInRevision,
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
							_eventListener.ChangeOccurred(new ErrorDeterminingChangeReport(
								_parentFileInRevision,
								_childFileInRevision,
								XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
								XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc),
								error));
							continue;
						}
						_eventListener.ChangeOccurred(new XmlChangedRecordReport(
														_parentFileInRevision,
														_childFileInRevision,
														XmlUtilities.GetDocumentNodeFromRawXml(parentStr, parentDoc),
														XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));
					}
				}
				else
				{
					_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
													_parentFileInRevision,
													XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
													null)); // Child Node? How can we put it in, if it was deleted?
				}
			}
			foreach (var child in childIndex.Values)
			{
				_eventListener.ChangeOccurred(new XmlAdditionChangeReport(
												_childFileInRevision,
												XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(child), childDoc)));
			}
		}
	}
}