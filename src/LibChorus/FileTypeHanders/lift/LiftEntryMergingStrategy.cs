using System.Collections.Generic;
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

			#region Base elements

			// <span
			//		lang='lang' (optional, so how can it really be used as a key to find a match?) NB: UML chart has it as 'string', not 'lang', but 'lang' is better for us
			//		href='URL' (optional, ignore?)
			//		class='string'> (optional)
			//		<span> (optional, multiple)
			// </span>
			// NB: 'lang' is optional
			var elementStrategy = AddKeyedElementType("span", "lang", true); // This may really need the new repeatable key business, that JohnT added, but which is not in this Product branch.
			elementStrategy.AttributesToIgnoreForMerging.Add("href");

			// Mixes text data and <span> elements
			// 'lang' attr from parent <form> element is the defacto same thing for the <text> element, but has no attrs itself.
			// <text>
			AddSingletonElementType("text");

			// <form
			//		lang='lang' (required)
			//		<text /> (Required, sig of <text>)
			//		<annotation /> [Optional, Multiple, sig=annotation]
			// </form>
			AddKeyedElementType("form", "lang", false);

			// Formally 'inherits' from <text>, so has all it has (parent 'lang' attr governs, and other <text> rules on <span>, etc.).
			// Gotchas:
			//	1. There are no occurrences of a span being used to store content except as part of a multitext.
			//	2. text [Optional] If there is only one form the form element itself is optional and a multitext
			//		may consist of a single text node containing the contents of the text.
			//		This means that if there is no form there is no span capability.
			// <multitext		No attrs
			//		<text> [Optional, inherited from <text>].
			//		<form> [Optional, Multiple, sig=form]
			//		<trait> [Optional, Multiple, sig=trait]
			// </multitext>
			// TODO: add some kind of element strategy for <multitext>. If it never is to appear in a file, it may be 'abstract'.
			// Note: If it is 'abstract', then no element strategy is needed.

			// <URLRef
			//		href="URL" [Required, sig=URL]
			//		<label> [Optional, sig=multitext]
			// </URLRef>
			// TODO: add some kind of element strategy for <URLRef>

			// Formal inheritance from <multitext>
			// Curiosity: "Fields are described as part of the header information so that applications can give some descriptive meaning to the information they add to a file."
			// <field
			//		type [Required, sig=key]
			//		dateCreated [Optional, sig=datetime]
			//		dateModified [Optional, sig=datetime]
			//		<text> [Optional, inherited from <text> via <multitext>].
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			//		<annotation> [Optional, Multiple, sig=annotation]
			// !!!!!!!!! HACK ALERT !!!!!!!!!
			// <field> is overloaded, as it also appears in the header's <fields>, but with a different key attribute (tag).
			// 1. Added a new case statement in ElementStrategy-GetKeyViaHack method for the "field" element
			//		A. Change "field" here to "mainfield" as the key in the element strategies
			AddKeyedElementType("mainfield", "type", false);

			// Notes: A trait is simply a reference to a single range-element
			//		in a range. It can be used to give the dialect for a variant or the status of an entry.
			//		The semantics of a trait in a particular context is given by the parent object and also by the range
			//		and range-element being referred to. Where no range is linked the name is informal or resolved by name.
			// <trait
			//		name [Required, sig=key]
			//		value [Required, sig=key]
			//		id [Optional, sig=key] // Note: Gives the particular trait an identifier such that it can be referenced by a sibling element.
			//									The id key only needs to be unique within the parent element, although globale keys may be used.
			//									There is no requirement that the key keeps its value across different versions of the file.
			//		<annotation /> [Optional, Multiple, sig=annotation]
			// </trait>
			// Use multi key business for <trait>
			elementStrategy = new ElementStrategy(false)
								{
									// Need both keys to find this puppy's match.
									MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> {"name", "value"})
								};
			_entryMerger.MergeStrategies.SetStrategy("trait", elementStrategy);

			// <annotation
			//		name [Required, key]
			//		value [Required, key]
			//		who  [Optional, key]
			//		when  [Optional, key]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			// </annotation>
			// Use multi key business for <annotation>
			elementStrategy = new ElementStrategy(false)
			{
				// Need both keys to find this puppy's match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "name", "value" })
			};
			_entryMerger.MergeStrategies.SetStrategy("annotation", elementStrategy);

			#endregion End Base elements

			#region Entry Elements

			// Nothing in here can be used as a key to find a match.
			// <note
			//		type [Optional, sig=key] There is only one note with a given type in any parent element.
			//									Thus translations of the note are held as different forms of the one note.
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			// </note>
			// TODO: Sure hope this is a singleton in the parent!

			// Notes: I (RBR) think this may only be a bundle of attrs and content others can use, rather than an actual element in the file.
			// <extensible
			//		dateCreated [Optional, datetime] // attr in UML
			//		dateModified [Optional, datetime] // attr in UML
			//		<field> [Optional, Multiple, field]
			//		<trait> [Optional, Multiple, trait] // NB: sig changed in text to 'flag', but 'trait' in UML.
			//		<annotation> [Optional, Multiple, annotation]
			// </extensible>
			// TODO: Note: If it is 'abstract', then no element strategy is needed.

			// <etymology
			//		type [Required, sig=key]
			//		source [Required, sig=key] // UML has 'key', doc has 'string'. Go with key
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<gloss> [Optional, Multiple, sig=form]
			//		<form> [Required, sig=form] // UML has Optional
			// </etymology>
			// TODO: use multi-keyed lookup or single. If single, which one?

			// <grammatical-info
			//		value [Required, sig=key] The part of speech tag into the grammatical-info range.
			//		<trait> {Optional, Multiple, sig=trait] Allows grammatical information to have attributes.
			// TODO: Really? Or keyed? Sense contains this as Optional, so singleton will do, while keyed is more future-safe.
			// TODO: Probably bad to use a key, since if a user changes the 'value', then we'd get two of them.
			AddSingletonElementType("grammatical-info");

			// <phonetic
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			//		<media> [Optional, Multiple, sig=URLRef]
			//		Spurious <form>, since it inherits <form> <multitext> // from <form> [Optional, Multiple, sig=span] // UML has it right.
			// </phonetic>
			// Everything is optional, and thus, no key
			// Sure hope it is a singleton in its parent. Alas, it is not a singleton, at least in the <variant> element.
			// TODO: Add el strat.

			// <reversal
			//		type [Optional, sig=key]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			//		<main> [Optional, sig=reversal]		// TODO: Q: Does main exist as an element? A: Yes, but its child single is not <reversal>, but its child nodes.
			//		<grammatical-info> [Optional, sig=grammatical-info]
			// </reversal>
			// TODO: Add el strat.
			/*
<reversal type="en">
	<form lang="en">
		<text>ask for</text>
	</form>
	<grammatical-info value="verb"/>
	<main>
		<form lang="en">
			<text>ask</text>
		</form>
		<grammatical-info value="verb"/>
	</main>
</reversal>
			*/
			AddSingletonElementType("main"); //reversal/main Since it is a singleton, and its children have their own el stats, it is probably all that is needed.

			// A translation is simply a multitext with an optional translation type attribute.
			// <translation
			//		type [Optional, key]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			// </translation>
			// TODO: Can't really be keyed off an optional attr. So, what to use? It is muiltiple in <example>

			/*
<example>
	<form lang="zpi">
		<text>ZPI text</text>
	</form>
	<translation type="Free translation">
		<form lang="en">
			<text>English translation of ZPI text.</text>
		</form>
		<form lang="es">
			<text>Spanish translation of ZPI text.</text>
		</form>
	</translation>
</example>
			*/
			// <example
			//		source [Optional, key] // Not suitable for keyed el strat.
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			//		<translation> [Optional, Multiple, sig=translation]
			// </example>
			// TODO: How on earth can one find the matching example? At least the default will keep them all, for good or ill.
#if MaybeSomeday
			AddExampleSentenceStrategy();
#endif

			// <relation
			//		type [Required, sig=key]
			//		ref [Required, sig=refid]
			//		order [Optional, sig=int]
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<usage> [Optional, sig=multitext]
			// </relation>
			// Use multi-attr key, or single attr key?
			// Go with multiple key, for now.
			elementStrategy = new ElementStrategy(false)
			{
				// Need both keys to find this puppy's match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "type", "ref" })
			};
			_entryMerger.MergeStrategies.SetStrategy("relation", elementStrategy);

			// <variant
			//		ref [Optional, refid] NOTE: Doc: refentry, UML: refid, so go with refid, since there is nothing called refentry. Gives the variation as a reference to another entry or sense rather than specifying the form
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<text> [Optional, sig=text, inherited from <text> via <multitext>]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <multitext>]
			//		<pronunciation> [Optional, Multiple, sig=phonetic]
			//		<relation> [Optional, Multiple, sig=relation]
			// </variant>
			// TODO: Add El Strat, of some kind.

			// <sense
			//		id [Optional, refid] The id is unique across all Senses in the lexicon and all Entries as well.
			//		order [Optional int]
			//		dateCreated [Optional, sig=datetime, inherited from <extensible>]
			//		dateModified [Optional, sig=datetime, inherited from <extensible>]
			//		<field> [Optional, Multiple, sig=field, inherited from <extensible>]
			//		<trait> [Optional, Multiple, sig=trait, inherited from <extensible>]
			//		<annotation> [Optional, Multiple, sig=annotation, inherited from <extensible>]
			//		<grammatical-info> [Optional, grammi] grammi? Beter go with grammatical-info.
			//		<gloss> [Optional, Multiple, form]
			//		<definition> [Optional, multitext]
			// </sense>
			AddKeyedElementType("sense", "id", true);
			AddKeyedElementType("gloss", "lang", false);
			AddSingletonElementType("definition");

			elementStrategy = AddKeyedElementType("entry", "id", false);
			elementStrategy.AttributesToIgnoreForMerging.Add("dateModified");
			elementStrategy.ContextDescriptorGenerator = new LexEntryContextGenerator();

			AddSingletonElementType("lexical-unit");
			AddSingletonElementType("citation");
			AddSingletonElementType("label");
			AddSingletonElementType("usage");

			#endregion End Entry Elements

			#region Header Elements

			AddSingletonElementType("ranges");
			strategy.OrderIsRelevant = false;
			AddSingletonElementType("fields");
			strategy.OrderIsRelevant = false;
			// !!!!!!!!! HACK ALERT !!!!!!!!!
			// <field> is overloaded, as it also appears in the entry/sense <fields>, but with a different key attribute (type).
			//		Change the header "field" to "headerfield" as the key in the element strategies
			AddKeyedElementType("headerfield", "tag", false);
			strategy = AddKeyedElementType("range", "id", false);
			strategy.AttributesToIgnoreForMerging.Add("href");
			// End of header and its contents

			//enhance: don't currently have a way of limiting etymology/form to a single instance but not multitext/form

			/*
	<pronunciation>
	  <form lang="foo">
		<text>bar</text>
	  </form>
	</pronunciation>
			*/

			#endregion End Header Elements

			#region Header From RNG grammar

			// Start of header element and its contents
			// TODO: Add some kind of context generator(s). One can go at this <header> level or XOR, one each can go at the <ranges> or <fields> levels.
			AddSingletonElementType("header");

			elementStrategy = AddSingletonElementType("description"); // I (RBR) wonder what this is? Does it have any child nodes?
			elementStrategy.OrderIsRelevant = false;
			// The <description> element contains "multitext-content",
			// which being interpreted, means 0, or more, <form> elements.
			// <form> should be declared elsewhere.




			#endregion #region Header From RNG grammar
		}

#if MaybeSomeday
		private void AddExampleSentenceStrategy()
		{
			ElementStrategy strategy = new ElementStrategy(true);
			strategy.MergePartnerFinder = new ExampleSentenceFinder();
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
		}
#endif

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
			return _entryMerger.Merge(listener, ourEntry, theirEntry, commonEntry).OuterXml;
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