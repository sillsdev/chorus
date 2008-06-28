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

	public class FindByEqualityOfTree : IFindNodeToMerge
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