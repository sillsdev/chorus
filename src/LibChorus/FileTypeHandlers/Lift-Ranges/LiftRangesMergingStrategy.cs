using System.Collections.Generic;
using System.Xml;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using SIL.Lift;

namespace Chorus.FileTypeHandlers
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
		public LiftRangesMergingStrategy(MergeOrder mergeOrder)
		{
			_merger = new XmlMerger(mergeOrder.MergeSituation)
				{
					EventListener = mergeOrder.EventListener
				};

			LiftBasicElementStrategiesMethod.AddLiftBasicElementStrategies(_merger.MergeStrategies);
			LiftRangesElementStrategiesMethod.AddLiftRangeElementStrategies(_merger.MergeStrategies);
		}

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _merger.Merge(eventListener, ourEntry.ParentNode, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _merger.MergeStrategies.GetElementStrategy(element);
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			return _merger.MergeStrategies;
		}

		/// <summary>
		/// lift-ranges can include multitext elements which potentially can include text elements with spans.
		/// So we need to suppress pretty-printing text elements.
		/// Theoretically, spans can contain nested spans, so suppress pretty-printing those also.
		/// </summary>
		/// <returns></returns>
		public HashSet<string> SuppressIndentingChildren()
		{
			return LiftSorter.LiftSuppressIndentingChildren;
		}
	}
}