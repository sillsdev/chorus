using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Merge a ptotentally atomic element,
	/// where 'atomic' means no real merging takes place within an element.
	/// </summary>
	internal static class MergeAtomicElementService
	{
		/// <summary>
		/// "Merge" elemement if it is 'atomic' and return true. Otherwise, do nothing and return false.
		/// </summary>
		/// <remarks>
		/// <param name="ours" /> may be changed to <param name="theirs"/>,
		/// if <param name="ours"/> is null and <param name="theirs"/> is not null.
		/// </remarks>
		/// <returns>'True' if the given elements were 'atomic'. Otherewise 'false'.</returns>
		internal static bool Run(XmlMerger merger, ref XmlNode ours, XmlNode theirs, XmlNode commonAncestor)
		{
			// One or two of the elements may be null.
			// If commonAncestor is null and one of the othere is null, then the other one added a new element.
			// if ours and theirs are both null, they each deleted the element.
			var nodeForStrategy = ours ?? (theirs ?? commonAncestor);
			// Here is where we sort out the new 'IsAtomic' business of ElementStrategy.
			// 1. Fetch the relevant ElementStrategy
			var elementStrategy = merger.MergeStrategies.GetElementStrategy(nodeForStrategy);
			if (!elementStrategy.IsAtomic)
				return false;

			if (commonAncestor == null)
			{
				if (ours ==  null)
				{
					ours = theirs; // They added it.
				}
				else if (theirs == null)
				{
					// We win.
				}
				else
				{
					// Can't get here from anywhere.
				}
			}
			else
			{
				// 'ours' & 'theirs' may both be null, if both deleted.
				if (ours == null && theirs == null)
				{
					// No problemo.
				}
				else
				{
					// 2A1. Compare 'ours' with 'theirs'.
					if (!XmlUtilities.AreXmlElementsEqual(ours, theirs))
					{
						// 2A1b. If different, then report a conflict (what kind of conflict?) and then stop.
						merger.EventListener.ConflictOccurred(merger.MergeSituation.ConflictHandlingMode ==
													   MergeOrder.ConflictHandlingModeChoices.WeWin
														? new BothEditedTheSameElement(ours.Name, ours, theirs, commonAncestor,
																					   merger.MergeSituation, null,
																					   MergeSituation.kAlphaUserId)
														: new BothEditedTheSameElement(theirs.Name, theirs, ours, commonAncestor,
																					   merger.MergeSituation, null,
																					   MergeSituation.kBetaUserId));
					}
				}
			}

			return true;
		}
	}
}