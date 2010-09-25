using System;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// A strategy for FieldWorks 7.0 xml data.
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
			const string xpath = "DateModified | DateResolved | RunDate";
			var ourDateTimeNodes = ourEntry.SelectNodes(xpath);
			var theirDateTimeNodes = theirEntry.SelectNodes(xpath);
			if ((ourDateTimeNodes != null && ourDateTimeNodes.Count > 0) || (theirDateTimeNodes != null && theirDateTimeNodes.Count > 0))
			{
				MergeTimestamps(ourDateTimeNodes, theirDateTimeNodes);
			}
			return GetOuterXml(_entryMerger.Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		private static void MergeTimestamps(XmlNodeList ourDateTimeNodes, XmlNodeList theirDateTimeNodes)
		{
			for (var i = 0; i < ourDateTimeNodes.Count; ++i)
			{
				var ourNode = ourDateTimeNodes[i];
				var asUtcOurs = GetTimestamp(ourNode);

				var theirNode = theirDateTimeNodes[i];
				var asUtcTheirs = GetTimestamp(theirNode);

				if (asUtcOurs == asUtcTheirs)
					return;

				if (asUtcOurs > asUtcTheirs)
					theirNode.Attributes["val"].Value = ourNode.Attributes["val"].Value;
				else
					ourNode.Attributes["val"].Value = theirNode.Attributes["val"].Value;
			}
		}

		private static DateTime GetTimestamp(XmlNode node)
		{
			var timestamp = node.Attributes["val"].Value;
			var dateParts = timestamp.Split(new[] { '-', ' ', ':', '.' });
			return new DateTime(
				Int32.Parse(dateParts[0]),
				Int32.Parse(dateParts[1]),
				Int32.Parse(dateParts[2]),
				Int32.Parse(dateParts[3]),
				Int32.Parse(dateParts[4]),
				Int32.Parse(dateParts[5]),
				Int32.Parse(dateParts[6]));
		}

		private static string GetOuterXml(XmlNode node)
		{
			return node.OuterXml;
		}

		#endregion
	}
}