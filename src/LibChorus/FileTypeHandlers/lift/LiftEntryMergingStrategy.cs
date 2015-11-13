using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using SIL.Lift;

namespace Chorus.FileTypeHandlers.lift
{
	public class LiftEntryMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _entryMerger;

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public LiftEntryMergingStrategy(MergeOrder mergeOrder)
		{
			_entryMerger = new XmlMerger(mergeOrder.MergeSituation)
							{
								MergeStrategies = {ElementToMergeStrategyKeyMapper = new LiftElementToMergeStrategyKeyMapper()},
								EventListener = mergeOrder.EventListener
							};

			LiftElementStrategiesMethod.AddLiftElementStrategies(_entryMerger.MergeStrategies);
		}

		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _entryMerger.MergeStrategies.GetElementStrategy(element);
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			return _entryMerger.MergeStrategies;
		}

		/// <summary>
		/// We must not pretty-print text elements, even if the only children are spans.
		/// Spans can theoretically nest, so suppress pretty-printing those, too.
		/// </summary>
		/// <returns></returns>
		public HashSet<string> SuppressIndentingChildren()
		{
			return LiftSorter.LiftSuppressIndentingChildren;
		}
	}
}