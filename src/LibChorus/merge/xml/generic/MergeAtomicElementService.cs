using System;
using System.Diagnostics;
using System.Xml;
using Chorus.FileTypeHandlers.xml;

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
		/// <returns>'True' if the given elements were 'atomic'. Otherwise 'false'.</returns>
		internal static void Run(XmlMerger merger, ref XmlNode ours, XmlNode theirs, XmlNode commonAncestor)
		{
			if (merger == null)
				throw new ArgumentNullException("merger"); // Route tested.
			if (ours == null && theirs == null && commonAncestor == null)
				throw new ArgumentNullException(); // Route tested.

			// One or two of the elements may be null.
			// If commonAncestor is null and one of the others is null, then the other one added a new element.
			// if ours and theirs are both null, they each deleted the element.
			var nodeForStrategy = ours ?? (theirs ?? commonAncestor);
			// Here is where we sort out the new 'IsAtomic' business of ElementStrategy.
			// 1. Fetch the relevant ElementStrategy
			var elementStrategy = merger.MergeStrategies.GetElementStrategy(nodeForStrategy);
			if (!elementStrategy.IsAtomic)
				throw new InvalidOperationException("This method class only handles elements that are atomic (basically binary type data that can't really be merged.)");

			if (commonAncestor == null)
			{
				if (ours == null)
				{
					// They can't all be null, or there would have been an exception thrown, above.
					//if (theirs == null)
					//{
					//    // Nobody did anything.
					//    return true;
					//}
					// They seem to have added a new one.
					// Route tested (x2).
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, theirs));
					ours = theirs; // They added it.
				}
				else
				{
					// Ours is not null.
					if (theirs != null)
					{
						// Neither is theirs.
						if (XmlUtilities.AreXmlElementsEqual(ours, theirs))
						{
							// Both added the same thing.
							// Route tested (x2).
							merger.EventListener.ChangeOccurred(new BothChangedAtomicElementReport(merger.MergeSituation.PathToFileInRepository, ours));
						}
						else
						{
							// Both added, but not the same thing.
							if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
							{
								// Route tested.
								merger.ConflictOccurred(new BothEditedTheSameAtomicElement(ours.Name,
									ours, theirs, null, merger.MergeSituation, elementStrategy, merger.MergeSituation.AlphaUserId));
							}
							else
							{
								// Route tested.
								merger.ConflictOccurred(new BothEditedTheSameAtomicElement(ours.Name,
									ours, theirs, null, merger.MergeSituation, elementStrategy, merger.MergeSituation.BetaUserId));
								ours = theirs;
							}
						}
					}
					else
					{
						// We added. They are still null.
						// Route tested (x2).
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
					}
				}
				return; // Routed used (x2).
			}

			// commonAncestor != null from here on out.
			if (ours == null && theirs == null)
			{
				// No problemo, since both deleted it.
				// Route tested (x2).
				merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, commonAncestor));
				return;
			}

			// 2A1. Compare 'ours' with 'theirs'.
			// If one is null, keep the other one, but only if it was edited.
			var theirsAndCommonAreEqual = theirs != null && XmlUtilities.AreXmlElementsEqual(theirs, commonAncestor);
			if (ours == null && !theirsAndCommonAreEqual)
			{
				// We deleted, they edited, so keep theirs under the least loss principle.
				// Route tested (x2 WeWin & !WeWin).
				merger.ConflictOccurred(new RemovedVsEditedElementConflict(theirs.Name, null, theirs,
																						 commonAncestor,
																						 merger.MergeSituation, elementStrategy,
																						 merger.MergeSituation.BetaUserId));
				ours = theirs;
				return;
			}

			var oursAndCommonAreEqual = ours != null && XmlUtilities.AreXmlElementsEqual(ours, commonAncestor);
			if (theirs == null && !oursAndCommonAreEqual)
			{
				// We edited, they deleted, so keep ours under the least loss principle.
				Debug.Assert(ours != null, "We shoudn't come here if ours is also null...both deleted is handled elsewhere");
				// Route tested (x2 WeWin & !WeWin)
				merger.ConflictOccurred(new EditedVsRemovedElementConflict(ours.Name, ours, null, commonAncestor,
																			   merger.MergeSituation, elementStrategy,
																			   merger.MergeSituation.AlphaUserId));
				return;
			}

			var oursAndTheirsAreEqual = ours != null && theirs != null && XmlUtilities.AreXmlElementsEqual(ours, theirs);
			if (oursAndTheirsAreEqual && !oursAndCommonAreEqual)
			{
				// Both made same changes.
				// Route tested (x2).
				merger.EventListener.ChangeOccurred(new BothChangedAtomicElementReport(merger.MergeSituation.PathToFileInRepository, ours));
				return;
			}

			if (!oursAndTheirsAreEqual)
			{
				if (ours == null)
				{
					// We deleted. They did nothing.
					Debug.Assert(theirs != null, "both deleted should be handled before this");
					Debug.Assert(theirsAndCommonAreEqual, "we deleted and they edited should be handled before this");
					// leave ours null, preserving the deletion.
					// We could plausibly call ChangeOccurred with an XmlDeletionChangeReport, but we are phasing those out.
					return; // Route tested
				}
				if (theirs == null)
				{
					// They deleted. We did nothing.
					Debug.Assert(oursAndCommonAreEqual, "we edited and they deleted should be handled before this");
					// Let the delete win.
					ours = null;
					// We could plausibly call ChangeOccurred with an XmlDeletionChangeReport, but we are phasing those out.
					return;  // Route tested
				}
				// Compare with common ancestor to see who made the change, if only one made it.\
				if (!oursAndCommonAreEqual && theirsAndCommonAreEqual)
				{
					// We edited it. They did nothing.
					// Route tested (x2).
					merger.EventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, ours.ParentNode, ours));
				}
				else if (!theirsAndCommonAreEqual && oursAndCommonAreEqual)
				{
					// They edited it. We did nothing.
					// Route tested (x2).
					merger.EventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, theirs.ParentNode, theirs));
					ours = theirs;
				}
				else
				{
					// Both edited.
					// 2A1b. If different, then report a conflict and then stop.
					// Route tested (x2 WeWin & !WeWin).
					merger.ConflictOccurred(merger.MergeSituation.ConflictHandlingMode ==
												   MergeOrder.ConflictHandlingModeChoices.WeWin
													? new BothEditedTheSameAtomicElement(ours.Name, ours, theirs, commonAncestor,
																				   merger.MergeSituation, elementStrategy,
																				   merger.MergeSituation.AlphaUserId)
													: new BothEditedTheSameAtomicElement(theirs.Name, ours, theirs, commonAncestor,
																				   merger.MergeSituation, elementStrategy,
																				   merger.MergeSituation.BetaUserId));
					if (merger.MergeSituation.ConflictHandlingMode != MergeOrder.ConflictHandlingModeChoices.WeWin)
						ours = theirs;
				}
			}

			// No changes.
			// Route tested (x2).
		}
	}
}