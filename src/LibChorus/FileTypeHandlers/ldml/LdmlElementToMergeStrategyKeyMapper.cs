using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Code;

namespace Chorus.FileTypeHandlers.ldml
{
	/// <summary>
	/// IKeyFinder implementation that can handle the quirks of in an ldml file.
	/// </summary>
	internal class LdmlElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
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

			var key = element.Name;

			if (key == "special")
			{
				foreach (var attrName in from XmlNode attr in element.Attributes select attr.Name)
				{
					switch (attrName)
					{
						case "xmlns:palaso2":
						case "xmlns:palaso":
						case "xmlns:fw":
						case "xmlns:sil":
							key += "_" + attrName;
							return key;
					}
				}
			}

			return key;
		}

		#endregion
	}
}