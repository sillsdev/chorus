using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers.ldml
{
	// identity
	// collations
	//		collation
	// special_xmlns:palaso
	// special_xmlns:palaso2
	//		palaso2:knownKeyboards
	//			palaso2:keyboard
	//		palaso2:version
	// special_xmlns:fw
	// special_xmlns:sil
	//		sil:kbd
	//		sil:font


	/// <summary>
	/// Class that creates a descriptor that can be used later to find the 'identity' element again, as when reviewing conflict.
	/// Also responsible for generating (and including as a label in the descriptor) a human-readable description of the context element,
	/// and (through the HtmlDetails method) an HTML representation of a conflicting node that can be diff'd to show the differences.
	/// </summary>
	internal sealed class LdmlContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode
	{
		#region Implementation of IGenerateContextDescriptor

		public ContextDescriptor GenerateContextDescriptor(string mergeElement, string filePath)
		{
			var doc = new XmlDocument();
			doc.LoadXml(mergeElement);
			return GenerateContextDescriptor(doc.DocumentElement, filePath);
		}

		#endregion

		#region Implementation of IGenerateContextDescriptorFromNode

		public ContextDescriptor GenerateContextDescriptor(XmlNode mergeElement, string filePath)
		{
			var name = mergeElement.Name;
			var label = "unknown";
			switch (name)
			{
				case "ldml":
					label = "LDML file";
					break;
				case "collations":
					label = "LDML 'collations' element";
					break;
				case "collation":
					label = "LDML 'collation' element";
					break;
				case "special_xmlns:palaso":
					label = "LDML Palaso 'special' element";
					break;
				case "special_xmlns:palaso2":
					label = "LDML Palaso2 'special' element";
					break;
				case "palaso2:knownKeyboards":
					label = "LDML Palaso2 'known keyboards";
					break;
				case "palaso2:keyboard":
					label = "LDML Palaso2 'keyboard'";
					break;
				case "palaso2:version":
					label = "LDML Palaso2 'version'";
					break;
				case "special_xmlns:fw":
					label = "LDML FieldWorks 'special' element";
					break;
				case "special_xmlns:sil":
					label = "LDML SIL 'special' element";
					break;
			}

			return new ContextDescriptor(label, "unknown");
		}

		#endregion
	}
}