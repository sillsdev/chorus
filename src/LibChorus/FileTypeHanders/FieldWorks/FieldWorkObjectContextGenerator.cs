using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// </summary>
	public class FieldWorkObjectContextGenerator : IGenerateContextDescriptor
	{
		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var rtElement = XElement.Parse(mergeElement);
			var label = rtElement.Attribute("class").Value + ": " + rtElement.Attribute("guid").Value;
			return new ContextDescriptor(label, "FIXTHIS");
		}
	}
}