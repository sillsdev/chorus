using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;


namespace Chorus.merge.xml.lift
{

	/// <summary>
	/// Given a parent and child lift file, reports on what changed.
	/// </summary>
	public class Lift2WayDiffer
	{
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _childDom;
		private readonly XmlDocument _parentDom;
		private IMergeEventListener EventListener;
		private IMergeStrategy _mergingStrategy;

		public static Lift2WayDiffer CreateFromFiles(IMergeStrategy mergeStrategy, string ancestorLiftPath, string ourLiftPath, IMergeEventListener eventListener)
		{
			return new Lift2WayDiffer(mergeStrategy,File.ReadAllText(ourLiftPath), File.ReadAllText(ancestorLiftPath), eventListener);
		}
		public static Lift2WayDiffer CreateFromStrings(IMergeStrategy mergeStrategy, string parentXml, string childXml, IMergeEventListener eventListener)
		{
			return new Lift2WayDiffer(mergeStrategy, childXml, parentXml, eventListener);
		}

		private Lift2WayDiffer(IMergeStrategy mergeStrategy, string childXml, string parentXml,IMergeEventListener eventListener)
		{
			_childDom = new XmlDocument();
			_parentDom = new XmlDocument();

			_childDom.LoadXml(childXml);
			_parentDom.LoadXml(parentXml);

			EventListener = eventListener;
			_mergingStrategy = mergeStrategy;

		}

		public void ReportDifferencesToListener()
		{
			foreach (XmlNode e in _childDom.SafeSelectNodes("lift/entry"))
			{
				ProcessEntry(e);
			}

			//now detect any deleted elements
			foreach (XmlNode e in _parentDom.SafeSelectNodes("lift/entry"))
			{
				if (!_processedIds.Contains(LiftUtils.GetId(e)))
				{
					EventListener.ChangeOccurred(new XmlDeletionChangeReport("hackFixThis.lift", e));
				}
			}
		}

		private void ProcessEntry(XmlNode child)
		{
			string id = LiftUtils.GetId(child);
			XmlNode parent = LiftUtils.FindEntry(_parentDom, id);
			if (parent == null) //it's new
			{
				//it's possible to create and entry, delete it, then checkin, leave us with this
				//spurious deletion messages
				if (string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(child, "dateDeleted")))
				{
					EventListener.ChangeOccurred(new XmlAdditionChangeReport("hackFixThis.lift", child));
				}
			}
			else if (LiftUtils.AreTheSame(child, parent))//unchanged or both made same change
			{
			}
			else //one or both changed
			{
				if (!string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(child, "dateDeleted")))
				{
					EventListener.ChangeOccurred(new XmlDeletionChangeReport("hackFixThis.lift", child));
				}
				else
				{
					//enhance... we are only using this because it will conveniently find the differences
					//and fire them off for us

					//enhance: we can skip this and just say "something changed in this entry",
					//until we really *need* the details (if ever), and have a way to call this then
					//_mergingStrategy.MakeMergedEntry(this.EventListener, child, parent, parent);
					EventListener.ChangeOccurred(new XmlChangedRecordReport("hackFixThis.lift", parent,child));
				}
			}
			_processedIds.Add(id);
		}
	}
}