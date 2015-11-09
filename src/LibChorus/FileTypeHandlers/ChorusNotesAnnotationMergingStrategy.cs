using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers
{
	public class ChorusNotesAnnotationMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _annotationMerger;

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public ChorusNotesAnnotationMergingStrategy(MergeOrder order)
		{
			_annotationMerger = new XmlMerger(order.MergeSituation)
				{
					EventListener = order.EventListener
				};

			SetupElementStrategies();
		}

		private void SetupElementStrategies()
		{
			_annotationMerger.MergeStrategies.SetStrategy("annotation", ElementStrategy.CreateForKeyedElement("guid", false));
			var messageStrategy = ElementStrategy.CreateForKeyedElement("guid", false);
			messageStrategy.IsImmutable = true;
			_annotationMerger.MergeStrategies.SetStrategy("message", messageStrategy);
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _annotationMerger.Merge(eventListener, ourEntry, theirEntry, commonEntry).OuterXml;
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			return _annotationMerger.MergeStrategies.GetElementStrategy(element);
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			return _annotationMerger.MergeStrategies;
		}

		public HashSet<string> SuppressIndentingChildren()
		{
			return new HashSet<string>();
		}

		#endregion
	}
}