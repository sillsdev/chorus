using System.Collections.Generic;
using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This class should be used when there is an optional attribute that would provide our match,
	/// but some other finder strategy is needed when it isn't there.
	/// </summary>
	public class OptionalKeyAttrFinder : IFindNodeToMerge
	{
		private readonly string _key;
		private readonly IFindNodeToMerge _backup;

		public OptionalKeyAttrFinder(string key, IFindNodeToMerge backupFinder)
		{
			_key = key;
			_backup = backupFinder;
		}

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn, HashSet<XmlNode> acceptableTargets)
		{
			if (parentToSearchIn == null || nodeToMatch == null)
				return null;
			var keyVal = nodeToMatch.Attributes[_key];
			if( keyVal != null)
			{
				foreach (XmlNode node in parentToSearchIn.ChildNodes)
				{
					if (!acceptableTargets.Contains(node) || !(node is XmlElement))
						continue;
					if (XmlUtilities.GetOptionalAttributeString(node, _key) == keyVal.Value)
						return node;
				}
			}

			return _backup.GetNodeToMerge(nodeToMatch, parentToSearchIn, acceptableTargets);

		}

		/// <summary>
		/// Get the query that is used to find a matching XmlNode
		/// </summary>
		/// <returns>A query fo find a matching node, or null/empty string, if ambiguous nodes aren't to be from a parent.</returns>
		public string GetMatchingNodeFindingQuery(XmlNode nodeToMatch)
		{
			return null;
		}

		/// <summary>
		/// Get a basic message that is suitable for use in a warning report where ambiguous nodes are found in the same parent node.
		/// </summary>
		/// <returns>A message string or null/empty string, if no message is needed for ambiguous nodes.</returns>
		public string GetWarningMessageForAmbiguousNodes(XmlNode nodeForMessage)
		{
			return null;
		}
	}
}