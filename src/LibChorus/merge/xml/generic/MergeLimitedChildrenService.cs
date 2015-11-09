using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHandlers.xml;
using SIL.Code;

namespace Chorus.merge.xml.generic
{

	internal static class MergeLimitedChildrenService
	{
		public static void Run(XmlMerger merger, ElementStrategy strategy, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
// All routes tested in this method.
			Guard.AgainstNull(merger, "merger"); // Route tested.
			Guard.AgainstNull(strategy, "strategy"); // Route tested.
			if (ours == null && theirs == null && ancestor == null)
				throw new ArgumentNullException(); // Route tested.

			if (XmlUtilities.IsTextLevel(ours, theirs, ancestor))
			{
				// Route tested.
				new MergeTextNodesMethod(merger, merger.MergeStrategies.GetElementStrategy(ours ?? theirs ?? ancestor),
					new HashSet<XmlNode>(),
					ref ours, new List<XmlNode>(),
					theirs, new List<XmlNode>(),
					ancestor, new List<XmlNode>()).Run();
				return;
			}

			List<XmlNode> ourChildren;
			List<XmlNode> theirChildren;
			List<XmlNode> ancestorChildren;
			switch (strategy.NumberOfChildren)
			{
				default:
					throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrMore is not legal."); // Route tested.
				case NumberOfChildrenAllowed.Zero:
					ourChildren = GetElementChildren(ours).ToList();
					if (ourChildren.Any())
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.Zero is not legal, when there are child element nodes."); // Route tested.
					theirChildren = GetElementChildren(theirs).ToList();
					if (theirChildren.Any())
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.Zero is not legal, when there are child element nodes."); // Route tested.
					ancestorChildren = GetElementChildren(ancestor).ToList();
					if (ancestorChildren.Any())
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.Zero is not legal, when there are child element nodes."); // Route tested.

					// Don't merge deeper than merging the attributes, since there aren't supposed to be any children.
					// Already done by caller MergeXmlAttributesService.MergeAttributes(merger, ref ours, theirs, ancestor);
					// Route tested.
					break;
				case NumberOfChildrenAllowed.ZeroOrOne:
					ourChildren = GetElementChildren(ours).ToList();
					if (ourChildren.Count > 1)
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrOne is not legal, when there are multiple child nodes."); // Route tested.
					theirChildren = GetElementChildren(theirs).ToList();
					if (theirChildren.Count > 1)
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrOne is not legal, when there are multiple child nodes."); // Route tested.
					ancestorChildren = GetElementChildren(ancestor).ToList();
					if (ancestorChildren.Count > 1)
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrOne is not legal, when there are child element nodes."); // Route tested.

					// Already done by caller MergeXmlAttributesService.MergeAttributes(merger, ref ours, theirs, ancestor);

					if (!ourChildren.Any() && !theirChildren.Any() && ancestor != null)
						return; // Route tested.

					// The return value of Run may be the original 'ours', a replacement for it, or null.
					ours = Run(merger, ours, theirs, ancestor); // Route tested.
					break;
			}
		}

		private static XmlNode Run(XmlMerger merger, XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ours == null && theirs == null && ancestor == null)
				return null;

			if (ancestor == null)
			{
				return HandleCaseOfNoAncestor(merger, ours, theirs);
			}
			// ancestor is not null at this point.
			var mergeSituation = merger.MergeSituation;
			var pathToFileInRepository = mergeSituation.PathToFileInRepository;
			if (ours == null && theirs == null)
			{
				// We both deleted main node.
// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(pathToFileInRepository, ancestor));
				return null;
			}
			if (ours == null)
			{
				return HandleOursNotPresent(merger, ancestor, theirs);
			}
			if (theirs == null)
			{
				return HandleTheirsNotPresent(merger, ancestor, ours);
			}
			// End of checking main parent node.

			// ancestor, ours, and theirs all exist here.
			var ourChild = GetElementChildren(ours).FirstOrDefault();
			var theirChild = GetElementChildren(theirs).FirstOrDefault();
			var ancestorChild = GetElementChildren(ancestor).FirstOrDefault();
			if (ourChild == null && theirChild == null && ancestorChild == null)
			{
				return ours; // All three are childless.
			}

			if (ancestorChild == null)
			{
				return HandleCaseOfNoAncestorChild(merger, ours, ourChild, theirChild);
			}
			var mergeStrategyForChild = merger.MergeStrategies.GetElementStrategy(ancestorChild);
			if (ourChild == null)
			{
				return HandleOurChildNotPresent(merger, ours, ancestor, theirChild, pathToFileInRepository, ancestorChild, mergeSituation, mergeStrategyForChild);
			}
			if (theirChild == null)
			{
				return HandleTheirChildNotPresent(merger, ours, ancestor, ancestorChild, ourChild, mergeSituation, mergeStrategyForChild, pathToFileInRepository);
			}

			// ancestorChild, ourChild, and theirChild all exist.
			// But, it could be that we or they deleted and added something.
			// Check for edit vs delete+add, or there can be two items in ours, which is not legal.
			var match = mergeStrategyForChild.MergePartnerFinder.GetNodeToMerge(ourChild, ancestor, SetFromChildren.Get(ancestor));
			if (match == null)
			{
				// we deleted it and added a new one.
				if (XmlUtilities.AreXmlElementsEqual(theirChild, ancestorChild))
				{
					// Our delete+add wins, since they did nothing.
					merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(pathToFileInRepository, ancestor, ancestorChild));
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, ourChild));
					return ours;
				}

				// They edited old one, so they win over our delete+add.
				merger.ConflictOccurred(new RemovedVsEditedElementConflict(theirChild.Name, theirChild, null, ancestorChild,
																		   mergeSituation, mergeStrategyForChild,
																		   mergeSituation.BetaUserId));
				ours.ReplaceChild(ours.OwnerDocument.ImportNode(theirChild, true), ourChild);
				return ours;
			}
			match = mergeStrategyForChild.MergePartnerFinder.GetNodeToMerge(theirChild, ancestor, SetFromChildren.Get(ancestor));
			if (match == null)
			{
				// they deleted it and added a new one.
				if (XmlUtilities.AreXmlElementsEqual(ourChild, ancestorChild))
				{
					// Their delete+add wins, since we did nothing.
					merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(pathToFileInRepository, ancestor, ancestorChild));
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, theirChild));
					ours.ReplaceChild(ours.OwnerDocument.ImportNode(theirChild, true), ourChild);
					return ours;
				}

				// We edited old one, so we win over their delete+add.
				merger.ConflictOccurred(new RemovedVsEditedElementConflict(ourChild.Name, ourChild, null, ancestorChild,
																		   mergeSituation, mergeStrategyForChild,
																		   mergeSituation.AlphaUserId));
				return ours;
			}

			merger.MergeInner(ref ourChild, theirChild, ancestorChild);

// Route tested. (UsingWith_NumberOfChildrenAllowed_ZeroOrOne_DoesNotThrowWhenParentHasOneChildNode)
			return ours;
		}

		private static XmlNode HandleTheirChildNotPresent(XmlMerger merger, XmlNode ours, XmlNode ancestor,
														  XmlNode ancestorChild, XmlNode ourChild, MergeSituation mergeSituation,
														  IElementDescriber mergeStrategyForChild, string pathToFileInRepository)
		{
			if (XmlUtilities.AreXmlElementsEqual(ancestorChild, ourChild))
			{
				// They deleted it. We did nothing.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(pathToFileInRepository, ancestor, ancestorChild));
			}
			else
			{
				// edit vs delete conflict.
				merger.ConflictOccurred(new EditedVsRemovedElementConflict(ourChild.Name, ourChild, null, ancestorChild,
																		   mergeSituation, mergeStrategyForChild,
																		   mergeSituation.AlphaUserId));
			}
			return ours;
		}

		private static XmlNode HandleOurChildNotPresent(XmlMerger merger, XmlNode ours, XmlNode ancestor, XmlNode theirChild,
														string pathToFileInRepository, XmlNode ancestorChild,
														MergeSituation mergeSituation, IElementDescriber mergeStrategyForChild)
		{
			if (theirChild == null)
			{
				// Both deleted it.
				merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(pathToFileInRepository, ancestorChild));
			}
			else
			{
				if (XmlUtilities.AreXmlElementsEqual(ancestorChild, theirChild))
				{
					// We deleted it. They did nothing.
					merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(pathToFileInRepository, ancestor, ancestorChild));
				}
				else
				{
					// delete vs edit conflict.
					merger.ConflictOccurred(new RemovedVsEditedElementConflict(theirChild.Name, theirChild, null, ancestorChild,
																			   mergeSituation, mergeStrategyForChild,
																			   mergeSituation.BetaUserId));
					ours.AppendChild(theirChild);
				}
			}
			return ours;
		}

		private static XmlNode HandleCaseOfNoAncestorChild(XmlMerger merger, XmlNode ours, XmlNode ourChild, XmlNode theirChild)
		{
			var mergeSituation = merger.MergeSituation;
			var pathToFileInRepository = mergeSituation.PathToFileInRepository;
			var mergeStrategyForChild = merger.MergeStrategies.GetElementStrategy(ourChild ?? theirChild);
			if (ourChild == null)
			{
				// they added child.
				merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, theirChild));
				ours.AppendChild(theirChild);
				return ours;
			}
			if (theirChild == null)
			{
				// We added child.
				merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, ourChild));
				return ours;
			}
			// Both added child.
			if (XmlUtilities.AreXmlElementsEqual(ourChild, theirChild))
			{
				// Both are the same.
				merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(pathToFileInRepository, ourChild));
				return ours;
			}
			// Both are different.
			if (XmlUtilities.IsTextLevel(ourChild, theirChild, null))
			{
				if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					merger.ConflictOccurred(new XmlTextBothAddedTextConflict(ourChild.Name, ourChild, theirChild, mergeSituation,
																			 mergeStrategyForChild, mergeSituation.AlphaUserId));
				}
				else
				{
					merger.ConflictOccurred(new XmlTextBothAddedTextConflict(theirChild.Name, theirChild, ourChild, mergeSituation,
																			 mergeStrategyForChild, mergeSituation.BetaUserId));
					ours.ReplaceChild(ours.OwnerDocument.ImportNode(theirChild, true), ourChild);
				}
			}
			else
			{
				if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					var ourChildClone = MakeClone(ourChild);
					var theirChildClone = MakeClone(theirChild);
					if (XmlUtilities.AreXmlElementsEqual(ourChildClone, theirChildClone))
					{
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, ourChild));
						var ourChildReplacement = ourChild;
						merger.MergeInner(ref ourChildReplacement, theirChild, null);
						if (!ReferenceEquals(ourChild, ourChildReplacement))
						{
							ours.ReplaceChild(ours.OwnerDocument.ImportNode(ourChildReplacement, true), ourChild);
						}
					}
					else
					{
						merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ourChild.Name, ourChild, theirChild,
																										mergeSituation, mergeStrategyForChild,
																										mergeSituation.AlphaUserId));
					}
				}
				else
				{
					var ourChildClone = MakeClone(ourChild);
					var theirChildClone = MakeClone(theirChild);
					if (XmlUtilities.AreXmlElementsEqual(ourChildClone, theirChildClone))
					{
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, theirChild));
						var ourChildReplacement = ourChild;
						merger.MergeInner(ref ourChildReplacement, theirChild, null);
						if (!ReferenceEquals(ourChild, ourChildReplacement))
						{
							ours.ReplaceChild(ours.OwnerDocument.ImportNode(ourChildReplacement, true), ourChild);
						}
					}
					else
					{
						merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(theirChild.Name, theirChild, ourChild,
																										mergeSituation, mergeStrategyForChild,
																										mergeSituation.BetaUserId));
						ours.ReplaceChild(ours.OwnerDocument.ImportNode(theirChild, true), ourChild);
					}
				}
			}
			return ours;
		}

		private static XmlNode HandleTheirsNotPresent(XmlMerger merger, XmlNode ancestor, XmlNode ours)
		{
			// They deleted it,
			var mergeSituation = merger.MergeSituation;
			if (XmlUtilities.AreXmlElementsEqual(ancestor, ours))
			{
				// and we did nothing
				// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(mergeSituation.PathToFileInRepository, ancestor, ours));
				return null;
			}

			// but we edited it.
			merger.ConflictOccurred(new RemovedVsEditedElementConflict(ancestor.Name, ours, null, ancestor, mergeSituation, merger.MergeStrategies.GetElementStrategy(ancestor), mergeSituation.AlphaUserId));
			return ours;
		}

		private static XmlNode HandleOursNotPresent(XmlMerger merger, XmlNode ancestor, XmlNode theirs)
		{
			// We deleted,
			var mergeSituation = merger.MergeSituation;
// We deleted it.
			if (XmlUtilities.AreXmlElementsEqual(ancestor, theirs))
			{
				// and they did nothing.
				// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(mergeSituation.PathToFileInRepository, ancestor, theirs));
				return null;
			}

			// but they edited it.
			merger.ConflictOccurred(new RemovedVsEditedElementConflict(ancestor.Name, theirs, null, ancestor, mergeSituation,
																	   merger.MergeStrategies.GetElementStrategy(ancestor), mergeSituation.BetaUserId));
			return theirs;
		}

		private static XmlNode HandleCaseOfNoAncestor(XmlMerger merger, XmlNode ours, XmlNode theirs)
		{
			var mainNodeStrategy = merger.MergeStrategies.GetElementStrategy(ours ?? theirs);
			var mergeSituation = merger.MergeSituation;
			var pathToFileInRepository = mergeSituation.PathToFileInRepository;
			if (ours == null)
			{
				// They added, we did nothing.
// Route tested, but the MergeChildrenMethod adds the change report for us.
				// So, theory has it one can't get here from any normal place.
				// But, keep it, in case MergeChildrenMethod gets 'fixed'.
				merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, theirs));
				return theirs;
			}
			if (theirs == null)
			{
				// We added, they did nothing.
// Route tested.
				merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, ours));
				return ours;
			}

			// Both added the special containing node.
			// Remove children nodes to see if main containing nodes are the same.
			if (XmlUtilities.AreXmlElementsEqual(ours, theirs))
			{
// Route tested.
				// Same content.
				merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(pathToFileInRepository, ours));
			}
			else
			{
				// Different content.
				var ourChild = GetElementChildren(ours).FirstOrDefault();
				var theirChild = GetElementChildren(theirs).FirstOrDefault();
				var ourClone = MakeClone(ours);
				var theirClone = MakeClone(theirs);
				if (XmlUtilities.AreXmlElementsEqual(ourClone, theirClone))
				{
					// new main elements are the same, but not the contained child
					merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(pathToFileInRepository, ourClone));
					if (ourChild == null && theirChild == null)
						return ours; // Nobody added the child node.
					if (ourChild == null)
					{
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, theirChild));
						ours.AppendChild(theirChild);
						return ours;
					}
					if (theirChild == null)
					{
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(pathToFileInRepository, ourChild));
						return ours;
					}
					// both children exist, but are different.
					var mergeStrategyForChild = merger.MergeStrategies.GetElementStrategy(ourChild);
					if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
					{
						// Do the clone thing on the two child nodes to see if the diffs are in the child or lower down.
						var ourChildClone = MakeClone(ourChild);
						var theirChildClone = MakeClone(theirChild);
						if (XmlUtilities.AreXmlElementsEqual(ourChildClone, theirChildClone))
						{
							var ourChildReplacement = ourChild;
							merger.MergeInner(ref ourChildReplacement, theirChild, null);
							if (!ReferenceEquals(ourChild, ourChildReplacement))
							{
								ours.ReplaceChild(ours.OwnerDocument.ImportNode(ourChildReplacement, true), ourChild);
							}
						}
						else
						{
							merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ourChild.Name, ourChild, theirChild,
																											mergeSituation, mergeStrategyForChild,
																											mergeSituation.AlphaUserId));
						}
					}
					else
					{
						// Do the clone thing on the two child nodes to see if the diffs are in the child or lower down.
						var ourChildClone = MakeClone(ourChild);
						var theirChildClone = MakeClone(theirChild);
						if (XmlUtilities.AreXmlElementsEqual(ourChildClone, theirChildClone))
						{
							var ourChildReplacement = ourChild;
							merger.MergeInner(ref ourChildReplacement, theirChild, null);
							if (!ReferenceEquals(ourChild, ourChildReplacement))
							{
								ours.ReplaceChild(ours.OwnerDocument.ImportNode(ourChildReplacement, true), ourChild);
							}
						}
						else
						{
							merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ourChild.Name, ourChild, theirChild,
																											mergeSituation, mergeStrategyForChild,
																											mergeSituation.AlphaUserId));
							ours.ReplaceChild(ours.OwnerDocument.ImportNode(theirChild, true), ourChild);
						}
					}
					return ours;
				}

				// Main containing element is not the same. Not to worry about child
				if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ours.Name, ours, theirs,
																									mergeSituation, mainNodeStrategy,
																									mergeSituation.AlphaUserId));
				}
				else
				{
					merger.ConflictOccurred(new XmlTextBothAddedTextConflict(theirs.Name, theirs, ours, mergeSituation,
																			 mainNodeStrategy, mergeSituation.BetaUserId));
					ours = theirs;
				}
			}
			return ours;
		}

		private static XmlNode MakeClone(XmlNode node)
		{
			var clone = node.Clone();
			while (clone.HasChildNodes)
			{
				clone.RemoveChild(clone.FirstChild);
			}
			return clone;
		}

		private static IEnumerable<XmlNode> GetElementChildren(XmlNode parent)
		{
			return parent == null
					? new List<XmlNode>()
					: (from XmlNode child in parent.ChildNodes
					   where child.NodeType == XmlNodeType.Element // || child.NodeType == XmlNodeType.Text
					   select child);
		}
	}
}