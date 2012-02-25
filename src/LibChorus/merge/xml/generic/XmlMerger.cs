using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders.text;
using Chorus.FileTypeHanders.xml;
using Chorus.Properties;

namespace Chorus.merge.xml.generic
{
	 public class NodeMergeResult : ChangeAndConflictAccumulator
		{
				public XmlNode MergedNode { get; internal set; }
		}

	public class XmlMerger
	{
		public IMergeEventListener EventListener { get; set; }
		public MergeSituation MergeSituation{ get; set;}
		public MergeStrategies MergeStrategies { get; set; }

		/// <summary>
		/// The nodes we were merging on the last MergeChildren call; these (specifically _oursContext) are the
		/// nodes that are the basis of the Context we set in calling the Listener's EnteringContext method.
		/// They are used to allow any Conflict objects we generate to BuildHtmlDetails.
		/// </summary>
		private XmlNode _oursContext, _theirsContext, _ancestorContext;

		private IGenerateHtmlContext _htmlContextGenerator;

		public XmlMerger(MergeSituation mergeSituation)
		{
			MergeSituation = mergeSituation;
			EventListener  = new NullMergeEventListener();
			MergeStrategies = new MergeStrategies();
		}


		public NodeMergeResult Merge(XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			var result = new NodeMergeResult();
			if (EventListener != null && EventListener is DispatchingMergeEventListener)
			{
				((DispatchingMergeEventListener)EventListener).AddEventListener(result);
			}
			else
			{
				DispatchingMergeEventListener dispatcher = new DispatchingMergeEventListener();
				dispatcher.AddEventListener(result);
				if (EventListener != null)
				{
					dispatcher.AddEventListener(EventListener);
				}
				EventListener = dispatcher;
			}
			MergeInner(ref ours, theirs, ancestor);
			result.MergedNode = ours;
			return result;
		}

		public XmlNode Merge(IMergeEventListener eventListener, XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			EventListener = eventListener;
			MergeInner(ref ours, theirs, ancestor);
			return ours;
		}

		internal void ConflictOccurred(IConflict conflict)
		{
			if (_htmlContextGenerator == null)
				_htmlContextGenerator = new SimpleHtmlGenerator();
			EventListener.RecordContextInConflict(conflict);
			conflict.MakeHtmlDetails(_oursContext, _theirsContext, _ancestorContext, _htmlContextGenerator);
			EventListener.ConflictOccurred(conflict);

		}

		class SimpleHtmlGenerator : IGenerateHtmlContext
		{
			public string HtmlContext(XmlNode mergeElement)
			{
				return XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml);
			}

			public string HtmlContextStyles(XmlNode mergeElement)
			{
				return ""; // GetXmlForShowingInHtml does not generate any classes
			}
		}

		/// <summary>
		/// This method does the actual work for the various public entry points of XmlMerge
		/// and from the MergeChildrenMethod class, as it processes child nodes.
		/// </summary>
		internal void MergeInner(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (MergeAtomicElementService.Run(this, ref ours, theirs, ancestor))
				return;

			MergeAttributes(ref ours, theirs, ancestor);

			// It could be possible for the elements to have no children, in which case, there is nothing more to merge, so just return.
			if (ours != null && !ours.HasChildNodes && theirs != null && !theirs.HasChildNodes && ancestor != null && !ancestor.HasChildNodes)
				return;

			MergeChildren(ref ours, theirs, ancestor);
		}

		public NodeMergeResult Merge(string ourXml, string theirXml, string ancestorXml)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode ourNode = XmlUtilities.GetDocumentNodeFromRawXml(ourXml, doc);
			XmlNode theirNode = XmlUtilities.GetDocumentNodeFromRawXml(theirXml, doc);
			XmlNode ancestorNode = XmlUtilities.GetDocumentNodeFromRawXml(ancestorXml, doc);

			return Merge(ourNode, theirNode, ancestorNode);
		}

		public NodeMergeResult MergeFiles(string ourPath, string theirPath, string ancestorPath)
		{
			//Debug.Fail("time to attach");
			XmlDocument ourDoc = new XmlDocument();
			ourDoc.Load(ourPath);
			XmlNode ourNode = ourDoc.DocumentElement;

			XmlDocument theirDoc = new XmlDocument();
			theirDoc.Load(theirPath);
			XmlNode theirNode = theirDoc.DocumentElement;

			XmlNode ancestorNode = null;
			if (File.Exists(ancestorPath)) // it's possible for the file to be created independently by each user, with no common ancestor
			{
				XmlDocument ancestorDoc = new XmlDocument();
				try
				{
					ancestorDoc.Load(ancestorPath);
					ancestorNode = ancestorDoc.DocumentElement;
				}
				catch (XmlException e)
				{
					if(File.ReadAllText(ancestorPath).Length>1 )
					{
						throw e;
					}
					//otherwise, it's likely an artifact of how hg seems to create an empty file
					//for the ancestor, if there wasn't one there before, and empty = not well-formed xml!
				}
			 }

			return Merge(ourNode, theirNode, ancestorNode);
		}

		private static IEnumerable<XmlAttribute> GetAttrs(XmlNode node)
		{
			return (node is XmlCharacterData || node == null)
					? new List<XmlAttribute>()
					: new List<XmlAttribute>(node.Attributes.Cast<XmlAttribute>()); // Need to copy so we can iterate while changing.
		}

		private static XmlAttribute GetAttributeOrNull(XmlNode node, string name)
		{
			return node == null ? null : node.Attributes.GetNamedItem(name) as XmlAttribute;
		}

		private void MergeAttributes(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			var extantNode = ours ?? theirs ?? ancestor;

			var newForOurs = new List<XmlAttribute>();
			// Deletions from ancestor, no matter who did it.
			foreach (var ancestorAttr in GetAttrs(ancestor))
			{
				var ourAttr = GetAttributeOrNull(ours, ancestorAttr.Name);
				var theirAttr = GetAttributeOrNull(theirs, ancestorAttr.Name);
				if (theirAttr == null)
				{
					if (ourAttr == null)
					{
						// Both deleted.
						EventListener.ChangeOccurred(new XmlAttributeBothDeletedReport(MergeSituation.PathToFileInRepository, ancestorAttr));
						ancestor.Attributes.Remove(ancestorAttr);
						continue;
					}
					if (ourAttr.Value != ancestorAttr.Value)
					{
						// They deleted, but we changed, so we win under the principle of
						// least data loss (an attribute can be a huge text element).
						ConflictOccurred(new EditedVsRemovedAttributeConflict(ourAttr.Name, ourAttr.Value, null, ancestorAttr.Value, MergeSituation, MergeSituation.AlphaUserId));
						continue;
					}
					// They deleted. We did zip.
					EventListener.ChangeOccurred(new XmlAttributeDeletedReport(MergeSituation.PathToFileInRepository, ancestorAttr));
					ancestor.Attributes.Remove(ancestorAttr);
					ours.Attributes.Remove(ourAttr);
					continue;
				}
				if (ourAttr == null)
				{
					if (ancestorAttr.Value != theirAttr.Value)
					{
						// We deleted it, but at the same time, they changed it. So just add theirs in, under the principle of
						// least data loss (an attribute can be a huge text element)
						newForOurs.Add(theirAttr);
						//var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr, true);
						//ours.Attributes.Append(importedAttribute);
						ConflictOccurred(new RemovedVsEditedAttributeConflict(theirAttr.Name, null, theirAttr.Value, ancestorAttr.Value, MergeSituation,
							MergeSituation.BetaUserId));
						continue;
					}
					// We deleted it. They did nothing.
					EventListener.ChangeOccurred(new XmlAttributeDeletedReport(MergeSituation.PathToFileInRepository, ancestorAttr));
					ancestor.Attributes.Remove(ancestorAttr);
					theirs.Attributes.Remove(theirAttr);
				}
			}

			foreach (var theirAttr in GetAttrs(theirs))
			{
				// Will never return null, since it will use the default one, if it can't find a better one.
				var mergeStrategy = MergeStrategies.GetElementStrategy(extantNode);
				var ourAttr = GetAttributeOrNull(ours, theirAttr.Name);
				var ancestorAttr = GetAttributeOrNull(ancestor, theirAttr.Name);

				if (ourAttr == null)
				{
					if (ancestorAttr == null)
					{
						var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr, true);
						ours.Attributes.Append(importedAttribute);
						EventListener.ChangeOccurred(new XmlAttributeAddedReport(MergeSituation.PathToFileInRepository, theirAttr));
					}
					// NB: Deletes are all handles above in first loop.
					//else if (ancestorAttr.Value == theirAttr.Value)
					//{
					//    continue; // we deleted it, they didn't touch it
					//}
					//else
					//{
					//    // We deleted it, but at the same time, they changed it. So just add theirs in, under the principle of
					//    // least data loss (an attribute can be a huge text element)
					//    var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr, true);
					//    ours.Attributes.Append(importedAttribute);

					//    EventListener.ConflictOccurred(new RemovedVsEditedAttributeConflict(theirAttr.Name, null, theirAttr.Value, ancestorAttr.Value, MergeSituation,
					//        MergeSituation.BetaUserId));
					//    continue;
					//}
				}
				else if (ancestorAttr == null) // Both introduced this attribute
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						EventListener.ChangeOccurred(new XmlAttributeBothAddedReport(MergeSituation.PathToFileInRepository, ourAttr));
						continue;
					}
					else
					{
						// Both added, but not the same.
						if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
						{
							if (MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
							{
								ConflictOccurred(new BothAddedAttributeConflict(theirAttr.Name, ourAttr.Value, theirAttr.Value, null, MergeSituation,
									MergeSituation.AlphaUserId));
							}
							else
							{
								ourAttr.Value = theirAttr.Value;
								ConflictOccurred(new BothAddedAttributeConflict(theirAttr.Name, ourAttr.Value, theirAttr.Value, null, MergeSituation,
									MergeSituation.BetaUserId));
							}
						}
					}
				}
				else if (ancestorAttr.Value == ourAttr.Value)
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						continue; // Nothing to do.
					}
					else
					{
						// They changed.
						if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
						{
							EventListener.ChangeOccurred(new XmlAttributeChangedReport(MergeSituation.PathToFileInRepository, theirAttr));
							ourAttr.Value = theirAttr.Value;
						}
						continue;
					}
				}
				else if (ourAttr.Value == theirAttr.Value)
				{
					// Both changed to same value
					EventListener.ChangeOccurred(new XmlAttributeBothMadeSameChangeReport(MergeSituation.PathToFileInRepository, ourAttr));
					continue;
				}
				else if (ancestorAttr.Value == theirAttr.Value)
				{
					// We changed the value. They did nothing.
					if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						EventListener.ChangeOccurred(new XmlAttributeChangedReport(MergeSituation.PathToFileInRepository, ourAttr));
						continue;
					}
				}
				else
				{
					//for unit test see Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
					if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						if (MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
						{
							ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value,
																							theirAttr.Value,
																							ancestorAttr.Value,
																							MergeSituation,
																							MergeSituation.AlphaUserId));
						}
						else
						{
							ourAttr.Value = theirAttr.Value;
							ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value,
																							theirAttr.Value,
																							ancestorAttr.Value,
																							MergeSituation,
																							MergeSituation.BetaUserId));
						}
					}
				}
			}

			foreach (var ourAttr in GetAttrs(ours))
			{
				var theirAttr = GetAttributeOrNull(theirs, ourAttr.Name);
				var ancestorAttr = GetAttributeOrNull(ancestor, ourAttr.Name);

				if (ancestorAttr == null)
				{
					if (theirAttr == null)
					{
						// We added it.
						EventListener.ChangeOccurred(new XmlAttributeAddedReport(MergeSituation.PathToFileInRepository, ourAttr));
						continue;
					}
					// They also added, and it may, or may not, be the same.
					continue;
				}
				if (theirAttr == null)
				{
					if (ourAttr.Value == ancestorAttr.Value) //we didn't change it, they deleted it
					{
						EventListener.ChangeOccurred(new XmlAttributeDeletedReport(MergeSituation.PathToFileInRepository, ourAttr));
						ours.Attributes.Remove(ourAttr);
					}
					// NB: Deletes are all handles above in first loop.
					//else
					//{
					//    // We changed it, and they deleted it, so stay with ours and add conflict report.
					//    EventListener.ConflictOccurred(new EditedVsRemovedAttributeConflict(ourAttr.Name, ourAttr.Value, null, ancestorAttr.Value, MergeSituation, MergeSituation.AlphaUserId));
					//}
				}
			}

			foreach (var newby in newForOurs)
			{
				// Wonder what happens if ours is null?
				ours.Attributes.Append((XmlAttribute)ours.OwnerDocument.ImportNode(newby, true));
			}
		}

		private void MergeChildren(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			_oursContext = ours;
			_theirsContext = theirs;
			_ancestorContext = ancestor;
			//is this a level of the xml file that would consitute the minimal unit conflict-understanding
			//from a user perspecitve?
			//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
			var generator = MergeStrategies.GetElementStrategy(ours).ContextDescriptorGenerator;
			if(generator != null)
			{
				//review: question: does this not get called at levels below the entry?
				//this would seem to fail at, say, a sense. I'm confused. (JH 30june09)
				ContextDescriptor descriptor;
				if (generator is IGenerateContextDescriptorFromNode)
				{
					// If the generator prefers the XmlNode, get the context that way.
					descriptor = ((IGenerateContextDescriptorFromNode) generator).GenerateContextDescriptor(ours,
						MergeSituation.PathToFileInRepository);
				}
				else
				{
					descriptor = generator.GenerateContextDescriptor(ours.OuterXml, MergeSituation.PathToFileInRepository);
				}
				EventListener.EnteringContext(descriptor);
				_htmlContextGenerator = (generator as IGenerateHtmlContext); // null is OK.
			}

			new MergeChildrenMethod(ours, theirs, ancestor, this).Run();
			// At some point, it may be necessary here to restore the pre-existing values of
			// _oursContext, _theirsContext, _ancestorContext, and _htmlContextGenerator.
			// and somehow restore the EventListener's Context.
			// Currently however no client generates further conflicts after calling MergeChildren.
		}
	}
}