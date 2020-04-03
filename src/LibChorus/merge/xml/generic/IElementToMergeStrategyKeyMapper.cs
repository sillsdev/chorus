using System;
using System.Collections.Generic;
using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Interface that domains can implement, if they have any special keys they have used in populating the MergeStrategies collection of ElementStrategy instances.
	///
	/// Implementers of this interface need to add the implementation to the KeyFinder property of MergeStrategies, when they set up its ElementStrategies.
	/// The implementation will then be called, so the domain can sort out the key to use from the given XmlNode element.
	/// </summary>
	public interface IElementToMergeStrategyKeyMapper
	{
		/// <summary>
		/// Get key to use to find ElementStrategy in the collection held by MergeStrategies
		/// </summary>
		/// <param name="keys">The keys in MergeStrategies dictionary.</param>
		/// <param name="element">The element currently being processed, that the key if needed for.</param>
		/// <returns>The key in the MergeStrategies dictionary that is used to look up the ElementStrategy.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <c>element</c> is null.</exception>
		string GetKeyFromElement(HashSet<string> keys, XmlNode element);
	}
}