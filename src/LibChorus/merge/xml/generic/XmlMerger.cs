using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders.text;
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

			// This loop is either a normal element, or it has only text content.
			var isTextLevel = ours != null && (ours.ChildNodes.Count == 1 && ours.FirstChild.NodeType == XmlNodeType.Text);
			isTextLevel = isTextLevel || theirs != null && (theirs.ChildNodes.Count == 1 && theirs.FirstChild.NodeType == XmlNodeType.Text);
			isTextLevel = isTextLevel || ancestor != null && (ancestor.ChildNodes.Count == 1 && ancestor.FirstChild.NodeType == XmlNodeType.Text);
			if (isTextLevel)
			{
				MergeTextNodes(ref ours, theirs, ancestor);
			}
			else
			{
				MergeChildren(ref ours, theirs, ancestor);
			}
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
					//otherwise, it's likely an artifact of how hg seems to create an emty file
					//for the ancestor, if there wasn't one there before, and empty = not well-formed xml!
				}
			 }

			return Merge(ourNode, theirNode, ancestorNode);
		}

		private static IEnumerable<XmlAttribute> GetAttrs(XmlNode node)
		{
			//need to copy so we can iterate while changing
			return new List<XmlAttribute>(node.Attributes.Cast<XmlAttribute>());
		}

		private static XmlAttribute GetAttributeOrNull(XmlNode node, string name)
		{
			if (node == null)
				return null;
			return node.Attributes.GetNamedItem(name) as XmlAttribute;
		}

		private void MergeAttributes(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			foreach (XmlAttribute theirAttr in GetAttrs(theirs))
			{
				XmlAttribute ourAttr = GetAttributeOrNull(ours, theirAttr.Name);
				XmlAttribute ancestorAttr = GetAttributeOrNull(ancestor, theirAttr.Name);

				if (ourAttr == null)
				{
					if (ancestorAttr == null)
					{
						var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr, true);
						ours.Attributes.Append(importedAttribute);
					}
					else if (ancestorAttr.Value == theirAttr.Value)
					{
						continue; // we deleted it, they didn't touch it
					}
					else //we deleted it, but at the same time, they changed it. So just add theirs in, under the principle of
						//least data loss (an attribute can be a huge text element)
					{
						var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr, true);
						ours.Attributes.Append(importedAttribute);

						EventListener.ConflictOccurred(new RemovedVsEditedAttributeConflict(theirAttr.Name, null, theirAttr.Value, ancestorAttr.Value, MergeSituation,
							MergeSituation.BetaUserId));
						continue;
					}
				}
				else if (ancestorAttr == null) // we both introduced this attribute
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						//nothing to do
						continue;
					}
					else
					{
						var strat = MergeStrategies.GetElementStrategy(ours);

						//for unit test see Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
						if (strat == null || !strat.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
						{
							EventListener.ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value, theirAttr.Value, null, MergeSituation,
								MergeSituation.AlphaUserId));
						}
					}
				}
				else if (ancestorAttr.Value == ourAttr.Value)
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						//nothing to do
						continue;
					}
					else //theirs is a change
					{
						ourAttr.Value = theirAttr.Value;
					}
				}
				else if (ourAttr.Value == theirAttr.Value)
				{
					//both changed to same value
					continue;
				}
				else if (ancestorAttr.Value == theirAttr.Value)
				{
					//only we changed the value
					continue;
				}
				else
				{
					var strat = this.MergeStrategies.GetElementStrategy(ours);

					//for unit test see Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
					if (strat == null || !strat.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						EventListener.ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value,
																						theirAttr.Value,
																						ancestorAttr.Value,
																						MergeSituation,
																						MergeSituation.AlphaUserId));
					}
				}
			}

			// deal with their deletions
			foreach (XmlAttribute ourAttr in GetAttrs(ours))
			{

				XmlAttribute theirAttr = GetAttributeOrNull(theirs, ourAttr.Name);
				XmlAttribute ancestorAttr = GetAttributeOrNull(ancestor,ourAttr.Name);

				if (theirAttr == null && ancestorAttr != null)
				{
					if (ourAttr.Value == ancestorAttr.Value) //we didn't change it, they deleted it
					{
						ours.Attributes.Remove(ourAttr);
					}
					else
					{
						EventListener.ConflictOccurred(new RemovedVsEditedAttributeConflict(ourAttr.Name, ourAttr.Value, null, ancestorAttr.Value, MergeSituation, MergeSituation.AlphaUserId));
					}
				}
			}
		}

		internal void MergeTextNodes(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ours != null && ours.ChildNodes.Count > 0 && ours.FirstChild.NodeType != XmlNodeType.Text)
				throw new ArgumentException(AnnotationImages.kNotATextElement, "ours");
			if (theirs != null && theirs.ChildNodes.Count > 0 && theirs.FirstChild.NodeType != XmlNodeType.Text)
				throw new ArgumentException(AnnotationImages.kNotATextElement, "ours");
			if (ancestor != null && ancestor.ChildNodes.Count > 0 && ancestor.FirstChild.NodeType != XmlNodeType.Text)
				throw new ArgumentException(AnnotationImages.kNotATextElement, "ours");

			var ourText = ours == null ? null : ours.InnerText.Trim();
			var theirText = theirs == null ? null : theirs.InnerText.Trim();
			var ancestorText = ancestor == null ? null : ancestor.InnerText.Trim();

			if (ourText == theirText)
			{
				// Null, empty strings or same.
				// [RBR: TODO: Right, but is it the same as ancestor, or did both make the same change?] It could be addition, deletion, or plain vanilla change.
				return; // we agree
			}

			if (string.IsNullOrEmpty(ourText))
			{
				if (ancestor == null || string.IsNullOrEmpty(ancestorText))
				{
					// TODO: Add new text added report.
					ours.InnerText = theirs.InnerText; // We had it empty, or null.
					return;
				}
				if (ancestorText == theirText)
				{
					// TODO: Add new text deleted report.
					// They didn't touch it. So leave it deleted
					return;
				}
				//they edited it. Keep theirs under the principle of least data loss.
				ours.InnerText = theirs.InnerText;
				EventListener.ConflictOccurred(new RemovedVsEditedTextConflict(ours, theirs, ancestor, MergeSituation,
					MergeSituation.BetaUserId));
				return;
			}

			if ((ancestor == null) || (ours.InnerText != ancestor.InnerText))
			{
				//we're not empty, we edited it, and we don't equal theirs.
				//  Moved  EventListener.ChangeOccurred(new TextEditChangeReport(this.MergeSituation.PathToFileInRepository, SafelyGetStringTextNode(ancestor), SafelyGetStringTextNode(ours)));
				if (string.IsNullOrEmpty(theirText))
				{
					if (ancestor == null)
					{
						// We both added at least the containing element for the text.
						if (ourText == theirText)
						{
							// Both added the same thing, even if it is only an empty element.
							// TODO: Add a "both added" change report.
							return;
						}
						if (theirText.Length > 0 && ourText == string.Empty)
						{
							// They added some content, even.
							// TODO: Add a "text addition" change report for the right person.
							return;
						}
						if (ourText.Length > 0 && theirText == string.Empty)
						{
							// We added some content, even.
							// TODO: Add a "text addition" change report for the right person.
							return;
						}
					}
					//we edited, they deleted it. Keep ours.
					EventListener.ConflictOccurred(new EditedVsRemovedTextConflict(ours, theirs, ancestor, MergeSituation,
						MergeSituation.AlphaUserId));
					return;
				}
				// We know: ours is different from theirs; ours is not empty; ours is different from ancestor;
				// theirs is not empty.
				if (ancestor != null && theirs.InnerText == ancestor.InnerText)
				{
					// Moved from aboe.
					EventListener.ChangeOccurred(new TextEditChangeReport(this.MergeSituation.PathToFileInRepository, SafelyGetStringTextNode(ancestor), SafelyGetStringTextNode(ours)));
					return; // we edited it, they did not, keep ours.
				}
				//both edited it. Keep ours, but report conflict.
				EventListener.ConflictOccurred(new BothEditedTextConflict(ours, theirs, ancestor, MergeSituation, MergeSituation.AlphaUserId));
				return;
			}
			// we didn't edit it, they did
			EventListener.ChangeOccurred(new TextEditChangeReport(MergeSituation.PathToFileInRepository, SafelyGetStringTextNode(ancestor), SafelyGetStringTextNode(theirs)));
			ours.InnerText = theirs.InnerText;
		}

		private static string SafelyGetStringTextNode(XmlNode node)
		{
			if(node==null || node.InnerText==null)
				return String.Empty;
			return node.InnerText.Trim();
		}

		private void MergeChildren(ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			//is this a level of the xml file that would consitute the minimal unit conflict-understanding
			//from a user perspecitve?
			//e.g., in a dictionary, this is the lexical entry.  In a text, it might be  a paragraph.
			var generator = MergeStrategies.GetElementStrategy(ours).ContextDescriptorGenerator;
			if(generator!=null)
			{
				//review: question: does this not get called at levels below the entry?
				//this would seem to fail at, say, a sense. I'm confused. (JH 30june09)
				var descriptor = generator.GenerateContextDescriptor(ours.OuterXml, MergeSituation.PathToFileInRepository);
				EventListener.EnteringContext(descriptor);
			}

			new MergeChildrenMethod(ours, theirs, ancestor, this).Run();
		}



	}
}