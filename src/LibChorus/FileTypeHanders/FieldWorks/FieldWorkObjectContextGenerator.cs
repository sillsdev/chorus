using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	public class FieldWorkObjectContextGenerator : IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			// var label = doc.SelectTextPortion("entry/lexical-unit/form/text");
			const string label = "Some object";
			return new ContextDescriptor(label, null); // LiftUtils.GetUrl(doc.FirstChild, Path.GetFileName(filePath), label)
		}
	}
}