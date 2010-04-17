using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	public class FieldWorksMergingStrategy : IMergeStrategy
	{
		private XmlMerger m_entryMerger;

		public FieldWorksMergingStrategy(MergeSituation mergeSituation)
		{
			m_entryMerger = new XmlMerger(mergeSituation);
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}