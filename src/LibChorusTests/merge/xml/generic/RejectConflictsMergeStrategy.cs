using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Test class that does not use XmlMerger, but simply returns the outer xml of ours, or null, if ours is null.
	///
	/// This class can be used by tests that don't really want to drill down into the XmlMerger code, such as the XmlMergeServiceTests test class.
	/// </summary>
	internal class RejectConflictsMergeStrategy : IMergeStrategy
	{
		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consider order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the collection of element merge strategies.
		/// </summary>
		public MergeStrategies GetStrategies()
		{
			// Only needed to get the reject conflict tests to pass.
			// Don't count on the returned value to be hepful in other contexts.
			var merger = new XmlMerger(new MergeSituation(null, null, null, null, null, MergeOrder.ConflictHandlingModeChoices.WeWin));
			var def = new ElementStrategy(true)
			{
				MergePartnerFinder = new FindByEqualityOfTree()
			};
			merger.MergeStrategies.SetStrategy("def", def);
			return merger.MergeStrategies;
		}

		public HashSet<string> SuppressIndentingChildren()
		{
			// Only needed to get the reject conflict tests to pass.
			// Don't count on the returned value to be hepful in other contexts.
			return new HashSet<string>();
		}

		public bool IsDifferenceSignificant(XmlNode commonAncestor, XmlNode survivor)
		{
			return false;
		}
		#endregion
	}
}