using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Properties;

namespace Chorus.FileTypeHanders.FieldWorks
{
	/// <summary>
	/// Services class used by FieldWorksMergingStrategy to create ElementStrategy instances
	/// (some shared and some not shared).
	/// </summary>
	internal static class FieldWorksMergingServices
	{
		private static readonly FindByKeyAttribute _wsKey = new FindByKeyAttribute(Ws);
		private static readonly FindByKeyAttribute _guidKey = new FindByKeyAttribute(GuidStr);
		private static readonly FindFirstElementWithSameName _sameName = new FindFirstElementWithSameName();
		private static readonly FieldWorkObjectContextGenerator _contextGen = new FieldWorkObjectContextGenerator();
		private const string MutableSingleton = "MutableSingleton";
		private const string DateCreated = "DateCreated";
		private const string Rt = "rt";
		private const string ImmutableSingleton = "ImmutableSingleton";
		private const string Objsur = "objsur";
		private const string GuidStr = "guid";
		private const string Str = "Str";
		private const string AStr = "AStr";
		private const string Uni = "Uni";
		private const string AUni = "AUni";
		private const string Ws = "ws";
		private const string Binary = "Binary";
		private const string Prop = "Prop";
		private const string Custom = "Custom";
		private const string Header = "header";

		internal static ElementStrategy AddSharedImmutableSingletonElementType(Dictionary<string, ElementStrategy> sharedElementStrategies, string name, bool orderOfTheseIsRelevant)
		{
			var strategy = AddSharedSingletonElementType(sharedElementStrategies, name, orderOfTheseIsRelevant);
			strategy.IsImmutable = true;
			return strategy;
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
								//ContextDescriptorGenerator = _contextGen
							};
			return strategy;
		}

		private static void CreateSharedElementStrategies(Dictionary<string, ElementStrategy> sharedElementStrategies)
		{
			// Set up immutable strategies.
			// Skip this one, since all DateTime props are, either legally (created), or are pre-processed for merging.
			//AddSharedImmutableSingletonElementType(sharedElementStrategies, DateCreated, false);
			AddSharedImmutableSingletonElementType(sharedElementStrategies, ImmutableSingleton, false);

			AddSharedSingletonElementType(sharedElementStrategies, MutableSingleton, false);
			var elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Str, false);
			elementStrategy.IsAtomic = true;
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Binary, false);
			elementStrategy.IsAtomic = true;
			elementStrategy = AddSharedSingletonElementType(sharedElementStrategies, Prop, false);
			elementStrategy.IsAtomic = true;
			AddSharedSingletonElementType(sharedElementStrategies, Uni, false);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AStr, false, true);
			AddSharedKeyedByWsElementType(sharedElementStrategies, AUni, false, true);
		}

		private static void AddSharedKeyedByWsElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, bool orderOfTheseIsRelevant, bool isAtomic)
		{
			AddKeyedElementType(sharedElementStrategies, elementName, _wsKey, orderOfTheseIsRelevant, isAtomic);
		}

		private static void AddKeyedElementType(IDictionary<string, ElementStrategy> sharedElementStrategies, string elementName, IFindNodeToMerge findBykeyAttribute, bool orderOfTheseIsRelevant, bool isAtomic)
		{
			var strategy = new ElementStrategy(orderOfTheseIsRelevant)
							{
								MergePartnerFinder = findBykeyAttribute,
								IsAtomic = isAtomic
							};
			//strategy.ContextDescriptorGenerator
			sharedElementStrategies.Add(elementName, strategy);
		}

		private static void CreateMergers(MetadataCache metadataCache, MergeSituation mergeSituation,
			IDictionary<string, ElementStrategy> sharedElementStrategies, IDictionary<string, XmlMerger> mergers)
		{
			var mutableSingleton = sharedElementStrategies[MutableSingleton];
			var immSingleton = sharedElementStrategies[ImmutableSingleton];
			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				var merger = new XmlMerger(mergeSituation);
				var strategiesForMerger = merger.MergeStrategies;
				strategiesForMerger.SetStrategy(Rt, sharedElementStrategies[Rt]);

				// Add top level strategy for the new-styled elements (rt->classname).
				// Eventually, the 'rt' stuff will go away.
				var classStrat = new ElementStrategy(false);
				classStrat.AttributesToIgnoreForMerging.Add("guid"); // guid is immutable.
				classStrat.MergePartnerFinder = _guidKey;
				classStrat.ContextDescriptorGenerator = _contextGen;
				strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);

				// Add all of the property bits.
				// NB: Each of the child elements (except for custom properties)
				// will be singletons.
				foreach (var propInfo in classInfo.AllProperties)
				{
					if (propInfo.IsCustomProperty)
					{
						ProcessCustomProperty(sharedElementStrategies, strategiesForMerger, propInfo, mutableSingleton);
					}
					else
					{
						ProcessStandardProperty(sharedElementStrategies, strategiesForMerger, propInfo, mutableSingleton, immSingleton);
					}
				}
				mergers.Add(classInfo.ClassName, merger);

			}

		}

		private static void ProcessCustomProperty(IDictionary<string, ElementStrategy> sharedElementStrategies, MergeStrategies strategiesForMerger, FdoPropertyInfo propInfo, ElementStrategy mutableSingleton)
		{
			// Start all with basic keyed strategy.
			var strategyForCurrentProperty = ElementStrategy.CreateForKeyedElement("name", false);
			ElementStrategy extantStrategy;
			switch (propInfo.DataType)
			{
				case DataType.Time: // Fall through
				case DataType.OwningCollection:
				case DataType.ReferenceCollection:
					strategyForCurrentProperty.IsImmutable = true;
					break;

				case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
				case DataType.ReferenceSequence:
					// Use IsAtomic for whole property.
					strategyForCurrentProperty.IsAtomic = true;
					break;
				case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
				case DataType.ReferenceAtomic:
					// If own seq/col & ref seq end up being atomic, then 'obsur' is a simple singleton.
					// Otherwise, things get more complicated, since this one is singleton, but the others would be keyed.
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Objsur, out extantStrategy))
						strategiesForMerger.SetStrategy(Objsur, mutableSingleton);
					break;

				// Other data types
				case DataType.MultiUnicode:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AUni, out extantStrategy))
						strategiesForMerger.SetStrategy(AUni, sharedElementStrategies[AUni]);
					break;
				case DataType.MultiString:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AStr, out extantStrategy))
						strategiesForMerger.SetStrategy(AStr, sharedElementStrategies[AStr]);
					break;
				case DataType.Unicode: // Ordinary C# string
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Uni, out extantStrategy))
						strategiesForMerger.SetStrategy(Uni, sharedElementStrategies[Uni]);
					break;
				case DataType.String: // TsString
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Str, out extantStrategy))
						strategiesForMerger.SetStrategy(Str, sharedElementStrategies[Str]);
					break;
				case DataType.Binary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Binary, out extantStrategy))
						strategiesForMerger.SetStrategy(Binary, sharedElementStrategies[Binary]);
					break;
				case DataType.TextPropBinary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Prop, out extantStrategy))
						strategiesForMerger.SetStrategy(Prop, sharedElementStrategies[Prop]);
					break;
				// NB: Booleans can never be in conflict in a 3-way merge environment.
				// One or the other can toggle the bool, so the one changing it 'wins'.
				// If both change it then it's no big deal either.
				case DataType.Boolean: // Fall through.
				case DataType.GenDate: // Fall through.
				case DataType.Guid: // Fall through.
				case DataType.Integer: // Fall through.
					// Simple, mutable properties.
					break;
			}
			strategiesForMerger.SetStrategy(Custom + "_" + propInfo.PropertyName, strategyForCurrentProperty);
		}

		private static void ProcessStandardProperty(IDictionary<string, ElementStrategy> sharedElementStrategies, MergeStrategies strategiesForMerger, FdoPropertyInfo propInfo, ElementStrategy mutableSingleton, ElementStrategy immSingleton)
		{
			// Start all with basic mutable singleton. Switch to 'immSingleton' for properties that are immutable.
			var strategyForCurrentProperty = mutableSingleton;
			ElementStrategy extantStrategy;
			switch (propInfo.DataType)
			{
				// These three are immutable, in a manner of speaking.
				// DateCreated is honestly, and the other two are because 'ours' and 'theirs' have been made to be the same already.
				case DataType.Time: // Fall through // DateTime
				case DataType.OwningCollection:
				case DataType.ReferenceCollection:
					strategyForCurrentProperty = immSingleton;
					break;

				case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
				case DataType.ReferenceSequence:
					// Use IsAtomic for whole property.
					strategyForCurrentProperty = CreateSingletonElementType(false);
					strategyForCurrentProperty.IsAtomic = true;
					break;
				case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
				case DataType.ReferenceAtomic:
					strategyForCurrentProperty = mutableSingleton;
					// If own seq/col & ref seq end up being atomic, then 'obsur' is a simple singleton.
					// Otherwise, things get more complicated, since this one is singleton, but the others would be keyed.
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Objsur, out extantStrategy))
						strategiesForMerger.SetStrategy(Objsur, mutableSingleton);
					break;

					// Other data types
				case DataType.MultiUnicode:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AUni, out extantStrategy))
						strategiesForMerger.SetStrategy(AUni, sharedElementStrategies[AUni]);
					break;
				case DataType.MultiString:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(AStr, out extantStrategy))
						strategiesForMerger.SetStrategy(AStr, sharedElementStrategies[AStr]);
					break;
				case DataType.Unicode: // Ordinary C# string
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Uni, out extantStrategy))
						strategiesForMerger.SetStrategy(Uni, sharedElementStrategies[Uni]);
					break;
				case DataType.String: // TsString
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Str, out extantStrategy))
						strategiesForMerger.SetStrategy(Str, sharedElementStrategies[Str]);
					break;
				case DataType.Binary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Binary, out extantStrategy))
						strategiesForMerger.SetStrategy(Binary, sharedElementStrategies[Binary]);
					break;
				case DataType.TextPropBinary:
					if (!strategiesForMerger.ElementStrategies.TryGetValue(Prop, out extantStrategy))
						strategiesForMerger.SetStrategy(Prop, sharedElementStrategies[Prop]);
					break;
					// NB: Booleans can never be in conflict in a 3-way merge environment.
					// One or the other can toggle the bool, so the one changing it 'wins'.
					// If both change it then it's no big deal either.
				case DataType.Boolean: // Fall through.
				case DataType.GenDate: // Fall through.
				case DataType.Guid: // Fall through.
				case DataType.Integer: // Fall through.
					// Simple, mutable properties.
					break;
			}
			strategiesForMerger.SetStrategy(propInfo.PropertyName, strategyForCurrentProperty);
		}

		internal static void MergeCheckSum(XmlNode ourEntry, XmlNode theirEntry, bool isNewStyle)
		{
			if (isNewStyle)
			{
				if (ourEntry.Name != "WfiWordform")
					return;
			}
			else
			{
				if (ourEntry.SelectSingleNode("@class").Value != "WfiWordform")
					return;
			}

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

		private static XmlNode GetPropertyNode(XmlNode parentNode, string propertyName)
		{
			return (parentNode == null) ? null : parentNode.SelectSingleNode(propertyName);
		}

		internal static void MergeCollectionProperties(IMergeEventListener eventListener, FdoClassInfo classWithCollectionProperties, XmlNode ourEntry, XmlNode theirEntry, XmlNode commonEntry, bool isNewStyle)
		{
			foreach (var collectionProperty in classWithCollectionProperties.AllCollectionProperties)
			{
				var isOwningProp = (collectionProperty.DataType == DataType.OwningCollection);
				if (isOwningProp && isNewStyle)
					continue; // Skip it, even if it was excluded, and thus, has objsur elements. The merge strategy thinks it is mutable, and try to merge it anyway.

				var commonValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var commonPropNode = GetPropertyNode(commonEntry, collectionProperty.PropertyName);
				if (commonPropNode != null)
				{
					var guids = from XmlNode objsurNode in commonPropNode.SafeSelectNodes(Objsur)
								   select objsurNode.GetStringAttribute(GuidStr);

					commonValues.UnionWith(guids);
				}
				var ourValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var ourPropNode = GetPropertyNode(ourEntry, collectionProperty.PropertyName);
				if (ourPropNode != null)
				{
					var guids = from XmlNode objsurNode in ourPropNode.SafeSelectNodes(Objsur)
								select objsurNode.GetStringAttribute(GuidStr);

					ourValues.UnionWith(guids);
					if (!commonValues.SetEquals(ourValues))
					{
						eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, ourEntry));
					}
				}
				var theirValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var theirPropNode = GetPropertyNode(theirEntry, collectionProperty.PropertyName);
				if (theirPropNode != null)
				{
					var guids = from XmlNode objsurNode in theirPropNode.SafeSelectNodes(Objsur)
								select objsurNode.GetStringAttribute(GuidStr);

					theirValues.UnionWith(guids);
					if (!commonValues.SetEquals(theirValues))
					{
						eventListener.ChangeOccurred(new XmlChangedRecordReport(null, null, commonEntry, theirEntry));
					}
				}

				// 0. If ours and theirs are the same, there is no conflict.
				if (ourValues.SetEquals(theirValues))
					continue; // NB: The merger will be told the prop is immutable, so it will not notice if the order is different.

				// 1. Keep ones that are in all three. (Excludes removed items.)
				var mergedCollection = new HashSet<string>(commonValues, StringComparer.OrdinalIgnoreCase);
				mergedCollection.IntersectWith(ourValues);
				mergedCollection.IntersectWith(theirValues);

				// 2. Re-add ones that were removed by one, but kept by the other (reference collections only).
				// It can't be done for owning collections, since the owned object would have been been deleted and we can't opt to keep it here.
				if (collectionProperty.DataType == DataType.ReferenceCollection)
				{
					mergedCollection.UnionWith(commonValues.Intersect(ourValues));
					mergedCollection.UnionWith(commonValues.Intersect(theirValues));
				}

				// 3. Add ones that either added.
				var ourAdditions = ourValues.Except(commonValues);
				mergedCollection.UnionWith(ourAdditions);
				var theirAdditions = theirValues.Except(commonValues);
				mergedCollection.UnionWith(theirAdditions);

				// 4. Update ours and theirs to the new collection.
				if (mergedCollection.Count == 0)
				{
					// Remove prop node from both.
					var gonerNode = GetPropertyNode(ourEntry, collectionProperty.PropertyName);
					if (gonerNode != null)
						gonerNode.ParentNode.RemoveChild(gonerNode);
					gonerNode = GetPropertyNode(theirEntry, collectionProperty.PropertyName);
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
						// TODO: This will be tricky for the new-style system with probable nested owning data.
						// TODO: For new-styled 'excluded' stuff, the prop will still have the objsur elements.
						// TODO: For new-styled included props, the prop will have the full objects,
						// TODO: so some way to merge possible changes to them will be needed.

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

		/// <summary>
		/// Bootstrap a merger for the new-styled (nested) files.
		/// </summary>
		/// <remarks>
		/// 1. A generic 'header' element will be handled, although it may not appear in the file.
		/// 2. All classes  will be included.
		/// 3. Merge strategies for class properties (regular or custom) will have keys of "classname+propname" to make them unique, system-wide.
		/// </remarks>
		internal static void BootstrapSystem(MetadataCache metadataCache, XmlMerger merger)
		{
			var sharedElementStrategies = new Dictionary<string, ElementStrategy>();
			CreateSharedElementStrategies(sharedElementStrategies);

			var strategiesForMerger = merger.MergeStrategies;

			foreach (var sharedKvp in sharedElementStrategies)
				strategiesForMerger.SetStrategy(sharedKvp.Key, sharedKvp.Value);

			var headerStrategy = CreateSingletonElementType(true);
			headerStrategy.ContextDescriptorGenerator = _contextGen;
			strategiesForMerger.SetStrategy(Header, headerStrategy);

			var classStrat = new ElementStrategy(false)
								{
									MergePartnerFinder = _guidKey,
									ContextDescriptorGenerator = _contextGen,
									IsAtomic = false
								};

			foreach (var classInfo in metadataCache.AllConcreteClasses)
			{
				strategiesForMerger.SetStrategy(classInfo.ClassName, classStrat);
				foreach (var propertyInfo in classInfo.AllProperties)
				{
					var isCustom = propertyInfo.IsCustomProperty;
					var propStrategy = isCustom
										? ElementStrategy.CreateForKeyedElement("name", false)
										: ElementStrategy.CreateSingletonElement();
					switch (propertyInfo.DataType)
					{
						case DataType.OwningCollection:
							propStrategy.IsImmutable = false; // Hard to really know, since some owning props can still have 'objsur' elements.
							break;
						// These two are immutable, in a manner of speaking.
						// DateCreated is honestly, and the other two are because 'ours' and 'theirs' have been made to be the same already.
						case DataType.Time: // Fall through // DateTime
						case DataType.ReferenceCollection:
							propStrategy.IsImmutable = true;
							break;

						case DataType.OwningSequence: // Fall through. // TODO: Sort out ownership issues for conflicts.
						case DataType.ReferenceSequence:
							// Use IsAtomic for whole property.
							propStrategy.IsAtomic = true;
							break;

						case DataType.OwningAtomic: // Fall through. // TODO: Think about implications of a conflict.
						case DataType.ReferenceAtomic:

						// Other data types
						case DataType.MultiUnicode:
						case DataType.MultiString:
						case DataType.Unicode: // Ordinary C# string
						case DataType.String: // TsString
						case DataType.Binary:
						case DataType.TextPropBinary:
						// NB: Booleans can never be in conflict in a 3-way merge environment.
						// One or the other can toggle the bool, so the one changing it 'wins'.
						// If both change it then it's no big deal either.
						case DataType.Boolean: // Fall through.
						case DataType.GenDate: // Fall through.
						case DataType.Guid: // Fall through.
						case DataType.Integer: // Fall through.
							// Simple, mutable properties.
							break;
					}
					strategiesForMerger.SetStrategy(String.Format("{0}{1}_{2}", isCustom ? "Custom_" : "", classInfo.ClassName, propertyInfo.PropertyName), propStrategy);
				}
			}
		}

		internal static void BootstrapSystem(MetadataCache mdc, Dictionary<string, ElementStrategy> sharedElementStrategies, Dictionary<string, XmlMerger> mergers, MergeSituation mergeSituation)
		{
			var strategy = new ElementStrategy(false)
			{
				MergePartnerFinder = _guidKey
			};
			strategy.AttributesToIgnoreForMerging.Add("class"); // Immutable
			strategy.AttributesToIgnoreForMerging.Add("guid"); // Immutable
			sharedElementStrategies.Add(Rt, strategy);
			strategy.ContextDescriptorGenerator = _contextGen;

			CreateSharedElementStrategies(sharedElementStrategies);
			CreateMergers(mdc, mergeSituation, sharedElementStrategies, mergers);
		}

		internal static void SetupMdc(MetadataCache mdc, MergeOrder mergeOrder, string customPropTargetDir, ushort levelsAboveCustomPropTargetDir)
		{
			if (mdc == null) throw new ArgumentNullException("mdc");
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");
			if (string.IsNullOrEmpty(customPropTargetDir)) throw new ArgumentException(AnnotationImages.kInvalidArgument, "customPropTargetDir");

			// Add optional custom property information to MDC.
			string mainCustomPropPathname;
			string altCustomPropPathname;
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				default:
					mainCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToOurs);
					altCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToTheirs);
					break;
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					mainCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToTheirs);
					altCustomPropPathname = Path.GetDirectoryName(mergeOrder.pathToOurs);
					break;
			}
			mdc.AddCustomPropInfo(mainCustomPropPathname, altCustomPropPathname, customPropTargetDir, levelsAboveCustomPropTargetDir);
		}
	}
}