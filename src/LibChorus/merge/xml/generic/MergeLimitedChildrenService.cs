using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Palaso.Code;

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

					// The return value of Run may be the original 'ours', or a replacement for it.
					ours = Run(merger, ours, theirs, ancestor); // Route tested.
					break;
			}
		}

		private static XmlNode Run(XmlMerger merger, XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ours == null && theirs == null && ancestor == null)
				return null;

			var ourChild = GetElementChildren(ours).FirstOrDefault();
			var theirChild = GetElementChildren(theirs).FirstOrDefault();
			var ancestorChild = GetElementChildren(ancestor).FirstOrDefault();
			XmlNode ourReplacementChild;
			if (ancestor == null)
			{
				if (ours == null)
				{
					// They added, we did nothing.
// Route tested, but the MergeChildrenMethod adds the change report for us.
					// So, theory has it one can't get here from any normal place.
					// But, keep it, in case MergeChildrenMethod gets 'fixed'.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, theirs));
					return theirs;
				}
				if (theirs == null)
				{
					// We added, they did nothing.
// Route tested.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
					return ours;
				}

				// Both added the special containing node.
// Route tested.
				merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(merger.MergeSituation.PathToFileInRepository, ours));

				if (ourChild == null && theirChild == null && ancestorChild == null)
					return null; // Route tested.

				if (ourChild == null && theirChild != null)
				{
// Route tested.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, theirChild));
					ours.AppendChild(theirChild);
					return ours;
				}
				if (theirChild == null)
				{
// Route tested.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ourChild));
					return ours;
				}

				// Both ourChild and theirChild exist, and may or may not be the same.
				var mergeStrategy = merger.MergeStrategies.GetElementStrategy(ourChild);
				var match = mergeStrategy.MergePartnerFinder.GetNodeToMerge(ourChild, theirs, SetFromChildren.Get(theirs));
				if (match == null)
				{
					XmlNode winner;
					string winnerId;
					XmlNode loser;
					// Work up conflict report (for both we and they win options), since the users didn't add the same thing.
					if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
					{
						winner = ourChild;
						winnerId = merger.MergeSituation.AlphaUserId;
						loser = theirChild;
					}
					else
					{
						winner = theirChild;
						winnerId = merger.MergeSituation.BetaUserId;
						loser = ourChild;
						ours = theirs;
					}
// Route tested.
					merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(winner.Name, winner, loser, merger.MergeSituation, mergeStrategy, winnerId));
					return ours;
				}

				// Matched nodes, as far as that goes. But, are they the same or not?
				if (XmlUtilities.AreXmlElementsEqual(ourChild, theirChild))
				{
					// Both added the same thing.
// Route tested.
					merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(merger.MergeSituation.PathToFileInRepository, ourChild));
					return ours;
				}

				// Move on down and merge them.
				// Both messed with the inner stuff, but not the same way.
// Route tested.
				ourReplacementChild = ourChild;
				merger.MergeInner(ref ourReplacementChild, theirChild, ancestorChild);
				if (!ReferenceEquals(ourChild, ourReplacementChild))
					ours.ReplaceChild(ours.OwnerDocument.ImportNode(ourReplacementChild, true), ourChild);

				return ours;
			}

			// ancestor is not null at this point.
			// Check the inner nodes
			if (ours == null && theirs == null)
			{
				// We both deleted main node.
// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor));
				return null;
			}
			if (ours == null)
			{
				// We deleted it.
// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor, theirs));
				ours = theirs;
				return ours;
			}
			if (theirs == null)
			{
				// They deleted it.
// Route tested, but the MergeChildrenMethod adds the change report for us.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor, ours));
				return ours;
			}

			// ours, theirs, and ancestor all exist here.
			new MergeChildrenMethod(ours, theirs, ancestor, merger).Run();

// Route tested. (UsingWith_NumberOfChildrenAllowed_ZeroOrOne_DoesNotThrowWhenParentHasOneChildNode)
			return ours;
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