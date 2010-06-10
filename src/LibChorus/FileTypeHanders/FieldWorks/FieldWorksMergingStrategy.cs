using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	public class FieldWorksMergingStrategy : IMergeStrategy
	{
		private readonly XmlMerger m_entryMerger;

		public FieldWorksMergingStrategy(MergeSituation mergeSituation)
		{
			m_entryMerger = new XmlMerger(mergeSituation);

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
			m_entryMerger.MergeStrategies.SetStrategy(name, strategy);
			return strategy;
		}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			return GetOuterXml(m_entryMerger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		private static string GetOuterXml(XmlNode node)
		{
			return node.OuterXml;
		}

		#endregion
	}
}