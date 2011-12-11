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
			return FieldWorksMergingServices.GetOuterXml(_merger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		#endregion
	}
}