using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Code;

namespace Chorus.FileTypeHandlers.lift
{
	internal class LiftElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
	{
		#region Implementation of IKeyFinder

		/// <summary>
		/// Get key to use to find ElementStrategy in the collection held by MergeStrategies
		/// </summary>
		/// <param name="keys">The keys in MergeStrategies dictionary.</param>
		/// <param name="element">The element currently being processed, that the key if needed for.</param>
		/// <returns>The key in the MergeStrategies disctionary that is used to look up the ElementStrategy.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <param name="element" /> is null.</exception>
		public string GetKeyFromElement(HashSet<string> keys, XmlNode element)
		{
			Guard.AgainstNull(keys, "keys is null.");
			Guard.AgainstNull(element, "Element is null.");

			return element.Name == "field"
					? (element.Attributes["type"] == null
						? "headerfield" // Fetch the strategy for the header "field" with its 'tag' key attr.
						: "mainfield") // Fetch the strategy for the main data (entry or sense) "field" with its 'type' key attr.
					: element.Name;
		}

		#endregion
	}
}