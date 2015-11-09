using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus.FileTypeHanders.lift
{

	/// <summary>
	/// Given a parent and child lift file, reports on what changed.
	/// </summary>
	public class Lift2WayDiffer
	{
		private readonly FileInRevision _parentFileInRevision;
		private readonly FileInRevision _childFileInRevision;
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _childDom;
		private readonly XmlDocument _parentDom;
		private IMergeEventListener EventListener;
		private readonly Dictionary<string, XmlNode> _parentIdToNodeIndex;

		public static Lift2WayDiffer CreateFromFileInRevision(IMergeStrategy mergeStrategy, FileInRevision parent, FileInRevision child, IMergeEventListener eventListener, HgRepository repository)
		{
			return new Lift2WayDiffer(mergeStrategy, parent.GetFileContents(repository), child.GetFileContents(repository), eventListener, parent, child);
		}
		public static Lift2WayDiffer CreateFromStrings(IMergeStrategy mergeStrategy, string parentXml, string childXml, IMergeEventListener eventListener)
		{
			return new Lift2WayDiffer(mergeStrategy, parentXml, childXml, eventListener);
		}

		private Lift2WayDiffer(IMergeStrategy mergeStrategy, string parentXml, string childXml,IMergeEventListener eventListener)
		{
			_childDom = new XmlDocument();
			_parentDom = new XmlDocument();

			_childDom.LoadXml(childXml);
			_parentDom.LoadXml(parentXml);
			_parentIdToNodeIndex = new Dictionary<string, XmlNode>();

			EventListener = eventListener;
		}

		private Lift2WayDiffer(IMergeStrategy mergeStrategy, string parentXml, string childXml , IMergeEventListener listener, FileInRevision parentFileInRevision, FileInRevision childFileInRevision)
			:this(mergeStrategy,parentXml, childXml, listener)
		{
			_parentFileInRevision = parentFileInRevision;
			_childFileInRevision = childFileInRevision;
		}

		public void ReportDifferencesToListener()
		{
			foreach (XmlNode node in _parentDom.SafeSelectNodes("lift/entry"))
			{
				string strId = LiftUtils.GetId(node);
				if (!_parentIdToNodeIndex.ContainsKey(strId))
					_parentIdToNodeIndex.Add(strId, node);
				else
					System.Diagnostics.Debug.WriteLine(String.Format("Found ID multiple times: {0}", strId));
			}

			foreach (XmlNode childNode in _childDom.SafeSelectNodes("lift/entry"))
			{
				try
				{
					ProcessEntry(childNode);
				}
				catch (Exception error)
				{
					EventListener.ChangeOccurred(new ErrorDeterminingChangeReport(_parentFileInRevision,
																				  _childFileInRevision, null, childNode,
																				  error));
				}
			}

			//now detect any removed (not just marked as deleted) elements
			foreach (XmlNode parentNode in _parentIdToNodeIndex.Values)// _parentDom.SafeSelectNodes("lift/entry"))
			{
				try
				{
					if (!_processedIds.Contains(LiftUtils.GetId(parentNode)))
					{
						EventListener.ChangeOccurred(new XmlDeletionChangeReport(_parentFileInRevision, parentNode,
																				 null));
					}
				}
				catch (Exception error)
				{
					EventListener.ChangeOccurred(new ErrorDeterminingChangeReport(_parentFileInRevision,
																				  _childFileInRevision,
																				  parentNode,
																				  null,
																				  error));
				}
			}
		}


		private void ProcessEntry(XmlNode child)
		{
			string id = LiftUtils.GetId(child);
			XmlNode parent=null;// = LiftUtils.FindEntryById(_parentDom, id);
			_parentIdToNodeIndex.TryGetValue(id, out parent);

			string path = string.Empty;
			if (_childFileInRevision != null && !string.IsNullOrEmpty(_childFileInRevision.FullPath))
			{
				path = Path.GetFileName(_childFileInRevision.FullPath);
			}
			string url = LiftUtils.GetUrl(child, path);

			if (parent == null) //it's new
			{
				//it's possible to create and entry, delete it, then checkin, leave us with this
				//spurious deletion messages
				if (string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(child, "dateDeleted")))
				{
					EventListener.ChangeOccurred(new XmlAdditionChangeReport(_childFileInRevision, child, url));
				}
			}
			else if (LiftUtils.AreTheSame(child, parent))//unchanged or both made same change
			{
			}
			else //one or both changed
			{
				if (!string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(child, "dateDeleted")))
				{
					EventListener.ChangeOccurred(new XmlDeletionChangeReport(_parentFileInRevision, parent, child));
				}
				else
				{
					//enhance... we are only using this because it will conveniently find the differences
					//and fire them off for us

					//enhance: we can skip this and just say "something changed in this entry",
					//until we really *need* the details (if ever), and have a way to call this then
					//_mergingStrategy.MakeMergedEntry(this.EventListener, child, parent, parent);
					EventListener.ChangeOccurred(new XmlChangedRecordReport(_parentFileInRevision, _childFileInRevision, parent,child, url));
				}
			}
			_processedIds.Add(id);
		}


	}
}