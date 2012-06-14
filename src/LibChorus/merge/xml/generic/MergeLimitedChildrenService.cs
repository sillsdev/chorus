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
			Guard.AgainstNull(merger, "merger"); // Route tested.
			Guard.AgainstNull(strategy, "strategy"); // Route tested.
			if (ours == null && theirs == null && ancestor == null)
				throw new ArgumentNullException(); // Route tested.

			List<XmlNode> children;
			switch (strategy.NumberOfChildren)
			{
				default:
					throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrMore is not legal."); // Route tested.
				case NumberOfChildrenAllowed.Zero:
					children = GetElementChildren(ours).ToList();
					if (children.Any())
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.Zero is not legal, when there are child nodes."); // Route tested.
					// Don't merge deeper, since there aren't supposed to be any children.
					// Route tested.
					break;
				case NumberOfChildrenAllowed.ZeroOrOne:
					children = GetElementChildren(ours).ToList();
					if (children.Count > 1)
						throw new InvalidOperationException("Using strategy with NumberOfChildren property of NumberOfChildrenAllowed.ZeroOrOne is not legal, when there are multiple child nodes."); // Route tested.

					if (!children.Any())
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
			var extantChildNode = ourChild ?? theirChild ?? ancestorChild;
			var mergeStrategy = merger.MergeStrategies.GetElementStrategy(extantChildNode);
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
					// Route tested, but the MergeChildrenMethod adds the change report for us.
					// So, theory has it one can't get here from any normal place.
					// But, keep it, in case MergeChildrenMethod gets 'fixed'.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
					return ours;
				}

				// Both added the containing node.
				// Route tested.
				merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
				MergeXmlAttributesService.MergeAttributes(merger, ref ours, theirs, null);

				var match = mergeStrategy.MergePartnerFinder.GetNodeToMerge(ourChild, theirs);
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
					merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(winner.Name, winner, loser, merger.MergeSituation, mergeStrategy, winnerId));
					return ours;
				}
				else
				{
					// TODO: Work on this.
					// Matched nodes.
					// But, are they the same or not?
					// ove on down and merge them.
					//var ourReplacementChild = ourChild;
					//merger.MergeInner(ref ourReplacementChild, theirChild, ancestorChild);
					//return ourReplacementChild;
				}
			}
			// TODO: ancestor is not null at this point.

			// TODO: This won't likely survive.
			var ourReplacementChild = ourChild;
			merger.MergeInner(ref ourReplacementChild, theirChild, ancestorChild);
			return ourReplacementChild;
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