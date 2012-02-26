using System.Xml;
using Chorus.merge.xml.generic;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Test class that does not use XmlMerger, but simply returns the outer xml of ours, or null, if ours is null.
	///
	/// This class can be used by tests that don't really want to drill down into the XmlMerger code, such as the XmlMergeServiceTests test class.
	/// </summary>
	internal class NullMergeStrategy : IMergeStrategy
	{
		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return ourEntry == null ? null : ourEntry.OuterXml;
		}

		#endregion
	}
}