using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftEntryMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _entryMerger;

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
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

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _entryMerger.MergeStrategies.GetElementStrategy(element);
		}
	}
}