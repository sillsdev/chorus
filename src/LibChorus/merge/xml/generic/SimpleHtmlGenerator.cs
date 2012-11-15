using System.Xml;

namespace Chorus.merge.xml.generic
{
	internal class SimpleHtmlGenerator : IGenerateHtmlContext
	{
		public string HtmlContext(XmlNode mergeElement)
		{
			return XmlUtilities.GetXmlForShowingInHtml(mergeElement.OuterXml);
		}

		public string HtmlContextStyles(XmlNode mergeElement)
		{
			return ""; // GetXmlForShowingInHtml does not generate any classes
		}
	}
}