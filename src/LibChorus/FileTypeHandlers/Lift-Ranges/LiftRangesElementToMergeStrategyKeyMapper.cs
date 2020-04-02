using System.Collections.Generic;
using System.Xml;
using Chorus.merge.xml.generic;
using SIL.Code;

namespace Chorus.FileTypeHandlers
{
	internal class LiftRangesElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
	{
		#region Implementation of IKeyFinder

		/// <inheritdoc/>
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