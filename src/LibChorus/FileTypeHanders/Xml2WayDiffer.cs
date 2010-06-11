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
	/// <summary>
	/// This class does some high level 2-way diffing between parent and child sets of XML data.
	/// If the xml for individual main elements is not the same byte array,
	/// then this class calls into the more detailed xml diffing code, the entry point of which is
	/// the XmlUtilities.AreXmlElementsEqual method.
	/// </summary>
	public class Xml2WayDiffer
	{
		private readonly IMergeEventListener _eventListener;
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private byte[] _parentBytes;
		private byte[] _childBytes;
		private readonly string _startTag;
		private readonly string _fileClosingTag;
		private readonly string _identfierAttribute;

		public static Xml2WayDiffer CreateFromFileInRevision(FileInRevision parent, FileInRevision child,
			IMergeEventListener eventListener, HgRepository repository,
			string startTag, string fileClosingTag, string identfierAttribute)
		{
			return new Xml2WayDiffer(parent.GetFileContentsAsBytes(repository), child.GetFileContentsAsBytes(repository),
				eventListener, parent, child,
				startTag, fileClosingTag, identfierAttribute);
		}

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
			_parentFileInRevision = parent;
			_childFileInRevision = child;
			_startTag = "<" + startTag.Trim();
			_fileClosingTag = "</" + fileClosingTag.Trim() + ">";
			_identfierAttribute = identfierAttribute;
			_parentBytes = parentBytes;
			_childBytes = childBytes;
			_eventListener = eventListener;
		}

		///<summary>
		/// Given the complete parent and child xml,
		/// this method compares 'records' (main xml elements) in the parent and child data sets
		/// at a high level of difference detection.
		///
		/// Low-level xl differences are handed to XmlUtilities.AreXmlElementsEqual for processing differences
		/// of each 'record'.
		///</summary>
		public void ReportDifferencesToListener()
		{
			// This arbitrary length (400) is based on two large databases,
			// one 360M with 474 bytes/object, and one 180M with 541.
			// It's probably not perfect, but we're mainly trying to prevent
			// fragmenting the large object heap by growing it MANY times.
			const int estimatedObjectCount = 400;
			var parentIndex = new Dictionary<string, byte[]>(_parentBytes.Length / estimatedObjectCount);
			using (var prepper = new DifferDictionaryPrepper(parentIndex, _parentBytes, _startTag, _fileClosingTag, _identfierAttribute))
			{
				prepper.Run();
			}
			_parentBytes = null;
			var childIndex = new Dictionary<string, byte[]>(_childBytes.Length / estimatedObjectCount);
			using (var prepper = new DifferDictionaryPrepper(childIndex, _childBytes, _startTag, _fileClosingTag, _identfierAttribute))
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
							continue;
					}

					const string deletedAttr = "dateDeleted=";
					var parentStr = enc.GetString(parentValue);
					var childStr = enc.GetString(childValue);
					// May have added dateDeleted' attr, in which case treat it as deleted, not changed.
					// NB: This is only for Lift diffing, not FW diffing,
					// so figure a way to have the client do this kind of check.
					if (childStr.Contains(deletedAttr))
					{
						// Only report it as deleted, if it is not already marked as deleted in the parent.
						if (!parentStr.Contains(deletedAttr))
						{
							_eventListener.ChangeOccurred(new XmlDeletionChangeReport(
															_parentFileInRevision,
															XmlUtilities.GetDocumentNodeFromRawXml(enc.GetString(kvpParent.Value), parentDoc),
															XmlUtilities.GetDocumentNodeFromRawXml(childStr, childDoc)));

						}
					}
					else
					{
						try
						{
							if (XmlUtilities.AreXmlElementsEqual(new XmlInput(childStr), new XmlInput(parentStr)))
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
