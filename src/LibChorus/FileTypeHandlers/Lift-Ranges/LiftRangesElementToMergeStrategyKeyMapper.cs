using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Code;

namespace Chorus.FileTypeHandlers
{
	internal class LiftRangesElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
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

			// TODO: Add expected new key finding code, as the lift ranges element strategies get implemented.

			return key;
		}

		#endregion
	}
}