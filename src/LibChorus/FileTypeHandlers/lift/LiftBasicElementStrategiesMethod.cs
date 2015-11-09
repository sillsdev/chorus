using System.Collections.Generic;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers.lift
{
	internal static class LiftBasicElementStrategiesMethod
	{
		/// <summary>
		/// Add all of the Lift range related ElementStrategy instacnes suitable for use in the lift file, and the lift-ranges file.
		///
		/// This will likely over-populate mergeStrategies with strategies for the lift file, but no matter.
		/// </summary>
		/// <remarks>
		/// NB: There are more element strategies needed to support ranges, but they are expected to be defined elsewhere, as they are common to other non-range elements.
		/// Examples: "description" and "form" elements.
		/// </remarks>
		internal static void AddLiftBasicElementStrategies(MergeStrategies mergeStrategies)
		{
			// ******************************* <form> **************************************************
			// <form
			//		lang='lang' (required)
			AddKeyedElementType(mergeStrategies, "form", "lang", false);
			//		<text> (Required, sig of <text>)
			//		<annotation> [Optional, Multiple, sig=annotation]
			// </form>
			// ******************************* </form> **************************************************

			// ******************************* <text> **************************************************
			// Mixes text data and <span> elements
			// 'lang' attr from parent <form> element is the defacto same thing for the <text> element, but has no attrs itself.
			// <text>
			var textStrategy = AddSingletonElementType(mergeStrategies, "text");
			textStrategy.IsAtomic = true; // don't attempt merge within text elements if they have non-text-node children, e.g., <span>
			// but we can do text-level merging if there are no spans; this allows text editing conflicts to be reported
			// and multiple text nodes which amount to the same inner text to be ignored.
			textStrategy.AllowAtomicTextMerge = true;
			// </text>
			// ******************************* </text> **************************************************

			// ******************************* <span> **************************************************
			// <span
			//		lang='lang' (optional, so how can it really be used as a key to find a match?) NB: UML chart has it as 'string', not 'lang', but 'lang' is better for us
			var elementStrategy = ElementStrategy.CreateForKeyedElementInList("lang");
			//		href='URL' (optional, ignore?)
			elementStrategy.AttributesToIgnoreForMerging.Add("href");
			//		class='string'> (optional)
			//		<span> (optional, multiple)
			// </span>
			mergeStrategies.SetStrategy("span", elementStrategy);
			// ******************************* </span> **************************************************

			// ******************************* <multitext> **************************************************
			// Formally 'inherits' from <text>, so has all it has (parent 'lang' attr governs, and other <text> rules on <span>, etc.).
			// Gotchas:
			//	1. There are no occurrences of a span being used to store content except as part of a multitext.
			//	2. text [Optional] If there is only one form the form element itself is optional and a multitext
			//		may consist of a single text node containing the contents of the text.
			//		This means that if there is no form there is no span capability.
			// <multitext		No attrs
			//		<form> [Optional, Multiple, sig=form]
			// </multitext>
			// Note: If it is 'abstract', then no element strategy is needed.
			// ******************************* </multitext> **************************************************

			// ******************************* <URLRef> **************************************************
			// <URLRef> element never occurs in the wild, as it always gets a name change.
			// <URLRef
			//		href="URL" [Required, sig=URL]
			//		<label> [Optional, sig=multitext]
			AddSingletonElementType(mergeStrategies, "label");
			// </URLRef>
			// ******************************* </URLRef> **************************************************

			// ******************************* <annotation> **************************************************
			// <annotation
			//		name [Required, key]
			//		value [Required, key]
			elementStrategy = new ElementStrategy(false)
			{
				// Need both keys to find the match.
				MergePartnerFinder = new FindByMultipleKeyAttributes(new List<string> { "name", "value" })
			};
			mergeStrategies.SetStrategy("annotation", elementStrategy);
			//		who  [Optional, key]
			//		when  [Optional, key]
			//		<form> [Optional, Multiple, sig=form, inherited from <multitext>]
			// </annotation>
			// ******************************* </annotation> **************************************************

			// Shared with <header> in lift file, <range-element> and <range> in lift and lift-ranges files.
			elementStrategy = AddSingletonElementType(mergeStrategies, "description");
			elementStrategy.OrderIsRelevant = false;
		}

		internal static ElementStrategy AddKeyedElementType(MergeStrategies mergeStrategies, string name, string attribute, bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = new FindByKeyAttribute(attribute)
							};
			mergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		internal static ElementStrategy AddSingletonElementType(MergeStrategies mergeStrategies, string name)
		{
			var strategy = new ElementStrategy(false)
							{
								MergePartnerFinder = new FindFirstElementWithSameName()
							};
			mergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}
	}
}