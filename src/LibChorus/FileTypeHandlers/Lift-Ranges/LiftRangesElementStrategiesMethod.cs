using Chorus.FileTypeHandlers.lift;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHandlers
{
	internal static class LiftRangesElementStrategiesMethod
	{
		/// <summary>
		/// Add all of the Lift range related ElementStrategy instances suitable for use in the lift file, and the lift-ranges file.
		/// </summary>
		/// <remarks>
		/// NB: There are more element strategies needed to support ranges, but they are expected to be defined elsewhere, as they are common to other non-range elements.
		/// Examples: "description" and "form" elements.
		/// </remarks>
		internal static void AddLiftRangeElementStrategies(MergeStrategies mergeStrategies)
		{
			// ******************************* <lift-ranges> **************************************************
			// The root element of the Lift Ranges file type.
			// <lift-ranges
			//		<range> [Required, Multiple, range]
			// </lift-ranges>
			// ******************************* </lift-ranges> **************************************************

			// ******************************* <range> **************************************************
			// <range
			//		id [Required, key]
			var elementStrategy = LiftBasicElementStrategiesMethod.AddKeyedElementType(mergeStrategies, "range", "id", false);
			elementStrategy.ContextDescriptorGenerator = new LiftRangeContextGenerator();
			//		guid [Optional, string]
			//		href [Optional, URL]
			elementStrategy.AttributesToIgnoreForMerging.Add("href");
			//		<description> [Optional, multitext] <description> holds zero or more <form> elements
			//		<range-element> [Optional, Multiple, range-element] RNG has <range-element>, doc and UML says <range> for a NAME OVERRIDE. Go with <range-element>, since that is what FLEx writes in the lift-ranges file.
			//		<label> [Optional, Multiple, multitext] <label> holds zero or more <form> elements
			//		<abbrev> [Optional, Multiple, multitext] <abbrev> holds zero or more <form> elements
			// </range>
			// ******************************* </range> **************************************************

			// ******************************* <ranges> **************************************************

			// ******************************* <range-element> **************************************************
			// This element appears to not be in the main lift file, so it will be 'extra', but ought not cause harm.
			// <range-element
			//		id [Required, key]
			LiftBasicElementStrategiesMethod.AddKeyedElementType(mergeStrategies, "range-element", "id", false);
			//		parent [Optional, key]
			//		guid [Optional, string]
			//		<description> [Optional, multitext] <description> holds zero or more <form> elements
			//		<label> [Optional, multitext] <label> holds zero or more <form> elements
			//		<abbrev> [Optional, multitext] <abbrev> holds zero or more <form> elements
			elementStrategy = LiftBasicElementStrategiesMethod.AddSingletonElementType(mergeStrategies, "abbrev");
			elementStrategy.OrderIsRelevant = false;
			// </range-element>
			// ******************************* </range-element> **************************************************
		}
	}
}