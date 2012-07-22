using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Service that manages Xml merging.
	/// </summary>
	public static class XmlMergeService
	{
		private static readonly XmlReaderSettings ReaderSettings = new XmlReaderSettings
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Fragment,
					ProhibitDtd = true,
					ValidationType = ValidationType.None,
					CloseInput = true,
					IgnoreWhitespace = true
				};

		private static readonly Encoding Utf8 = Encoding.UTF8;

		/// <summary>
		/// Do some repeated lines in one place.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict)
		{
			AddConflictToListener(listener, conflict, null, null, null);
		}

		/// <summary>
		/// Do some repeated lines in one place.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict, XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext)
		{
			AddConflictToListener(listener, conflict, oursContext, theirsContext, ancestorContext, new SimpleHtmlGenerator());
		}

		/// <summary>
		/// Do some repeated lines in one place.
		/// </summary>
		public static void AddConflictToListener(IMergeEventListener listener, IConflict conflict, XmlNode oursContext, XmlNode theirsContext, XmlNode ancestorContext, IGenerateHtmlContext htmlContextGenerator)
		{
			// NB: All three of these are crucially ordered.
			listener.RecordContextInConflict(conflict);
			conflict.MakeHtmlDetails(oursContext, theirsContext, ancestorContext, htmlContextGenerator);
			listener.ConflictOccurred(conflict);
		}

		/// <summary>
		/// Perform the 3-way merge.
		/// </summary>
		public static void Do3WayMerge(MergeOrder mergeOrder, IMergeStrategy mergeStrategy, // Get from mergeOrder: IMergeEventListener listener,
			bool sortRepeatingRecordOutputByKeyIdentifier,
			string optionalFirstElementMarker,
			string repeatingRecordElementName, string repeatingRecordKeyIdentifier,
			Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate)
		{
			// NB: The FailureSimulator is *only* used in tests.
			FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");

			Guard.AgainstNull(mergeStrategy, string.Format("'{0}' is null.", mergeStrategy));
			Guard.AgainstNull(mergeOrder, string.Format("'{0}' is null.", mergeOrder));
			Guard.AgainstNull(mergeOrder.EventListener, string.Format("'{0}' is null.", "mergeOrder.EventListener"));
			Guard.AgainstNull(writePreliminaryInformationDelegate, string.Format("'{0}' is null.", writePreliminaryInformationDelegate));
			Guard.AgainstNullOrEmptyString(repeatingRecordElementName, "No primary record element name.");
			Guard.AgainstNullOrEmptyString(repeatingRecordKeyIdentifier, "No identifier attribute for primary record element.");

			var commonAncestorPathname = mergeOrder.pathToCommonAncestor;
			Require.That(File.Exists(commonAncestorPathname), string.Format("'{0}' does not exist.", commonAncestorPathname));
			// Do not change outputPathname, or be ready to fix SyncScenarioTests.CanCollaborateOnLift()!
			var outputPathname = mergeOrder.pathToOurs;

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
			EnsureCommonAncestorFileHasMimimalContent(commonAncestorPathname, pathToWinner);

			// Step 1. Load each of the three files.
			Dictionary<string, string> allLoserData;
			HashSet<string> allLoserIds;
			Dictionary<string, string> allCommonAncestorData;
			HashSet<string> allCommonAncestorIds;
			HashSet<string> allWinerIds;
			Dictionary<string, string> allWinnerData;
			LoadDataFiles(mergeOrder,
				optionalFirstElementMarker, repeatingRecordElementName, repeatingRecordKeyIdentifier,
				commonAncestorPathname, pathToWinner, pathToLoser,
				out allCommonAncestorData, out allCommonAncestorIds,
				out allWinnerData, out allWinerIds,
				out allLoserData, out allLoserIds);

			// Step 2. Collect up new items from winner and loser.
			HashSet<string> allNewIdsFromBoth;
			HashSet<string> allNewIdsFromBothWithSameData;
			Dictionary<string, XmlNode> fluffedUpLoserNodes;
			Dictionary<string, XmlNode> fluffedUpWinnerNodes;
			CollectNewItemsFromWinnerAndLoser(allWinnerData, allWinerIds, allLoserIds, allLoserData, allCommonAncestorIds,
				out allNewIdsFromBoth, out allNewIdsFromBothWithSameData, out fluffedUpLoserNodes, out fluffedUpWinnerNodes);

			// Step 3. Collect up deleted items from winner and loser.
			HashSet<string> allIdsRemovedByWinner;
			HashSet<string> allIdsRemovedByLoser;
			HashSet<string> allIdsRemovedByBoth;
			CollectDeletedIdsFromWinnerAndLoser(allLoserIds, allCommonAncestorIds, allWinerIds,
				out allIdsRemovedByWinner, out allIdsRemovedByLoser, out allIdsRemovedByBoth);

			// Step 4. Collect up modified items from winner and loser.
			HashSet<string> allIdsWhereUsersMadeDifferentChanges;
			HashSet<string> allIdsLoserModified;
			HashSet<string> allIdsWinnerModified;
			HashSet<string> allBothUsersMadeSameChanges;
			HashSet<string> allIdsForUniqueLoserChanges;
			CollectModifiedIdsFromWinnerAndLoser(allWinnerData, allLoserData, allWinerIds, allCommonAncestorIds, allLoserIds, allCommonAncestorData, out allIdsWhereUsersMadeDifferentChanges, out allIdsLoserModified, out allIdsWinnerModified, out allBothUsersMadeSameChanges, out allIdsForUniqueLoserChanges);

			// Step 5. Collect up items modified by one user, but deleted by the other.
			HashSet<string> allDeletedByWinnerButEditedByLoserIds;
			HashSet<string> allDeletedByLoserButEditedByWinnerIds;
			CollectIdsOfEditVsDelete(allIdsRemovedByLoser, allIdsWinnerModified, allIdsLoserModified, allIdsRemovedByWinner, out allDeletedByWinnerButEditedByLoserIds, out allDeletedByLoserButEditedByWinnerIds);

			// Step 6. Do merging and report conflcits.
			var fluffedUpAncestorNodes = new Dictionary<string, XmlNode>();
			ReportEditConflictsForBothAddedNewObjectsWithDifferentContent(mergeOrder, mergeStrategy, pathToWinner,
				allLoserData, fluffedUpLoserNodes,
				allNewIdsFromBothWithSameData, allNewIdsFromBoth, allWinnerData, fluffedUpWinnerNodes);
			ReportNormalEditConflicts(mergeOrder, mergeStrategy,
				allLoserData, fluffedUpAncestorNodes, allCommonAncestorData, allIdsWhereUsersMadeDifferentChanges,
				fluffedUpWinnerNodes, fluffedUpLoserNodes, allWinnerData);
			ReportDeleteVsEditConflicts(mergeOrder, mergeStrategy,
				fluffedUpLoserNodes, allLoserData, loserId, allDeletedByWinnerButEditedByLoserIds,
				allCommonAncestorData, fluffedUpAncestorNodes);
			ReportEditVsDeleteConflicts(mergeOrder, mergeStrategy,
				fluffedUpWinnerNodes, allWinnerData, winnerId, allDeletedByLoserButEditedByWinnerIds,
				allCommonAncestorData, fluffedUpAncestorNodes);
			fluffedUpAncestorNodes.Clear();
			fluffedUpWinnerNodes.Clear();
			fluffedUpLoserNodes.Clear();

			// Step 7. Collect all ids that are to be written out.
			var allWritableIds = new HashSet<string>(allCommonAncestorIds, StringComparer.InvariantCultureIgnoreCase);
			allWritableIds.UnionWith(allWinerIds);
			allWritableIds.UnionWith(allLoserIds);
			allWritableIds.UnionWith(allNewIdsFromBoth); // Adds new ones from winner & loser
			allWritableIds.ExceptWith(allIdsRemovedByWinner); // Removes deletions by winner.
			allWritableIds.ExceptWith(allIdsRemovedByLoser); // Removes deletions by loser.
			allWritableIds.ExceptWith(allIdsRemovedByBoth); // Removes deletions by both.
			allWritableIds.UnionWith(allDeletedByWinnerButEditedByLoserIds); // Puts back the loser edited vs winner deleted ids.
			allWritableIds.UnionWith(allDeletedByLoserButEditedByWinnerIds); // Puts back the winner edited vs loser deleted ids.
			var allWritableData = sortRepeatingRecordOutputByKeyIdentifier ? new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) : new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) as IDictionary<string, string>;
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
				allWritableData.Add(identifier, updatedData);
			}

			// Step 8. Clear out most dictionaries and hash sets.
			allCommonAncestorData = null;
			allCommonAncestorIds = null;
			allWinnerData = null;
			allWinerIds = null;
			allLoserData = null;
			allLoserIds = null;
			allNewIdsFromBoth = null;
			allNewIdsFromBothWithSameData = null;
			allIdsRemovedByWinner = null;
			allIdsRemovedByLoser = null;
			allIdsRemovedByBoth = null;
			allIdsWhereUsersMadeDifferentChanges = null;
			allIdsLoserModified = null;
			allIdsWinnerModified = null;
			allBothUsersMadeSameChanges = null;
			allIdsForUniqueLoserChanges = null;
			allDeletedByWinnerButEditedByLoserIds = null;
			allDeletedByLoserButEditedByWinnerIds = null;
			GC.Collect(2, GCCollectionMode.Forced);

			// Step 9. Write all objects.
			WriteMainOutputData(optionalFirstElementMarker, writePreliminaryInformationDelegate, allWritableData, commonAncestorPathname, outputPathname);
		}

		private static void WriteMainOutputData(string optionalFirstElementMarker, Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate,
												IDictionary<string, string> allWritableData, string commonAncestorPathname,
												string outputPathname)
		{
			using (var writer = XmlWriter.Create(outputPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				using (var reader = XmlReader.Create(new FileStream(commonAncestorPathname, FileMode.Open), new XmlReaderSettings
								{
									CheckCharacters = false,
									ConformanceLevel = ConformanceLevel.Document,
									ProhibitDtd = true,
									ValidationType = ValidationType.None,
									CloseInput = true,
									IgnoreWhitespace = true}))
				{
					// This must be client specific behavior to rebuild the root element, plus any of its attributes.
					writePreliminaryInformationDelegate(reader, writer);
				}

				if (!string.IsNullOrEmpty(optionalFirstElementMarker))
				{
					// [NB: Write optional element first, if found.]
					if (allWritableData.ContainsKey(optionalFirstElementMarker))
					{
						WriteNode(writer, allWritableData[optionalFirstElementMarker]);
						allWritableData.Remove(optionalFirstElementMarker);
					}
				}
				foreach (var dataKvp in allWritableData)
				{
					WriteNode(writer, dataKvp.Value);
				}
			}
		}

		private static void ReportEditVsDeleteConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
														IDictionary<string, XmlNode> fluffedUpWinnerNodes, IDictionary<string, string> allWinnerData,
														string winnerId, IEnumerable<string> allDeletedByLoserButEditedByWinnerIds,
														IDictionary<string, string> allCommonAncestorData, IDictionary<string, XmlNode> fluffedUpAncestorNodes)
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
				AddConflictToListener(
					mergeOrder.EventListener,
					new EditedVsRemovedElementConflict(commonNode.Name, winnerNode, null, commonNode, mergeOrder.MergeSituation,
													   mergeStrategy.GetElementStrategy(commonNode), winnerId),
					winnerNode, winnerNode, commonNode);
			}
		}

		private static void ReportDeleteVsEditConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
														IDictionary<string, XmlNode> fluffedUpLoserNodes, IDictionary<string, string> allLoserData, string loserId,
														IEnumerable<string> allDeletedByWinnerButEditedByLoserIds,
														IDictionary<string, string> allCommonAncestorData, IDictionary<string, XmlNode> fluffedUpAncestorNodes)
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
				AddConflictToListener(
					mergeOrder.EventListener,
					new RemovedVsEditedElementConflict(commonNode.Name, null, loserNode, commonNode, mergeOrder.MergeSituation,
													   mergeStrategy.GetElementStrategy(commonNode), loserId),
					null, loserNode, commonNode);
			}
		}

		private static void ReportNormalEditConflicts(MergeOrder mergeOrder, IMergeStrategy mergeStrategy,
													  IDictionary<string, string> allLoserData, IDictionary<string, XmlNode> fluffedUpAncestorNodes,
													  IDictionary<string, string> allCommonAncestorData,
													  IEnumerable<string> allIdsWhereUsersMadeDifferentChanges,
													  IDictionary<string, XmlNode> fluffedUpWinnerNodes, IDictionary<string, XmlNode> fluffedUpLoserNodes,
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
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin ? winnerNode : loserNode,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin ? loserNode : winnerNode,
					commonNode);
				allWinnerData[identifier] = mergedResult;
				fluffedUpWinnerNodes.Remove(identifier);
				allLoserData[identifier] = mergedResult; // They really are the same now, but is it a good idea to make them the same?
				fluffedUpLoserNodes.Remove(identifier);
				allCommonAncestorData[identifier] = mergedResult;
					// They really are the same now, but is it a good idea to make them the same?
				fluffedUpAncestorNodes.Remove(identifier);
			}
		}

		private static void ReportEditConflictsForBothAddedNewObjectsWithDifferentContent(MergeOrder mergeOrder, IMergeStrategy mergeStrategy, string pathToWinner,
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
					mergeOrder.EventListener.EnteringContext(generator.GenerateContextDescriptor(allWinnerData[identifier], pathToWinner));
				}
				//AddConflictToListener(
				//    mergeOrder.EventListener,
				//    new BothAddedMainElementButWithDifferentContentConflict(
				//        winnerNode.Name,
				//        winnerNode,
				//        loserNode,
				//        mergeOrder.MergeSituation,
				//        elementStrategy,
				//        winnerId),
				//    winnerNode,
				//    loserNode,
				//    null);
				var mergedResult = mergeStrategy.MakeMergedEntry(
					mergeOrder.EventListener,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin ? winnerNode : loserNode,
					mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin ? loserNode : winnerNode,
					null);
				allWinnerData[identifier] = mergedResult;
				fluffedUpWinnerNodes.Remove(identifier);
				allLoserData[identifier] = mergedResult; // They really are the same now, but is it a good idea to make them the same?
				fluffedUpLoserNodes.Remove(identifier);
			}
		}

		private static void CollectIdsOfEditVsDelete(IEnumerable<string> allIdsRemovedByLoser, IEnumerable<string> allIdsWinnerModified,
													 IEnumerable<string> allIdsLoserModified, IEnumerable<string> allIdsRemovedByWinner,
													 out HashSet<string> allDeletedByWinnerButEditedByLoserIds,
													 out HashSet<string> allDeletedByLoserButEditedByWinnerIds)
		{
			allDeletedByWinnerButEditedByLoserIds = new HashSet<string>(allIdsRemovedByWinner.Intersect(allIdsLoserModified),
																		StringComparer.InvariantCultureIgnoreCase);
			allDeletedByLoserButEditedByWinnerIds = new HashSet<string>(allIdsRemovedByLoser.Intersect(allIdsWinnerModified),
																		StringComparer.InvariantCultureIgnoreCase);
		}

		private static void CollectModifiedIdsFromWinnerAndLoser(IDictionary<string, string> allWinnerData, IDictionary<string, string> allLoserData,
																 IEnumerable<string> allWinerIds, HashSet<string> allCommonAncestorIds,
																 IEnumerable<string> allLoserIds, IDictionary<string, string> allCommonAncestorData,
																 out HashSet<string> allIdsWhereUsersMadeDifferentChanges,
																 out HashSet<string> allIdsLoserModified,
																 out HashSet<string> allIdsWinnerModified,
																 out HashSet<string> allBothUsersMadeSameChanges,
																 out HashSet<string> allIdsForUniqueLoserChanges)
		{
			allIdsWinnerModified = new HashSet<string>(allCommonAncestorIds
														.Intersect(allWinerIds)
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
			allBothUsersMadeSameChanges = new HashSet<string>(allIdsWinnerModified
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
			allIdsForUniqueLoserChanges = new HashSet<string>(allIdsLoserModified.Except(allBothUsersMadeSameChanges).Except(allIdsWhereUsersMadeDifferentChanges));
		}

		private static void CollectDeletedIdsFromWinnerAndLoser(
			IEnumerable<string> allLoserIds, HashSet<string> allCommonAncestorIds, IEnumerable<string> allWinerIds,
			out HashSet<string> allIdsRemovedByWinner, out HashSet<string> allIdsRemovedByLoser, out HashSet<string> allIdsRemovedByBoth)
		{
			allIdsRemovedByWinner =
				new HashSet<string>(allCommonAncestorIds.Except(allWinerIds, StringComparer.InvariantCultureIgnoreCase));
			allIdsRemovedByLoser =
				new HashSet<string>(allCommonAncestorIds.Except(allLoserIds, StringComparer.InvariantCultureIgnoreCase));
			allIdsRemovedByBoth = new HashSet<string>(allIdsRemovedByWinner.Intersect(allIdsRemovedByLoser));
		}

		private static void LoadDataFiles(MergeOrder mergeOrder,
			string optionalFirstElementMarker, string repeatingRecordElementName, string repeatingRecordKeyIdentifier,
			string commonAncestorPathname, string pathToWinner, string pathToLoser,
			out Dictionary<string, string> allCommonAncestorData, out HashSet<string> allCommonAncestorIds,
			out Dictionary<string, string> allWinnerData, out HashSet<string> allWinerIds,
			out Dictionary<string, string> allLoserData, out HashSet<string> allLoserIds)
		{
			allCommonAncestorData = MakeRecordDictionary(mergeOrder.EventListener,
																					commonAncestorPathname, optionalFirstElementMarker,
																					repeatingRecordElementName, repeatingRecordKeyIdentifier);
			allCommonAncestorIds = new HashSet<string>(allCommonAncestorData.Keys, StringComparer.InvariantCultureIgnoreCase);
			allWinnerData = MakeRecordDictionary(mergeOrder.EventListener, pathToWinner, optionalFirstElementMarker,
												 repeatingRecordElementName, repeatingRecordKeyIdentifier);
			allWinerIds = new HashSet<string>(allWinnerData.Keys, StringComparer.InvariantCultureIgnoreCase);
			allLoserData = MakeRecordDictionary(mergeOrder.EventListener, pathToLoser, optionalFirstElementMarker, repeatingRecordElementName,
												repeatingRecordKeyIdentifier);
			allLoserIds = new HashSet<string>(allLoserData.Keys, StringComparer.InvariantCultureIgnoreCase);
		}

		private static void CollectNewItemsFromWinnerAndLoser(IDictionary<string, string> allWinnerData, IEnumerable<string> allWinerIds, IEnumerable<string> allLoserIds,
															  IDictionary<string, string> allLoserData, HashSet<string> allCommonAncestorIds,
															  out HashSet<string> allNewIdsFromBoth,
															  out HashSet<string> allNewIdsFromBothWithSameData,
															  out Dictionary<string, XmlNode> fluffedUpLoserNodes,
															  out Dictionary<string, XmlNode> fluffedUpWinnerNodes)
		{
			var allNewIdsFromWinner = new HashSet<string>(allWinerIds, StringComparer.InvariantCultureIgnoreCase);
			allNewIdsFromWinner.ExceptWith(allCommonAncestorIds);
			var allNewIdsFromLoser = new HashSet<string>(allLoserIds, StringComparer.InvariantCultureIgnoreCase);
			allNewIdsFromLoser.ExceptWith(allCommonAncestorIds);
			// Step 2A. Should be quite rare, and they may be the same or different.
			allNewIdsFromBoth = new HashSet<string>(allNewIdsFromWinner.Intersect(allNewIdsFromLoser, StringComparer.InvariantCultureIgnoreCase));
			var innerFluffedUpWinnerNodes = new Dictionary<string, XmlNode>();
			fluffedUpWinnerNodes = innerFluffedUpWinnerNodes;
			var innerFluffedUpLoserNodes = new Dictionary<string, XmlNode>();
			fluffedUpLoserNodes = innerFluffedUpLoserNodes;
			allNewIdsFromBothWithSameData = new HashSet<string>(allNewIdsFromBoth
																	.Where(identifier =>
																	{
																		XmlNode winnerNode;
																		if (
																			!innerFluffedUpWinnerNodes.TryGetValue(identifier,
																											  out winnerNode))
																		{
																			winnerNode =
																				XmlUtilities.GetDocumentNodeFromRawXml(
																					allWinnerData[identifier], new XmlDocument());
																			innerFluffedUpWinnerNodes.Add(identifier, winnerNode);
																		}
																		XmlNode loserNode;
																		if (
																			!innerFluffedUpLoserNodes.TryGetValue(identifier,
																											 out loserNode))
																		{
																			loserNode =
																				XmlUtilities.GetDocumentNodeFromRawXml(
																					allLoserData[identifier], new XmlDocument());
																			innerFluffedUpLoserNodes.Add(identifier, loserNode);
																		}
																		return XmlUtilities.AreXmlElementsEqual(winnerNode,
																												loserNode);
																	}), StringComparer.InvariantCultureIgnoreCase);

			allNewIdsFromWinner.ExceptWith(allNewIdsFromBothWithSameData);
			allNewIdsFromLoser.ExceptWith(allNewIdsFromBothWithSameData);
		}

		/// <summary>
		/// This function should return true if the Run method should continue on
		/// If this function is not provide by the client an exception will be thrown if a duplicate is encountered.
		/// </summary>
		private static Func<string, bool> ShouldContinueAfterDuplicateKey;
		private static Dictionary<string, string> MakeRecordDictionary(IMergeEventListener mainMergeEventListener, string pathname, string firstElementMarker, string recordStartingTag, string identifierAttribute)
		{
			ShouldContinueAfterDuplicateKey = message =>
				{
					mainMergeEventListener.WarningOccurred(new MergeWarning(pathname + ": " + message));
					return true;
				};
			var records = new Dictionary<string, string>(EstimatedObjectCount(pathname), StringComparer.InvariantCultureIgnoreCase);
			using (var fastSplitter = new FastXmlElementSplitter(pathname))
			{
				bool foundOptionalFirstElement;
				foreach (var record in fastSplitter.GetSecondLevelElementStrings(firstElementMarker, recordStartingTag, out foundOptionalFirstElement))
				{
					if (foundOptionalFirstElement)
					{
						records.Add(firstElementMarker.ToLowerInvariant(), record);
						foundOptionalFirstElement = false;
					}
					else
					{
						string message;
						AddKeyToIndex(records, identifierAttribute, record, out message);
						if (!string.IsNullOrEmpty(message) && !ShouldContinueAfterDuplicateKey(message))
						{
							throw new ArgumentException(message);
						}
					}
				}
			}

			return records;
		}

		private static int EstimatedObjectCount(string pathname)
		{
			const int estimatedObjectSize = 400;
			var fileInfo = new FileInfo(pathname);
			return (int)(fileInfo.Length / estimatedObjectSize);
		}

		private static void AddKeyToIndex(IDictionary<string, string> records, string identifierAttribute, string data, out string message)
		{
			message = null;

			// Skip tombstones.
			if (GetAttribute("dateDeleted", data) != null)
				return;

			var identifier = GetAttribute(identifierAttribute, data);
			if (string.IsNullOrEmpty(identifierAttribute))
			{
				message = "There was no identifier for the record";
			}
			else
			{
				if (!records.ContainsKey(identifier))
				{
					records.Add(identifier, data);
				}
				else
				{
					message = "There is more than one element with the identifier '" + identifier + "'";
				}
			}
		}

		private static string GetAttribute(string identifierAttribute, string data)
		{
			string attributeValue = null;
			var readerSettings = new XmlReaderSettings
			{
				CheckCharacters = false,
				ConformanceLevel = ConformanceLevel.Document,
				ProhibitDtd = true,
				ValidationType = ValidationType.None,
				CloseInput = true,
				IgnoreWhitespace = true
			};
			using (var reader = XmlReader.Create(new StringReader(data), readerSettings))
			{
				reader.MoveToContent();
				if (reader.MoveToAttribute(identifierAttribute))
				{
					attributeValue = reader.Value;
				}
			}
			return attributeValue;
		}

		private static void EnsureCommonAncestorFileHasMimimalContent(string commonAncestorPathname, string pathToWinner)
		{
			using(var reader = new XmlTextReader(commonAncestorPathname))
			{
				try
				{
					if (reader.Read())
					{
						return;
					}
				}
				catch (XmlException)
				{
					//we need to build the ancestor document if it was empty of xml
				}
			}
			BuildAncestorDocument(commonAncestorPathname, pathToWinner);
		}

		private static void BuildAncestorDocument(string commonAncestorDoc, string pathToOtherFile)
		{
			using(var writer = XmlWriter.Create(commonAncestorDoc))
			using (var reader = new XmlTextReader(pathToOtherFile))
			{
				//suck in the first node
				reader.MoveToContent();
				writer.WriteStartDocument();
				writer.WriteStartElement(reader.Name, reader.NamespaceURI);
				//move to the first attribute of the element.
				while (reader.MoveToNextAttribute())
				{
					writer.WriteAttributeString(reader.Name, reader.NamespaceURI, reader.Value);
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();
				reader.Close();
			}
		}

		private static void WriteNode(XmlWriter writer, string dataToWrite)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(Utf8.GetBytes(dataToWrite)), ReaderSettings))
			{
				writer.WriteNode(nodeReader, false);
			}
		}
	}
}