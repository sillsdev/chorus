using System;
using System.Collections.Generic;
using System.Xml;
using Chorus.merge;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Services class used by FieldWorksMergingStrategy to create ElementStrategy instances
	/// (some shared and some not shared).
	/// </summary>
	internal static class FieldWorksMergingServices
	{
		private static readonly FindFirstElementWithSameName _sameName = new FindFirstElementWithSameName();
		private static readonly FieldWorkObjectContextGenerator _contextGen = new FieldWorkObjectContextGenerator();
		private const string DateCreated = "DateCreated";
		private const string Rt = "rt";

		internal static void AddSharedImmutableSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = AddSharedSingletonElementType(sharedElementStrategies, name, orderOfTheseIsRelevant);
			strategy.IsImmutable = true;
		}

		internal static ElementStrategy AddSharedSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = CreateSingletonElementType(orderOfTheseIsRelevant);
			sharedElementStrategies.Add(name, strategy);
			return strategy;
		}

		internal static ElementStrategy CreateSingletonElementType(bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = _sameName,
								ContextDescriptorGenerator = _contextGen
							};
			return strategy;
		}

		internal static void AddSharedMainElement(Dictionary<string, ElementStrategy> sharedElementStrategies)
		{
			var strategy = new ElementStrategy(false)
			{
				MergePartnerFinder = new FindByKeyAttribute("guid")
			};
			strategy.AttributesToIgnoreForMerging.Add("class"); // Immutable
			strategy.AttributesToIgnoreForMerging.Add("guid"); // Immutable
			sharedElementStrategies.Add(Rt, strategy);
			strategy.ContextDescriptorGenerator = _contextGen;
		}

		private static void CreateSharedElementStrategies(Dictionary<string, ElementStrategy> sharedElementStrategies)
		{
			AddSharedMainElement(sharedElementStrategies);

			// Set one up (immutable) for DateCreated properties.
			AddSharedImmutableSingletonElementType(sharedElementStrategies, DateCreated, false);
		}

		private static void CreateMergers(MetadataCache metadataCache, Dictionary<string, ElementStrategy> sharedElementStrategies, Dictionary<string, XmlMerger> mergers, MergeSituation mergeSituation)
		{
			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				var merger = new XmlMerger(mergeSituation);
				merger.MergeStrategies.SetStrategy(Rt, sharedElementStrategies[Rt]);
				// TODO: Add all of the property bits.
				mergers.Add(classInfo.ClassName, merger);
			}
		}

		internal static void MergeCheckSum(XmlNode ourEntry, XmlNode theirEntry)
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

		internal static void MergeTimestamps(XmlNode ourEntry, XmlNode theirEntry)
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

		internal static string GetOuterXml(XmlNode node)
		{
			return node.OuterXml;
		}

		internal static void MergeCollectionProperties(FdoClassInfo classWithCollectionProperties, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry)
		{
			var ourEntryClassName = XmlUtilities.GetStringAttribute(ourEntry, "class");
			if (classWithCollectionProperties.ClassName != ourEntryClassName)
				return; // Not the same class of object.

			//throw new NotImplementedException();
		}

		internal static void BootstrapSystem(MetadataCache mdc, Dictionary<string, ElementStrategy> sharedElementStrategies, Dictionary<string, XmlMerger> mergers, MergeSituation mergeSituation)
		{
			CreateSharedElementStrategies(sharedElementStrategies);
			CreateMergers(mdc, sharedElementStrategies, mergers, mergeSituation);
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
	}
}