using System;
using System.Collections.Generic;
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

			#region Base elements

			LiftBasicElementStrategiesMethod.AddLiftBasicElementStrategies(_entryMerger.MergeStrategies);

			#endregion End Base elements

			#region Entry Elements
			// ******************************* <extensible> **************************************************
			// Notes: This is only be a bundle of attrs and content others can use, rather than an actual element in the file.
			// <extensible
			//		dateCreated [Optional, datetime] // attr in UML
			//		dateModified [Optional, datetime] // attr in UML
			//		<field> [Optional, Multiple, field]
			//		<trait> [Optional, Multiple, trait] // NB: sig changed in text to 'flag', but 'trait' in UML.
			//		<annotation> [Optional, Multiple, annotation]
			// </extensible>
			// It is 'abstract', so no element strategy is needed.
			// ******************************* </extensible> **************************************************

			// ******************************* <field> **************************************************
			// Technically part of Base, but I don't know why, as the <field> elements appear in entries (or maybe senses).
			// Formal inheritance from <multitext>, and partial from <extensible> (all but <field>).
			// Observation: <field> inherits everything, except its 'type' attribute.
			// <field
			//		type [Required, sig=key]
			// !!!!!!!!! HACK ALERT !!!!!!!!!
			var elementStrategy = AddKeyedElementType("mainfield", "type", false);
			// !!!!!!!!! END HACK ALERT !!!!!!!!!
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<trait> [Optional, Multiple, trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			// </field>
			// ******************************* </field> **************************************************

			// ******************************* <trait> **************************************************
			// Notes: A trait is simply a reference to a single range-element
			//		in a range. It can be used to give the dialect for a variant or the status of an entry.
			//		The semantics of a trait in a particular context is given by the parent object and also by the range
			//		and range-element being referred to. Where no range is linked the name is informal or resolved by name.
			// <trait
			//		name [Required, sig=key]
			//		value [Required, sig=key]
			elementStrategy = new ElementStrategy(false)
			{
				// Need both keys to find the match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "name", "value" })
			};
			_entryMerger.MergeStrategies.SetStrategy("trait", elementStrategy);
			//		id [Optional, sig=key] // Note: Gives the particular trait an identifier such that it can be referenced by a sibling element.
			//									The id key only needs to be unique within the parent element, although globale keys may be used.
			//									There is no requirement that the key keeps its value across different versions of the file.
			//		<annotation /> [Optional, Multiple, sig=annotation]
			// </trait>
			// ******************************* </trait> **************************************************

			// ******************************* <note> **************************************************
			// <note
			//		type [Optional, sig=key] There is only one note with a given type in any parent element.
			//									Thus translations of the note are held as different forms of the one note.
			elementStrategy = AddKeyedElementType("note", "type", false);
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			// </note>
			// ******************************* </note> **************************************************

			// ******************************* <relation> **************************************************
			// <relation
			//		type [Required, sig=key]
			//		ref [Required, sig=refid]
			elementStrategy = new ElementStrategy(false)
			{
				// Need both keys to find this puppy's match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "type", "ref" })
			};
			_entryMerger.MergeStrategies.SetStrategy("relation", elementStrategy);
			//		order [Optional, sig=int]
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<usage> [Optional, sig=multitext]
			AddSingletonElementType("usage");
			// </relation>
			// ******************************* </relation> **************************************************

			#region Entry

			// ******************************* <entry> **************************************************
			// <entry
			//		id [Optional, refid] This gives a unique identifier to this Entry. Notice that this is unique across all Entrys and **all Senses**.
			elementStrategy = AddKeyedElementType("entry", "id", false);
			elementStrategy.ContextDescriptorGenerator = new LexEntryContextGenerator();
			//		order [Optional, int]
			//		guid [Optional, string]
			//		dateDeleted [Optional, datetime]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateDeleted"); // One might think it is immutable, but it might not be true.
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<lexical-unit> [Optional, multitext]
			AddSingletonElementType("lexical-unit");
			//		<citation> [Optional, multitext]
			AddSingletonElementType("citation");
			//		<pronunciation> [Optional, Multiple, phonetic] NAME OVERRIDE
			AddPronunciationStrategy();
			//		<variant> [Optional, Multiple, variant] dealt with below
			//		<sense> [Optional, Multiple, Sense]
			//		<note> [Optional, Multiple, note]
			//		<relation> [Optional, Multiple, relation]
			//		<etymology> [Optional, Multiple, etymology]
			// </entry>
			// ******************************* </entry> **************************************************

			// ******************************* <variant> **************************************************
			// <variant
			//		ref [Optional, refid] NOTE: Doc: refentry, UML: refid, so go with refid, since there is nothing called refentry. Gives the variation as a reference to another entry or sense rather than specifying the form
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<pronunciation> [Optional, Multiple, sig=phonetic]
			//		<relation> [Optional, Multiple, sig=relation]
			// </variant>
			AddVariantStrategy();
			// ******************************* </variant> **************************************************

			// ******************************* <phonetic> **************************************************
			// <phonetic
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>] ignored in phonetic strategy
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<media> [Optional, Multiple, sig=URLRef] NAME OVERRIDE
			AddPhoneticStrategy();
			AddKeyedElementType("media", "href", false);
			// </phonetic>
			// No suitable key attr(s)
			// The default Element Strategy will be used, for good, or ill.
			// ******************************* </phonetic> **************************************************

			// ******************************* <etymology> **************************************************
			// <etymology
			//		type [Required, sig=key]
			//		source [Required, sig=key] // UML has 'key', doc has 'string'. Go with key
			elementStrategy = new ElementStrategy(false)
			{
				// SteveMc says to use them both.
				// Need both keys to find the match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "type", "source" })
			};
			_entryMerger.MergeStrategies.SetStrategy("etymology", elementStrategy);
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<gloss> [Optional, Multiple, sig=form]
			//		<form> [Required, sig=form] // UML has Optional
			// </etymology>
			// ******************************* </etymology> **************************************************

			#endregion Entry

			#region Sense

			// ******************************* <sense> **************************************************
			// <sense
			//		id [Optional, refid] The id is unique across all Senses in the lexicon and all Entries as well.
			elementStrategy = AddKeyedElementType("sense", "id", true);// main sense and nested senses, according to doc
			//		order [Optional int]
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<grammatical-info> [Optional, grammi] grammi? Better go with grammatical-info. (Added below)
			//		<gloss> [Optional, Multiple, form]
			AddKeyedElementType("gloss", "lang", false);
			//		<definition> [Optional, multitext]
			AddSingletonElementType("definition");
			//		<relation> [Optional, Multiple, relation] (Added below)
			//		<note> [Optional, Multiple, note] (Added below)
			//		<example> [Optional, Multiple, example] (Must use default element strategy, or some other hand-made one.)
			//		<reversal> [Optional, Multiple, reversal] (Added below)
			//		<illustration> [Optional, Multiple, URLref] NAME OVERRIDE
			AddKeyedElementType("illustration", "href", false);
			//		<subsense> [Optional, Multiple, sense] NAME OVERRIDE
			AddKeyedElementType("subsense", "id", true); // nested sense in a <sense>, according to rng
			// </sense>
			// ******************************* </sense> **************************************************

			// ******************************* <reversal> **************************************************
			// <reversal
			//		type [Optional, sig=key]
			AddReversalStrategy();
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<main> [Optional, sig=reversal] NAME OVERRIDE
			AddSingletonElementType("main");
			//		<grammatical-info> [Optional, sig=grammatical-info] (Added elsewhere)
			// </reversal>
			// ******************************* </reversal> **************************************************

			// ******************************* <grammatical-info> **************************************************
			// <grammatical-info
			//		value [Required, sig=key] The part of speech tag into the grammatical-info range.
			//		<trait> {Optional, Multiple, sig=trait] Allows grammatical information to have attributes.
			// </grammatical-info>
			// Sense and reversal have this as Optional, so singleton will do, while keyed is more future-safe.
			// It may bad to use a key, since if a user changes the 'value', then we'd get two of them.
			AddSingletonElementType("grammatical-info");
			// ******************************* </grammatical-info> **************************************************

			// ******************************* <example> **************************************************
			// <example
			AddExampleSentenceStrategy();
			// 'dateModified' is ignored in AddExampleSentenceStrategy
			//		source [Optional, key] // Not suitable for keyed el strat.
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<translation> [Optional, Multiple, sig=translation]
			// </example>
			// ******************************* </example> **************************************************

			// ******************************* <translation> **************************************************
			// A translation is simply a multitext with an optional translation type attribute.
			// <translation
			//		type [Optional, key]
			AddTranslationStrategy();
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			// </translation>
			// ******************************* </translation> **************************************************

			#endregion Sense

			#endregion End Entry Elements

			#region Header Elements

			//enhance: don't currently have a way of limiting etymology/form to a single instance but not multitext/form

			// ******************************* <lift> **************************************************
			// Added to complete the whole file, but it gets no ElementStrategy.
			// <lift
			//		version [Required, (string?)]
			//		producer [Optional, string]
			//		<header> [Optional, header]
			//		<entry> [Optional, Multiple, Entry]
			// </lift>
			// ******************************* <lift> **************************************************

			// ******************************* <header> **************************************************
			// TODO: Add some kind of context generator(s). One can go at this <header> level XOR one each can go at the <ranges> or <fields> levels.
			// <header
			AddSingletonElementType("header");
			//		<description> [Optional, multitext] NAME OVERRIDE (Declared in [LiftBasicElementStrategiesMethod], as it is shared here and in the lift-ranges file.)
			//		<ranges> [Optional, ranges]
			elementStrategy = AddSingletonElementType("ranges");
			elementStrategy.OrderIsRelevant = false;
			//		<fields> [Optional, field-defns] NAME OVERRIDE
			elementStrategy = AddSingletonElementType("fields");
			elementStrategy.OrderIsRelevant = false;
			// </header>
			// ******************************* </header> **************************************************

			// ******************************* <ranges> **************************************************
			// <ranges
			//		<range> [Optional, Multiple, range-ref] NAME OVERRIDE
			// </ranges>
			// ******************************* </ranges> **************************************************

			// ******************************* <field-defns> **************************************************
			// <field-defns
			//		<field> [Optional, Multiple, field-defn] NAME OVERRIDE
			// </field-defns>
			// ******************************* </field-defns> **************************************************

			// ******************************* <field-defn> **************************************************
			// <field-defn> element never occurs in the wild (0.13), as it always gets a name change to <field>.
			// <field-defn (aka <field> in 0.13)
			//		tag [Required, key]
			// !!!!!!!!! HACK ALERT !!!!!!!!!
			AddKeyedElementType("headerfield", "tag", false);
			// !!!!!!!!! END HACK ALERT !!!!!!!!!
			//		<form> [optional, multiple]
			// </field-defn>
			// ******************************* </field-defn> **************************************************

			LiftRangesElementStrategiesMethod.AddLiftRangeElementStrategies(_entryMerger.MergeStrategies);

			#endregion #region Header Elements
		}

		private void AddReversalStrategy()
		{
			// No. There can be multiple ones with the same 'type', AddKeyedElementType("reversal", "type", true);
			var strategy = new ElementStrategy(true);
			strategy.MergePartnerFinder = new FormMatchingFinder();
			_entryMerger.MergeStrategies.SetStrategy("reversal", strategy);
			//elementStrategy.MergePartnerFinder = new FindByKeyAttributeInList();
		}

		private void AddTranslationStrategy()
		{
			var strategy = new ElementStrategy(true);
			strategy.MergePartnerFinder = new OptionalKeyAttrFinder("type", new FormMatchingFinder());
			_entryMerger.MergeStrategies.SetStrategy("translation", strategy);
		}

		private void AddPhoneticStrategy()
		{
			var strategy = new ElementStrategy(true);
			strategy.AttributesToIgnoreForMerging.Add("dateModified");
			strategy.MergePartnerFinder = new FormMatchingFinder();
			_entryMerger.MergeStrategies.SetStrategy("phonetic", strategy);
		}

		private void AddPronunciationStrategy()
		{
			var strategy = new ElementStrategy(true);
			strategy.AttributesToIgnoreForMerging.Add("dateModified");
			strategy.MergePartnerFinder = new FormMatchingFinder();
			_entryMerger.MergeStrategies.SetStrategy("pronunciation", strategy);
		}

		private void AddVariantStrategy()
		{
			var strategy = new ElementStrategy(true);
			strategy.AttributesToIgnoreForMerging.Add("dateModified");
			strategy.MergePartnerFinder = new OptionalKeyAttrFinder("ref", new FormMatchingFinder());
			_entryMerger.MergeStrategies.SetStrategy("variant", strategy);
		}

		private void AddExampleSentenceStrategy()
		{
			var strategy = new ElementStrategy(true);
			strategy.AttributesToIgnoreForMerging.Add("dateModified");
			strategy.MergePartnerFinder = new OptionalKeyAttrFinder("source", new FormMatchingFinder());
			_entryMerger.MergeStrategies.SetStrategy("example", strategy);
		}

		private ElementStrategy AddKeyedElementType(string name, string attribute, bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = new FindByKeyAttribute(attribute)
							};
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		private ElementStrategy AddSingletonElementType(string name)
		{
			var strategy = new ElementStrategy(false)
							{
								MergePartnerFinder = new FindFirstElementWithSameName()
							};
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		public string MakeMergedEntry(IMergeEventListener listener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry).OuterXml;
		}
	}

	/// <summary>
	/// Custom finder for matching example sentences by form.
	/// </summary>
	public class FormMatchingFinder : IFindNodeToMerge
	{
		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (nodeToMatch == null || parentToSearchIn == null)
				return null;
			var nodeName = nodeToMatch.LocalName;
			var ourForms = nodeToMatch.SafeSelectNodes(nodeName + "/form");

			foreach (XmlNode example in parentToSearchIn.SafeSelectNodes(nodeName))
			{
			   XmlNodeList forms = example.SafeSelectNodes("form");
			   if(!SameForms(example, forms, ourForms))
				   continue;

				return example;
			}

			return null; //couldn't find a match

		}

		private bool SameForms(XmlNode example, XmlNodeList list1, XmlNodeList list2)
		{
			foreach (XmlNode form in list1)
			{
				var x = example.SafeSelectNodes("form[@lang='{0}']", form.GetStringAttribute("lang"));
				if (x.Count == 0)
					break;
				var lang = form.GetStringAttribute("lang");
				foreach (XmlNode form2 in list2)
				{
					if (form2.GetStringAttribute("lang") != lang)
						continue;
					if (form2.InnerText != form.InnerText)
						return false; // they differ
				}
			}
			return true;
		}
	}

	/// <summary>
	/// This class should be used when there is an optional attribute that would provide our match,
	/// but some other finder strategy is needed when it isn't there.
	/// </summary>
	public class OptionalKeyAttrFinder : IFindNodeToMerge
	{
		private string _key;
		private IFindNodeToMerge _backup;

		public OptionalKeyAttrFinder(string key, IFindNodeToMerge backupFinder)
		{
			_key = key;
			_backup = backupFinder;
		}

		public XmlNode GetNodeToMerge(XmlNode nodeToMatch, XmlNode parentToSearchIn)
		{
			if (parentToSearchIn == null || nodeToMatch == null)
				return null;
			var keyVal = nodeToMatch.Attributes[_key];
			if( keyVal != null)
			{
				var match = parentToSearchIn.SelectSingleNode(nodeToMatch.LocalName + "[" + keyVal + "]");
				if (match != null)
					return match;
			}

			return _backup.GetNodeToMerge(nodeToMatch, parentToSearchIn);

		}
	}
}