using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers.LexiconSettings
{
	// ProjectLexiconSettings
	// WritingSystems
	//		WritingSystem
	//			Abbreviation
	//			LanguageName
	//			ScriptName
	//			RegionName
	//			SpellCheckingId
	//			LegacyMapping
	//			Keyboard
	//			SystemCollation


	/// <summary>
	/// Class responsible for generating (and including as a label in the descriptor) a human-readable description of the context element,
	/// and (through the HtmlDetails method) an HTML representation of a conflicting node that can be diff'd to show the differences.
	/// </summary>
	internal sealed class ProjectLexiconSettingsContextGenerator : IGenerateContextDescriptor, IGenerateContextDescriptorFromNode
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
				case "ProjectLexiconSettings":
					label = "Project lexicon settings file";
					break;
				case "WritingSystems":
					label = "Writing Systems element";
					break;
				case "WritingSystem":
					label = "Writing System (WS) element";
					break;
				case "Abbreviation":
					label = "WS Abbreviation element";
					break;
				case "LanguageName":
					label = "WS Language name element";
					break;
				case "ScriptName":
					label = "WS Script name element";
					break;
				case "RegionName":
					label = "WS Region name element";
					break;
				case "LegacyMapping":
					label = "WS Legacy mapping element";
					break;
				case "Keyboard":
					label = "WS Keyboard element";
					break;
				case "SpellCheckingId":
					label = "WS Spellchecking Id element";
					break;
				case "SystemCollation":
					label = "WS System collation element";
					break;
			}

			return new ContextDescriptor(label, "unknown");
		}

		#endregion
	}
}