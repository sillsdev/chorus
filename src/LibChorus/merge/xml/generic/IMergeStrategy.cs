using System.Collections.Generic;
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

		/// <summary>
		/// This provides information for the post-merge step of pretty-printing the XML in a way that minimizes spurious
		/// conflicts. It provides a list of elements whose children should not be indented, typically because the extra
		/// white space would be significant and cause problems. The pretty-printer automatically suppresses indentation
		/// for nodes that have at least one text child, but this allows it to be further suppressed for nodes where
		/// white space should not be added, even if the children are all elements (e.g., a text node where the whole content
		/// is a span).
		/// </summary>
		/// <returns></returns>
		HashSet<string> SuppressIndentingChildren();

		/// <summary>
		/// This gives the merge process the information it needs to decide if an edit is serious enough to cause a conflict
		/// (for now this is used in edit vs delete situations to decide whether or not to keep the edit).
		/// </summary>
		/// <param name="commonAncestor">This is the common ancestor node.</param>
		/// <param name="survivor">This can be either ours or theirs, depending on the caller</param>
		/// <returns>'true' if the client should create a conflict report, otherwise 'false'.</returns>
		/// <remarks>
		/// This method was added in the context of the edit vs delete methods, so only one 'survivor' node is given.
		/// It could be enhanced for more general use, but then all three nodes needs parameters, evn if the 'loser' is null.
		/// </remarks>
		bool ShouldCreateConflictReport(XmlNode commonAncestor, XmlNode survivor);
	}
}