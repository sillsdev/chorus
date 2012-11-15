using System.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This is a really high level approach.
	/// </summary>
	public interface IMergeStrategy
	{
		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry);

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		ElementStrategy GetElementStrategy(XmlNode element);

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		MergeStrategies GetStrategies();
	}
}