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
		private readonly bool _returnOurs;

		internal NullMergeStrategy(bool returnOurs)
		{
			_returnOurs = returnOurs;
		}

		#region Implementation of IMergeStrategy

		/// <summary>
		/// Produce a string that represents the 3-way merger of the given three elements.
		/// </summary>
		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return _returnOurs
					? (ourEntry == null ? null : ourEntry.OuterXml)
					: (theirEntry == null ? null : theirEntry.OuterXml);
		}

		/// <summary>
		/// Return the ElementStrategy instance for the given <param name="element"/>, or a default instance set up like this:
		/// ElementStrategy def = new ElementStrategy(true);//review: this says the default is to consder order relevant
		/// def.MergePartnerFinder = new FindByEqualityOfTree();
		/// </summary>
		public ElementStrategy GetElementStrategy(XmlNode element)
		{
			var def = new ElementStrategy(true)
			{
				MergePartnerFinder = new FindByEqualityOfTree()
			};
			return def;
		}

		#endregion
	}
}