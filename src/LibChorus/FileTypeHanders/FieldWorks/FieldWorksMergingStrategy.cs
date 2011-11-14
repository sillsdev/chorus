using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// A merge strategy for FieldWorks 7.0 xml data.
	///
	/// As the FW team develops this, it will do lots more for the various domain classes.
	/// </summary>
	/// <remarks>
	/// I think the approach I'll take on this is is to have one XmlMerger instance for each concrete class of CmObject.
	/// The MakeMergedEntry method would then get the right XmlMerger instance for the given class of CmObject
	/// from a Dictionary (key being a string of the class name of the CmObject).
	///
	/// The various XmlMerger instances would be populated with class-specific property level instances of ElementStrategy,
	/// and some ElementStrategy instances for each data type (e.g., string, bool, etc.)
	/// These common ElementStrategy instances ought to be reusable by the various XmlMerger instances,
	/// provided they are not changed by the XmlMerger while in use.
	/// </remarks>
	public sealed class FieldWorksMergingStrategy : IMergeStrategy
	{
		private readonly MetadataCache _mdc;
		private readonly Dictionary<string, ElementStrategy> _sharedElementStrategies = new Dictionary<string, ElementStrategy>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, XmlMerger> _mergers = new Dictionary<string, XmlMerger>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Constructor.
		/// </summary>
		public FieldWorksMergingStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mdc = mdc;
			FieldWorksMergingServices.BootstrapSystem(_mdc, _sharedElementStrategies, _mergers, mergeSituation);
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Do a 3-way merge of two CmObject XmlNodes ('rt' element nodes).
		/// </summary>
		/// <returns>XML of the merged object.</returns>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			var extantNode = ourEntry ?? theirEntry ?? commonEntry;

			// TODO: Figure out what to do about an owner being changed.
			// 1. If only one person changed it, there should be no problem.
			// 2. If two people changed the owner, there could be trouble:
			//		A. The owned would report a merge conflict,
			//		B. The new owner(s) may not report a conflict,
			//		So, if the owned object reports an owner conflict,
			//		then make sure the 'loser' owner also has a conflict.

			// These steps will do some 'pre-merging' work,
			// which will avoid what could otherwise be conflicts.
			FieldWorksMergingServices.MergeTimestamps(ourEntry, theirEntry);
			FieldWorksMergingServices.MergeCheckSum(ourEntry, theirEntry);
			var className = XmlUtilities.GetStringAttribute(extantNode, "class");
			FdoClassInfo info;
			if (_mdc.ClassesWithCollectionProperties.TryGetValue(className, out info))
				FieldWorksMergingServices.MergeCollectionProperties(eventListener, info, ourEntry, theirEntry, commonEntry);
			var merger = _mergers[className];

			return FieldWorksMergingServices.GetOuterXml(merger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		#endregion
	}
}