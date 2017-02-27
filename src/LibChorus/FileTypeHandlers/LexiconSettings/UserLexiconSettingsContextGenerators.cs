using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers.LexiconSettings
{
	// UserLexiconSettings
	// WritingSystems
	//		WritingSystem
	//			LocalKeyboard
	//			KnownKeyboards
	//				KnownKeyboard
	//			DefaultFontName
	//			DefaultFontSize
	//			IsGraphiteEnabled


	/// <summary>
	/// Class responsible for generating (and including as a label in the descriptor) a human-readable description of the context element,
	/// and (through the HtmlDetails method) an HTML representation of a conflicting node that can be diff'd to show the differences.
	/// </summary>
	internal sealed class UserLexiconSettingsContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode
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
			string name = mergeElement.Name;
			string label = "unknown";
			switch (name)
			{
				case "UserLexiconSettings":
					label = "User lexicon settings file";
					break;
				case "WritingSystems":
					label = "Writing Systems element";
					break;
				case "WritingSystem":
					label = "Writing System (WS) element";
					break;
				case "LocalKeyboard":
					label = "WS Local Keyboard element";
					break;
				case "KnownKeyboards":
					label = "WS Known Keyboards element";
					break;
				case "KnownKeyboard":
					label = "WS Known Keyboard element";
					break;
				case "DefaultFontName":
					label = "WS Default Font name element";
					break;
				case "DefaultFontSize":
					label = "WS Default Font size element";
					break;
				case "IsGraphiteEnabled":
					label = "WS Is Graphite enabled element";
					break;
			}

			return new ContextDescriptor(label, "unknown");
		}

		#endregion
	}
}
