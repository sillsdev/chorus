using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks.Linguistics.Reversals
{
	public class FieldWorksReversalMergeStrategy : IMergeStrategy
	{
		private readonly MetadataCache _mdc;
		private readonly XmlMerger _merger;

		/// <summary>
		/// Constructor.
		/// </summary>
		public FieldWorksReversalMergeStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mdc = mdc;
			_merger = new XmlMerger(mergeSituation);
			FieldWorksMergingServices.BootstrapSystem(_mdc, _merger);
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			// These steps will do some 'pre-merging' work,
			// which will avoid what could otherwise be conflicts.
			if (ourEntry.Name == "header")
			{
				// The 'header' element needs some pecial treatment, since it has PartOfSpeech objects that have 'DateModified' elements that need special merge handling.
				// As of 12 Dec 2011, no other elements in the reversal system have this.
				XmlNode parent = null;
				foreach (XmlNode dateStampElement in ourEntry.SafeSelectNodes("descendant-or-self::*[name()='DateModified' or name()='DateResolved' or name()='RunDate']"))
				{
					if (parent == dateStampElement.ParentNode)
						continue;
					parent = dateStampElement.ParentNode;
					// Get corresponding element in 'theirEntry'.
					// We will know for sure it is the matching one, by the guid attr of the parent node.
					var parentGuid = parent.Attributes["guid"].Value;
					var theirMatchingParentNode = theirEntry.SafeSelectNodes(string.Format("descendant-or-self::*[@guid='{0}']", parentGuid))[0];
					FieldWorksMergingServices.MergeTimestamps(parent, theirMatchingParentNode);
				}
			}

			// NB: The 'Subentries' property of ReversalIndexEntry is an owning collection, but I (RBR) don't think that is a worry in this new system.
			// They will all be reordered when written out, and they are nested now. Time will tell, if my theroy is right. :-)

			return FieldWorksMergingServices.GetOuterXml(_merger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		#endregion
	}
}