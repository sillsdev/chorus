using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.xml;

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
		private readonly IElementDescriber _elementDescriber;
		private readonly HashSet<XmlNode> _skipInnerMergeFor;

		/// <summary>
		/// This is for regular three-way merges.
		/// </summary>
		public MergeTextNodesMethod(XmlMerger merger, IElementDescriber elementDescriber,
			HashSet<XmlNode> skipInnerMergeFor,
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
			_elementDescriber = elementDescriber;
			_skipInnerMergeFor = skipInnerMergeFor;
		}

		/// <summary>
		/// Merges the children into the "ours" xmlnode, and uses the merger's listener to publish what happened.
		/// </summary>
		public void Run()
		{
			var extantNode = _ours ?? _theirs ?? _ancestor;
			if (extantNode.NodeType != XmlNodeType.Element)
				return;

			// Deletions from ancestor have already been done, so _ours and _theirs ought never be null.
			// _ancestor could be null, however, for adds.
			var ourText = _ours.InnerText.Trim();
			var theirText = _theirs.InnerText.Trim();
			var ancestorText = _ancestor == null ? null : _ancestor.InnerText.Trim();

			if (ourText == theirText && ourText == ancestorText)
				return; // No changes by anyone. // Not used.

			if (ancestorText == null)
			{
				// We both added something to ancestor.
				//if (ourText == theirText)
				//{
				//    // We both added the same thing. It could be content or empty strings.
				//    // This case seems to be handled by MergeChildrenMethod.
				//    _merger.EventListener.ChangeOccurred(new XmlTextBothAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
				//    return;
				//}
				// At this point, ancestor is null, and ourText is *not* the same as theirText.
				if (ourText == string.Empty)
				{
					// We added empty node. They added content, so go with simple add report.
					// Route tested.
					_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
					_ours.InnerText = _theirs.InnerText;
					return;
				}
				if (theirText == string.Empty)
				{
					// They added empty node. We added content, so go with simple add report.
					// Route tested.
					_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					return;
				}
				// Add conflict and set winner to merge situation declaration.
				if (_merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					// Route tested.
					_merger.ConflictOccurred(new XmlTextBothAddedTextConflict(_ours.Name,
						_ours, _theirs,
						_merger.MergeSituation, _elementDescriber,
						_merger.MergeSituation.AlphaUserId));
					return;
				}

				// Route tested.
				_merger.ConflictOccurred(new XmlTextBothAddedTextConflict(_ours.Name,
					_ours, _theirs,
					_merger.MergeSituation, _elementDescriber,
					_merger.MergeSituation.BetaUserId));
				_ours.InnerText = _theirs.InnerText;
				return;
			}

			if (ancestorText == string.Empty)
			{
				if (ourText == string.Empty)
				{
					// Already checked, above.
					//if (theirText == string.Empty)
					//	return;  // No changes by anyone.
					if (theirText.Length > 0)
					{
						// They added text to otherwise empty node. We did nothing.
						// Route tested.
						_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
						_ours.InnerText = _theirs.InnerText;
						//return;
					}
					//// They added text content into node. We did nothing.
					//// This case seems to be handled by MergeChildrenMethod.
					//_ours.InnerText = _theirs.InnerText;
					//_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
					return;
				}
				if (theirText == string.Empty)
				{
					// Already checked, above.
					//if (ourText == string.Empty)
					//	return;  // No changes by anyone.
					if (ourText.Length > 0)
					{
						// We edited. They did nothing. Keep ours.
						// Route tested.
						_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					}
					return;
				}
				if (ourText == theirText)
				{
					// Both added same text to empty node.
					// Route tested.
					_merger.EventListener.ChangeOccurred(new XmlTextBothAddedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
					return;
				}
			}

			if (ancestorText.Length <= 0)
				return;

			// ourText is not null nor an empty string.
			if (ourText == ancestorText && ourText != theirText)
			{
				_ours.InnerText = _theirs.InnerText;
				// They changed it, nobody else did anything.
				// Route tested.
				_merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _theirs));
				return;
			}
			if (theirText == ancestorText && ourText != ancestorText)
			{
				// We changed. They did nothing.
				// Route tested.
				_merger.EventListener.ChangeOccurred(new XmlTextChangedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
				return;
			}
			if (theirText != ourText && ourText != ancestorText)
			{
				if (theirText == string.Empty)
				{
					// They deleted the text string, but we edited it, so we win.
					// Route tested.
					_merger.ConflictOccurred(new XmlTextEditVsRemovedConflict(_ancestor.Name,
																			  _ours, _theirs, _ancestor,
																			  _merger.MergeSituation, _elementDescriber,
																			  _merger.MergeSituation.AlphaUserId));
					return;
				}
				if (ourText == string.Empty)
				{
					// We deleted the text string, but they edited it, so they win.
					// Route tested.
					_ours.InnerText = _theirs.InnerText;
					_merger.ConflictOccurred(new XmlTextRemovedVsEditConflict(_ancestor.Name,
																			  _ours, _theirs, _ancestor,
																			  _merger.MergeSituation, _elementDescriber,
																			  _merger.MergeSituation.BetaUserId));
					return;
				}
				// Both edited, but not the same edit.
				if (_merger.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					// Route tested.
					_merger.ConflictOccurred(new XmlTextBothEditedTextConflict(_ours.Name,
																			   _ours, _theirs, _ancestor,
																			   _merger.MergeSituation, _elementDescriber,
																			   _merger.MergeSituation.AlphaUserId));
					return;
				}
				// They win
				// Route tested.
				_ours.InnerText = _theirs.InnerText;
				_merger.ConflictOccurred(new XmlTextBothEditedTextConflict(_ours.Name,
																		   _ours, _theirs, _ancestor,
																		   _merger.MergeSituation, _elementDescriber,
																		   _merger.MergeSituation.BetaUserId));
				//return;
			}
			//if (ourText == theirText && ourText != ancestorText)
			//{
			//    // Both did the same edit.
			//    // This case seems to be handled by MergeChildrenMethod.
			//    _merger.EventListener.ChangeOccurred(new XmlTextBothMadeSameChangeReport(_merger.MergeSituation.PathToFileInRepository, _ours));
			//}
		}

		/// <summary>
		/// Handles the first (deletions) of a two step series of merges, and uses the merger's listener to publish what happened.
		/// </summary>
		/// <remarks>
		/// NB: from MergeChildrenMethod DoDeletions method, which can call here.
		///
		/// Remove from ancestorKeepers any node that does not correspond to anything (both deleted)
		/// Remove from ancestorKeepers and theirKeepers any pair that correspond to each other but not to anything in ours. Report conflict (delete/edit) if pair not identical.
		/// Remove from ancestorKeepers and ourKeepers any pair that correspond to each other and are identical, but don't correspond to anything in theirs (they deleted)
		/// Report conflict (edit/delete) on any pair that correspond in ours and ancestor, but nothing in theirs, and that are NOT identical. (but keep them...we win)
		/// </remarks>
		internal void DoDeletions()
		{
			var extantNode = _ours ?? _theirs ?? _ancestor;
			if (extantNode.NodeType != XmlNodeType.Element)
				return;

			var ourText = _ours == null ? null : _ours.InnerText.Trim();
			var theirText = _theirs == null ? null : _theirs.InnerText.Trim();
			var ancestorText = _ancestor == null ? null : _ancestor.InnerText.Trim();

			if (!string.IsNullOrEmpty(ancestorText))
			{
				if (ourText == string.Empty)
				{
					if (theirText == string.Empty)
					{
						// Both deleted the text and left the node.
						// Route tested.
						_merger.EventListener.ChangeOccurred(new XmlTextBothDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
						if (_ours != null)
							_skipInnerMergeFor.Add(_ours); // Route used.
						if (_theirs != null)
							_skipInnerMergeFor.Add(_theirs); // Route used.
						if (_ancestor != null)
							_skipInnerMergeFor.Add(_ancestor); // Route used.
						return;
					}
					if (theirText == ancestorText)
					{
						// We deleted the text and left the node.
						// Route tested.
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
						if (_ours != null)
							_skipInnerMergeFor.Add(_ours); // Route used.
						if (_theirs != null)
							_skipInnerMergeFor.Add(_theirs); // Route used.
						if (_ancestor != null)
							_skipInnerMergeFor.Add(_ancestor); // Route used.
						return;
					}
				}
				// ourText is not null nor an empty string.
				if (ourText == ancestorText && ourText != theirText)
				{
					if (theirText == string.Empty)
					{
						// They deleted the text, but left the node.
						if (_ours != null)
							_skipInnerMergeFor.Add(_ours); // Route used.
						if (_theirs != null)
							_skipInnerMergeFor.Add(_theirs); // Route used.
						if (_ancestor != null)
							_skipInnerMergeFor.Add(_ancestor); // Route used.
						// Route tested.
						_ours.InnerText = _theirs.InnerText;
						_theirs.InnerText = "MadeUp"; // This avoids a second report off in MergeChildrenMethod, but feels like a big kludge.
						_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ours));
						return;
					}
				}
			}
			if (_ours == null && _theirs == null)
			{
				// 1A. We both deleted entire node.
				// Route tested.
				_merger.EventListener.ChangeOccurred(new XmlTextBothDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
				_ancestorKeepers.Remove(_ancestor);
				return;
			}
			//if (ourText == string.Empty && theirText == string.Empty && !string.IsNullOrEmpty(ancestorText))
			//{
			//    // 1. We both deleted something.
			//    // 1B. We both deleted the text, but left the node.
			//    // Test case is covered, above.
			//    _merger.EventListener.ChangeOccurred(new XmlTextBothDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
			//    // NB: Don't remove anything from the keepers, since the node remains, albeit empty.
			//    return;
			//}

			if (_ours == null)
			{
				// NOTE: _theirs can't be null here, or it would have been handled in #1, above.
				// 2. We deleted whole node.
				if (theirText == ancestorText)
				{
					// Report deletion of node.
					// Route tested
					_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
					_ancestorKeepers.Remove(_ancestor);
					_theirKeepers.Remove(_theirs);
					return;
				}
				if (theirText == string.Empty && ancestorText != string.Empty)
				{
					// 2A. Well, they almost deleted everything, so just go with a simple we deleted node, rather than a conflict.
					// Route tested.
					_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
					_ancestorKeepers.Remove(_ancestor);
					//_ours = _theirs; // Let our node deletion stand.
					//_ourKeepers.Add(_theirs); // Review JohnT (RandyR): Is this right or not?
					//if (_ours != null)
					//	_skipInnerMergeFor.Add(_ours);
					if (_theirs != null)
						_skipInnerMergeFor.Add(_theirs);
					if (_ancestor != null)
						_skipInnerMergeFor.Add(_ancestor);
					return;
				}
				if (theirText != ancestorText)
				{
					// 2B. But they changed, so keep theirs and register XmlTextRemovedVsEditConflictReport.
					// Route tested.
					_merger.ConflictOccurred(new XmlTextRemovedVsEditConflict(_ancestor.Name,
						_ours, _theirs, _ancestor,
						_merger.MergeSituation, _elementDescriber,
						_merger.MergeSituation.BetaUserId));
					_ours = _theirs;
					//_ourKeepers.Add(_theirs); // Review JohnT (RandyR): Is this right or not?
					if (_ours != null)
						_skipInnerMergeFor.Add(_ours);
					if (_theirs != null)
						_skipInnerMergeFor.Add(_theirs);
					if (_ancestor != null)
						_skipInnerMergeFor.Add(_ancestor);
				}
				return;
			}

			//if (ourText == string.Empty && theirText == ancestorText && theirText != string.Empty)
			//{
			//    // We deleted text, but left node, They did nothing.
			//    // Route tested, above.
			//    _merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
			//    if (_ours != null)
			//        _skipInnerMergeFor.Add(_ours);
			//    if (_theirs != null)
			//        _skipInnerMergeFor.Add(_theirs);
			//    if (_ancestor != null)
			//        _skipInnerMergeFor.Add(_ancestor);
			//    return;
			//}
			if (_theirs == null)
			{
				// NOTE: _ours can't be null here, or it would have been handled in #1, above.
				// 3. They deleted.
				if (ourText == string.Empty && ancestorText != string.Empty)
				{
					// 3A. Well, We almost deleted everything, so just go with a simple they deleted, rather than a conflict.
					// Route tested.
					_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
					_ancestorKeepers.Remove(_ancestor);
					_ourKeepers.Remove(_ours);
					_ours = null;
					return;
				}
				if (ourText != ancestorText)
				{
					// 3B. But we changed, so keep ours and register XmlTextEditVsRemovedConflict.
					// Route tested
					_merger.ConflictOccurred(new XmlTextEditVsRemovedConflict(_ancestor.Name,
						_ours, _theirs, _ancestor,
						_merger.MergeSituation, _elementDescriber,
						_merger.MergeSituation.AlphaUserId));
					return;
				}
				if (ourText == ancestorText)
				{
					// They deleted entire node, We did nothing.
					// Route tested
					_merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
					_ancestorKeepers.Remove(_ancestor);
					_ourKeepers.Remove(_ours);
					_ours = null;
				}
				//return; // If it can get here, I need another 'if' or two.
			}
			//if (theirText == string.Empty && ourText == ancestorText && ourText != string.Empty)
			//{
			//    // They deleted text, but left node, We did nothing.
			//    // Route tested, above.
			//    _merger.EventListener.ChangeOccurred(new XmlTextDeletedReport(_merger.MergeSituation.PathToFileInRepository, _ancestor));
			//    if (_ours != null)
			//        _skipInnerMergeFor.Add(_ours);
			//    if (_theirs != null)
			//        _skipInnerMergeFor.Add(_theirs);
			//    if (_ancestor != null)
			//        _skipInnerMergeFor.Add(_ancestor);
			//}
		}
	}
}