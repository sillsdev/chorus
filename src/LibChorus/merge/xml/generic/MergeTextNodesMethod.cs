using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.Properties;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Method class that processes XmlNodes that have XmlNodeType.Text for a child.
	/// </summary>
	public class MergeTextNodesMethod
	{
		private XmlNode _ours;
		private readonly List<XmlNode> _ourKeepers;
		private readonly XmlNode _theirs;
		private readonly List<XmlNode> _theirKeepers;
		private readonly XmlNode _ancestor;
		private readonly List<XmlNode> _ancestorKeepers;
		private readonly XmlMerger _merger;

		/// <summary>
		/// This is for regular three-way merges.
		/// </summary>
		public MergeTextNodesMethod(XmlMerger merger,
			ref XmlNode ours, List<XmlNode> ourKeepers,
			XmlNode theirs, List<XmlNode> theirKeepers,
			XmlNode ancestor, List<XmlNode> ancestorKeepers)
		{
			_ours = ours;
			_ourKeepers = ourKeepers;
			_theirs = theirs;
			_theirKeepers = theirKeepers;
			_ancestor = ancestor;
			_ancestorKeepers = ancestorKeepers;
			_merger = merger;

		}

		/// <summary>
		/// Merges the children into the "ours" xmlnode, and uses the merger's listener to publish what happened.
		/// </summary>
		/// <remarks>
		/// NB: from MergeChildrenMethod DoDeletions method, which can call here.
		///
		/// Remove from ancestorKeepers any node that does not correspond to anything (both deleted)
		/// Remove from ancestorKeepers and theirKeepers any pair that correspond to each other but not to anything in ours. Report conflict (delete/edit) if pair not identical.
		/// Remove from ancestorKeepers and ourKeepers any pair that correspond to each other and are identical, but don't correspond to anything in theirs (they deleted)
		/// Report conflict (edit/delete) on any pair that correspond in ours and ancestor, but nothing in theirs, and that are NOT identical. (but keep them...we win)
		/// </remarks>
		public void Run()
		{
			var extantNode = _ours ?? _theirs ?? _ancestor;
			if (extantNode.NodeType != XmlNodeType.Element)
				return;

			var ourText = _ours == null ? null : _ours.InnerText.Trim();
			var theirText = _theirs == null ? null : _theirs.InnerText.Trim();
			var ancestorText = _ancestor == null ? null : _ancestor.InnerText.Trim();

			if (ourText == theirText && ourText == ancestorText)
				return; // No changes by anyone.

			if ((ancestorText != string.Empty && ourText == string.Empty && theirText == string.Empty)
				|| (ancestorText != null && ourText == null && theirText == null))
			{
				// It appears we and they both deleted ancestor.
				_merger.EventListener.ChangeOccurred(new XmlTextBothDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
				_ancestorKeepers.Remove(_ancestor);
				//_ourKeepers.Remove(_ours);
				//_theirKeepers.Remove(_theirs);
				return;
			}

			if (ancestorText == null)
			{
				// We both added something to ancestor.
				if (ourText == theirText)
				{
					// We both added the same thing. It could be content or empty strings.
					_merger.EventListener.ChangeOccurred(new XmlTextBothAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					return;
				}
				// At this point, ancestor is null, and ourText is *not* the same as theirText, and ourText and theirText are not null.
				// TODO: Add conflict and set winner to merge sit. Then, be sure our.inner is reset, if they won.
				return;
			}

			if (ancestorText == string.Empty)
			{
				if (ourText == string.Empty)
				{
					if (theirText == string.Empty)
						return;  // No changes by anyone.
					if (theirText == null)
					{
						// They deleted whole element. We did nothing
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
						_ancestorKeepers.Remove(_ancestor);
						_ourKeepers.Remove(_ours);
						_ours = null;
						return;
					}
					if (theirText.Length > 0)
					{
						// They added text to otherwise empty node. We did nothing.
						_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
						_ours.InnerText = _theirs.InnerText;
						return;
					}
					// They added text content into node. We did nothing.
					_ours.InnerText = _theirs.InnerText;
					_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
					return;
				}
				if (ourText == null)
				{
					if (theirText.Length > 0)
					{
						// We deleted. They edited. Keep theirs under least loss principle.
						// TODO: Add XmlTextRemovedVsEditConflict.
						_ours = _theirs;
						return;
					}
				}
				if (theirText == string.Empty)
				{
					if (ourText == string.Empty)
						return;  // No changes by anyone.
					if (ourText == null)
					{
						// We deleted. They did nothing.
						_ancestorKeepers.Remove(_ancestor);
						_theirKeepers.Remove(_theirs);
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository,  _ours));
						return;
					}
					if (ourText.Length > 0)
					{
						// We edited. They did nothing. Keep ours.
						_merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
						return;
					}
					return;
				}
				if (theirText == null)
				{
					if (ourText.Length > 0)
					{
						// They deleted. We edited. Keep ours under least loss principle.
						// TODO: Add XmlTextEditVsRemovedConflict.
						return;
					}
					return;
				}
			}

			if (ancestorText.Length > 0)
			{
				if (ourText == string.Empty)
				{
					// We deleted the text and left the node.
					if (theirText == null)
					{
						// We deleted the text and left the node. But, they deleted the text and the node.
						// TODO: Add new type of conflict report for this. Left the node fom ours.
						return;
					}
					if (theirText == string.Empty)
					{
						// Both deleted the text and left the node.
						_merger.EventListener.ChangeOccurred(new XmlTextBothDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
						return;
					}
					if (theirText == ancestorText)
					{
						// We deleted the text and left the node.
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
						return;
					}
				}
				if (ourText == null)
				{
					return;
				}
				// ourText is not null nor an empty string.
				if (ourText == ancestorText && ourText != theirText)
				{
					_ours.InnerText = _theirs.InnerText;
					if (theirText == string.Empty)
					{
						// They deleted the text, but left the node.
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					}
					else
					{
						// They changed it, nobody else did anything.
						_merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
					}
					return;
				}
				if (theirText == ancestorText && ourText != ancestorText)
				{
					// We changed. They did nothing.
					_merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					return;
				}
				return;
			}
			/******** Not sure how one can get to this point. *********/

			//if (string.IsNullOrEmpty(ourText))
			//{
			//    if (_ancestor == null || string.IsNullOrEmpty(ancestorText))
			//    {
			//        _merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
			//        _ours.InnerText = _theirs.InnerText; // We had it empty, or null.
			//        return;
			//    }
			//    if (ancestorText == theirText)
			//    {
			//        // They didn't touch it. So leave our deletion standing.
			//        _merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
			//        return;
			//    }
			//    //they edited it. Keep theirs under the principle of least data loss.
			//    _ours.InnerText = _theirs.InnerText;
			//    _merger.EventListener.ConflictOccurred(new RemovedVsEditedTextConflict(_ours, _theirs, _ancestor, _merger.MergeSituation,
			//        _merger.MergeSituation.BetaUserId));
			//    return;
			//}

			//if ((_ancestor == null) || (ourText != ancestorText))
			//{
			//    //we're not empty, we edited it, and we don't equal theirs.
			//    //  Moved  EventListener.ChangeOccurred(new TextEditChangeReport(this.MergeSituation.PathToFileInRepository, SafelyGetStringTextNode(ancestor), SafelyGetStringTextNode(ours)));
			//    if (string.IsNullOrEmpty(theirText))
			//    {
			//        if (_ancestor == null || ancestorText == string.Empty)
			//        {
			//            // We both added at least the containing element for the text.
			//            if (ourText == theirText)
			//            {
			//                // Both added the same thing, even if it is only an empty element.
			//                // TODO: Add a "both added" change report.
			//                return;
			//            }
			//            if (theirText.Length > 0 && ourText == string.Empty)
			//            {
			//                // They added some content, even.
			//                //EventListener.ChangeOccurred(new XmlTextAddedReport(MergeSituation.PathToFileInRepository, theirs));
			//                return;
			//            }
			//            if (ourText.Length > 0 && theirText == string.Empty)
			//            {
			//                // We added some content, even.
			//                _merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
			//                return;
			//            }
			//        }
			//        //we edited, they deleted it. Keep ours.
			//        //EventListener.ConflictOccurred(new EditedVsRemovedTextConflict(ours, theirs, ancestor, MergeSituation,
			//        //	MergeSituation.AlphaUserId));
			//        return;
			//    }
			//    // We know: ours is different from theirs; ours is not empty; ours is different from ancestor;
			//    // theirs is not empty.
			//    if (_ancestor != null && _theirs.InnerText == _ancestor.InnerText)
			//    {
			//        // Moved from above.
			//        _merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
			//        return; // we edited it, they did not, keep ours.
			//    }
			//    //both edited it. Keep ours, but report conflict.
			//    //EventListener.ConflictOccurred(new BothEditedTextConflict(ours, theirs, ancestor, MergeSituation, MergeSituation.AlphaUserId));
			//    return;
			//}

			//// we didn't edit it, they did
			//if (theirText != ancestorText)
			//{
			//    if (theirText == string.Empty)
			//    {
			//        _merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
			//        _ours.InnerText = theirText;
			//        return;
			//    }
			//}
			////EventListener.ChangeOccurred(new TextEditChangeReport(MergeSituation.PathToFileInRepository, SafelyGetStringTextNode(ancestor), SafelyGetStringTextNode(theirs)));
			//_ours.InnerText = _theirs.InnerText;
		}
	}
}