using System.Collections.Generic;
using System.Xml;
using SIL.Code;

namespace Chorus.merge.xml.generic
{
	internal class DefaultElementToMergeStrategyKeyMapper : IElementToMergeStrategyKeyMapper
	{
		#region Implementation of IKeyFinder

		/// <inheritdoc/>
		public string GetKeyFromElement(HashSet<string> keys, XmlNode element)
		{
			Guard.AgainstNull(element, "Element is null.");

			return element.Name;
		}

		#endregion
	}
}