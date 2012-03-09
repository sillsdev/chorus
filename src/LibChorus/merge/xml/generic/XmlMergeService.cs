using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
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
			string firstElementMarker,
			string recordElementName, string id,
			Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate)
		{
			string pathToWinner;
			string pathToLoser;
			string winnerId;
			switch (mergeOrder.MergeSituation.ConflictHandlingMode)
			{
				case MergeOrder.ConflictHandlingModeChoices.WeWin:
					pathToWinner = mergeOrder.pathToOurs;
					pathToLoser = mergeOrder.pathToTheirs;
					winnerId = mergeOrder.MergeSituation.AlphaUserId;
					break;
				default: //case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					pathToWinner = mergeOrder.pathToTheirs;
					pathToLoser = mergeOrder.pathToOurs;
					winnerId = mergeOrder.MergeSituation.BetaUserId;
					break;
			}
			var commonAncestorPathname = mergeOrder.pathToCommonAncestor;
			// Do not change outputPathname, or be ready to fix SyncScenarioTests.CanCollaborateOnLift()!
			var outputPathname = mergeOrder.pathToOurs;

			Guard.AgainstNull(mergeStrategy, string.Format("'{0}' is null.", mergeStrategy));
			Guard.AgainstNull(mergeOrder, string.Format("'{0}' is null.", mergeOrder));
			Guard.AgainstNull(mergeOrder.EventListener, string.Format("'{0}' is null.", "mergeOrder.EventListener"));
			Guard.AgainstNull(writePreliminaryInformationDelegate, string.Format("'{0}' is null.", writePreliminaryInformationDelegate));
			Require.That(File.Exists(commonAncestorPathname), string.Format("'{0}' does not exist.", commonAncestorPathname));
			Require.That(File.Exists(pathToWinner), string.Format("'{0}' does not exist.", pathToWinner));
			Require.That(File.Exists(pathToLoser), string.Format("'{0}' does not exist.", pathToLoser));
			Guard.AgainstNullOrEmptyString(recordElementName, "No primary record element name.");
			Guard.AgainstNullOrEmptyString(id, "No identifier attribute for primary record element.");

			var winnerNewbies = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			// Do diff between winner and common
			var winnerGoners = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			var winnerDirtballs = new Dictionary<string, ChangedElement>(StringComparer.OrdinalIgnoreCase);
			var parentIndex = Do2WayDiff(commonAncestorPathname, pathToWinner, winnerGoners, winnerDirtballs, winnerNewbies,
				firstElementMarker,
				recordElementName, id);

			// Do diff between loser and common
			var loserNewbies = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			var loserGoners = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			var loserDirtballs = new Dictionary<string, ChangedElement>(StringComparer.OrdinalIgnoreCase);
			Do2WayDiff(parentIndex, pathToLoser, loserGoners, loserDirtballs, loserNewbies,
				firstElementMarker,
				recordElementName, id);

			// At this point we have two sets of diffs, but we need to merge them.
			// Newbies from both get added.
			// A conflict has 'winner' stay, but with a report.
			using (var writer = XmlWriter.Create(outputPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				// Need a reader on '_commonAncestorXml', much as is done for FW, but sans thread.
				// Blend in newbies, goners, and dirtballs to 'outputPathname' as in FW.
				var readerSettings = new XmlReaderSettings
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Document,
					ProhibitDtd = true,
					ValidationType = ValidationType.None,
					CloseInput = true,
					IgnoreWhitespace = true
				};
				using (var reader = XmlReader.Create(new FileStream(commonAncestorPathname, FileMode.Open), readerSettings))
				{
					// This must be client specific behavior.
					writePreliminaryInformationDelegate(reader, writer);

					if (!string.IsNullOrEmpty(firstElementMarker))
					{
						ProcessFirstElement(
							mergeStrategy, mergeOrder, mergeOrder.EventListener,
							parentIndex,
							pathToWinner, winnerNewbies, winnerGoners, winnerDirtballs,
							pathToLoser, loserNewbies, loserGoners, loserDirtballs,
							reader, writer,
							winnerId, firstElementMarker);
					}

					ProcessMainRecordElements(
						mergeStrategy, mergeOrder, mergeOrder.EventListener,
						parentIndex,
						winnerGoners, winnerDirtballs,
						loserGoners, loserDirtballs,
						reader, writer,
						id, winnerId, recordElementName);

					// Check to see if they both added the exact same element by some fluke. (Hand edit could do it.)
					CheckForIdenticalNewbies(mergeOrder, mergeOrder.EventListener, writer,
						winnerNewbies, loserNewbies);

					WriteOutNewObjects(mergeOrder.EventListener, winnerNewbies.Values, pathToWinner, writer);
					WriteOutNewObjects(mergeOrder.EventListener, loserNewbies.Values, pathToLoser, writer);

					writer.WriteEndElement();
				}
			}
		}

		private static void CheckForIdenticalNewbies(
			MergeOrder mergeOrder, IMergeEventListener listener, XmlWriter writer,
			IDictionary<string, XmlNode> winnerNewbies, IDictionary<string, XmlNode> loserNewbies)
		{
			var winnersToRemove = new HashSet<string>();
			foreach (var winnerKvp in winnerNewbies)
			{
				var winnerKey = winnerKvp.Key;
				if (!loserNewbies.ContainsKey(winnerKey))
					continue; // Route used.
				if (XmlUtilities.AreXmlElementsEqual(winnerNewbies[winnerKey], loserNewbies[winnerKey]))
				{
					// Code after this method will then add the one newbie, with one addition report.
					// Route used (x2).
					// Both added same thing.
					listener.ChangeOccurred(new XmlBothAddedSameChangeReport(mergeOrder.pathToOurs, winnerNewbies[winnerKey]));
					WriteNode(winnerNewbies[winnerKey], writer);
					loserNewbies.Remove(winnerKey);
					winnersToRemove.Add(winnerKey);
					continue;
				}
				// Pick one, based on MergeOrder.
				if (mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin)
				{
					// We win. Work up conflict report.
					// Route tested.
					var winnerElement = winnerKvp.Value;
					AddConflictToListener(
						listener,
						new BothAddedMainElementButWithDifferentContentConflict(
							winnerKvp.Value.Name,
							winnerElement,
							loserNewbies[winnerKey],
							mergeOrder.MergeSituation,
							ElementStrategy.CreateForKeyedElement(winnerKey, false),
							mergeOrder.MergeSituation.AlphaUserId),
						winnerElement,
						loserNewbies[winnerKey],
						null);
					loserNewbies.Remove(winnerKey);
					winnersToRemove.Add(winnerKey);
					WriteNode(winnerElement, writer);
				}
				else
				{
					// They win. Work up conflict report.
					// Route tested.
					var loserElement = loserNewbies[winnerKey];
					AddConflictToListener(
						listener,
						new BothAddedMainElementButWithDifferentContentConflict(
							winnerKvp.Value.Name,
							winnerKvp.Value,
							loserNewbies[winnerKey],
							mergeOrder.MergeSituation,
							ElementStrategy.CreateForKeyedElement(winnerKey, false),
							mergeOrder.MergeSituation.BetaUserId),
						winnerKvp.Value,
						loserNewbies[winnerKey],
						null);
					winnersToRemove.Add(winnerKey);
					loserNewbies.Remove(winnerKey);
					WriteNode(loserElement, writer);
				}
			}
			foreach (var winnerKey in winnersToRemove)
			{
				winnerNewbies.Remove(winnerKey);
			}
		}

		private static void ProcessFirstElement(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, byte[]> parentIndex,
			string pathToWinner, IDictionary<string, XmlNode> winnerNewbies, IDictionary<string, XmlNode> winnerGoners, IDictionary<string, ChangedElement> winnerDirtballs,
			string pathToLoser, IDictionary<string, XmlNode> loserNewbies, IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			XmlReader reader, XmlWriter writer,
			string winnerId, string firstElementMarker)
		{
			XmlNode currentNode;
			if (winnerNewbies.TryGetValue(firstElementMarker, out currentNode))
			{
				XmlNode loserFirstElement;
				if (loserNewbies.TryGetValue(firstElementMarker, out loserFirstElement))
				{
					if (!XmlUtilities.AreXmlElementsEqual(currentNode, loserFirstElement))
					{
						// Bother. They are not the same.
						// Do it the hard way via a merge.
						// Route tested (x2).
						var results = mergeStrategy.MakeMergedEntry(listener,
							currentNode,
							loserFirstElement,
							null); // ancestor is null, since they each added the optional first element.
						var doc = new XmlDocument();
						doc.LoadXml(results);
						WriteNode(doc, writer);
					}
					else
					{
						// Both of them added the same thing.
						// Route tested (x2).
						listener.ChangeOccurred(new XmlBothAddedSameChangeReport(pathToWinner, currentNode));
						WriteNode(currentNode, writer);
					}
					winnerNewbies.Remove(firstElementMarker);
					loserNewbies.Remove(firstElementMarker);
					// These should never have them, but make sure.
					winnerGoners.Remove(firstElementMarker);
					winnerDirtballs.Remove(firstElementMarker);
					loserGoners.Remove(firstElementMarker);
					loserDirtballs.Remove(firstElementMarker);
				}
				else
				{
					// Brand new, so write it out and quit.
					// In this case the winner added it.
					// Route tested.
					listener.ChangeOccurred(new XmlAdditionChangeReport(pathToWinner, currentNode));
					WriteNode(currentNode, writer);

					winnerNewbies.Remove(firstElementMarker);
					// These should never have them, but make sure.
					winnerGoners.Remove(firstElementMarker);
					winnerDirtballs.Remove(firstElementMarker);
					loserGoners.Remove(firstElementMarker);
					loserDirtballs.Remove(firstElementMarker);
				}
				return; // Route used.
			}

			if (loserNewbies.TryGetValue(firstElementMarker, out currentNode))
			{
				// Brand new, so write it out and quit.
				// Loser added it.
				// Route tested.
				loserNewbies.Remove(firstElementMarker);
				listener.ChangeOccurred(new XmlAdditionChangeReport(pathToLoser, currentNode));
				WriteNode(currentNode, writer);

				// These should never have them, but make sure.
				winnerGoners.Remove(firstElementMarker);
				winnerDirtballs.Remove(firstElementMarker);
				loserGoners.Remove(firstElementMarker);
				loserDirtballs.Remove(firstElementMarker);

				return;
			}

			if (((!winnerGoners.ContainsKey(firstElementMarker) && !winnerDirtballs.ContainsKey(firstElementMarker)) &&
				 !loserGoners.ContainsKey(firstElementMarker)) && !loserDirtballs.ContainsKey(firstElementMarker))
			{
				// It existed before, and nobody touched it.
				if (reader.LocalName == firstElementMarker)
					writer.WriteNode(reader, false); // Route tested (x2).
				return; // Route tested.
			}

			// Do it the hard way for the others.
			var transferUntouched = true;
			ProcessCurrentElement(mergeOrder, firstElementMarker, mergeStrategy, winnerId, listener, writer, firstElementMarker,
									parentIndex,
								  loserDirtballs, loserGoners,
								  winnerDirtballs, winnerGoners, ref transferUntouched);

			if (transferUntouched)
				return;

			// Read to next main element,
			// Which skips writing out the current element.
			reader.ReadOuterXml(); // Route tested (x3).

			// Nobody did anything with the current source node, so just copy it to output.
			// This case is handled, above.
			//writer.WriteNode(reader, false);
		}

		private static void ProcessCurrentElement(MergeOrder mergeOrder, string currentKey, IMergeStrategy mergeStrategy, string winnerId, IMergeEventListener listener, XmlWriter writer, string elementMarker,
			IDictionary<string, byte[]> parentIndex,
			IDictionary<string, ChangedElement> loserDirtballs, IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> winnerDirtballs, IDictionary<string, XmlNode> winnerGoners,
			ref bool transferUntouched)
		{
			if (winnerGoners.ContainsKey(currentKey))
			{
				// Route used.
				transferUntouched = false;
				ProcessDeletedRecordFromWinningData(mergeOrder, listener, parentIndex, winnerGoners, currentKey, winnerId, elementMarker, loserGoners, loserDirtballs, writer);
			}

			if (winnerDirtballs.ContainsKey(currentKey))
			{
				//Route used.
				transferUntouched = false;
				ProcessWinnerEditedRecord(mergeStrategy, mergeOrder, listener, currentKey, winnerId, elementMarker, loserGoners, loserDirtballs, winnerDirtballs, writer);
			}

			if (loserGoners.ContainsKey(currentKey))
			{
				// Loser deleted it but winner did nothing to it.
				// If winner had either deleted or edited it,
				// then the code above here would have been involved,
				// and currentKey would have been removed from loserGoners.
				// The net effect is that it will be removed.
				// Route tested.
				AddDeletionReport(mergeOrder.pathToTheirs, currentKey, listener, parentIndex, loserGoners);
				transferUntouched = false;
				loserGoners.Remove(currentKey);
			}
			if (!loserDirtballs.ContainsKey(currentKey))
				return; // Route used.

			// Loser changed it, but winner did nothing to it.
			// Route tested (x2-optional first elment, x2-main record)
			transferUntouched = false;
			// Make change report(s) the hard way.
			var changedElement = loserDirtballs[currentKey];
			// Since winner did nothing, it ought to be the same as parent.
			var oursIsWinner = mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin;
			var ours = oursIsWinner ? changedElement._parentNode : changedElement._childNode;
			var theirs = oursIsWinner ? changedElement._childNode : changedElement._parentNode;
			mergeStrategy.MakeMergedEntry(listener, ours, theirs, changedElement._parentNode);
			ReplaceCurrentNode(writer, loserDirtballs, currentKey);
			// ReplaceCurrentNode removes currentKey from loserDirtballs.
		}

		private static void AddDeletionReport(string pathforRemover, string currentKey, IMergeEventListener listener,
											  IDictionary<string, byte[]> parentIndex, IDictionary<string, XmlNode> goners)
		{
			// Route used (x3) [both/winner/loser].
			var doc = new XmlDocument();
			doc.LoadXml(Encoding.UTF8.GetString(parentIndex[currentKey.ToLowerInvariant()]));
			listener.ChangeOccurred(new XmlDeletionChangeReport(pathforRemover, doc.DocumentElement, goners[currentKey]));
		}

		private static void WriteOutNewObjects(IMergeEventListener listener, IEnumerable<XmlNode> newbies, string pathname, XmlWriter writer)
		{
			foreach (var newby in newbies)
			{
				// Route tested (x3).
				listener.ChangeOccurred(new XmlAdditionChangeReport(pathname, newby));
				WriteNode(newby, writer);
			}
		}

		private static void WriteNode(XmlNode nodeToWrite, XmlWriter writer)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(Utf8.GetBytes(nodeToWrite.OuterXml)), ReaderSettings))
			{
				writer.WriteNode(nodeReader, false);
			}
		}

		private static void ProcessMainRecordElements(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, Byte[]> parentIndex,
			IDictionary<string, XmlNode> winnerGoners,
			IDictionary<string, ChangedElement> winnerDirtballs,
			IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> loserDirtballs,
			XmlReader reader, XmlWriter writer,
			string id, string winnerId, string recordElementName)
		{
			var keepReading = true;
			while (keepReading)
			{
				if (reader.EOF) // moved to lift handler to deal with || !reader.IsStartElement())
					break; // Route used.

				// 'entry' node is current node in reader.
				// Fetch id from 'entry' node and see if it is in either
				// of the modified/deleted dictionaries.
				// Route used.
				var transferUntouched = true;
				var currentKey = reader.GetAttribute(id);

				ProcessCurrentElement(mergeOrder, currentKey, mergeStrategy, winnerId, listener, writer, recordElementName,
					parentIndex,
					loserDirtballs, loserGoners,
					winnerDirtballs, winnerGoners, ref transferUntouched);

				if (!transferUntouched)
				{
					// Route used.
					// NB: The FailureSimulator is *only* used in tests.
					FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");

					// Read to next record element,
					// Which skips writing our the current element.
					reader.ReadOuterXml();
					keepReading = reader.IsStartElement();
					continue;
				}

				// Nobody did anything with the current source node, so just copy it to output.
				// Route used.
				writer.WriteNode(reader, false);
				keepReading = reader.IsStartElement();
			}
		}

		private static void ProcessWinnerEditedRecord(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			string currentKey, string winnerId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			IDictionary<string, ChangedElement> winnerDirtballs,
			XmlWriter writer)
		{
			if (loserGoners.ContainsKey(currentKey))
			{
				// Route tested (x2).
				// Winner edited it, but loser deleted it.
				// Make a conflict report.
				var dirtballChangedElement = winnerDirtballs[currentKey];
				AddConflictToListener(
					listener,
					new EditedVsRemovedElementConflict(
						recordElementName,
						dirtballChangedElement._childNode,
						loserGoners[currentKey],
						dirtballChangedElement._parentNode,
						mergeOrder.MergeSituation,
						new ElementStrategy(false),
						winnerId),
					dirtballChangedElement._childNode,
					loserGoners[currentKey],
					dirtballChangedElement._parentNode);

				ReplaceCurrentNode(writer, dirtballChangedElement._childNode);
				winnerDirtballs.Remove(currentKey);
				loserGoners.Remove(currentKey);
			}
			else
			{
				var oursIsWinner = mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin;
				XmlNode ours;
				XmlNode theirs;
				XmlNode commonAncestor;
				if (loserDirtballs.ContainsKey(currentKey))
				{
					// Both edited it.
					// Route tested (x2-optional first elment, x2-main record).
					var dirtballChangedElement = winnerDirtballs[currentKey];
					ours = oursIsWinner ? dirtballChangedElement._childNode : loserDirtballs[currentKey]._childNode;
					theirs = oursIsWinner ? loserDirtballs[currentKey]._childNode : dirtballChangedElement._childNode;
					commonAncestor = dirtballChangedElement._parentNode;
					loserDirtballs.Remove(currentKey);
				}
				else
				{
					// Winner edited it. Loser did nothing with it. Loser is the same as parent.
					// Route tested (x2-optional first elment, x2-main record)
					var dirtballChangedElement = winnerDirtballs[currentKey];
					ours = oursIsWinner ? dirtballChangedElement._childNode : dirtballChangedElement._parentNode;
					theirs = oursIsWinner ? dirtballChangedElement._parentNode : dirtballChangedElement._childNode;
					commonAncestor = dirtballChangedElement._parentNode;
					winnerDirtballs.Remove(currentKey);
				}
				var mergedResult = mergeStrategy.MakeMergedEntry(listener, ours, theirs, commonAncestor);
				ReplaceCurrentNode(writer, mergedResult);
			}
		}

		private static void ReplaceCurrentNode(XmlWriter writer, IDictionary<string, ChangedElement> loserDirtballs, string currentKey)
		{
			ReplaceCurrentNode(writer, loserDirtballs[currentKey]._childNode);
			loserDirtballs.Remove(currentKey);
		}

		private static void ReplaceCurrentNode(XmlWriter writer, XmlNode replacementNode)
		{
			ReplaceCurrentNode(writer, replacementNode.OuterXml);
		}

		private static void ReplaceCurrentNode(XmlWriter writer, string replacementValue)
		{
			using (var tempReader = XmlReader.Create(
				new MemoryStream(Encoding.UTF8.GetBytes(replacementValue)),
				new XmlReaderSettings
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Fragment,
					ProhibitDtd = true,
					ValidationType = ValidationType.None,
					CloseInput = true,
					IgnoreWhitespace = true
				}))
			{
				writer.WriteNode(tempReader, false);
			}
		}

		private static void ProcessDeletedRecordFromWinningData(
			MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, byte[]> parentIndex,
			IDictionary<string, XmlNode> winnerGoners,
			string currentKey, string winnerId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			XmlWriter writer)
		{
			var wantDeletionChangeReport = false;
			if (loserGoners.ContainsKey(currentKey))
			{
				// Both deleted it.
				// Route tested.
				wantDeletionChangeReport = true;
				loserGoners.Remove(currentKey);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentKey))
				{
					var dirtball = loserDirtballs[currentKey];
					// Winner deleted it, but loser edited it.
					// Make a conflict report.
					// Route tested (x2).
					AddConflictToListener(
						listener,
						new RemovedVsEditedElementConflict(
							recordElementName,
							winnerGoners[currentKey],
							dirtball._childNode,
							dirtball._parentNode,
							mergeOrder.MergeSituation,
							new ElementStrategy(false),
							winnerId),
						winnerGoners[currentKey],
						dirtball._childNode,
						dirtball._parentNode);
					// Write out edited node, under the least loss principle.
					ReplaceCurrentNode(writer, loserDirtballs, currentKey);
					loserDirtballs.Remove(currentKey);
				}
				else
				{
					// Winner deleted it and loser did nothing with it.
					// Route tested.
					wantDeletionChangeReport = true;
				}
			}
			if (wantDeletionChangeReport)
			{
				AddDeletionReport(mergeOrder.pathToOurs, currentKey, listener, parentIndex, winnerGoners);
			}
			winnerGoners.Remove(currentKey);
		}

		private static void Do2WayDiff(Dictionary<string, byte[]> parentIndex, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> dirtballs, IDictionary<string, XmlNode> newbies,
			string firstElementMarker,
			string recordElementName, string id)
		{
			try
			{
				foreach (var winnerDif in Xml2WayDiffService.ReportDifferences(
					parentIndex, childPathname,
					new ChangeAndConflictAccumulator(),
					firstElementMarker,
					recordElementName, id))
				{
					Do2WayDiffCore(id, winnerDif, goners, dirtballs, newbies);
				}
			}
			catch
			{ }
		}

		private static Dictionary<string, byte[]> Do2WayDiff(string parentPathname, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> dirtballs, IDictionary<string, XmlNode> newbies,
			string firstElementMarker,
			string recordElementName, string id)
		{
			Dictionary<string, byte[]> parentIndex = null;
			try
			{
				foreach (var winnerDif in Xml2WayDiffService.ReportDifferencesForMerge(
					parentPathname, childPathname,
					new ChangeAndConflictAccumulator(),
					firstElementMarker,
					recordElementName, id, out parentIndex))
				{
					Do2WayDiffCore(id, winnerDif, goners, dirtballs, newbies);
				}
			}
			catch
			{ }
			return parentIndex;
		}

		private static void Do2WayDiffCore(string id, IChangeReport winnerDif, IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> dirtballs, IDictionary<string, XmlNode> newbies)
		{
			if (!(winnerDif is IXmlChangeReport))
				return;

			var asXmlReport = (IXmlChangeReport)winnerDif;
			switch (winnerDif.GetType().Name)
			{
				case "XmlDeletionChangeReport":
					var gonerNode = asXmlReport.ParentNode;
					goners.Add(GetKey(gonerNode, id), gonerNode);
					break;
				case "XmlChangedRecordReport":
					var originalNode = asXmlReport.ParentNode;
					var updatedNode = asXmlReport.ChildNode;
					dirtballs.Add(GetKey(originalNode, id), new ChangedElement
																		{
																			_parentNode = originalNode,
																			_childNode = updatedNode
																		});
					break;
				case "XmlAdditionChangeReport":
					var newbieNode = asXmlReport.ChildNode;
					newbies.Add(GetKey(newbieNode, id), newbieNode);
					break;
			}
		}

		private static string GetKey(XmlNode node, string id)
		{
			var attr = node.Attributes[id];
			return (attr == null) ? node.LocalName : attr.Value;
		}

		private class ChangedElement
		{
			internal XmlNode _parentNode;
			internal XmlNode _childNode;
		}
	}
}