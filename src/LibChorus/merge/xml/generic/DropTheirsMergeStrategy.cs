using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// This strategy doesn't even try to put the entries together.  It just returns ours.
	/// </summary>
	public class DropTheirsMergeStrategy : IMergeStrategy
	{
		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode unusedCommonEntry)
		{
			return ourEntry.OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			var def = new ElementStrategy(true)
						{
							MergePartnerFinder = new FindByEqualityOfTree()
						};
			return def;
		}
	}
}