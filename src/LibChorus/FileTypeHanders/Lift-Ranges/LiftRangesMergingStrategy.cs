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
		/// IMergeStrategy method
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}
	}
}