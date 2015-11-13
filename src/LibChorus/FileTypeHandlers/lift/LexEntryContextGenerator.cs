using System.IO;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.lift
{
	public class LexEntryContextGenerator :IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			var label = doc.SelectTextPortion("entry/lexical-unit/form/text");
			return new ContextDescriptor(label, LiftUtils.GetUrl(doc.FirstChild, Path.GetFileName(filePath), label));
		}
		/*        public ContextDescriptor GenerateContextDescriptor(string mergeElement)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			var label = doc.SelectTextPortion("entry/lexical-unit/form/text");
			var node = doc.FirstChild.Attributes.GetNamedItem("guid");
			if(node!=null)
			{
				return new ContextDescriptor(label, String.Format("lift/entry[@guid='{0}']", node.Value));
			}
			node = doc.FirstChild.Attributes.GetNamedItem("id");
			if(node!=null)
			{
				return new ContextDescriptor(label,String.Format("lift/entry[@id='{0}']", node.Value));
			}
			throw new ApplicationException("Could not get guid or id attribute out of "+mergeElement);

		}*/
	}
}