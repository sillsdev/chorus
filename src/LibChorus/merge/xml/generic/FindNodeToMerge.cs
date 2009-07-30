using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic.xmldiff;


namespace Chorus.merge.xml.generic
{
	public interface IFindNodeToMerge
	{
		/// <summary>
		/// Should return null if parentToSearchIn is null
		/// </summary>
		XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn);
	}

	/// <summary>
	/// Defines a secondary algorithm for finding less ideal matches, e.g., we allow an empty
	/// node to match a non-empty (but no duplicates).
	/// </summary>
	public interface IFindPossibleNodeToMerge
	{
		XmlNode GetPossibleNodeToMerge(XmlNode nodeToMatch, List<XmlNode> possibleMatches);
	}

	public class FindByKeyAttribute : IFindNodeToMerge
	{
		private string _keyAttribute;

		public FindByKeyAttribute(string keyAttribute)
		{
			_keyAttribute = keyAttribute;
		}


		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return null;

			string key = XmlUtilities.GetOptionalAttributeString(nodeToMatch, _keyAttribute);
			if (string.IsNullOrEmpty(key) || parentToSearchIn == null)
			{
				return null;
			}
			string xpath = string.Format("{0}[@{1}='{2}']", nodeToMatch.Name, _keyAttribute, key);

			return parentToSearchIn.SelectSingleNode(xpath);
		}

	}

	/// <summary>
	/// e.g. <grammatical-info>  there can only be one
	/// </summary>
	public class FindFirstElementWithSameName : IFindNodeToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return null;

			return parentToSearchIn.SelectSingleNode(nodeToMatch.Name);
		}
	}

	public class FindByEqualityOfTree : IFindNodeToMerge, IFindPossibleNodeToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return null;

			//match any exact xml matches, including all the children

			foreach (XmlNode node in parentToSearchIn.ChildNodes)
			{
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
		//todo: this won't cope with multiple text child nodes in the same element

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return null;

			//just match first text we find

			foreach (XmlNode node in parentToSearchIn.ChildNodes)
			{
				if(node.NodeType == XmlNodeType.Text)
					return node;
			}
			return null;
		}
	}
}