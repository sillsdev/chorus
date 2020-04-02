#define USEOPTIMIZEDVERSION
using System;
using System.Collections.Generic;
using SIL.Extensions;
#if USEOPTIMIZEDVERSION
using System.Linq;
#endif
using System.Text;
using System.Web;
using System.Xml;
using Chorus.merge.xml.generic.xmldiff;
using SIL.Code;


namespace Chorus.merge.xml.generic
{
	public interface IFindNodeToMerge
	{
		/// <summary>
		/// Should return null if parentToSearchIn is null. Non-null result should be a value in acceptableTargets,
		/// which will be a subset (or all) of the children of parentToSearchIn; any other result will
		/// be treated as no match.
		/// </summary>
		XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets);
	}

	/// <summary>
	/// Helper class that gets the default set of children of interest for finding matching children.
	/// </summary>
	public static class SetFromChildren
	{
		public static HashSet<XmlNode> Get(XmlNode parent)
		{
			var result = new HashSet<XmlNode>();
			if (parent == null)
				return result;
			foreach (XmlNode node in parent.ChildNodes)
			{
				if (node is XmlElement || node is XmlText)
					result.Add(node);
			}
			return result;
		}
	}

	/// <summary>
	/// An additional interface that extends IFindNodeToMerge to return multiple matches.
	/// </summary>
	public interface IFindMatchingNodesToMerge : IFindNodeToMerge
	{
		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn);

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage);
	}

	/// <summary>
	/// Defines a secondary algorithm for finding less ideal matches, e.g., we allow an empty
	/// node to match a non-empty (but no duplicates).
	/// </summary>
	public interface IFindPossibleNodeToMerge
	{
		XmlNode GetPossibleNodeToMerge(XmlNode nodeToMatch, List<XmlNode> possibleMatches);
	}

	public class FindByKeyAttribute : IFindMatchingNodesToMerge
	{
		private readonly string _keyAttribute;
#if USEOPTIMIZEDVERSION
		private readonly List<XmlNode> _parentsToSearchIn = new List<XmlNode>();

		/// <summary>
		/// Stores nodes from the xml object that have child nodes which need to be identified by both their key attribute and name
		/// </summary>
		private readonly Dictionary<int, Dictionary<Tuple<string,string>, XmlNode>> _indexedSoughtAfterNodes = new Dictionary<int, Dictionary<Tuple<string,string>, XmlNode>>();
#endif

		public FindByKeyAttribute(string keyAttribute)
		{
			_keyAttribute = keyAttribute;
		}

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			var key = XmlUtilities.GetOptionalAttributeString(nodeToMatch, _keyAttribute);
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

#if USEOPTIMIZEDVERSION
			var parentIdx = _parentsToSearchIn.IndexOf(parentToSearchIn);
			if (parentIdx == -1)
			{
				_parentsToSearchIn.Add(parentToSearchIn);
				parentIdx = _parentsToSearchIn.IndexOf(parentToSearchIn);
				// The child node we want is identified by a combination of it's key attribute and name.
				var childrenWithKeys = new Dictionary<Tuple<string,string>, XmlNode>(); // StringComparer.OrdinalIgnoreCase NO: Bad idea, since I (RBR) saw a case in a data file that had both upper and lower-cased variations.
				_indexedSoughtAfterNodes.Add(parentIdx, childrenWithKeys);
				var childrenWithKeyAttr = (from XmlNode childNode in parentToSearchIn.ChildNodes
										   where childNode.Attributes != null && childNode.Attributes[_keyAttribute] != null
										   select childNode).ToList();
				foreach (var nodeWithKeyAttribute in childrenWithKeyAttr)
				{
					try
					{

						childrenWithKeys.Add(new Tuple<string, string>(nodeWithKeyAttribute.Name, nodeWithKeyAttribute.Attributes[_keyAttribute].Value), nodeWithKeyAttribute);
					}
					catch(ArgumentException)
					{
						string parentAttributes = String.Empty;
						if(parentToSearchIn.Attributes != null)
						{
							foreach(XmlAttribute attr in parentToSearchIn.Attributes)
							{
								parentAttributes += String.Format("{0}={1};", attr.Name, attr.Value);
							}
						}
						var usefulMessage = String.Format("Unexpectedly found duplicate children with key attribute {0} in parent element {1} with attributes [{2}].",
							nodeWithKeyAttribute.Attributes[_keyAttribute].Value, parentToSearchIn.Name, parentAttributes);

						throw new ArgumentException(usefulMessage);
					}
				}
			}

			XmlNode matchingNode;
			_indexedSoughtAfterNodes[parentIdx].TryGetValue(new Tuple<string, string>(nodeToMatch.Name, key), out matchingNode);
			// JohnT: consider replacing the line above with this if we decide to deal with duplicate keys. This branch currently won't cope with this,
			// because the Add above will fail on any duplicate key.
			//if (_indexedSoughtAfterNodes[parentIdx].TryGetValue(key, out matchingNode) && !acceptableTargets.Contains(matchingNode))
			//{
			//    // The one we retrieved is not acceptable (typically we concluded it should be deleted).
			//    // It's just possible that there's a duplicate key, and one of the acceptable matches is also a match.
			//    // Not finding a match is relatively rare, so try a sequential search.
			//    // We don't just search in acceptableTargets itself, because we still prefer the first (acceptable) match.
			//    return (from XmlNode node in parentToSearchIn.ChildNodes
			//        where node is XmlElement && acceptableTargets.Contains(node) && GetKey(node) == key
			//        select node).FirstOrDefault();
			//}
			return matchingNode; // May be null, which is fine.
#else
			var matches = GetMatchingNodes(nodeToMatch, parentToSearchIn).Where(node=>acceptableNodes.Contains(node).toList();
			return (matches.Count > 0) ? matches[0] : null;
#endif
		}

		private string GetKey(XmlNode node)
		{
			var keyAttr = node.Attributes[_keyAttribute];
			if (keyAttr == null)
				return null;
			return keyAttr.Value;
		}

		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		public IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return new List<XmlNode>();

			var key = XmlUtilities.GetOptionalAttributeString(nodeToMatch, _keyAttribute);
			if (string.IsNullOrEmpty(key))
			{
				return new List<XmlNode>();
			}

			var matches = new List<XmlNode>();
			foreach (XmlNode childNode in parentToSearchIn.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				if (nodeToMatch == childNode)
				{
					matches.Add(childNode);
					continue;
				}
				var keyAttr = childNode.Attributes[_keyAttribute];
				if (keyAttr == null || keyAttr.Value != key)
					continue;
				matches.Add(childNode);
			}
			return matches;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			Guard.AgainstNull(nodeForMessage, "nodeForMessage");

			return string.Format("The key attribute '{0}' has values that are the same '{1}'",
				_keyAttribute, nodeForMessage.Attributes[_keyAttribute].Value);
		}
	}

	/// <summary>
	/// Assuming the children of the parent to search in form a list (order matters, duplicates allowed), and so do the
	/// children of the nodeToMatch, find the corresponding object in the list. A corresponding node will have the same key,
	/// and the same number of preceding siblings with the same key among the acceptableTargets, and the same element name.
	/// This is fairly simplistic, but good enough for merging
	/// lists of objsur elements in FieldWorks reference sequence properties.
	/// </summary>
	public class FindByKeyAttributeInList : IFindNodeToMerge
	{
		private string _keyAttribute;

		public FindByKeyAttributeInList(string keyAttribute)
		{
			_keyAttribute = keyAttribute;
		}

		/// <summary>
		/// The parent of the most recent target node (if any).
		/// </summary>
		private XmlNode _sourceNode;
		/// <summary>
		/// Map from each child of _sourceNode that has a key to its KeyPosition in the children of SourceNode.
		/// </summary>
		Dictionary<XmlNode, KeyPosition> _sourceMap = new Dictionary<XmlNode, KeyPosition>();

		/// <summary>
		/// Most recent parentNodeToSearchIn, if any.
		/// </summary>
		private XmlNode _parentNode;

		/// <summary>
		/// Map from KeyPosition in _parentNode to corresponding node (for each node that has a key).
		/// </summary>
		readonly Dictionary<KeyPosition, XmlNode> _parentMap = new Dictionary<KeyPosition, XmlNode>();

		/// <summary>
		/// Of the last nodeToMatch.
		/// </summary>
		private string _elementName;

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			string key = XmlUtilities.GetOptionalAttributeString(nodeToMatch, _keyAttribute);
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			if (nodeToMatch.ParentNode == null)
				return null;

			if (nodeToMatch.Name != _elementName)
			{
				_parentNode = _sourceNode = null; // force regenerate both maps
				_elementName = nodeToMatch.Name;
			}

			if (_sourceNode != nodeToMatch.ParentNode)
			{
				_sourceMap.Clear();
				_sourceNode = nodeToMatch.ParentNode;
				GetKeyPositions(_sourceNode.ChildNodes.Cast<XmlNode>(), (node, kp) => _sourceMap[node] = kp);
			}

			if (_parentNode != parentToSearchIn || acceptableTargets != null)
			{
				_parentMap.Clear();
				_parentNode = parentToSearchIn;
				GetKeyPositions(_parentNode.ChildNodes.Cast<XmlNode>().Where(node=>acceptableTargets.Contains(node)),
					(node, kp) => _parentMap[kp] = node);
			}

			KeyPosition targetKp;
			if (!_sourceMap.TryGetValue(nodeToMatch, out targetKp))
				return null;
			XmlNode result;
			_parentMap.TryGetValue(targetKp, out result);
			return result;
		}

		private void GetKeyPositions(IEnumerable<XmlNode> input, Action<XmlNode, KeyPosition> saveIt)
		{
			Dictionary<string, int> Occurrences = new Dictionary<string, int>();
			foreach (XmlNode node in input)
			{
				if (node.Attributes == null)
					continue;
				if (node.Name != _elementName)
					continue;
				var key1 = XmlUtilities.GetOptionalAttributeString(node, _keyAttribute);
				if (string.IsNullOrEmpty(key1))
					continue;
				int oldCount;
				Occurrences.TryGetValue(key1, out oldCount);
				saveIt(node, new KeyPosition(key1, oldCount));
				Occurrences[key1] = oldCount + 1;
			}
		}
	}

	class KeyPosition
	{
		public string Key; // Key attribute of some XmlNode
		// Position of the XmlNode among those children of its parent that have the same key.
		// Technically, a count of the number of preceding nodes among its siblings that have the same key.
		public int Position;
		public KeyPosition(string key, int position)
		{
			Key = key;
			Position = position;
		}

		public override bool Equals(object obj)
		{
			var other = obj as KeyPosition;
			if (other == null)
				return false;
			return other.Key == Key && other.Position == Position;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode() ^ Position;
		}
	}

	///<summary>
	/// Search for a matching element where multiple attribute names (not values) combine
	/// to make a single "key" to identify a matching elment.
	///</summary>
	public class FindByMatchingAttributeNames : IFindMatchingNodesToMerge
	{
		private readonly HashSet<string> _keyAttributes;

		public FindByMatchingAttributeNames(HashSet<string> keyAttributes)
		{
			_keyAttributes = keyAttributes;
		}

		#region Implementation of IFindNodeToMerge

		/// <summary>
		/// Should return null if parentToSearchIn is null
		/// </summary>
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;


			foreach (var possibleMatch in parentToSearchIn.SelectNodes(nodeToMatch.Name))
			{
				var retval = (XmlNode)possibleMatch;
				if (!acceptableTargets.Contains(retval))
					continue;
				var actualAttrs = new HashSet<string>();
				foreach (XmlNode attr in retval.Attributes)
					actualAttrs.Add(attr.Name);
				if (_keyAttributes.IsSubsetOf(actualAttrs))
					return retval;
			}

			return null;
		}

		#endregion

		#region Implementation of IFindMatchingNodesToMerge

		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		public IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return new List<XmlNode>();

			var matches = new List<XmlNode>();
			foreach (XmlNode childNode in parentToSearchIn.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				if (nodeToMatch == childNode)
				{
					matches.Add(nodeToMatch);
					continue;
				}
				var isMatch = true;
				foreach (var keyAttrName in _keyAttributes)
				{
					var keyAttr = childNode.Attributes[keyAttrName];
					if (keyAttr == null)
					{
						isMatch = false;
						break;
					}
				}
				if (!isMatch)
					continue;
				matches.Add(childNode);
			}
			return matches;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			Guard.AgainstNull(nodeForMessage, "nodeForMessage");

			var bldr = new StringBuilder();
			var keyAttrsAsList = _keyAttributes.ToList();
			for (var i = 0; i < keyAttrsAsList.Count; ++i)
			{
				if (i > 0)
					bldr.Append(", ");
				if (i == keyAttrsAsList.Count - 1)
					bldr.Append(" and ");
				var currentAttrName = keyAttrsAsList[i];
				bldr.AppendFormat("attribute '{0}'", currentAttrName);
			}

			return string.Format("The key attribute(s) are the same: {0}", bldr);
		}

		#endregion
	}

	/// <summary>
	/// Search for a matching elment where multiple attributes combine
	/// to make a single "key" to identify a matching elment.
	/// </summary>
	public class FindByMultipleKeyAttributes : IFindMatchingNodesToMerge
	{
		private readonly List<string> _keyAttributes;

		public FindByMultipleKeyAttributes(List<string> keyAttributes)
		{
			_keyAttributes = keyAttributes;
		}

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			var matches = GetMatchingNodes(nodeToMatch, parentToSearchIn).Where(acceptableTargets.Contains).ToList();
			return (matches.Count > 0)
				? matches[0]
				: null;
		}

		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		public IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return new List<XmlNode>();

			Dictionary<string, string> extantValues = _keyAttributes.ToDictionary(keyAttribute => keyAttribute, keyAttribute => (nodeToMatch.Attributes[keyAttribute] != null) ? nodeToMatch.Attributes[keyAttribute].Value : null);
			var matches = new List<XmlNode>();
			foreach (XmlNode childNode in parentToSearchIn.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				if (nodeToMatch == childNode)
				{
					matches.Add(nodeToMatch);
					continue;
				}
				var isMatch = true;
				foreach (var kvp in extantValues)
				{
					var keyAttr = childNode.Attributes[kvp.Key];
					if ((keyAttr == null ? null : keyAttr.Value) != kvp.Value)
					{
						isMatch = false;
						break;
					}
				}
				if (!isMatch)
					continue;
				matches.Add(childNode);
			}
			return matches;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			Guard.AgainstNull(nodeForMessage, "nodeForMessage");

			var bldr = new StringBuilder();
			for (var i = 0; i < _keyAttributes.Count; ++i)
			{
				if (i > 0)
					bldr.Append(", ");
				if (i == _keyAttributes.Count - 1)
					bldr.Append(" and ");
				var currentAttrName = _keyAttributes[i];
				var currentAttrValue = nodeForMessage.Attributes[currentAttrName].Value;
				bldr.AppendFormat("attribute '{0}' with value of '{1}'", currentAttrName, currentAttrValue);
			}

			return string.Format("The key attribute(s) have value(s) that are the same: {0}", bldr);
		}
	}

	/// <summary>
	/// e.g. &lt;grammatical-info&gt; there can only be one
	/// </summary>
	public class FindFirstElementWithSameName : IFindMatchingNodesToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			var matches = GetMatchingNodes(nodeToMatch, parentToSearchIn).Where(acceptableTargets.Contains).ToList();
			return (matches.Count > 0)
				? matches[0]
				: null;
		}

		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		public IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return new List<XmlNode>();

			var matches = new List<XmlNode>();
			foreach (XmlNode childNode in parentToSearchIn.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				if (nodeToMatch == childNode)
				{
					matches.Add(childNode);
					continue;
				}
				if (nodeToMatch.Name != childNode.Name)
					continue;
				matches.Add(childNode);
			}
			return matches;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			Guard.AgainstNull(nodeForMessage, "nodeForMessage");

			return string.Format("The elements are named: '{0}'", nodeForMessage.Name);
		}
	}

	/// <summary>
	/// e.g. <exemplarCharacters></exemplarCharacters> as different from <exemplarCharacters type="foo"></exemplarCharacters>
	/// </summary>
	public class FindFirstElementWithZeroAttributes : IFindMatchingNodesToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			var matches = GetMatchingNodes(nodeToMatch, parentToSearchIn).Where(acceptableTargets.Contains).ToList();
			return (matches.Count > 0)
				? matches[0]
				: null;
		}

		/// <summary>
		/// Get all matching nodes, or an empty collection, if there are no matches.
		/// </summary>
		/// <returns>A collection of zero, or more, matching nodes.</returns>
		/// <remarks><paramref name="nodeToMatch" /> may, or may not, be a child of <paramref name="parentToSearchIn"/>.</remarks>
		public IEnumerable<XmlNode> GetMatchingNodes(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return new List<XmlNode>();

			var matches = new List<XmlNode>();
			foreach (XmlNode childNode in parentToSearchIn.ChildNodes)
			{
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				if (nodeToMatch == childNode)
				{
					matches.Add(childNode);
					continue;
				}
				if (nodeToMatch.Name != childNode.Name)
					continue;
				if (childNode.Attributes?.Count > 0)
					continue;
				matches.Add(childNode);
			}
			return matches;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			Guard.AgainstNull(nodeForMessage, "nodeForMessage");

			return string.Format("The elements are named: '{0}'", nodeForMessage.Name);
		}
	}

	public class FindByEqualityOfTree : IFindNodeToMerge, IFindPossibleNodeToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			//match any exact xml matches, including all the children.
			// (Could just search in acceptableTargets, but the previous version would return the FIRST match
			// in the parent, and that just MIGHT be important somehow.)
			foreach (XmlNode node in parentToSearchIn.ChildNodes)
			{
				if (nodeToMatch.Name != node.Name || !acceptableTargets.Contains(node))
				{
					continue; // can't be equal if they don't even have the same name
				}

				if (node.GetType() == typeof(XmlText))
				{
					throw new ApplicationException("Please report: regression in FindByEqualityOfTree where the node is simply a text.");
				}
				XmlDiff d = new XmlDiff(nodeToMatch.OuterXml, node.OuterXml);
				DiffResult result = d.Compare();
				if (result == null || result.Equal)
				{
					return node;
				}
			}
			return null;
		}

		#region IFindPossibleNodeToMerge Members

		/// <summary>
		/// When looking for an exact match, we allow the fall-back of matching an empty
		/// node against one with content.
		/// </summary>
		/// <param name="nodeToMatch"></param>
		/// <param name="possibleMatches"></param>
		/// <returns></returns>
		public XmlNode GetPossibleNodeToMerge(XmlNode nodeToMatch, List<XmlNode> possibleMatches)
		{
			if (nodeToMatch.ChildNodes.Count == 0 && nodeToMatch.Attributes.Count == 0)
			{
				foreach (XmlNode possible in possibleMatches)
				{
					if (possible.Name == nodeToMatch.Name)
						return possible;
				}
			}
			else
			{
				foreach (XmlNode possible in possibleMatches)
				{
					if (possible.Name == nodeToMatch.Name && possible.ChildNodes.Count == 0
						&& possible.Attributes.Count == 0)
					{
						return possible;
					}
				}
			}
			return null;
		}

		#endregion
	}

	public class FindTextDumb : IFindNodeToMerge
	{
		// This won't cope with multiple text child nodes in the same element
		// No, but then use FormMatchingFinder for that scenario.

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null)
				return null;

			//just match first text we find

			foreach (XmlNode node in parentToSearchIn.ChildNodes)
			{
				if (node.NodeType == XmlNodeType.Text && acceptableTargets.Contains(node))
					return node;
			}
			return null;
		}
	}
}