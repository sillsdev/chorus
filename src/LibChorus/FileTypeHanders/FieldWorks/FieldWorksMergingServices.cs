using System;
using System.Collections.Generic;
using System.Linq;
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
		private static readonly FindByKeyAttribute _wsKey = new FindByKeyAttribute(Ws);
		private static readonly FindFirstElementWithSameName _sameName = new FindFirstElementWithSameName();
		private static readonly FieldWorkObjectContextGenerator _contextGen = new FieldWorkObjectContextGenerator();
		private const string MutableSingleton = "MutableSingleton";
		private const string DateCreated = "DateCreated";
		private const string Rt = "rt";
		private const string ImmutableSingleton = "ImmutableSingleton";
		private const string Objsur = "objsur";
		private const string GuidStr = "guid";
		private const string AStr = "AStr";
		private const string AUni = "AUni";
		private const string Ws = "ws";

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
			AddSharedImmutableSingletonElementType(sharedElementStrategies, ImmutableSingleton, false);

			AddSharedSingletonElementType(sharedElementStrategies, MutableSingleton, false);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AStr, false);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AUni, false);
		}

		private static void AddSharedKeyedByWsElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, bool orderOfTheseIsRelevant)
		{
			AddKeyedElementType(sharedElementStrategies, elementName, _wsKey, orderOfTheseIsRelevant);
		}

		private static void AddKeyedElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, FindByKeyAttribute findBykeyAttribute, bool orderOfTheseIsRelevant)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
			{
				MergePartnerFinder = findBykeyAttribute
			};
			sharedElementStrategies.Add(elementName, strategy);
		}

		private static void CreateMergers(MetadataCache metadataCache, MergeSituation mergeSituation,
			IDictionary<string, ElementStrategy> sharedElementStrategies, IDictionary<string, XmlMerger> mergers)
		{
			var mutableSingleton = sharedElementStrategies[MutableSingleton];
			var immSingleton = sharedElementStrategies[ImmutableSingleton];
			ElementStrategy extantStrategy;
			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				var merger = new XmlMerger(mergeSituation);
				var strategiesForMerger = merger.MergeStrategies;
				strategiesForMerger.SetStrategy(Rt, sharedElementStrategies[Rt]);
				// Add all of the property bits.
				// NB: Each of the child elements (except for custom properties, when we get to the point of handling them)
				// will be singletons.
				foreach (var propInfo in classInfo.AllProperties)
				{
					switch (propInfo.DataType)
					{
						// These three are immutable, in a manner of speaking.
						// DateCreated is honestly, and the other two are because 'ours' and 'theirs' have been made to be the same already.
						case DataType.Time: // DateTime
							strategiesForMerger.SetStrategy(propInfo.PropertyName, immSingleton);
							break;
						case DataType.OwningCollection:
							break; // TODO: Deal with these, including the fact that the owned object may have been moved elsewhere.
						case DataType.ReferenceCollection:
							strategiesForMerger.SetStrategy(propInfo.PropertyName, immSingleton);
							break;

						// Using regular xmlmerger code: strategiesForMerger.SetStrategy(propInfo.PropertyName, mutableSingleton);

						case DataType.OwningSequence:
							// TODO: Can we pre-process seq props like we did with collections?
							break;
						case DataType.ReferenceSequence:
							// TODO: Can we pre-process seq props like we did with collections?
							break;
						case DataType.OwningAtomic:
							break;
						case DataType.ReferenceAtomic:
							break;

						// Other data types
						case DataType.MultiUnicode:
							/*
This ought to be a keyed widget with the key being the ws. Order of the <AUni> elements is not relevant.
The diff could be in presence or absence of the property or an entire AUni element, or just in the content string.
A conflict can happen if:
							 * 1. an alternative was edited by both where the resulting string is not the same.
							 * 2. somebody deleted an alternative, but the other edited it.
							 * 3. both added the same alternative, but with different values.
These are not conflicts, and can be merged automatically:
							 * 1. Somebody deleted an alternative, and the other had not edited it.
							 * 2. One person added a new alternative.
							 * 3. Both added an alternative, but with the same value.
<AUni ws="en">Status</AUni>
<AUni ws="es">Estado</AUni>
							*/
							// Use new IsAtomic.
							strategiesForMerger.SetStrategy(propInfo.PropertyName, CreateSingletonElementType(false));
							if (!strategiesForMerger.ElementStrategies.TryGetValue(AUni, out extantStrategy))
								strategiesForMerger.SetStrategy(AUni, sharedElementStrategies[AUni]);
							break;
						case DataType.MultiString:
							/*
This ought to be a keyed widget with the key being the ws. Order of the <AStr> elements is not relevant.
The diff could be in presence or absence of the property or an entire AUni element, or just in the content string.
No attempt is to be made to try and merge the run(s) within an alternative.
We will just see if the corresponding <AStr> elements are the same or different.
1. Add any new alternates from winner or loser.
<SummaryDefinition>
<AStr ws="en">
<Run ws="en">go</Run>
</AStr>
<AStr ws="es">
<Run ws="es">ir</Run>
</AStr>
</SummaryDefinition>
							*/
							// Use new IsAtomic.
							break;
						case DataType.Unicode: // Ordinary C# string
							/*
The entire property element may be missing, but if present, there should then be one <Uni> element.
These are conflicts:
							 * 1. Somebody removed the <Uni> element and the other edited it.
							 * 2. Both edited it, with different ending content.
							 * 3. Both added the <Uni> element, but with different content.
These are not conflicts:
							 * 1. Both removed the <Uni> element
							 * 2. Both added the <Uni> element with the same contents.
							 * 3. Both edited the <Uni> element contents, with the same resulting value.
<Name>
<Uni>Anthro Category</Uni>
</Name>
							*/
							break;
						case DataType.String: // TsString
							/*
The entire property element may be missing, but if present, there should then be one or more runs.
No attempt is to be made to try and merge the run(s).
We will just see if the corresponding <Str> elements are the same or different.
<Str>
<Run ws="grc">Αἴγυπτος</Run>
</Str>
							*/
							// Use new IsAtomic.
							break;
						case DataType.Integer:
							break;
						case DataType.Boolean:
							// NB: These can never be in conflict in a 3-way merge environment.
							// One or the other can toggle the bool, so the one changing it 'wins'.
							// If both change it then it's no big deal either.
							break;
						case DataType.GenDate:
							break;
						case DataType.Guid:
							break;
						case DataType.Binary:
							// We can't really merge these, so pick the one that changed it.
							// Use new IsAtomic.
							break;
						case DataType.TextPropBinary:
							// Use new IsAtomic.
							break;
					}
				}
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
			foreach (var collectionProperty in classWithCollectionProperties.AllCollectionProperties)
			{
				var commonValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var propNode = commonEntry.SelectSingleNode(collectionProperty.PropertyName);
				if (propNode != null)
				{
					commonValues.UnionWith(from XmlNode objsurNode in propNode.SafeSelectNodes(Objsur)
										   select objsurNode.GetStringAttribute(GuidStr));
				}
				var ourValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var ourPropNode = ourEntry.SelectSingleNode(collectionProperty.PropertyName);
				if (ourPropNode != null)
				{
					ourValues.UnionWith(from XmlNode objsurNode in ourPropNode.SafeSelectNodes(Objsur)
										select objsurNode.GetStringAttribute(GuidStr));
				}
				var theirValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var theirPropNode = theirEntry.SelectSingleNode(collectionProperty.PropertyName);
				if (theirPropNode != null)
				{
					theirValues.UnionWith(from XmlNode objsurNode in theirPropNode.SafeSelectNodes(Objsur)
										  select objsurNode.GetStringAttribute(GuidStr));
				}

				// 0. If ours and theirs are the same, there is no conflict.
				if (ourValues.SetEquals(theirValues))
					continue; // NB: The merger will be told the prop is immutable, so it will not notice if the order is different.

				// 1. Keep ones that are in all three. (Excludes removed items.)
				var mergedCollection = new HashSet<string>(commonValues, StringComparer.OrdinalIgnoreCase);
				mergedCollection.IntersectWith(ourValues);
				mergedCollection.IntersectWith(theirValues);

				// 2. Add ones that either added.
				var ourAdditions = ourValues.Except(commonValues);
				mergedCollection.UnionWith(ourAdditions);
				var theirAdditions = theirValues.Except(commonValues);
				mergedCollection.UnionWith(theirAdditions);

				// 3. Update ours and theirs to the new collection.
				if (mergedCollection.Count == 0)
				{
					// Remove prop node from both.
					var gonerNode = ourEntry.SelectSingleNode(collectionProperty.PropertyName);
					if (gonerNode != null)
						gonerNode.ParentNode.RemoveChild(gonerNode);
					gonerNode = theirEntry.SelectSingleNode(collectionProperty.PropertyName);
					if (gonerNode != null)
						gonerNode.ParentNode.RemoveChild(gonerNode);
				}
				else
				{
					var ourDoc = ourEntry.OwnerDocument;
					var theirDoc = theirEntry.OwnerDocument;
					if (ourPropNode == null)
					{
						ourPropNode = ourDoc.CreateNode(XmlNodeType.Element, collectionProperty.PropertyName, null);
						ourEntry.AppendChild(ourPropNode);
					}
					else
					{
						ourPropNode.RemoveAll();
					}
					if (theirPropNode == null)
					{
						theirPropNode = theirDoc.CreateNode(XmlNodeType.Element, collectionProperty.PropertyName, null);
						theirEntry.AppendChild(theirPropNode);
					}
					else
					{
						theirPropNode.RemoveAll();
					}
					var propType = (collectionProperty.DataType == DataType.ReferenceCollection) ? "r" : "o";
					foreach (var newValue in mergedCollection)
					{
						// Add it to ours and theirs.
						CreateObjsurNode(ourDoc, newValue, propType, ourPropNode);
						CreateObjsurNode(theirDoc, newValue, propType, theirPropNode);
					}
				}
			}
		}

		private static void CreateObjsurNode(XmlDocument srcDoc, string newValue, string propType, XmlNode srcPropNode)
		{
			var srcObjsurNode = srcDoc.CreateNode(XmlNodeType.Element, Objsur, null);
			srcPropNode.AppendChild(srcObjsurNode);
			var srcGuidAttrNode = srcDoc.CreateAttribute(GuidStr);
			srcGuidAttrNode.Value = newValue;
			srcObjsurNode.Attributes.Append(srcGuidAttrNode);
			var srcPropTypeAttrNode = srcDoc.CreateAttribute("t");
			srcPropTypeAttrNode.Value = propType;
			srcObjsurNode.Attributes.Append(srcPropTypeAttrNode);
		}

		internal static void BootstrapSystem(MetadataCache mdc, Dictionary<string, ElementStrategy> sharedElementStrategies, Dictionary<string, XmlMerger> mergers, MergeSituation mergeSituation)
		{
			CreateSharedElementStrategies(sharedElementStrategies);
			CreateMergers(mdc, mergeSituation, sharedElementStrategies, mergers);
		}
	}
}