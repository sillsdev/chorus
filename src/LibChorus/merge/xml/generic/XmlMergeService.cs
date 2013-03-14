using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Chorus.Utilities;
using Palaso.Code;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Service that manages Xml merging for formats that are basically a long list of the
	/// same element type, like a database table.
	/// </summary>
	public static class XmlMergeService
	{
		public static bool RemoveAmbiguousChildNodes = true;

		/// <summary>
		/// Add conflict.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict)
		{
			AddConflictToListener(listener, conflict, null, null, null);
		}

		/// <summary>
		/// Add conflict.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict, XmlNode oursContext,
												 XmlNode theirsContext, XmlNode ancestorContext)
		{
			AddConflictToListener(listener, conflict, oursContext, theirsContext, ancestorContext, new SimpleHtmlGenerator());
		}

		/// <summary>
		/// Add conflict.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict, XmlNode oursContext,
												 XmlNode theirsContext, XmlNode ancestorContext,
												 IGenerateHtmlContext htmlContextGenerator)
		{
			AddConflictToListener(listener, conflict, oursContext, theirsContext, ancestorContext, htmlContextGenerator, null, null);
		}

		/// <summary>
		/// Add conflict. If RecordContextInConflict fails to set a context, and nodeToFindGeneratorFrom is non-null,
		/// attempt to add a context based on the argument.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict, XmlNode oursContext,
												 XmlNode theirsContext, XmlNode ancestorContext,
												 IGenerateHtmlContext htmlContextGenerator, XmlMerger merger, XmlNode nodeToFindGeneratorFrom)
		{
			// NB: All these steps are crucially ordered.
			listener.RecordContextInConflict(conflict);
			if ((conflict.Context == null || conflict.Context is NullContextDescriptor) && nodeToFindGeneratorFrom != null)
			{
				// We are too far up the stack for the listener to have been told a context.
				// Make one out of the current node.
				conflict.Context = merger.GetContext(nodeToFindGeneratorFrom);
			}
			conflict.MakeHtmlDetails(oursContext, theirsContext, ancestorContext, htmlContextGenerator);
			listener.ConflictOccurred(conflict);
		}

		/// <summary>
		/// Add warning.
		/// </summary>
		public static void AddWarningToListener(IMergeEventListener listener, IConflict warning)
		{
			AddWarningToListener(listener, warning, null, null, null);
		}

		/// <summary>
		/// Add warning.
		/// </summary>
		public static void AddWarningToListener(IMergeEventListener listener, IConflict warning, XmlNode oursContext,
												 XmlNode theirsContext, XmlNode ancestorContext)
		{
			AddWarningToListener(listener, warning, oursContext, theirsContext, ancestorContext, new SimpleHtmlGenerator());
		}

		/// <summary>
		/// Add warning.
		/// </summary>
		public static void AddWarningToListener(IMergeEventListener listener, IConflict warning, XmlNode oursContext,
												XmlNode theirsContext, XmlNode ancestorContext,
												IGenerateHtmlContext htmlContextGenerator)
		{
			// NB: All three of these are crucially ordered.
			listener.RecordContextInConflict(warning);
			warning.MakeHtmlDetails(oursContext, theirsContext, ancestorContext, htmlContextGenerator);
			listener.WarningOccurred(warning);
		}

		/// <summary>
		/// Perform the 3-way merge.
		/// </summary>
		public static void Do3WayMerge(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
									   // Get from mergeOrder: IMergeEventListener listener,
									   bool sortRepeatingRecordOutputByKeyIdentifier,
									   string optionalFirstElementMarker,
									   string repeatingRecordElementName, string repeatingRecordKeyIdentifier)
		{
			// NB: The FailureSimulator is *only* used in tests.
			FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");

			Guard.AgainstNull(mergeStrategy, string.Format("'{0}' is null.", mergeStrategy));
			Guard.AgainstNull(mergeOrder, string.Format("'{0}' is null.", mergeOrder));
			Guard.AgainstNull(mergeOrder.EventListener, string.Format("'{0}' is null.", "mergeOrder.EventListener"));
			Guard.AgainstNullOrEmptyString(repeatingRecordElementName, "No primary record element name.");
			Guard.AgainstNullOrEmptyString(repeatingRecordKeyIdentifier, "No identifier attribute for primary record element.");

			var commonAncestorPathname = mergeOrder.pathToCommonAncestor;
			Require.That(File.Exists(commonAncestorPathname), string.Format("'{0}' does not exist.", commonAncestorPathname));

			string pathToWinner;
			string pathToLoser;
			string winnerId;
			string loserId;
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				case MergeOrder.ConflictHandlingModeChoices.WeWin:
					pathToWinner = mergeOrder.pathToOurs;
					pathToLoser = mergeOrder.pathToTheirs;
					winnerId = mergeOrder.MergeSituation.AlphaUserId;
					loserId = mergeOrder.MergeSituation.BetaUserId;
					break;
				default: //case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					pathToWinner = mergeOrder.pathToTheirs;
					pathToLoser = mergeOrder.pathToOurs;
					winnerId = mergeOrder.MergeSituation.BetaUserId;
					loserId = mergeOrder.MergeSituation.AlphaUserId;
					break;
			}
			Require.That(File.Exists(pathToWinner), string.Format("'{0}' does not exist.", pathToWinner));
			Require.That(File.Exists(pathToLoser), string.Format("'{0}' does not exist.", pathToLoser));
			SortedDictionary<string, string> sortedAttributes;
			var rootElementName = GetRootElementData(mergeOrder.pathToOurs, mergeOrder.pathToTheirs,
													 mergeOrder.pathToCommonAncestor, out sortedAttributes);
			EnsureCommonAncestorFileHasMinimalXmlContent(commonAncestorPathname, rootElementName, sortedAttributes);

			// Do main merge work.
			var allWritableData = DoMerge(mergeOrder, mergeStrategy,
										  sortRepeatingRecordOutputByKeyIdentifier, optionalFirstElementMarker,
										  repeatingRecordElementName, repeatingRecordKeyIdentifier,
										  pathToLoser, winnerId, pathToWinner, loserId, commonAncestorPathname);

			GC.Collect(2, GCCollectionMode.Forced); // Not nice, but required for the 164Meg ChorusNotes file.

			// Write all objects.
			WriteMainOutputData(allWritableData,
								mergeOrder.pathToOurs,
								// Do not change to another output file, or be ready to fix SyncScenarioTests.CanCollaborateOnLift()!
								optionalFirstElementMarker, rootElementName, sortedAttributes, mergeStrategy.SuppressIndentingChildren());
		}

		private static IDictionary<string, string> DoMerge(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
														   bool sortRepeatingRecordOutputByKeyIdentifier,
														   string optionalFirstElementMarker,
														   string repeatingRecordElementName,
														   string repeatingRecordKeyIdentifier,
														   string pathToLoser, string winnerId, string pathToWinner,
														   string loserId,
														   string commonAncestorPathname)
		{
			// Step 1. Load each of the three files.
			HashSet<string> allCommonAncestorIds;
			HashSet<string> allWinnerIds;
			HashSet<string> allLoserIds;
			Dictionary<string, string> allLoserData;
			Dictionary<string, string> allWinnerData;
			Dictionary<string, string> allCommonAncestorData;
			LoadDataFiles(mergeOrder, mergeStrategy,
						  optionalFirstElementMarker, repeatingRecordElementName, repeatingRecordKeyIdentifier,
						  commonAncestorPathname, pathToWinner, pathToLoser,
						  out allCommonAncestorData, out allCommonAncestorIds,
						  out allWinnerData, out allWinnerIds,
						  out allLoserData, out allLoserIds);

			// Step 2. Collect up new items from winner and loser and report relevant conflicts.
			var fluffedUpAncestorNodes = new Dictionary<string, XmlNode>();
			var fluffedUpLoserNodes = new Dictionary<string, XmlNode>();
			var fluffedUpWinnerNodes = new Dictionary<string, XmlNode>();
			var allNewIdsFromBoth = CollectDataAndReportEditConflictsForBothAddedNewObjectsWithDifferentContent(mergeOrder,
																												mergeStrategy,
																												pathToWinner,
																												fluffedUpWinnerNodes,
																												fluffedUpLoserNodes,
																												allCommonAncestorIds,
																												allWinnerIds,
																												allWinnerData,
																												allLoserData,
																												allLoserIds);

			// Step 3. Collect up deleted items from winner and loser.
			HashSet<string> allIdsRemovedByWinner;
			HashSet<string> allIdsRemovedByLoser;
			HashSet<string> allIdsRemovedByBoth;
			CollectDeletedIdsFromWinnerAndLoser(allLoserIds, allCommonAncestorIds, allWinnerIds,
												out allIdsRemovedByWinner, out allIdsRemovedByLoser, out allIdsRemovedByBoth);

			// Step 4. Collect up modified items from winner and loser and report all other conflicts.
			HashSet<string> allDeletedByLoserButEditedByWinnerIds;
			HashSet<string> allDeletedByWinnerButEditedByLoserIds;
			var allIdsForUniqueLoserChanges = CollectDataAndReportAllConflicts(mergeOrder, mergeStrategy, winnerId, loserId,
																			   allLoserIds, allWinnerData,
																			   allIdsRemovedByWinner, allIdsRemovedByLoser,
																			   allCommonAncestorIds, allCommonAncestorData,
																			   fluffedUpLoserNodes, allWinnerIds, allLoserData,
																			   fluffedUpWinnerNodes, fluffedUpAncestorNodes,
																			   pathToWinner,
																			   pathToLoser,
																			   out allDeletedByLoserButEditedByWinnerIds,
																			   out allDeletedByWinnerButEditedByLoserIds);

			// Step 5. Collect all ids that are to be written out.
			var allWritableIds = new HashSet<string>(allCommonAncestorIds, StringComparer.InvariantCultureIgnoreCase);
			allWritableIds.UnionWith(allWinnerIds);
			allWritableIds.UnionWith(allLoserIds);
			allWritableIds.UnionWith(allNewIdsFromBoth); // Adds new ones from winner & loser
			allWritableIds.ExceptWith(allIdsRemovedByWinner); // Removes deletions by winner.
			allWritableIds.ExceptWith(allIdsRemovedByLoser); // Removes deletions by loser.
			allWritableIds.ExceptWith(allIdsRemovedByBoth); // Removes deletions by both.
			allWritableIds.UnionWith(allDeletedByWinnerButEditedByLoserIds); // Puts back the loser edited vs winner deleted ids.
			allWritableIds.UnionWith(allDeletedByLoserButEditedByWinnerIds); // Puts back the winner edited vs loser deleted ids.
			// Write out data in sorted identifier order or 'ot luck' order.
			var allWritableData = sortRepeatingRecordOutputByKeyIdentifier
									  ? new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
									  : new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) as
										IDictionary<string, string>;
			foreach (var identifier in allWritableIds)
			{
				string updatedData;
				if (allIdsForUniqueLoserChanges.Contains(identifier))
				{
					// Loser made change. Winner did nothing.
					updatedData = allLoserData[identifier];
				}
				else
				{
					allWinnerData.TryGetValue(identifier, out updatedData);
					if (updatedData == null)
						updatedData = allLoserData[identifier];
				}
				allWinnerData.Remove(identifier);
				allLoserData.Remove(identifier);
				allWritableData.Add(identifier, updatedData);
			}

			return allWritableData;
		}

		private static HashSet<string> CollectDataAndReportAllConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
																		string winnerId, string loserId,
																		IEnumerable<string> allLoserIds,
																		IDictionary<string, string> allWinnerData,
																		IEnumerable<string> allIdsRemovedByWinner,
																		IEnumerable<string> allIdsRemovedByLoser,
																		HashSet<string> allCommonAncestorIds,
																		IDictionary<string, string> allCommonAncestorData,
																		IDictionary<string, XmlNode> fluffedUpLoserNodes,
																		IEnumerable<string> allWinnerIds,
																		IDictionary<string, string> allLoserData,
																		IDictionary<string, XmlNode> fluffedUpWinnerNodes,
																		IDictionary<string, XmlNode> fluffedUpAncestorNodes,
																		string pathToWinner,
																		string pathToLoser,
																		out HashSet<string>
																			allDeletedByLoserButEditedByWinnerIds,
																		out HashSet<string>
																			allDeletedByWinnerButEditedByLoserIds)
		{
			HashSet<string> allIdsForUniqueLoserChanges;
			HashSet<string> allIdsWinnerModified;
			var allIdsLoserModified = CollectDataAndReportNormalEditConflicts(mergeOrder, mergeStrategy, fluffedUpLoserNodes,
																			  allCommonAncestorData,
																			  fluffedUpAncestorNodes, fluffedUpWinnerNodes,
																			  allLoserData, allWinnerData, allWinnerIds,
																			  allLoserIds, allCommonAncestorIds,
																			  out allIdsForUniqueLoserChanges,
																			  out allIdsWinnerModified);

			// Step 5. Collect up items modified by one user, but deleted by the other.
			CollectIdsOfEditVsDelete(allIdsRemovedByLoser, allIdsWinnerModified, allIdsLoserModified, allIdsRemovedByWinner,
									 out allDeletedByWinnerButEditedByLoserIds, out allDeletedByLoserButEditedByWinnerIds);
			allIdsWinnerModified.Clear();
			allIdsLoserModified.Clear();

			// Step 6. Do merging and report conflicts.
			ReportDeleteVsEditConflicts(mergeOrder, mergeStrategy,
										fluffedUpLoserNodes, allLoserData, loserId, allDeletedByWinnerButEditedByLoserIds,
										allCommonAncestorData, fluffedUpAncestorNodes, pathToLoser);
			ReportEditVsDeleteConflicts(mergeOrder, mergeStrategy,
										fluffedUpWinnerNodes, allWinnerData, winnerId, allDeletedByLoserButEditedByWinnerIds,
										allCommonAncestorData, fluffedUpAncestorNodes, pathToWinner);

			return allIdsForUniqueLoserChanges;
		}

		private static HashSet<string> CollectDataAndReportNormalEditConflicts(MergeOrder mergeOrder,
																			   IMergeStrategy mergeStrategy,
																			   IDictionary<string, XmlNode>
																				   fluffedUpLoserNodes,
																			   IDictionary<string, string>
																				   allCommonAncestorData,
																			   IDictionary<string, XmlNode>
																				   fluffedUpAncestorNodes,
																			   IDictionary<string, XmlNode>
																				   fluffedUpWinnerNodes,
																			   IDictionary<string, string> allLoserData,
																			   IDictionary<string, string> allWinnerData,
																			   IEnumerable<string> allWinnerIds,
																			   IEnumerable<string> allLoserIds,
																			   HashSet<string> allCommonAncestorIds,
																			   out HashSet<string> allIdsForUniqueLoserChanges,
																			   out HashSet<string> allIdsWinnerModified)
		{
			HashSet<string> allIdsLoserModified;
			HashSet<string> allIdsWhereUsersMadeDifferentChanges;
			CollectModifiedIdsFromWinnerAndLoser(allWinnerData, allLoserData, allWinnerIds, allCommonAncestorIds, allLoserIds,
												 allCommonAncestorData, out allIdsWhereUsersMadeDifferentChanges,
												 out allIdsLoserModified, out allIdsWinnerModified,
												 out allIdsForUniqueLoserChanges);
			ReportNormalEditConflicts(mergeOrder, mergeStrategy, // current
									  allLoserData, fluffedUpAncestorNodes, allCommonAncestorData,
									  allIdsWhereUsersMadeDifferentChanges,
									  fluffedUpWinnerNodes, fluffedUpLoserNodes, allWinnerData);
			return allIdsLoserModified;
		}

		private static IEnumerable<string> CollectDataAndReportEditConflictsForBothAddedNewObjectsWithDifferentContent(
			MergeOrder mergeOrder, IMergeStrategy mergeStrategy, string pathToWinner,
			IDictionary<string, XmlNode> fluffedUpWinnerNodes,
			IDictionary<string, XmlNode> fluffedUpLoserNodes, HashSet<string> allCommonAncestorIds,
			IEnumerable<string> allWinnerIds, IDictionary<string, string> allWinnerData,
			IDictionary<string, string> allLoserData, IEnumerable<string> allLoserIds)
		{
			HashSet<string> allNewIdsFromBoth;
			HashSet<string> allNewIdsFromBothWithSameData;
			CollectNewItemsFromWinnerAndLoser(allWinnerData, allWinnerIds, allLoserIds, allLoserData, allCommonAncestorIds,
											  out allNewIdsFromBoth, out allNewIdsFromBothWithSameData, fluffedUpLoserNodes,
											  fluffedUpWinnerNodes);
			ReportEditConflictsForBothAddedNewObjectsWithDifferentContent(mergeOrder, mergeStrategy, pathToWinner,
																		  allLoserData, fluffedUpLoserNodes,
																		  allNewIdsFromBothWithSameData, allNewIdsFromBoth,
																		  allWinnerData, fluffedUpWinnerNodes);
			return allNewIdsFromBoth;
		}

		private static string GetRootElementData(string pathToOurs, string pathToTheirs, string pathToCommon,
												 out SortedDictionary<string, string> sortedAttributes)
		{
			return GetRootElementData(pathToOurs, out sortedAttributes)
				   ?? GetRootElementData(pathToTheirs, out sortedAttributes)
				   ?? GetRootElementData(pathToCommon, out sortedAttributes);
		}

		private static string GetRootElementData(string pathname, out SortedDictionary<string, string> sortedAttributes)
		{
			string rootElementName;
			sortedAttributes = new SortedDictionary<string, string>();
			try
			{
				using (
					var reader = XmlReader.Create(new FileStream(pathname, FileMode.Open),
												  CanonicalXmlSettings.CreateXmlReaderSettings()))
				{
					reader.MoveToContent();
					rootElementName = reader.Name;
					if (reader.HasAttributes)
					{
						reader.MoveToFirstAttribute();
						do
						{
							sortedAttributes.Add(reader.Name, reader.Value);
						} while (reader.MoveToNextAttribute());
					}
				}
			}
			catch (XmlException)
			{
				sortedAttributes = null;
				rootElementName = null;
			}
			return rootElementName;
		}

		private static void WriteMainOutputData(IDictionary<string, string> allWritableData,
												string outputPathname, string optionalFirstElementMarker,
												string rootElementName, SortedDictionary<string, string> sortedAttributes,
												HashSet<string> suppressIndentingChildren)
		{
			using (var writer = XmlWriter.Create(outputPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(rootElementName);
				foreach (var attrKvp in sortedAttributes)
				{
					writer.WriteAttributeString(attrKvp.Key, attrKvp.Value);
				}

				if (!string.IsNullOrEmpty(optionalFirstElementMarker))
				{
					// [NB: Write optional element first, if found.]
					if (allWritableData.ContainsKey(optionalFirstElementMarker))
						WriteNode(writer, allWritableData[optionalFirstElementMarker], suppressIndentingChildren);
					allWritableData.Remove(optionalFirstElementMarker);
				}
				foreach (var record in allWritableData.Values)
				{
					WriteNode(writer, record, suppressIndentingChildren);
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}

		private static void ReportEditVsDeleteConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
														IDictionary<string, XmlNode> fluffedUpWinnerNodes,
														IDictionary<string, string> allWinnerData,
														string winnerId,
														IEnumerable<string> allDeletedByLoserButEditedByWinnerIds,
														IDictionary<string, string> allCommonAncestorData,
														IDictionary<string, XmlNode> fluffedUpAncestorNodes,
														string pathToWinner)
		{
			foreach (var allDeletedByLoserButEditedByWinnerId in allDeletedByLoserButEditedByWinnerIds)
			{
				// Report winner edited vs loser deleted
				XmlNode commonNode;
				if (!fluffedUpAncestorNodes.TryGetValue(allDeletedByLoserButEditedByWinnerId, out commonNode))
				{
					commonNode = XmlUtilities.GetDocumentNodeFromRawXml(allCommonAncestorData[allDeletedByLoserButEditedByWinnerId],
																		new XmlDocument());
					fluffedUpAncestorNodes.Add(allDeletedByLoserButEditedByWinnerId, commonNode);
				}
				XmlNode winnerNode;
				if (!fluffedUpWinnerNodes.TryGetValue(allDeletedByLoserButEditedByWinnerId, out winnerNode))
				{
					winnerNode = XmlUtilities.GetDocumentNodeFromRawXml(allWinnerData[allDeletedByLoserButEditedByWinnerId],
																		new XmlDocument());
					fluffedUpWinnerNodes.Add(allDeletedByLoserButEditedByWinnerId, winnerNode);
				}
				var elementStrategy = mergeStrategy.GetElementStrategy(winnerNode);
				var generator = elementStrategy.ContextDescriptorGenerator;
				if (generator != null)
				{
					mergeOrder.EventListener.EnteringContext(generator.GenerateContextDescriptor(allWinnerData[allDeletedByLoserButEditedByWinnerId],
																								 pathToWinner));
				}
				AddConflictToListener(
					mergeOrder.EventListener,
					new EditedVsRemovedElementConflict(commonNode.Name, winnerNode, null, commonNode, mergeOrder.MergeSituation,
													   mergeStrategy.GetElementStrategy(commonNode), winnerId),
					winnerNode, null, commonNode, generator as IGenerateHtmlContext ?? new SimpleHtmlGenerator());
			}
		}

		private static void ReportDeleteVsEditConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
														IDictionary<string, XmlNode> fluffedUpLoserNodes,
														IDictionary<string, string> allLoserData, string loserId,
														IEnumerable<string> allDeletedByWinnerButEditedByLoserIds,
														IDictionary<string, string> allCommonAncestorData,
														IDictionary<string, XmlNode> fluffedUpAncestorNodes, string pathToLoser)
		{
			foreach (var allDeletedByWinnerButEditedByLoserId in allDeletedByWinnerButEditedByLoserIds)
			{
				// Report winner deleted vs loser edited.
				XmlNode commonNode;
				if (!fluffedUpAncestorNodes.TryGetValue(allDeletedByWinnerButEditedByLoserId, out commonNode))
				{
					commonNode = XmlUtilities.GetDocumentNodeFromRawXml(allCommonAncestorData[allDeletedByWinnerButEditedByLoserId],
																		new XmlDocument());
					fluffedUpAncestorNodes.Add(allDeletedByWinnerButEditedByLoserId, commonNode);
				}
				XmlNode loserNode;
				if (!fluffedUpLoserNodes.TryGetValue(allDeletedByWinnerButEditedByLoserId, out loserNode))
				{
					loserNode = XmlUtilities.GetDocumentNodeFromRawXml(allLoserData[allDeletedByWinnerButEditedByLoserId],
																	   new XmlDocument());
					fluffedUpLoserNodes.Add(allDeletedByWinnerButEditedByLoserId, loserNode);
				}
				var elementStrategy = mergeStrategy.GetElementStrategy(loserNode);
				var generator = elementStrategy.ContextDescriptorGenerator;
				if (generator != null)
				{
					mergeOrder.EventListener.EnteringContext(generator.GenerateContextDescriptor(allLoserData[allDeletedByWinnerButEditedByLoserId],
																								 pathToLoser));
				}

				AddConflictToListener(
					mergeOrder.EventListener,
					new RemovedVsEditedElementConflict(commonNode.Name, null, loserNode, commonNode, mergeOrder.MergeSituation,
													   mergeStrategy.GetElementStrategy(commonNode), loserId),
					null, loserNode, commonNode, generator as IGenerateHtmlContext ?? new SimpleHtmlGenerator());
			}
		}

		private static void ReportNormalEditConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
													  IDictionary<string, string> allLoserData,
													  IDictionary<string, XmlNode> fluffedUpAncestorNodes,
													  IDictionary<string, string> allCommonAncestorData,
													  IEnumerable<string> allIdsWhereUsersMadeDifferentChanges,
													  IDictionary<string, XmlNode> fluffedUpWinnerNodes,
													  IDictionary<string, XmlNode> fluffedUpLoserNodes,
													  IDictionary<string, string> allWinnerData)
		{
			foreach (var identifier in allIdsWhereUsersMadeDifferentChanges)
			{
				// Report normal edit conflicts for 'allIdsWhereUsersMadeDifferentChanges'
				XmlNode winnerNode;
				if (!fluffedUpWinnerNodes.TryGetValue(identifier, out winnerNode))
				{
					winnerNode = XmlUtilities.GetDocumentNodeFromRawXml(allWinnerData[identifier], new XmlDocument());
					fluffedUpWinnerNodes.Add(identifier, winnerNode);
				}
				XmlNode loserNode;
				if (!fluffedUpLoserNodes.TryGetValue(identifier, out loserNode))
				{
					loserNode = XmlUtilities.GetDocumentNodeFromRawXml(allLoserData[identifier], new XmlDocument());
					fluffedUpLoserNodes.Add(identifier, loserNode);
				}
				XmlNode commonNode;
				if (!fluffedUpAncestorNodes.TryGetValue(identifier, out commonNode))
				{
					commonNode = XmlUtilities.GetDocumentNodeFromRawXml(allCommonAncestorData[identifier], new XmlDocument());
					fluffedUpAncestorNodes.Add(identifier, commonNode);
				}
				var mergedResult = mergeStrategy.MakeMergedEntry(
					mergeOrder.EventListener,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin
						? winnerNode
						: loserNode,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin
						? loserNode
						: winnerNode,
					commonNode);
				allWinnerData[identifier] = mergedResult;
				fluffedUpWinnerNodes.Remove(identifier);
				allLoserData[identifier] = mergedResult;
					// They really are the same now, but is it a good idea to make them the same?
				fluffedUpLoserNodes.Remove(identifier);
				allCommonAncestorData[identifier] = mergedResult;
				// They really are the same now, but is it a good idea to make them the same?
				fluffedUpAncestorNodes.Remove(identifier);
			}
		}

		private static void ReportEditConflictsForBothAddedNewObjectsWithDifferentContent(
			MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
			string pathToWinner,
			IDictionary<string, string> allLoserData, IDictionary<string, XmlNode> fluffedUpLoserNodes,
			IEnumerable<string> allNewIdsFromBothWithSameData, IEnumerable<string> allNewIdsFromBoth,
			IDictionary<string, string> allWinnerData, IDictionary<string, XmlNode> fluffedUpWinnerNodes)
		{
			foreach (var identifier in allNewIdsFromBoth.Except(allNewIdsFromBothWithSameData))
			{
				//// These were added by both, but they do not have the same content.
				XmlNode winnerNode;
				if (!fluffedUpWinnerNodes.TryGetValue(identifier, out winnerNode))
				{
					winnerNode = XmlUtilities.GetDocumentNodeFromRawXml(allWinnerData[identifier], new XmlDocument());
					fluffedUpWinnerNodes.Add(identifier, winnerNode);
				}
				XmlNode loserNode;
				if (!fluffedUpLoserNodes.TryGetValue(identifier, out loserNode))
				{
					loserNode = XmlUtilities.GetDocumentNodeFromRawXml(allLoserData[identifier], new XmlDocument());
					fluffedUpLoserNodes.Add(identifier, loserNode);
				}

				var elementStrategy = mergeStrategy.GetElementStrategy(winnerNode);
				var generator = elementStrategy.ContextDescriptorGenerator;
				if (generator != null)
				{
					mergeOrder.EventListener.EnteringContext(generator.GenerateContextDescriptor(allWinnerData[identifier],
																								 pathToWinner));
				}
				var mergedResult = mergeStrategy.MakeMergedEntry(
					mergeOrder.EventListener,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin
						? winnerNode
						: loserNode,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin
						? loserNode
						: winnerNode,
					null);
				allWinnerData[identifier] = mergedResult;
				fluffedUpWinnerNodes.Remove(identifier);
				allLoserData[identifier] = mergedResult;
					// They really are the same now, but is it a good idea to make them the same?
				fluffedUpLoserNodes.Remove(identifier);
			}
		}

		private static void CollectIdsOfEditVsDelete(IEnumerable<string> allIdsRemovedByLoser,
													 IEnumerable<string> allIdsWinnerModified,
													 IEnumerable<string> allIdsLoserModified,
													 IEnumerable<string> allIdsRemovedByWinner,
													 out HashSet<string> allDeletedByWinnerButEditedByLoserIds,
													 out HashSet<string> allDeletedByLoserButEditedByWinnerIds)
		{
			allDeletedByWinnerButEditedByLoserIds = new HashSet<string>(allIdsRemovedByWinner.Intersect(allIdsLoserModified),
																		StringComparer.InvariantCultureIgnoreCase);
			allDeletedByLoserButEditedByWinnerIds = new HashSet<string>(allIdsRemovedByLoser.Intersect(allIdsWinnerModified),
																		StringComparer.InvariantCultureIgnoreCase);
		}

		private static void CollectModifiedIdsFromWinnerAndLoser(IDictionary<string, string> allWinnerData,
																 IDictionary<string, string> allLoserData,
																 IEnumerable<string> allWinnerIds,
																 HashSet<string> allCommonAncestorIds,
																 IEnumerable<string> allLoserIds,
																 IDictionary<string, string> allCommonAncestorData,
																 out HashSet<string> allIdsWhereUsersMadeDifferentChanges,
																 out HashSet<string> allIdsLoserModified,
																 out HashSet<string> allIdsWinnerModified,
																 out HashSet<string> allIdsForUniqueLoserChanges)
		{
			allIdsWinnerModified = new HashSet<string>(allCommonAncestorIds
														   .Intersect(allWinnerIds)
														   .Where(
															   identifier =>
															   !XmlUtilities.AreXmlElementsEqual(allCommonAncestorData[identifier],
																								 allWinnerData[identifier])),
													   StringComparer.InvariantCultureIgnoreCase);
			allIdsLoserModified = new HashSet<string>(allCommonAncestorIds
														  .Intersect(allLoserIds)
														  .Where(
															  identifier =>
															  !XmlUtilities.AreXmlElementsEqual(allCommonAncestorData[identifier],
																								allLoserData[identifier])),
													  StringComparer.InvariantCultureIgnoreCase);
			var allBothUsersMadeSameChanges = new HashSet<string>(allIdsWinnerModified
																	  .Intersect(allIdsLoserModified)
																	  .Where(
																		  identifier =>
																		  XmlUtilities.AreXmlElementsEqual(allWinnerData[identifier],
																										   allLoserData[identifier])),
																  StringComparer.InvariantCultureIgnoreCase);
			allIdsWhereUsersMadeDifferentChanges = new HashSet<string>(allIdsWinnerModified
																		   .Intersect(allIdsLoserModified)
																		   .Where(
																			   identifier =>
																			   !XmlUtilities.AreXmlElementsEqual(
																				   allWinnerData[identifier], allLoserData[identifier])),
																	   StringComparer.InvariantCultureIgnoreCase);
			allIdsForUniqueLoserChanges =
				new HashSet<string>(
					allIdsLoserModified.Except(allBothUsersMadeSameChanges).Except(allIdsWhereUsersMadeDifferentChanges));
		}

		private static void CollectDeletedIdsFromWinnerAndLoser(
			IEnumerable<string> allLoserIds, HashSet<string> allCommonAncestorIds, IEnumerable<string> allWinnerIds,
			out HashSet<string> allIdsRemovedByWinner, out HashSet<string> allIdsRemovedByLoser,
			out HashSet<string> allIdsRemovedByBoth)
		{
			allIdsRemovedByWinner =
				new HashSet<string>(allCommonAncestorIds.Except(allWinnerIds, StringComparer.InvariantCultureIgnoreCase));
			allIdsRemovedByLoser =
				new HashSet<string>(allCommonAncestorIds.Except(allLoserIds, StringComparer.InvariantCultureIgnoreCase));
			allIdsRemovedByBoth = new HashSet<string>(allIdsRemovedByWinner.Intersect(allIdsRemovedByLoser));
		}

		private static void LoadDataFiles(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
										  string optionalFirstElementMarker, string repeatingRecordElementName,
										  string repeatingRecordKeyIdentifier,
										  string commonAncestorPathname, string pathToWinner, string pathToLoser,
										  out Dictionary<string, string> allCommonAncestorData,
										  out HashSet<string> allCommonAncestorIds,
										  out Dictionary<string, string> allWinnerData, out HashSet<string> allWinnerIds,
										  out Dictionary<string, string> allLoserData, out HashSet<string> allLoserIds)
		{
			allCommonAncestorData = MakeRecordDictionary(mergeOrder.EventListener, mergeStrategy,
														 commonAncestorPathname, false, optionalFirstElementMarker,
														 repeatingRecordElementName, repeatingRecordKeyIdentifier);
			allCommonAncestorIds = new HashSet<string>(allCommonAncestorData.Keys, StringComparer.InvariantCultureIgnoreCase);
			allWinnerData = MakeRecordDictionary(mergeOrder.EventListener, mergeStrategy,
												 pathToWinner, true, optionalFirstElementMarker,
												 repeatingRecordElementName, repeatingRecordKeyIdentifier);
			allWinnerIds = new HashSet<string>(allWinnerData.Keys, StringComparer.InvariantCultureIgnoreCase);
			allLoserData = MakeRecordDictionary(mergeOrder.EventListener, mergeStrategy,
												pathToLoser, true, optionalFirstElementMarker, repeatingRecordElementName,
												repeatingRecordKeyIdentifier);
			allLoserIds = new HashSet<string>(allLoserData.Keys, StringComparer.InvariantCultureIgnoreCase);
		}

		private static void CollectNewItemsFromWinnerAndLoser(IDictionary<string, string> allWinnerData,
															  IEnumerable<string> allWinnerIds,
															  IEnumerable<string> allLoserIds,
															  IDictionary<string, string> allLoserData,
															  HashSet<string> allCommonAncestorIds,
															  out HashSet<string> allNewIdsFromBoth,
															  out HashSet<string> allNewIdsFromBothWithSameData,
															  IDictionary<string, XmlNode> fluffedUpLoserNodes,
															  IDictionary<string, XmlNode> fluffedUpWinnerNodes)
		{
			var allNewIdsFromWinner = new HashSet<string>(allWinnerIds, StringComparer.InvariantCultureIgnoreCase);
			allNewIdsFromWinner.ExceptWith(allCommonAncestorIds);
			var allNewIdsFromLoser = new HashSet<string>(allLoserIds, StringComparer.InvariantCultureIgnoreCase);
			allNewIdsFromLoser.ExceptWith(allCommonAncestorIds);
			// Step 2A. Should be quite rare, and they may be the same or different.
			allNewIdsFromBoth =
				new HashSet<string>(allNewIdsFromWinner.Intersect(allNewIdsFromLoser, StringComparer.InvariantCultureIgnoreCase));
			allNewIdsFromBothWithSameData = new HashSet<string>(allNewIdsFromBoth
																	.Where(identifier =>
																		{
																			XmlNode winnerNode;
																			if (
																				!fluffedUpWinnerNodes.TryGetValue(identifier,
																												  out winnerNode))
																			{
																				winnerNode =
																					XmlUtilities.GetDocumentNodeFromRawXml(
																						allWinnerData[identifier], new XmlDocument());
																				fluffedUpWinnerNodes.Add(identifier, winnerNode);
																			}
																			XmlNode loserNode;
																			if (
																				!fluffedUpLoserNodes.TryGetValue(identifier,
																												 out loserNode))
																			{
																				loserNode =
																					XmlUtilities.GetDocumentNodeFromRawXml(
																						allLoserData[identifier], new XmlDocument());
																				fluffedUpLoserNodes.Add(identifier, loserNode);
																			}
																			return XmlUtilities.AreXmlElementsEqual(winnerNode,
																													loserNode);
																		}), StringComparer.InvariantCultureIgnoreCase);

			allNewIdsFromWinner.ExceptWith(allNewIdsFromBothWithSameData);
			allNewIdsFromLoser.ExceptWith(allNewIdsFromBothWithSameData);
		}

		private static Dictionary<string, string> MakeRecordDictionary(IMergeEventListener mainMergeEventListener,
			IMergeStrategy mergeStrategy,
			string pathname,
			bool removeAmbiguousChildren,
			string firstElementMarker,
			string recordStartingTag,
			string identifierAttribute)
		{
			var records = new Dictionary<string, string>(EstimatedObjectCount(pathname),
														 StringComparer.InvariantCultureIgnoreCase);
			using (var fastSplitter = new FastXmlElementSplitter(pathname))
			{
				bool foundOptionalFirstElement;
				foreach (var record in fastSplitter.GetSecondLevelElementStrings(firstElementMarker, recordStartingTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						var key = firstElementMarker.ToLowerInvariant();
						if (records.ContainsKey(key))
						{
							mainMergeEventListener.WarningOccurred(
								new MergeWarning(string.Format("{0}: There is more than one optional first element '{1}'", pathname, key)));
						}
						else
						{
							if (removeAmbiguousChildren)
							{
								var possiblyRevisedRecord = RemoveAmbiguousChildren(mainMergeEventListener, mergeStrategy.GetStrategies(), record);
								records.Add(key, possiblyRevisedRecord);
							}
							else
							{
								records.Add(key, record);
							}
						}
						foundOptionalFirstElement = false;
					}
					else
					{
						var attrValues = XmlUtils.GetAttributes(record, new HashSet<string> {"dateDeleted", identifierAttribute});

						// Eat tombstones.
						if (attrValues["dateDeleted"] != null)
							continue;

						var identifier = attrValues[identifierAttribute];
						if (string.IsNullOrEmpty(identifierAttribute))
						{
							mainMergeEventListener.WarningOccurred(
								new MergeWarning(string.Format("{0}: There was no identifier for the record", pathname)));
							continue;
						}
						if (records.ContainsKey(identifier))
						{
							mainMergeEventListener.WarningOccurred(
								new MergeWarning(string.Format("{0}: There is more than one element with the identifier '{1}'", pathname,
															   identifier)));
						}
						else
						{
							if (removeAmbiguousChildren)
							{
								var possiblyRevisedRecord = RemoveAmbiguousChildren(mainMergeEventListener, mergeStrategy.GetStrategies(), record);
								records.Add(identifier, possiblyRevisedRecord);
							}
							else
							{
								records.Add(identifier, record);
							}
						}
					}
				}
			}

			return records;
		}

		/// <summary>
		/// <para>Before we attempt to compare a node with a corresponding one from another revision,
		/// we want to make sure that it is in a good state for matching its children with the children of the corresponding node.</para>
		///
		/// <para>To do this, for each child, we allow the Finder which will be used to find a corresponding child in
		/// another revision to provide a query, which can be used to check whether the parent node has 'ambiguous' children,
		/// that is, groups of children which the finder would consider indistinguisable.</para>
		/// </summary>
		/// <remarks>
		/// This is recursive, since it moves on down to all child nodes.
		/// </remarks>
		public static string RemoveAmbiguousChildren(IMergeEventListener eventListener,
			MergeStrategies mergeStrategies,
			string parent)
		{
			if (!RemoveAmbiguousChildNodes)
				return parent;
			var parentNode = XmlUtilities.GetDocumentNodeFromRawXml(parent, new XmlDocument());
			RemoveAmbiguousChildren(
				eventListener,
				mergeStrategies,
				parentNode);
			return parentNode.OuterXml;
		}

		/// <summary>
		/// <para>Before we attempt to compare a node with a corresponding one from another revision,
		/// we want to make sure that it is in a good state for matching its children with the children of the corresponding node.</para>
		///
		/// <para>To do this, for each child, we allow the Finder which will be used to find a corresponding child in
		/// another revision to provide a query, which can be used to check whether the parent node has 'ambiguous' children,
		/// that is, groups of children which the finder would consider indistinguisable.</para>
		/// </summary>
		/// <remarks>
		/// This is recursive, since it moves on down to all child nodes.
		/// </remarks>
		public static void RemoveAmbiguousChildren(IMergeEventListener eventListener,
			MergeStrategies mergeStrategies,
			XmlNode parent)
		{
			if (parent == null || !parent.HasChildNodes || !RemoveAmbiguousChildNodes)
				return;

			var elementStrat = mergeStrategies.GetElementStrategy(parent);
			if (elementStrat.IsImmutable)
				return;
			var ambiguousNodes = new List<XmlNode>();
			foreach (XmlNode childNode in parent.ChildNodes)
			{
				if (ambiguousNodes.Contains(childNode))
					continue; // Already found it, so don't bother processing it again.

				elementStrat = mergeStrategies.GetElementStrategy(childNode);
				if (elementStrat.IsImmutable)
					continue;
				var finder = elementStrat.MergePartnerFinder;
				if (!(finder is IFindMatchingNodesToMerge))
					continue;
				var finderOfMultiples = (IFindMatchingNodesToMerge) finder;
				var matches = finderOfMultiples.GetMatchingNodes(childNode, parent).ToList();
				if (matches.Count < 2)
					continue; // No duplicates were found, so keep going.

				// The following code implements the "DropAmbiguitiesAndAddErrorNote" option of a possible three in "AmbiguousSiblingPolicy"
				// TODO (maybe): If we ever implement the "ThrowException" option, then throw here instead of adding the warning and eating the ambiguities.
				// TODO (maybe): If we ever implement the "LiveWithIt" option, then just do a return at the start of this method, or don't bother calling the method at all.
				// Since they are ambiguities (at least as far as the finder is concerned),
				// we really only need one warning. Add the count, so the user knows how many there were, though.
				// Possible enhancement: Generate reports that go into the details of what might be different between the keeper and each ambiguous sibling.
				AddWarningToListener(eventListener,
									 new MergeWarning(string.Format("Parent element '{0}' contains {1} ambiguous child elements. Only the first one is retained. Details: {2}.",
																	parent.Name,
																	matches.Count,
																	finderOfMultiples.GetWarningMessageForAmbiguousNodes(matches[0]))));
				// Add all but the current one (the first one), so they can be removed in the next loop.
				ambiguousNodes.AddRange(matches.Where(match => match != childNode));
			}

			foreach (var ambiguousNode in ambiguousNodes)
			{
				// Remove all but the first one from the parent.
				parent.RemoveChild(ambiguousNode);
			}

			foreach (XmlNode childNode in parent.ChildNodes)
			{
				// Drill on down the child nodes to see if there are any other ambiguities in lower-level nodes.
				// Since the ambiguous nodes have been removed from parent, they won't get processed again.
				RemoveAmbiguousChildren(eventListener, mergeStrategies, childNode);
			}
		}

		private static int EstimatedObjectCount(string pathname)
		{
			const int estimatedObjectSize = 400;
			var fileInfo = new FileInfo(pathname);
			return (int)(fileInfo.Length / estimatedObjectSize);
		}

		private static void EnsureCommonAncestorFileHasMinimalXmlContent(string commonAncestorPathname, string rootElementName, SortedDictionary<string, string> sortedAttributes)
		{
			using(var reader = new XmlTextReader(commonAncestorPathname))
			{
				try
				{
					reader.Read();
					return;
				}
				catch (XmlException)
				{
					//we need to build the ancestor document if it was empty of xml
				}
			}
			BuildAncestorDocument(commonAncestorPathname, rootElementName, sortedAttributes);
		}

		private static void BuildAncestorDocument(string commonAncestorDoc, string rootElementName, SortedDictionary<string, string> sortedAttributes)
		{
			using(var writer = XmlWriter.Create(commonAncestorDoc))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(rootElementName);
				foreach (var attrKvp in sortedAttributes)
				{
					writer.WriteAttributeString(attrKvp.Key, attrKvp.Value);
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}

		/// <summary>
		/// Write a node out containing the XML in dataToWrite, pretty-printed according to the rules of writer, except
		/// that we suppress indentation for children of nodes whose names are listed in suppressIndentingChildren,
		/// and also for "mixed" nodes (where some children are text).
		/// </summary>
		internal static void WriteNode(XmlWriter writer, string dataToWrite, HashSet<string> suppressIndentingChildren)
		{
			// <mergenotice>
			// When the WeSay1.3 branch gets merged, do this:
			// 1. Pick the 'suppressIndentingChildren' parameter, not the temporary partial port of "CurrentSuppressIndentingChildren"
			// 2. Remove this <mergenotice> comment and its 'end tag' comment.
			XmlUtils.WriteNode(writer, dataToWrite, suppressIndentingChildren);
			// </mergenotice>

			// This is the original code of this method. It is probably more efficient, and does ALMOST the same thing. But not quite.
			// See the unit tests WriteNode_DoesNotIndentFirstChildOfMixedNode and WriteNode_DoesNotIndentChildWhenSuppressed for WriteNode.
			// If a mixed (text and element children) node has an element as its FIRST
			// child, WriteNode will indent it. This is wrong, since it adds a newline and tabs to the body of a parent where text is significant.
			// Even if a node is not mixed, it may be wrong to indent it, if white space is significant.
			//using (var nodeReader = XmlReader.Create(new MemoryStream(Utf8.GetBytes(dataToWrite)), CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Fragment)))
			//{
			//    writer.WriteNode(nodeReader, false);
			//}
		}
	}
}