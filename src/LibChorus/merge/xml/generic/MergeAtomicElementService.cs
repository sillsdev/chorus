using System;
using System.Xml;
using Chorus.FileTypeHanders.xml;

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
			// If commonAncestor is null and one of the others is null, then the other one added a new element.
			// if ours and theirs are both null, they each deleted the element.
			var nodeForStrategy = ours ?? (theirs ?? commonAncestor);
			// Here is where we sort out the new 'IsAtomic' business of ElementStrategy.
			// 1. Fetch the relevant ElementStrategy
			var elementStrategy = merger.MergeStrategies.GetElementStrategy(nodeForStrategy);
			if (!elementStrategy.IsAtomic)
				return false;

			if (commonAncestor == null)
			{
				if (ours == null)
				{
					if (theirs == null)
					{
						// Nobody did anything.
						return true;
					}
					// They seem to have added a new one.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, theirs));
					ours = theirs; // They added it.
					return true;
				}
				// else // We added it.
				merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
				return true;
			}

			// commonAncestor != null from here on out.
			if (ours == null && theirs == null)
			{
				// No problemo, since both deleted it.
				merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, commonAncestor));
				return true;
			}

			// 2A1. Compare 'ours' with 'theirs'.
			// If one is null, keep the other one, but only if it was edited.
			var theirsAndCommonAreEqual = theirs != null && XmlUtilities.AreXmlElementsEqual(theirs, commonAncestor);
			if (ours == null && !theirsAndCommonAreEqual)
			{
				// We deleted, they edited, so keep theirs under the least loss principle.
				merger.EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(theirs.Name, null, theirs,
																						 commonAncestor,
																						 merger.MergeSituation, elementStrategy,
																						 merger.MergeSituation.BetaUserId));
				ours = theirs;
				return true;
			}

			var oursAndCommonAreEqual = XmlUtilities.AreXmlElementsEqual(ours, commonAncestor);
			if (theirs == null && !oursAndCommonAreEqual)
			{
				// We edited, they deleted, so keep ours under the least loss principle.
				merger.EventListener.ConflictOccurred(new EditedVsRemovedElementConflict(ours.Name, ours, null, commonAncestor,
																			   merger.MergeSituation, elementStrategy,
																			   merger.MergeSituation.AlphaUserId));
				return true;
			}

			var oursAndTheirsAreEqual = XmlUtilities.AreXmlElementsEqual(ours, theirs);
			if (oursAndTheirsAreEqual && !oursAndCommonAreEqual)
			{
				// Both made same changes.
				merger.EventListener.ChangeOccurred(new BothChangedAtomicElementReport(merger.MergeSituation.PathToFileInRepository, ours));
				return true;
			}

			if (!oursAndTheirsAreEqual)
			{
				// Compare with common ancestor to see who made the change, if only one made it.\
				if (!oursAndCommonAreEqual && theirsAndCommonAreEqual)
				{
					// We edited it. They did nothing.
					merger.EventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, ours.ParentNode, ours));
				}
				else if (!theirsAndCommonAreEqual && oursAndCommonAreEqual)
				{
					// They edited it. We did nothing.
					merger.EventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, theirs.ParentNode, theirs));
					ours = theirs;
				}
				else
				{
					// Both edited.
					// 2A1b. If different, then report a conflict (what kind of conflict?) and then stop.
					merger.EventListener.ConflictOccurred(merger.MergeSituation.ConflictHandlingMode ==
												   MergeOrder.ConflictHandlingModeChoices.WeWin
													? new BothEditedTheSameElement(ours.Name, ours, theirs, commonAncestor,
																				   merger.MergeSituation, elementStrategy,
																				   merger.MergeSituation.AlphaUserId)
													: new BothEditedTheSameElement(theirs.Name, ours, theirs, commonAncestor,
																				   merger.MergeSituation, elementStrategy,
																				   merger.MergeSituation.BetaUserId));
					if (merger.MergeSituation.ConflictHandlingMode != MergeOrder.ConflictHandlingModeChoices.WeWin)
						ours = theirs;
				}
				return true;
			}

			// No changes.
			return true;
		}
	}
}