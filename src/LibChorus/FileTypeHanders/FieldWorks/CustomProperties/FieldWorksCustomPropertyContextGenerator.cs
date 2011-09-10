using System.Xml.Linq;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks.CustomProperties
{
	/// <summary>
	/// Class that creates a descriptor that can be used later to find the element again, as when reviewing conflict.
	/// </summary>
	public class FieldWorksCustomPropertyContextGenerator : IGenerateContextDescriptor
	{
		#region Implementation of IGenerateContextDescriptor

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var customPropertyElement = XElement.Parse(mergeElement);
// ReSharper disable PossibleNullReferenceException
			var label = customPropertyElement.Attribute("class").Value + ": " + customPropertyElement.Attribute("name").Value;
// ReSharper restore PossibleNullReferenceException
			return new ContextDescriptor(label, "FIXTHIS");
		}

		#endregion
	}
}
