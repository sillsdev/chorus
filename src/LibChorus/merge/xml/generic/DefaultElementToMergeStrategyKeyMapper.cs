using System;
using System.Collections.Generic;
using System.Xml;
using Palaso.Code;

namespace Chorus.merge.xml.generic
{
	internal class DefaultElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
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
			Guard.AgainstNull(element, "Element is null.");

			return element.Name;
		}

		#endregion
	}
}