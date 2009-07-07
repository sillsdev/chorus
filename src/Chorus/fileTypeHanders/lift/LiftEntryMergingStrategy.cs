using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public class LiftEntryMergingStrategy : IMergeStrategy
	{
		private XmlMerger _entryMerger;

		public LiftEntryMergingStrategy(MergeSituation mergeSituation)
		{
			_entryMerger = new XmlMerger(mergeSituation);

			//now customize the XmlMerger with LIFT-specific info

			var elementStrategy =AddKeyedElementType("entry", "id", false);
			elementStrategy.ContextDescriptorGenerator = new LexEntryContextGenerator();

			AddKeyedElementType("sense", "id", true);
			AddKeyedElementType("form", "lang", false);
			AddKeyedElementType("gloss", "lang", false);
			AddKeyedElementType("field", "type", false);

			AddSingletonElementType("text");
			AddSingletonElementType("grammatical-info");
			AddSingletonElementType("lexical-unit" );
			AddSingletonElementType("citation" );
			AddSingletonElementType("definition");
			AddSingletonElementType("label");
			AddSingletonElementType("usage");
			AddSingletonElementType("header");
			AddSingletonElementType("description"); // in header
			AddSingletonElementType("ranges"); // in header
			AddSingletonElementType("fields"); // in header

			//enhance: don't currently have a way of limitting etymology/form to a single instance but not multitext/form

			AddSingletonElementType("main"); //reversal/main

		}

		private ElementStrategy AddKeyedElementType(string name, string attribute, bool orderOfTheseIsRelevant)
		{
			ElementStrategy strategy = new ElementStrategy(orderOfTheseIsRelevant);
			strategy.MergePartnerFinder = new FindByKeyAttribute(attribute);
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		private ElementStrategy AddSingletonElementType(string name)
		{
			ElementStrategy strategy = new ElementStrategy(false);
			strategy.MergePartnerFinder = new FindFirstElementWithSameName();
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			XmlNode n = _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry);
			return n.OuterXml;
		}
	}
}