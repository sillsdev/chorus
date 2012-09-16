using System.Xml;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Custom finder for matching example sentences by form.
	/// </summary>
	public class FormMatchingFinder : IFindNodeToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (nodeToMatch == null || parentToSearchIn == null)
				return null;
			var nodeName = nodeToMatch.LocalName;
			var ourForms = nodeToMatch.SafeSelectNodes(nodeName + "/form");

			foreach (XmlNode example in parentToSearchIn.SafeSelectNodes(nodeName))
			{
				XmlNodeList forms = example.SafeSelectNodes("form");
				if(!SameForms(example, forms, ourForms))
					continue;

				return example;
			}

			return null; //couldn't find a match

		}

		private static bool SameForms(XmlNode example, XmlNodeList list1, XmlNodeList list2)
		{
			foreach (XmlNode form in list1)
			{
				var x = example.SafeSelectNodesWithParms("form[@lang='{0}']", form.GetStringAttribute("lang"));
				if (x.Count == 0)
					break;
				var lang = form.GetStringAttribute("lang");
				foreach (XmlNode form2 in list2)
				{
					if (form2.GetStringAttribute("lang") != lang)
						continue;
					if (form2.InnerText != form.InnerText)
						return false; // they differ
				}
			}
			return true;
		}
	}
}