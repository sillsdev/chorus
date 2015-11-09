using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Merge the attributes of an element.
	/// </summary>
	internal static class MergeXmlAttributesService
	{
		internal static void MergeAttributes(XmlMerger merger, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			// Review: What happens if 'ours' is null?
			// My (RandyR) theory is that deletions are handled in the MergeChildrenMethod code, as are additions.
			//	That being the case, then that code will only call back to the XmlMerger MergerInner code when all three nodes are present,
			//	and will thus never get here.

			var skipProcessingInOurs = new HashSet<string>();

			// Deletions from ancestor, no matter who did it.
			foreach (var ancestorAttr in XmlUtilities.GetAttrs(ancestor))
			{
				var ourAttr = XmlUtilities.GetAttributeOrNull(ours, ancestorAttr.Name);
				var theirAttr = XmlUtilities.GetAttributeOrNull(theirs, ancestorAttr.Name);
				if (theirAttr == null)
				{
					if (ourAttr == null)
					{
						// Both deleted.
						// Route tested (x1).
						merger.EventListener.ChangeOccurred(new XmlAttributeBothDeletedReport(merger.MergeSituation.PathToFileInRepository, ancestorAttr));
						ancestor.Attributes.Remove(ancestorAttr);
						continue;
					}
					if (ourAttr.Value != ancestorAttr.Value)
					{
						// They deleted, but we changed, so we win under the principle of
						// least data loss (an attribute can be a huge text element).
						// Route tested (x1).
						merger.ConflictOccurred(new EditedVsRemovedAttributeConflict(ourAttr.Name, ourAttr.Value, null, ancestorAttr.Value, merger.MergeSituation, merger.MergeSituation.AlphaUserId));
						continue;
					}
					// They deleted. We did zip.
					// Route tested (x1).
					if(theirs != null) //if there is no theirs node, then attributes weren't actually removed
					{
						//Route tested (x1)
						merger.EventListener.ChangeOccurred(new XmlAttributeDeletedReport(merger.MergeSituation.PathToFileInRepository, ancestorAttr));
						ancestor.Attributes.Remove(ancestorAttr);
						ours.Attributes.Remove(ourAttr);
					}
					continue;
				}
				if (ourAttr != null)
					continue; // Route used.

				// ourAttr == null
				if (ancestorAttr.Value != theirAttr.Value)
				{
					// We deleted it, but at the same time, they changed it. So just add theirs in, under the principle of
					// least data loss (an attribute can be a huge text element)
					// Route tested (x1).
					skipProcessingInOurs.Add(theirAttr.Name); // Make sure we don't process it again in 'ours' loop, below.
					var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr.CloneNode(true), true);
					ours.Attributes.Append(importedAttribute);
					merger.ConflictOccurred(new RemovedVsEditedAttributeConflict(theirAttr.Name, null, theirAttr.Value, ancestorAttr.Value, merger.MergeSituation, merger.MergeSituation.BetaUserId));
					continue;
				}
				// We deleted it. They did nothing.
				// Route tested (x1).
				merger.EventListener.ChangeOccurred(new XmlAttributeDeletedReport(merger.MergeSituation.PathToFileInRepository, ancestorAttr));
				ancestor.Attributes.Remove(ancestorAttr);
				theirs.Attributes.Remove(theirAttr);
			}

			var extantNode = ours ?? theirs ?? ancestor;
			foreach (var theirAttr in XmlUtilities.GetAttrs(theirs))
			{
				// Will never return null, since it will use the default one, if it can't find a better one.
				var mergeStrategy = merger.MergeStrategies.GetElementStrategy(extantNode);
				var ourAttr = XmlUtilities.GetAttributeOrNull(ours, theirAttr.Name);
				var ancestorAttr = XmlUtilities.GetAttributeOrNull(ancestor, theirAttr.Name);

				if (ourAttr == null)
				{
					if (ancestorAttr == null)
					{
						// Route tested (x1).
						skipProcessingInOurs.Add(theirAttr.Name); // Make sure we don't process it again in 'ours loop, below.
						var importedAttribute = (XmlAttribute)ours.OwnerDocument.ImportNode(theirAttr.CloneNode(true), true);
						ours.Attributes.Append(importedAttribute);
						merger.EventListener.ChangeOccurred(new XmlAttributeAddedReport(merger.MergeSituation.PathToFileInRepository, theirAttr));
					}
					// NB: Deletes are all handled above in first loop.
					continue;
				}

				if (ancestorAttr == null) // Both introduced this attribute
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						// Route tested (x1).
						merger.EventListener.ChangeOccurred(new XmlAttributeBothAddedReport(merger.MergeSituation.PathToFileInRepository, ourAttr));
						continue;
					}

					// Both added, but not the same.
					if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
						{
							// Route tested (x1).
							merger.ConflictOccurred(new BothAddedAttributeConflict(theirAttr.Name, ourAttr.Value, theirAttr.Value, merger.MergeSituation,
								merger.MergeSituation.AlphaUserId));
						}
						else
						{
							// Route tested (x1).
							ourAttr.Value = theirAttr.Value;
							merger.ConflictOccurred(new BothAddedAttributeConflict(theirAttr.Name, ourAttr.Value, theirAttr.Value, merger.MergeSituation,
								merger.MergeSituation.BetaUserId));
						}
					}
					continue; // Route used.
				}

				if (ancestorAttr.Value == ourAttr.Value)
				{
					if (ourAttr.Value == theirAttr.Value)
					{
						// Route used.
						continue; // Nothing to do.
					}

					// They changed.
					if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						// Route tested (x1).
						skipProcessingInOurs.Add(theirAttr.Name);
						merger.EventListener.ChangeOccurred(new XmlAttributeChangedReport(merger.MergeSituation.PathToFileInRepository, theirAttr));
						ourAttr.Value = theirAttr.Value;
					}
					continue;
				}

				if (ourAttr.Value == theirAttr.Value)
				{
					// Both changed to same value
					if (skipProcessingInOurs.Contains(theirAttr.Name))
						continue; // Route used.
					// Route tested (x1).
					merger.EventListener.ChangeOccurred(new XmlAttributeBothMadeSameChangeReport(merger.MergeSituation.PathToFileInRepository, ourAttr));
					continue;
				}
				if (ancestorAttr.Value == theirAttr.Value)
				{
					// We changed the value. They did nothing.
					if (!mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					{
						// Route tested (x1).
						merger.EventListener.ChangeOccurred(new XmlAttributeChangedReport(merger.MergeSituation.PathToFileInRepository, ourAttr));
					}
					continue;
				}

				if (mergeStrategy.AttributesToIgnoreForMerging.Contains(ourAttr.Name))
					continue; // Route used (FileLevelMergeTests)

				//for unit test see Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
				if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					// Route tested (x1).
					merger.ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value,
																			theirAttr.Value,
																			ancestorAttr.Value,
																			merger.MergeSituation,
																			merger.MergeSituation.AlphaUserId));
				}
				else
				{
					// Route tested (x1).
					ourAttr.Value = theirAttr.Value;
					merger.ConflictOccurred(new BothEditedAttributeConflict(theirAttr.Name, ourAttr.Value,
																			theirAttr.Value,
																			ancestorAttr.Value,
																			merger.MergeSituation,
																			merger.MergeSituation.BetaUserId));
				}
			}

			foreach (var ourAttr in XmlUtilities.GetAttrs(ours))
			{
				if (skipProcessingInOurs.Contains(ourAttr.Name))
					continue;

				var theirAttr = XmlUtilities.GetAttributeOrNull(theirs, ourAttr.Name);
				var ancestorAttr = XmlUtilities.GetAttributeOrNull(ancestor, ourAttr.Name);
				if (ancestorAttr != null || theirAttr != null)
					continue;
				merger.EventListener.ChangeOccurred(new XmlAttributeAddedReport(merger.MergeSituation.PathToFileInRepository, ourAttr));
			}
		}
	}
}