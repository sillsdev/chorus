using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftEntryMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _entryMerger;

		public LiftEntryMergingStrategy(MergeSituation mergeSituation)
		{
			_entryMerger = new XmlMerger(mergeSituation)
							{
								MergeStrategies = {KeyFinder = new LiftKeyFinder()}
							};

			LiftElementStrategiesMethod.AddLiftElementStrategies(_entryMerger.MergeStrategies);
		}

		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry).OuterXml;
		}
	}
}