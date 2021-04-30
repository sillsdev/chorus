using System.Collections.Generic;
using System.Xml;

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
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
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

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			var merger = new XmlMerger(new MergeSituation(null, null, null, null, null, MergeOrder.ConflictHandlingModeChoices.WeWin));
			var def = new ElementStrategy(true)
						{
							MergePartnerFinder = new FindByEqualityOfTree()
						};
			merger.MergeStrategies.SetStrategy("def", def);
			return merger.MergeStrategies;
		}

		public HashSet<string> SuppressIndentingChildren()
		{
			return new HashSet<string>();
		}
	}
}