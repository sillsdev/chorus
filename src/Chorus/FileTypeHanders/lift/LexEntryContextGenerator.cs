using System;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public class LexEntryContextGenerator :IGenerateContextDescriptor
	{
		public string GenerateContextDescriptor(string mergeElement)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			var node = doc.FirstChild.Attributes.GetNamedItem("guid");
			if(node!=null)
			{
				return String.Format("lift/entry[@guid='{0}']", node.Value);
			}
			node = doc.FirstChild.Attributes.GetNamedItem("id");
			if(node!=null)
			{
				return String.Format("lift/entry[@id='{0}']", node.Value);
			}
			throw new ApplicationException("Could not get guid or id attribute out of "+mergeElement);

		}
	}
}