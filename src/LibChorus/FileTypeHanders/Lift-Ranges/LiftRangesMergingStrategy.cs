using System.Xml;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// IMergeStrategy implementation for the lift-ranges file.
	/// </summary>
	public class LiftRangesMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor
		/// </summary>
		public LiftRangesMergingStrategy(MergeSituation mergeSituation)
		{
			_merger = new XmlMerger(mergeSituation);

			LiftBasicElementStrategiesMethod.AddLiftBasicElementStrategies(_merger.MergeStrategies);
			LiftRangesElementStrategiesMethod.AddLiftRangeElementStrategies(_merger.MergeStrategies);
		}

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _merger.MergeStrategies.GetElementStrategy(element);
		}
	}
}