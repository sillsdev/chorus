using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;


namespace Chorus.FileTypeHanders
{
	public class ChorusNotesDiffer
	{
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _childDom;
		private readonly XmlDocument _parentDom;
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private readonly string _fullPath;
		private IMergeEventListener EventListener;

		public static ChorusNotesDiffer CreateFromFiles(FileInRevision parent, FileInRevision child, string ancestorLiftPath, string ourLiftPath, IMergeEventListener eventListener)
		{
			return new ChorusNotesDiffer(parent, child, ourLiftPath, File.ReadAllText(ourLiftPath), File.ReadAllText(ancestorLiftPath), eventListener);
		}
		private ChorusNotesDiffer(FileInRevision parentFileInRevision, FileInRevision childFileInRevision,
			string fullPath, string childXml, string parentXml, IMergeEventListener eventListener)
		{
			_childDom = new XmlDocument();
			_parentDom = new XmlDocument();

			_childDom.LoadXml(childXml);
			_parentDom.LoadXml(parentXml);

			_parentFileInRevision = parentFileInRevision;
			_childFileInRevision = childFileInRevision;
			_fullPath = fullPath;
			EventListener = eventListener;
		}

		public void ReportDifferencesToListener()
		{
			foreach (XmlNode e in _childDom.SafeSelectNodes("notes/annotation"))
			{
				ProcessEntry(e);
			}

			//now detect any removed (not just marked as deleted) elements
			//            foreach (XmlNode parentNode in _parentDom.SafeSelectNodes("notes/annotation"))
//            {
//                if (!_processedIds.Contains(LiftUtils.GetId(parentNode)))
//                {
//                    EventListener.ChangeOccurred(new XmlDeletionChangeReport("hackFixThis.lift", parentNode, null));
//                }
//            }
		}

		private void ProcessEntry(XmlNode child)
		{
			string id = GetGuid(child);
			XmlNode parent = FindMatch(_parentDom, id);
			if (parent == null) //it's new
			{
				EventListener.ChangeOccurred(new XmlAdditionChangeReport(_fullPath, child));
			}
			else if (XmlUtilities.AreXmlElementsEqual(child.OuterXml, parent.OuterXml))//unchanged or both made same change
			{
			}
			else //one or both changed
			{
				EventListener.ChangeOccurred(new XmlChangedRecordReport(_parentFileInRevision,_childFileInRevision, parent,child));
			}
			_processedIds.Add(id);
		}

		public static string GetGuid(XmlNode e)
		{
			return e.Attributes["guid"].Value;
		}
		public static XmlNode FindMatch(XmlNode doc, string guid)
		{
			return doc.SelectSingleNode("notes/annotation[@guid=\"" + guid + "\"]");
		}
	}
}