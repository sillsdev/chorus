using System.Xml;
using Chorus.FileTypeHandlers.xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Class that processes merging of immutable elements.
	///
	/// This may sound like an oxymoron, since such elements are immutable, but they can be added and deleted,
	/// or in a pathological case changed (via buggy code or human hand edits),
	/// so this class will deal with those cases.
	/// </summary>
	internal static class ImmutableElementMergeService
	{
		internal static void DoMerge(XmlMerger merger, ref XmlNode ours, XmlNode theirs, XmlNode ancestor)
		{
			if (ours == null && theirs == null && ancestor == null)
				return; // I don't think this is possible, but....

			if (ancestor == null)
			{
				// Somebody added it.
				if (ours == null && theirs != null)
				{
						// They added it.
						merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, theirs));
						ours = theirs;
						return;
				}
				if (theirs == null && ours != null)
				{
					// We added it.
					merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
					return;
				}

				// Both added.
				if (XmlUtilities.AreXmlElementsEqual(ours, theirs))
				{
					// Both added the same thing.
					merger.EventListener.ChangeOccurred(new XmlBothAddedSameChangeReport(merger.MergeSituation.PathToFileInRepository, ours));
					return;
				}

				// Both added it, but it isn't the same thing.
				if (merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					// We win.
					merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ours.Name, ours, theirs, merger.MergeSituation, merger.MergeStrategies.GetElementStrategy(ours), merger.MergeSituation.AlphaUserId));
				}
				else
				{
					// They win.
					merger.ConflictOccurred(new BothAddedMainElementButWithDifferentContentConflict(ours.Name, theirs, ours, merger.MergeSituation, merger.MergeStrategies.GetElementStrategy(theirs), merger.MergeSituation.BetaUserId));
					ours = theirs;
					return;
				}
				return;
			}

			// ancestor is not null here.
			if (ours == null)
			{
				if (theirs == null)
				{
					// Both deleted it. Add change report.
					merger.EventListener.ChangeOccurred(new XmlBothDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor));
				}
				else
				{
					// We deleted. We don't care if they made any changes, since such changes aren't legal.
					merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor.ParentNode, ancestor));
				}
				return;
			}
			if (theirs == null)
			{
				// They deleted.  We don't care if we made any changes, since such changes aren't legal.
				merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(merger.MergeSituation.PathToFileInRepository, ancestor.ParentNode, ancestor));
				ours = null;
				return;
			}

			// Nothing is null here.
			if (!XmlUtilities.AreXmlElementsEqual(ours, ancestor))
				ours = ancestor; // Restore ours to ancestor, since ours was changed by some buggy client code or by a hand edit.

			// At this point, we don't need to test if ancestor and theirs are the same, or not,
			// since we have restored current (ours) to the original, or ours was the same as ancestor.
			// There is probably no point to adding some kind of warning.
		}
	}
}
