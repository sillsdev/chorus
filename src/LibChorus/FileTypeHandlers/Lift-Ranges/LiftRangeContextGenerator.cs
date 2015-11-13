using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.lift
{
	public class LiftRangeContextGenerator : IGenerateContextDescriptor
	{
		#region Implementation of IGenerateContextDescriptor

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			var label = doc.SelectTextPortion("range");
			return new ContextDescriptor(label, "unknown");
		}

		#endregion
	}
}