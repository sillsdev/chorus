using System;
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
			if (merger == null)
				throw new ArgumentNullException("merger");
			if (ours == null && theirs == null && commonAncestor == null)
				throw new ArgumentNullException();

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
				if (theirs != null)
				{
					ours = theirs; // They added it.
				}
				// else // We added it.
			}
			else
			{
				if (ours == null && theirs == null)
				{
					// No problemo, since both deleted it.
				}
				else
				{
					// 2A1. Compare 'ours' with 'theirs'.
						// If one is null, keep the other one, but only if it was edited.
					if (ours == null && !XmlUtilities.AreXmlElementsEqual(theirs, commonAncestor))
					{
						ours = theirs;
						// We deleted, they edited, so keep theirs under the least loss principle.
						if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
						{
							merger.EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(ours.Name, ours, theirs, commonAncestor,
																									 merger.MergeSituation, null,
																									 MergeSituation.kAlphaUserId));
						}
						else
						{
							merger.EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(theirs.Name, theirs, ours,
																									 commonAncestor,
																									 merger.MergeSituation, null,
																									 MergeSituation.kBetaUserId));
						}
					}
					else if (theirs == null && !XmlUtilities.AreXmlElementsEqual(ours, commonAncestor))
					{
						// We edited, they deleted, so keep ours under the least loss principle.
						merger.EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(ours.Name, ours, theirs, commonAncestor,
																					   merger.MergeSituation, null,
																					   MergeSituation.kAlphaUserId));
					}
					else if (!XmlUtilities.AreXmlElementsEqual(ours, theirs))
					{
						var oursAndCommonAreEqual = XmlUtilities.AreXmlElementsEqual(ours, commonAncestor);
						var theirsAndCommonAreEqual = XmlUtilities.AreXmlElementsEqual(theirs, commonAncestor);
						// Compare with common ancestor to see who made the change, if only one made it.\
						if (!oursAndCommonAreEqual && theirsAndCommonAreEqual)
						{
							// We edited it.
						}
						else if (!theirsAndCommonAreEqual && oursAndCommonAreEqual)
						{
							// They edited it.
							ours = theirs;
						}
						else
						{
							// Both edited.
							// 2A1b. If different, then report a conflict (what kind of conflict?) and then stop.
// ReSharper disable PossibleNullReferenceException
							merger.EventListener.ConflictOccurred(merger.MergeSituation.ConflictHandlingMode ==
														   MergeOrder.ConflictHandlingModeChoices.WeWin
															? new BothEditedTheSameElement(ours.Name, ours, theirs, commonAncestor,
																						   merger.MergeSituation, null,
																						   MergeSituation.kAlphaUserId)
															: new BothEditedTheSameElement(theirs.Name, theirs, ours, commonAncestor,
																						   merger.MergeSituation, null,
																						   MergeSituation.kBetaUserId));
// ReSharper restore PossibleNullReferenceException
						}
					}
						// else
						// No changes, or both made the same change(s).
				}
			}

			return true;
		}
	}
}