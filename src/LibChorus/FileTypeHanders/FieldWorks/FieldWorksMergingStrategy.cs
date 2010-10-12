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
	public sealed partial class FieldWorksMergingStrategy : IMergeStrategy
	{
		private readonly MetadataCache _mdc;
		private readonly Dictionary<string, ElementStrategy> _elementStrategies = new Dictionary<string, ElementStrategy>();
		private readonly Dictionary<string, XmlMerger> _mergers = new Dictionary<string, XmlMerger>();
		const string Rt = "rt";

		public FieldWorksMergingStrategy(MergeSituation mergeSituation, MetadataCache mdc)
		{
			_mdc = mdc;
			CreateSharedElementStrategies();
			CreateMergers(mergeSituation);
		}

		private void CreateSharedElementStrategies()
		{
			var strategy = new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByKeyAttribute("guid")
			};
			_elementStrategies.Add(Rt, strategy);
			strategy.ContextDescriptorGenerator = new FieldWorkObjectContextGenerator();
		}

		private void CreateMergers(MergeSituation mergeSituation)
		{
			var merger = new XmlMerger(mergeSituation);
			merger.MergeStrategies.SetStrategy(Rt, _elementStrategies[Rt]);
			_mergers.Add("CmObject", merger);
		}

		//private ElementStrategy AddKeyedElementType(XmlMerger merger, string elementName, string keyAttribute, bool orderOfTheseIsRelevant)
		//{
		//    var strategy = new ElementStrategy(orderOfTheseIsRelevant)
		//        {
		//            MergePartnerFinder = new FindByKeyAttribute(keyAttribute)
		//        };
		//    merger.MergeStrategies.SetStrategy(elementName, strategy);
		//    return strategy;
		//}

		#region Implementation of IMergeStrategy

		public string MakeMergedEntry(IMergeEventListener eventListener, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			MergeTimestamps(ourEntry, theirEntry);
			MergeCheckSum(ourEntry, theirEntry);
			return GetOuterXml(_mergers["CmObject"].Merge(eventListener, ourEntry, theirEntry, commonEntry));
		}

		private static void MergeCheckSum(XmlNode ourEntry, XmlNode theirEntry)
		{
			if (ourEntry.SelectSingleNode("@class").Value != "WfiWordform")
				return;

			const string xpath = "Checksum";
			var ourChecksumNode = ourEntry.SelectSingleNode(xpath);
			var theirChecksumNode = theirEntry.SelectSingleNode(xpath);
			if (ourChecksumNode == null && theirChecksumNode != null)
			{
				var attr = theirChecksumNode.Attributes["val"];
				attr.Value = "0";
				var ourDoc = ourEntry.OwnerDocument;
				var newChecksumElement = ourDoc.CreateElement("Checksum");
				ourEntry.AppendChild(newChecksumElement);

				var newAttr = ourDoc.CreateAttribute("val");
				newChecksumElement.SetAttributeNode(newAttr);
				newAttr.Value = "0";
				return;
			}
			if (theirChecksumNode == null && ourChecksumNode != null)
			{
				var attr = ourChecksumNode.Attributes["val"];
				attr.Value = "0";
				var theirDoc = theirEntry.OwnerDocument;
				var newChecksumElement = theirDoc.CreateElement("Checksum");
				theirEntry.AppendChild(newChecksumElement);

				var newAttr = theirDoc.CreateAttribute("val");
				newChecksumElement.SetAttributeNode(newAttr);
				newAttr.Value = "0";
				return;
			}
			var ourAttr = ourChecksumNode.Attributes["val"];
			var theirAttr = theirChecksumNode.Attributes["val"];
			if (ourAttr.Value == theirAttr.Value)
				return;

			// Set both to 0.
			ourAttr.Value = "0";
			theirAttr.Value = "0";
		}

		private static void MergeTimestamps(XmlNode ourEntry, XmlNode theirEntry)
		{
			const string xpath = "DateModified | DateResolved | RunDate";
			var ourDateTimeNodes = ourEntry.SelectNodes(xpath);
			var theirDateTimeNodes = theirEntry.SelectNodes(xpath);
			if ((ourDateTimeNodes == null || ourDateTimeNodes.Count == 0) &&
				(theirDateTimeNodes == null || theirDateTimeNodes.Count == 0))
				return;

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