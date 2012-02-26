using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using Chorus.FileTypeHanders.xml;

namespace Chorus.merge.xml.generic
{
	class MergeChildrenMethod
	{
		private XmlNode _ours;
		private XmlNode _theirs;
		private XmlNode _ancestor;
		private XmlMerger _merger;
		private List<XmlNode> _ourKeepers = new List<XmlNode>();
		private List<XmlNode> _theirKeepers = new List<XmlNode>();
		private List<XmlNode> _ancestorKeepers = new List<XmlNode>();
		private HashSet<XmlNode> _skipInnerMergeFor = new HashSet<XmlNode>();

		/// <summary>
		/// Use this one for a diff of one xml node against another
		/// </summary>
		 public MergeChildrenMethod(XmlNode after, XmlNode before, XmlMerger merger)
			:this(after, before, before, merger)
		{
		}

		public MergeChildrenMethod(XmlNode ours, XmlNode theirs, XmlNode ancestor, XmlMerger merger)
		{
			_ours = ours;
			_theirs = theirs;
			_ancestor = ancestor;
			_merger = merger;
		}

		/// <summary>
		/// Merges the children into the "ours" xmlnode, and uses the merger's listener to publish what happened
		/// </summary>
		public void Run()
		{
			// Initialise lists of keepers to current ancestorChildren, ourChildren, theirChildren
			CopyChildrenToList(_ancestor, _ancestorKeepers);
			CopyChildrenToList(_ours, _ourKeepers);
			CopyChildrenToList(_theirs, _theirKeepers);

			// Deal with deletions.
			DoDeletions();

			if (XmlUtilities.IsTextLevel(_ours, _theirs, _ancestor))
			{
				new MergeTextNodesMethod(_merger, _merger.MergeStrategies.GetElementStrategy(_ours ?? _theirs ?? _ancestor), _skipInnerMergeFor, ref _ours, _ourKeepers, _theirs, _theirKeepers, _ancestor, _ancestorKeepers).Run();
			}

			ChildOrderer oursOrderer = new ChildOrderer(_ourKeepers, _theirKeepers,
				MakeCorrespondences(_ourKeepers, _theirKeepers, _theirs), _merger);

			ChildOrderer resultOrderer = oursOrderer; // default

			if (oursOrderer.OrderIsDifferent)
			{
				// The order of the two lists is not consistent. Compare with ancestor to see who changed.
				ChildOrderer ancestorOursOrderer = new ChildOrderer(_ancestorKeepers, _ourKeepers,
					MakeCorrespondences(_ancestorKeepers, _ourKeepers, _ours), _merger);

				ChildOrderer ancestorTheirsOrderer = new ChildOrderer(_ancestorKeepers, _theirKeepers,
					MakeCorrespondences(_ancestorKeepers, _theirKeepers, _theirs), _merger);

				if (ancestorTheirsOrderer.OrderIsDifferent)
				{
					if (ancestorOursOrderer.OrderIsDifferent)
					{
						// stick with our orderer (we win), but report conflict.
						_merger.ConflictOccurred(new BothReorderedElementConflict(_ours.Name, _ours,
							_theirs, _ancestor, _merger.MergeSituation, _merger.MergeStrategies.GetElementStrategy(_ours),
							_merger.MergeSituation.AlphaUserId));
					}
					else
					{
						// only they re-ordered; take their order as primary.
						resultOrderer = new ChildOrderer(_theirKeepers, _ourKeepers,
							MakeCorrespondences(_theirKeepers, _ourKeepers, _ours), _merger);
					}
				}
				else if(!ancestorOursOrderer.OrderIsDifferent)
				{
					// our order is different from theirs, but neither is different from the ancestor.
					// the only way this can be true is if both inserted the same thing, but in
					// different places. Stick with our orderer (we win), but report conflict.
					_merger.ConflictOccurred(new BothInsertedAtDifferentPlaceConflict(_ours.Name, _ours,
						_theirs, _ancestor, _merger.MergeSituation, _merger.MergeStrategies.GetElementStrategy(_ours),
						_merger.MergeSituation.AlphaUserId));
				}
				// otherwise we re-ordered, but they didn't. That's not a problem, unless it resulted
				// in inconsistency or ambiguity in other stuff that someone added.
			}

			if (!resultOrderer.OrderIsConsistent ||
				(resultOrderer.OrderIsDifferent && resultOrderer.OrderIsAmbiguous))
			{
				_merger.ConflictOccurred(new AmbiguousInsertReorderConflict(_ours.Name, _ours,
					_theirs, _ancestor, _merger.MergeSituation, _merger.MergeStrategies.GetElementStrategy(_ours),
					_merger.MergeSituation.AlphaUserId));
			}
			else if (resultOrderer.OrderIsAmbiguous)
			{
				_merger.ConflictOccurred(new AmbiguousInsertConflict(_ours.Name, _ours,
					_theirs, _ancestor, _merger.MergeSituation, _merger.MergeStrategies.GetElementStrategy(_ours),
					_merger.MergeSituation.AlphaUserId));
			}

			List<XmlNode> newChildren = resultOrderer.GetResultList();
			// Merge corresponding nodes.
			for (int i = 0; i < newChildren.Count; i++)
			{
				XmlNode ourChild = newChildren[i];
				XmlNode theirChild;
				var ancestorChild = FindMatchingNode(ourChild, _ancestor);
				if (resultOrderer.Correspondences.TryGetValue(ourChild, out theirChild)
					&& !ChildrenAreSame(ourChild, theirChild))
				{
					// There's a corresponding node and it isn't the same as ours...
					if (!_skipInnerMergeFor.Contains(ourChild))
					{
						_merger.MergeInner(ref ourChild, theirChild, ancestorChild);
						newChildren[i] = ourChild;
					}
				}
				else
				{
					//Review JohnT (jh): Is this the correct interpretation?
					if (ancestorChild == null)
					{
						if (XmlUtilities.IsTextNodeContainer(ourChild) == TextNodeStatus.IsTextNodeContainer) // No, it hasn't. MergeTextNodesMethod has already added the addition report.
							_merger.EventListener.ChangeOccurred(new XmlTextAddedReport(_merger.MergeSituation.PathToFileInRepository, ourChild));
						else if (!(ourChild is XmlCharacterData))
							_merger.EventListener.ChangeOccurred(new XmlAdditionChangeReport(_merger.MergeSituation.PathToFileInRepository, ourChild));
					}
				}
			}

			// Plug results back into 'ours'
			for (int i = 0; i < newChildren.Count; i++)
			{
				XmlNode ourChild = newChildren[i];
				while (_ours.ChildNodes.Count > i && ourChild != _ours.ChildNodes[i])
					_ours.RemoveChild(_ours.ChildNodes[i]);
				if (_ours.ChildNodes.Count > i)
					continue; // we found the exact node already present, leave it alone.
				if (ourChild.ParentNode == _ours)
					_ours.AppendChild(ourChild);
				else
				{
					if (ourChild is XmlElement)
						_ours.AppendChild(XmlUtilities.GetDocumentNodeFromRawXml(ourChild.OuterXml, _ours.OwnerDocument));
					else if (ourChild is XmlText)
						_ours.AppendChild(_ours.OwnerDocument.CreateTextNode(ourChild.OuterXml));
					else
						Debug.Fail("so far we only know how to copy elements and text nodes at this point");
				}
			}
			// Remove any leftovers.
			while (_ours.ChildNodes.Count > newChildren.Count)
				_ours.RemoveChild(_ours.ChildNodes[newChildren.Count]);



			//    foreach (XmlNode theirChild in _theirs.ChildNodes)
			//    {



			//        IFindNodeToMerge finder = _merger.MergeSituation.GetMergePartnerFinder(theirChild);
			//        XmlNode ourChild = finder.GetNodeToMerge(theirChild, _ours);
			//        XmlNode ancestorChild = finder.GetNodeToMerge(theirChild, _ancestor);


			//        if (theirChild.NodeType == XmlNodeType.Text)
			//        {
			//            _merger.MergeTextNodes(ref ourChild, theirChild, ancestorChild);
			//            continue;
			//        }

			//        if (ourChild == null)
			//        {
			//            if (ancestorChild == null)
			//            {
			//                _ours.AppendChild(XmlUtilities.GetDocumentNodeFromRawXml(theirChild.OuterXml, _ours.OwnerDocument));
			//            }
			//            else if (XmlUtilities.AreXmlElementsEqual(ancestorChild, theirChild))
			//            {
			//                continue; // we deleted it, they didn't touch it
			//            }
			//            else //we deleted it, but at the same time, they changed it
			//            {
			//                _merger.EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(theirChild.Name, null,
			//                    theirChild, ancestorChild, _merger.MergeSituation));
			//                continue;
			//            }
			//        }
			//        else if ((ancestorChild != null) && XmlUtilities.AreXmlElementsEqual(ourChild, ancestorChild))
			//        {
			//            if (XmlUtilities.AreXmlElementsEqual(ourChild, theirChild))
			//            {
			//                //nothing to do
			//                continue;
			//            }
			//            else //theirs is new
			//            {
			//                _merger.MergeInner(ref ourChild, theirChild, ancestorChild);
			//            }
			//        }
			//        else
			//        {
			//            _merger.MergeInner(ref ourChild, theirChild, ancestorChild);
			//        }
			//    }

			//// deal with their deletions (elements and text)
			//foreach (XmlNode ourChild in _ours.ChildNodes)
			//{
			//    if (ourChild.NodeType != XmlNodeType.Element && ourChild.NodeType != XmlNodeType.Text)
			//    {
			//        continue;
			//    }
			//    IFindNodeToMerge finder = _merger.MergeSituation.GetMergePartnerFinder(ourChild);
			//    XmlNode ancestorChild = finder.GetNodeToMerge(ourChild, _ancestor);
			//    XmlNode theirChild = finder.GetNodeToMerge(ourChild, _theirs);

			//    if (theirChild == null && ancestorChild != null)
			//    {
			//        if (XmlUtilities.AreXmlElementsEqual(ourChild, ancestorChild))
			//        {
			//            _ours.RemoveChild(ourChild);
			//        }
			//        else
			//        {
			//            if (ourChild.NodeType == XmlNodeType.Element)
			//            {
			//                _merger.EventListener.ConflictOccurred(
			//                    new RemovedVsEditedElementConflict(ourChild.Name, ourChild, null, ancestorChild,
			//                    _merger.MergeSituation));
			//            }
			//            else
			//            {
			//                _merger.EventListener.ConflictOccurred(
			//                    new RemovedVsEditedTextConflict(ourChild, null, ancestorChild, _merger.MergeSituation));
			//            }
			//        }
			//    }
			//}

		}

		private bool ChildrenAreSame(XmlNode ourChild, XmlNode theirChild)
		{
			return  _merger.MergeStrategies.GetElementStrategy(ourChild).IsImmutable // don't bother comparing
					|| XmlUtilities.AreXmlElementsEqual(ourChild, theirChild);
		}

		private Dictionary<XmlNode, XmlNode> MakeCorrespondences(List<XmlNode> primary, List<XmlNode> others, XmlNode otherParent)
		{
			Dictionary<XmlNode, XmlNode> result = new Dictionary<XmlNode, XmlNode>(_ourKeepers.Count);
			foreach (XmlNode node in primary)
			{
				XmlNode other = FindMatchingNode(node, otherParent);
				if (other != null)
					result[node] = other;
			}
			return result;
		}

		private XmlNode FindMatchingNode(XmlNode node, XmlNode otherParent)
		{
			IFindNodeToMerge finder = _merger.MergeStrategies.GetMergePartnerFinder(node);
			return finder.GetNodeToMerge(node, otherParent);
		}

		/// <summary>
		/// Remove from ancestorKeepers any node that does not correspond to anything (both deleted)
		/// Remove from ancestorKeepers and theirKeepers any pair that correspond to each other but not to anything in ours. Report conflict (delete/edit) if pair not identical.
		/// Remove from ancestorKeepers and ourKeepers any pair that correspond to each other and are identical, but don't correspond to anything in theirs (they deleted)
		/// Report conflict (edit/delete) on any pair that correspond in ours and ancestor, but nothing in theirs, and that are NOT identical. (but keep them...we win)
		/// </summary>
		private void DoDeletions()
		{
			// loop over a copy of the list, since we may modify ancestorKeepers.
			List<XmlNode> loopSource = new List<XmlNode>(_ancestorKeepers);
			foreach (XmlNode ancestorChild in loopSource)
			{
				ElementStrategy mergeStrategy = _merger.MergeStrategies.GetElementStrategy(ancestorChild);
				IFindNodeToMerge finder = mergeStrategy.MergePartnerFinder;
				XmlNode ourChild = finder.GetNodeToMerge(ancestorChild, _ours);
				XmlNode theirChild = finder.GetNodeToMerge(ancestorChild, _theirs);

				var extantNode = ancestorChild ?? ourChild ?? theirChild;
				if (extantNode is XmlCharacterData)
					return; // Already done.

				if (XmlUtilities.IsTextLevel(ourChild, theirChild, ancestorChild))
				{
					new MergeTextNodesMethod(_merger, mergeStrategy, _skipInnerMergeFor,
						ref ourChild, _ourKeepers,
						theirChild, _theirKeepers,
						ancestorChild, _ancestorKeepers).DoDeletions();
				}
				else if (ourChild == null)
				{
					// We deleted it.
					if (theirChild == null)
					{
						// We both deleted it. Forget it ever existed.
						_merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(_merger.MergeSituation.PathToFileInRepository, ancestorChild, null));
						_ancestorKeepers.Remove(ancestorChild);
					}
					else
					{
						if (!XmlUtilities.AreXmlElementsEqual(ancestorChild, theirChild))
						{
							// We deleted, they modified, report conflict.
							if (theirChild.NodeType == XmlNodeType.Element)
							{
								_merger.ConflictOccurred(
									new RemovedVsEditedElementConflict(theirChild.Name, null,
																	   theirChild, ancestorChild,
																	   _merger.MergeSituation,
																	   _merger.MergeStrategies.
																		   GetElementStrategy(theirChild),
																	   _merger.MergeSituation.BetaUserId));
							}
							else
							{
								_merger.ConflictOccurred(
									new RemovedVsEditedTextConflict(null, theirChild,
																	   ancestorChild,
																	   _merger.MergeSituation,
																	   _merger.MergeSituation.BetaUserId));
							}
							_ancestorKeepers.Remove(ancestorChild);//review hatton added dec 2009, wanting whoever edited it to win (previously "we" always won)
						}
						else
						{
							//We deleted it, they didn't edit it. So just make it go away.
							_merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(_merger.MergeSituation.PathToFileInRepository, ancestorChild, theirChild));
							_ancestorKeepers.Remove(ancestorChild);
							_theirKeepers.Remove(theirChild);
						}
					}
				}
				else if (theirChild == null)
				{
					// they deleted it (and we didn't)
					if (XmlUtilities.AreXmlElementsEqual(ancestorChild, ourChild))
					{
						// We didn't touch it, allow their deletion to go forward, forget it existed.
						_merger.EventListener.ChangeOccurred(new XmlDeletionChangeReport(_merger.MergeSituation.PathToFileInRepository, ancestorChild, ourChild));
						_ancestorKeepers.Remove(ancestorChild);
						_ourKeepers.Remove(ourChild);
					}
					else
					{
						// We changed it, ignore their deletion and report conflict.
						if (ourChild.NodeType == XmlNodeType.Element)
						{
							_merger.ConflictOccurred(
								new EditedVsRemovedElementConflict(ourChild.Name, ourChild, null, ancestorChild,
																   _merger.MergeSituation, _merger.MergeStrategies.GetElementStrategy(ourChild), _merger.MergeSituation.AlphaUserId));
						}
						else
						{
							_merger.ConflictOccurred(
								new EditedVsRemovedTextConflict(ourChild, null, ancestorChild, _merger.MergeSituation, _merger.MergeSituation.AlphaUserId));
						}
					}
				}
			}
		}

		/// <summary>
		/// Copy the children of the specified parent to a list, dropping (and asserting)
		/// any we don't know how to merge.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="list"></param>
		private static void CopyChildrenToList(XmlNode parent, List<XmlNode> list)
		{
			if (parent == null)
				return;
			foreach (XmlNode child in parent.ChildNodes)
			{
				if (child.NodeType != XmlNodeType.Element && (child.NodeType != XmlNodeType.Text))
				{
					if (child.NodeType == XmlNodeType.Whitespace)
					{
						continue;
					}
					if (child.NodeType == XmlNodeType.Comment)
					{
						continue;
					}
					Debug.Fail("We don't know how to merge this type of child: " + child.NodeType.ToString());
					continue; //we don't know about merging other kinds of things
				}
				list.Add(child);
			}
		}
	}

	/// <summary>
	/// Record of what we know about where in the primary sequence a node from the other sequence should go.
	/// </summary>
	class PositionRecord
	{
		// if the 'other' node corresponds to one in primary, this is the index of the corresponding
		// primary node. Otherwise -1.
		internal int exactPosition = -1;
		// The index in primary of the first node that we know the other node must go before.
		// Only used if exactPosition is not known.
		internal int indexOfFollowingPrimaryNode = Int32.MaxValue;
		// the index in primary of the last node that we know the other node must go after.
		// Only used if exactPosition is not known.
		internal int indexofPrecedingPrimaryNode = -1;
		internal XmlNode _other; // the actual XML node.

		internal PositionRecord(XmlNode other)
		{
			_other = other;
		}

		/// <summary>
		/// The position in primary that this node either corresponds to, or must follow.
		/// </summary>
		internal int Position
		{
			get
			{
				if (exactPosition == -1)
					return indexofPrecedingPrimaryNode;
				return exactPosition;
			}
		}

		/// <summary>
		/// The position
		/// </summary>
		internal XmlNode Other
		{
			get { return _other; }
			set { _other = value; }
		}

		/// <summary>
		/// A sort key that allows us to order things by their position, keeping ones that must follow
		/// a primary node after the one (if any) that corresponds exactly.
		/// </summary>
		internal int Key
		{
			get
			{
				if (exactPosition == -1)
					return indexofPrecedingPrimaryNode * 2 + 1;
				else return exactPosition*2;
			}
		}

		internal bool HasExactPosition
		{
			get { return exactPosition != -1; }
		}

		// the position of a node from the 'other' list is consistent if either it corresponds,
		// so we know exactly where it goes, or else its earliest and latest possible positions
		// are in order.
		public bool OrderIsConsistent
		{
			get { return HasExactPosition || indexofPrecedingPrimaryNode < indexOfFollowingPrimaryNode; }
		}

		/// <summary>
		/// Return true if the position determined for this element is ambiguous, that is,
		/// it doesn't correspond exactly, and the nodes it must go between are not adjacent.
		/// (Note: Could be reported as ambiguous when it is actually inconsistent.)
		/// </summary>
		public bool OrderIsAmbiguous(int cPrimary)
		{
			if (HasExactPosition)
				return false; // no ambiguity if it corresponds exactly!
			if (indexofPrecedingPrimaryNode == indexOfFollowingPrimaryNode - 1)
				return false; // goes exactly between two nodes, or right at start (preceding = -1)
			if (indexofPrecedingPrimaryNode == cPrimary - 1)
				return false; // goes exactly at end
			return true; // more than two possible positions.
		}
	}

	/// <summary>
	/// This class takes two lists of nodes and seeks to order them so that the nodes in the
	/// primary list stay in order, and the ones in the other list are inserted in such a way as
	/// to preserve as far as possible the correspondences between nodes that we know of.
	/// </summary>
	class ChildOrderer
	{
		/// <summary>
		/// Map from node in _others to index of position record (and, initially, own position in _others).
		/// </summary>
		private Dictionary<XmlNode, int> _indexInPositions;
		private List<XmlNode> _primary;
		private List<XmlNode> _others;
		private Dictionary<XmlNode, XmlNode> _correspondences;
		private PositionRecord[] _positions;
		private XmlMerger _merger;
		private bool _orderIsConsistent;
		private bool _orderIsDifferent;
		private bool _orderIsAmbiguous;

		internal ChildOrderer(List<XmlNode> primary, List<XmlNode> others, Dictionary<XmlNode, XmlNode> correspondences,
			XmlMerger merger)
		{
			_primary = primary;
			_others = others;
			_correspondences = correspondences;
			_positions = new PositionRecord[others.Count];
			Debug.Assert(merger != null);
			_merger = merger;
		}

		internal Dictionary<XmlNode, XmlNode> Correspondences
		{
			get { return _correspondences; }
		}

		internal void Organize()
		{
			// Fill in the positions array (with default values).
			for (int i = 0; i < _positions.Length; i++)
				_positions[i] = new PositionRecord(_others[i]);

			// Set up the index in others map.
			_indexInPositions = new Dictionary<XmlNode, int>(_others.Count);
			for (int i = 0; i < _others.Count; i++)
				_indexInPositions[_others[i]] = i;

			// mark exact nodes: each node in _others that has a corresponding node in primary is marked
			// with the index of that primary node.
			for (int i = 0; i < _primary.Count; i++)
			{
				XmlNode primaryChild = _primary[i];
				XmlNode otherChild;
				if (_correspondences.TryGetValue(primaryChild, out otherChild))
					_positions[_indexInPositions[otherChild]].exactPosition = i;
			}

			if (_correspondences.Count < _others.Count)
				SetRangesForUnmatchedOthers();

			// determine whether order is different
			for (int i = 0; i < _positions.LongLength - 1; i++)
			{
				if (_positions[i].Key > _positions[i + 1].Key)
				{
					_orderIsDifferent = true;
					break;
				}
			}
			_orderIsConsistent = true;
			foreach (PositionRecord pr in _positions)
			{
				if (!pr.OrderIsConsistent)
					_orderIsConsistent = false;
			}
			// NB sort AFTER determining whether order is changed and consistent,
			// since it changes the information used to determine that.
			SortPositions();
			ResolveAmbiguities();
			_orderIsAmbiguous = IsAmbiguous();
		}

		/// <summary>
		/// Run before evaluating whether insert position is ambiguous.
		/// </summary>
		void ResolveAmbiguities()
		{
			// Correct the lookup map; note that _others is no longer valid.
			_others = null; // no longer corresponds to sorted positions, using is invalid, force error.
			for (int i = 0; i < _positions.Length; i++)
			{
				_indexInPositions[_positions[i].Other] = i;
			}
			if (_correspondences.Count < _positions.Length)
				MakePossibleCorrespondences();
		}

		/// <summary>
		/// See if we can make additional correspondences that might be helpful. We only consider sub-sequences
		/// between definitely matching pairs.
		/// </summary>
		private void MakePossibleCorrespondences()
		{
			int startPrimaryRange = -1; // no range in progress
			int endPrimaryRange = -1;
			for (int iPrimary = 0; iPrimary < _primary.Count; iPrimary++)
			{
				XmlNode primaryChild = _primary[iPrimary];
				XmlNode otherChild;
				if (_correspondences.TryGetValue(primaryChild, out otherChild))
				{
					MakePossibleCorrespondences(startPrimaryRange, endPrimaryRange);
					startPrimaryRange = -1; // no longer one in progress.
				}
				else
				{
					// no match: start range if not in one, and adjust end
					if (startPrimaryRange == -1)
						startPrimaryRange = iPrimary;
					endPrimaryRange = iPrimary;
				}
			}
			MakePossibleCorrespondences(startPrimaryRange, endPrimaryRange);
		}

		/// <summary>
		/// Determine possible correspondences for the given range of currently unmatched items.
		/// </summary>
		/// <param name="startPrimaryRange"></param>
		/// <param name="endPrimaryRange"></param>
		private void MakePossibleCorrespondences(int startPrimaryRange, int endPrimaryRange)
		{
			if (startPrimaryRange == -1)
				return;  // empty range
			int limPrimaryRange = endPrimaryRange + 1;
			int startOtherRange = 0;
			int limOtherRange = _positions.Length;
			if (startPrimaryRange > 0)
			{
				startOtherRange = _indexInPositions[_correspondences[_primary[startPrimaryRange - 1]]] + 1;
			}
			if (endPrimaryRange < _primary.Count - 1)
			{
				limOtherRange = _indexInPositions[_correspondences[_primary[limPrimaryRange]]];
			}
			if (limOtherRange - startOtherRange <= 0)
				return; // no objects that might correspond
			List<XmlNode> possibleMatches = new List<XmlNode>(limOtherRange - startOtherRange);

			for (int i = startOtherRange; i < limOtherRange; i++)
				possibleMatches.Add(_positions[i].Other);

			for (int iPrimary = startPrimaryRange; iPrimary < limPrimaryRange; iPrimary++)
			{
				XmlNode ourChild = _primary[iPrimary];
				IFindNodeToMerge finder = _merger.MergeStrategies.GetMergePartnerFinder(ourChild);
				IFindPossibleNodeToMerge possibleFinder = finder as IFindPossibleNodeToMerge;
				if (possibleFinder == null)
					continue;
				XmlNode otherChild = possibleFinder.GetPossibleNodeToMerge(ourChild, possibleMatches);
				if (otherChild == null)
					continue;
				// We don't want to reorder things here, so future searches are limited to subsequent
				// possible matches.
				possibleMatches.RemoveRange(0, possibleMatches.IndexOf(otherChild) + 1);
				_correspondences[ourChild] = otherChild;

				// Take advantage of the new match to reduce ambiguity.
				int iPositionChanged = _indexInPositions[otherChild];
				_positions[iPositionChanged].exactPosition = iPrimary;
				for (int ipos = iPositionChanged + 1; ipos < limOtherRange; ipos++)
				{
					_positions[ipos].indexofPrecedingPrimaryNode = iPrimary;
				}
				for (int ipos = iPositionChanged - 1;
					ipos >= startOtherRange && _positions[ipos].exactPosition == -1;
					ipos--)
				{
					_positions[ipos].indexOfFollowingPrimaryNode = iPrimary;
				}
			}
		}

		private void SetRangesForUnmatchedOthers()
		{
			for (int iPrimary = 0; iPrimary < _primary.Count; iPrimary++)
			{
				XmlNode otherChild;
				if (!_correspondences.TryGetValue(_primary[iPrimary], out otherChild))
					continue;

				// For any nodes following the corresponding one that don't have exact position, this is their
				// preceding node (they must follow the merged node).
				int iOther = _indexInPositions[otherChild];
				int min = iOther + 1; // first 'follower'
				int lim = iOther + 1; // limit of 'follower' range
				while (lim < _positions.Length && !_positions[lim].HasExactPosition)
					lim++;
				for (int i = min; i < lim; i++)
					_positions[i].indexofPrecedingPrimaryNode = iPrimary;

				// For any nodes preceding the corresponding one that don't have exact position, this is their
				// following node (they must precede the merged node).
				lim = iOther;
				min = iOther;
				while (min > 0 && !_positions[min - 1].HasExactPosition)
					min--;
				for (int i = min; i < lim; i++)
					_positions[i].indexOfFollowingPrimaryNode = iPrimary;
			}
		}

		/// <summary>
		/// Return true if the order information determined for the 'others' is consistent, that is,
		/// every node that does not have an exactPositon has minBefore and maxAfter consistent.
		/// </summary>
		internal bool OrderIsConsistent
		{
			get
			{
				if (_indexInPositions == null)
					Organize();
				return _orderIsConsistent;
			}
		}

		internal bool OrderIsAmbiguous
		{
			get
			{
				if (_indexInPositions == null)
					Organize();
				return _orderIsAmbiguous;
			}
		}

		private bool IsAmbiguous()
		{
			// Special case: source has no children at all. Therefore, even though currently
			// we show all others as inserted between -1 and MaxInt, that is not ambiguous.
			if (_primary.Count == 0)
				return false;
			foreach (PositionRecord pr in _positions)
			{
				//REVIEW JT (johnH): Can you decide if this is sufficient or if there is a better approach?
				var strategy = _merger.MergeStrategies.GetElementStrategy(pr._other);
				var bypassAmbiguityStuff = (strategy != null && !strategy.OrderIsRelevant);

				if (!bypassAmbiguityStuff && pr.OrderIsAmbiguous(_primary.Count))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Return true if the order determined for the other list is different from its original order.
		/// </summary>
		internal bool OrderIsDifferent
		{
			get
			{
				if (_indexInPositions == null)
					Organize();
				return _orderIsDifferent;
			}
		}

		internal class PositionRecordComparer : IComparer
		{
			internal PositionRecordComparer()
			{

			}

			int IComparer.Compare(Object x, Object y)
			{
				PositionRecord xpr = x as PositionRecord;
				PositionRecord ypr = y as PositionRecord;

				if (xpr.Key < ypr.Key)
					return -1;
				if (xpr.Key > ypr.Key)
					return 1;
				return 0;
			}
		}


		// Sort _positions by Key, then if keys are equal, by current position (just use merge sort).
		private void SortPositions()
		{
			MergeSort.Sort(_positions, new PositionRecordComparer());
		}

		internal List<XmlNode> GetResultList()
		{
			//Finally do a merge of the primary keepers in original order and secondary keepers as just sorted.
			//    - at each step we are comparing the current position in the primary list (primaryPosition)
			//		with the position in the current 'other list'. Let 'otherPosition' be the exactPosition of
			//		the current 'other node', or its minAfter if it has no exact position.
			//    - if primaryPosition < otherPosition, transfer primaryPosition node and increment primaryPosition
			//    - else if primaryPosition > otherPosition, transfer otherPosition node and increment the index into the otherPositions list.
			//    - else if otherPosition comes from exactPosition, merge the two current nodes and increment both indexes
			//    - otherwise, otherPosition comes from minAfter, meaning the 'other' node is an insertion after the primaryPosition. Transfer primaryPositon node and increment primaryPosition.
			int iPrimary = 0;
			int iOther = 0;
			List<XmlNode> result = new List<XmlNode>(_primary.Count + _positions.Length);

			while (iPrimary < _primary.Count || iOther < _positions.Length)
			{
				if (iPrimary >= _primary.Count)
				{
					result.Add(_positions[iOther].Other);
					iOther++;
					continue;
				}
				if (iOther >= _positions.Length)
				{
					result.Add(_primary[iPrimary]);
					iPrimary++;
					continue;
				}
				if (iPrimary == _positions[iOther].exactPosition)
				{
					// they are a corresponding pair. Copy just the primary...caller will merge.
					result.Add(_primary[iPrimary]);
					iPrimary++;
					iOther++; // advance this too! We've 'included' a pair from both lists.
					continue;
				}
				if (iPrimary < _positions[iOther].Position)
				{
					result.Add(_primary[iPrimary]);
					iPrimary++;
					continue;
				}
				// the others node is not an exact match, nor does it come after the current
				// primary, so it must come before it.
				result.Add(_positions[iOther].Other);
				iOther++;
			}
			return result;
		}
	}
}
