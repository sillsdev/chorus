using System;
using System.Xml;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders
{
	public class LiftRangesMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _merger;

		public LiftRangesMergingStrategy(MergeSituation mergeSituation)
		{
			_merger = new XmlMerger(mergeSituation);

			LiftBasicElementStrategiesMethod.AddLiftBasicElementStrategies(_merger.MergeStrategies);
			LiftRangesElementStrategiesMethod.AddLiftRangeElementStrategies(_merger.MergeStrategies);
		}

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _merger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}
	}
}