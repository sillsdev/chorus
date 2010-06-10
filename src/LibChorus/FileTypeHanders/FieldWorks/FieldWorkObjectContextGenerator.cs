using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	public class FieldWorkObjectContextGenerator : IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var rtElement = XElement.Parse(mergeElement);
			var label = rtElement.Attribute("class").Value + ": " + rtElement.Attribute("guid").Value;
			return new ContextDescriptor(label, null);
		}
	}
}