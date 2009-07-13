using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;


namespace Chorus.FileTypeHanders
{
	public class ConflictDiffer
	{
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _childDom;
		private readonly XmlDocument _parentDom;
		private IMergeEventListener EventListener;

		public static ConflictDiffer CreateFromFiles(string ancestorLiftPath, string ourLiftPath, IMergeEventListener eventListener)
		{
			return new ConflictDiffer(File.ReadAllText(ourLiftPath), File.ReadAllText(ancestorLiftPath), eventListener);
		}
		private ConflictDiffer(string childXml, string parentXml, IMergeEventListener eventListener)
		{
			_childDom = new XmlDocument();
			_parentDom = new XmlDocument();

			_childDom.LoadXml(childXml);
			_parentDom.LoadXml(parentXml);

			EventListener = eventListener;
		}

		public void ReportDifferencesToListener()
		{
			foreach (XmlNode e in _childDom.SafeSelectNodes("conflicts/conflict"))
			{
				ProcessEntry(e);
			}

			//now detect any removed (not just marked as deleted) elements
			//            foreach (XmlNode parentNode in _parentDom.SafeSelectNodes("conflicts/conflict"))
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
				EventListener.ChangeOccurred(new XmlAdditionChangeReport("hackFixThis.conflicts", child));
			}
			else if (XmlUtilities.AreXmlElementsEqual(child.OuterXml, parent.OuterXml))//unchanged or both made same change
			{
			}
			else //one or both changed
			{
				EventListener.ChangeOccurred(new XmlChangedRecordReport("hackFixThis.conflicts", parent,child));
			}
			_processedIds.Add(id);
		}

		public static string GetGuid(XmlNode e)
		{
			return e.Attributes["guid"].Value;
		}
		public static XmlNode FindMatch(XmlNode doc, string guid)
		{
			return doc.SelectSingleNode("conflicts/conflict[@guid=\"" + guid + "\"]");
		}
	}
}