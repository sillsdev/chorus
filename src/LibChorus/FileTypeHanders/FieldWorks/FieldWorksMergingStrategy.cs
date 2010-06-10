using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Almost a 'do-nothing' strategy for FieldWorks 7.0 xml data.
	///
	/// As the FW team develops this, it will do lots more for the various domain classes.
	/// </summary>
	public class FieldWorksMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger _entryMerger;

		public FieldWorksMergingStrategy(MergeSituation mergeSituation)
		{
			_entryMerger = new XmlMerger(mergeSituation);

			// Customize the XmlMerger with FW-specific info (cf. LiftEntryMergingStrategy for how Lift dees this.)
			// Start with the <rt> element.
			var elementStrategy = AddKeyedElementType("rt", "guid", false);
			elementStrategy.ContextDescriptorGenerator = new FieldWorkObjectContextGenerator();
		}

		private ElementStrategy AddKeyedElementType(string name, string attribute, bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
				{
					MergePartnerFinder = new FindByKeyAttribute(attribute)
				};
			_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return GetOuterXml(_entryMerger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		private static string GetOuterXml(XmlNode node)
		{
			return node.OuterXml;
		}

		#endregion
	}
}