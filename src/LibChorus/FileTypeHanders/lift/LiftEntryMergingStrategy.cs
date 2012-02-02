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

			var elementStrategy = AddKeyedElementType("entry", "id", false);
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");

			elementStrategy.ContextDescriptorGenerator = new LexEntryContextGenerator();

			AddKeyedElementType("sense", "id", true);
			AddKeyedElementType("form", "lang", false);
			AddKeyedElementType("gloss", "lang", false);
			AddKeyedElementType("field", "type", false);

			AddExampleSentenceStrategy();

			AddSingletonElementType("text");
			AddSingletonElementType("grammatical-info");
			AddSingletonElementType("lexical-unit" );
			AddSingletonElementType("citation" );
			AddSingletonElementType("definition");
			AddSingletonElementType("label");
			AddSingletonElementType("usage");
			AddSingletonElementType("header");
			var strategy = AddSingletonElementType("description"); // in header
			strategy.OrderIsRelevant = false; // Order may well be important, in the end, but for now, I (RBR) have no idea what is to be in the header.
			AddSingletonElementType("ranges"); // in header
			strategy.OrderIsRelevant = false; // Order may well be important, in the end, but for now, I (RBR) have no idea what is to be in the header.
			AddSingletonElementType("fields"); // in header
			strategy.OrderIsRelevant = false; // Order may well be important, in the end, but for now, I (RBR) have no idea what is to be in the header.

			//enhance: don't currently have a way of limitting etymology/form to a single instance but not multitext/form

			AddSingletonElementType("main"); //reversal/main

		}

		private void AddExampleSentenceStrategy()
		{
			#if MaybeSomeday
			ElementStrategy strategy = new ElementStrategy(true);
			strategy.MergePartnerFinder = new ExampleSentenceFinder();
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			#endif
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

#if MaybeSomeday
	public class ExampleSentenceFinder : IFindNodeToMerge//, IFindPossibleNodeToMerge
	{
//        public XmlNode GetPossibleNodeToMerge(XmlNode nodeToMatch, List<XmlNode> possibleMatches)
//        {
//
//        }

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null)
				return null;

			var ourForms = nodeToMatch.SafeSelectNodes("example/form");

			foreach (XmlNode example in parentToSearchIn.SafeSelectNodes("example"))
			{
			   XmlNodeList forms = example.SafeSelectNodes("form");
			   if(!SameForms(forms, ourForms))
				   continue;

				return example;
			}

			return null; //couldn't find a match

		}

		private bool SameForms(XmlNodeList list1, XmlNodeList list2)
		{
			if (list1.Count != list2.Count)
				return false; //enhance... this is giving up to easily

			foreach (XmlNode form in list1)
			{
				var lang = form.GetStringAttribute("lang");
				foreach (XmlNode form2 in list2)
				{
					if (form2.GetStringAttribute("lang")!=lang)
						continue;
					if (form2.InnerText != form.InnerText)
						return false;// they differ

					}
				var x = example.SafeSelectNodes("form[@lang='{0}'", form.GetStringAttribute("lang"));
				if (x.Count == 0)
					break;
			}

		}

	}
#endif
}