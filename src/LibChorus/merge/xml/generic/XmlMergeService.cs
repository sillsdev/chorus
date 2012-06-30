using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Service that manages Xml merging for formats that are basically a long list of the
	/// same element type, like a database table.
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
		/// <param name="mergeOrder"></param>
		/// <param name="mergeStrategy"></param>
		/// <param name="firstElementMarker"></param>
		/// <param name="recordElementName"></param>
		/// <param name="id"></param>
		/// <param name="writePreliminaryInformationDelegate">TODO: Improve this: "allows the client to manage writing the root element and any of its attrs". When do you need this?</param>
		public static void Do3WayMerge(MergeOrder mergeOrder, IMergeStrategy mergeStrategy, // Get from mergeOrder: IMergeEventListener listener,
			string firstElementMarker,
			string recordElementName, string id,
			Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate)
		{
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
			var commonAncestorPathname = mergeOrder.pathToCommonAncestor;
			EnsureCommonAncestorFileHasMimimalContent(commonAncestorPathname, pathToWinner, pathToLoser);
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
			var winnerChangedObjects = new Dictionary<string, ChangedElement>(StringComparer.OrdinalIgnoreCase);
			var parentIndex = Do2WayDiff(commonAncestorPathname, pathToWinner, winnerGoners, winnerChangedObjects, winnerNewbies,
				firstElementMarker,
				recordElementName, id);

			// Do diff between loser and common
			var loserNewbies = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			var loserGoners = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			var loserChangedObjects = new Dictionary<string, ChangedElement>(StringComparer.OrdinalIgnoreCase);
			Do2WayDiffX(parentIndex, pathToLoser, loserGoners, loserChangedObjects, loserNewbies,
				firstElementMarker,
				recordElementName, id, mergeOrder.EventListener);

			// At this point we have two sets of diffs, but we need to merge them.
			// Newbies from both get added.
			// A conflict has 'winner' stay, but with a report.
			using (var writer = XmlWriter.Create(outputPathname, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				// Need a reader on '_commonAncestorXml', much as is done for FW, but sans thread.
				// Blend in newbies, goners, and ChangedObjects to 'outputPathname' as in FW.
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
							pathToWinner, winnerNewbies, winnerGoners, winnerChangedObjects,
							pathToLoser, loserNewbies, loserGoners, loserChangedObjects,
							reader, writer,
							winnerId, firstElementMarker, loserId);
					}

					ProcessMainRecordElements(
						mergeStrategy, mergeOrder, mergeOrder.EventListener,
						parentIndex,
						winnerGoners, winnerChangedObjects,
						loserGoners, loserChangedObjects,
						reader, writer,
						id, winnerId, recordElementName, loserId);

					// Check to see if they both added the exact same element by some fluke. (Hand edit could do it.)
					CheckForIdenticalNewbies(mergeStrategy, mergeOrder, mergeOrder.EventListener, writer,
						winnerNewbies, winnerId, pathToWinner, loserNewbies);

					WriteOutNewObjects(mergeOrder.EventListener, winnerNewbies.Values, pathToWinner, writer);
					WriteOutNewObjects(mergeOrder.EventListener, loserNewbies.Values, pathToLoser, writer);

					writer.WriteEndElement();
				}
			}
		}

		private static void EnsureCommonAncestorFileHasMimimalContent(string commonAncestorPathname, string pathToWinner, string pathToLoser)
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
				reader.MoveToNextAttribute();
				do
				{
					writer.WriteAttributeString(reader.Name, reader.NamespaceURI, reader.Value);
				} while (reader.MoveToNextAttribute());
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();
				reader.Close();
			}
		}

		private static void CheckForIdenticalNewbies(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener, XmlWriter writer,
			IDictionary<string, XmlNode> winnerNewbies, string winnerId, string pathToWinner, IDictionary<string, XmlNode> loserNewbies)
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
				// winnerNewbies and loserNewbies are already offset for mergeOrder.MergeSituation.ConflictHandlingMode.
				var winnerElement = winnerKvp.Value;
				var elementStrategy = mergeStrategy.GetElementStrategy(winnerElement);
				var generator = elementStrategy.ContextDescriptorGenerator;
				if (generator != null)
				{
					listener.EnteringContext(generator.GenerateContextDescriptor(winnerElement.OuterXml, pathToWinner));
				}
				AddConflictToListener(
					listener,
					new BothAddedMainElementButWithDifferentContentConflict(
						winnerKvp.Value.Name,
						winnerElement,
						loserNewbies[winnerKey],
						mergeOrder.MergeSituation,
						elementStrategy,
						winnerId),
					winnerElement,
					loserNewbies[winnerKey],
					null);
				loserNewbies.Remove(winnerKey);
				winnersToRemove.Add(winnerKey);
				WriteNode(winnerElement, writer);
			}
			foreach (var winnerKey in winnersToRemove)
			{
				winnerNewbies.Remove(winnerKey);
			}
		}

		private static void ProcessFirstElement(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, byte[]> parentIndex,
			string pathToWinner, IDictionary<string, XmlNode> winnerNewbies, IDictionary<string, XmlNode> winnerGoners, IDictionary<string, ChangedElement> winnerChangedObjects,
			string pathToLoser, IDictionary<string, XmlNode> loserNewbies, IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserChangedObjects,
			XmlReader reader, XmlWriter writer,
			string winnerId, string firstElementMarker, string loserId)
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
					winnerChangedObjects.Remove(firstElementMarker);
					loserGoners.Remove(firstElementMarker);
					loserChangedObjects.Remove(firstElementMarker);
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
					winnerChangedObjects.Remove(firstElementMarker);
					loserGoners.Remove(firstElementMarker);
					loserChangedObjects.Remove(firstElementMarker);
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
				winnerChangedObjects.Remove(firstElementMarker);
				loserGoners.Remove(firstElementMarker);
				loserChangedObjects.Remove(firstElementMarker);

				return;
			}

			if (((!winnerGoners.ContainsKey(firstElementMarker) && !winnerChangedObjects.ContainsKey(firstElementMarker)) &&
				 !loserGoners.ContainsKey(firstElementMarker)) && !loserChangedObjects.ContainsKey(firstElementMarker))
			{
				// It existed before, and nobody touched it.
				if (reader.LocalName == firstElementMarker)
					writer.WriteNode(reader, false); // Route tested (x2).
				return; // Route tested.
			}

			// Do it the hard way for the others.
			var transferUntouched = true;
			ProcessCurrentElement(mergeStrategy, mergeOrder, firstElementMarker, winnerId, loserId, listener, writer, firstElementMarker,
									parentIndex,
								  loserChangedObjects, loserGoners,
								  winnerChangedObjects, winnerGoners, ref transferUntouched);

			if (transferUntouched)
				return;

			// Read to next main element,
			// Which skips writing out the current element.
			reader.ReadOuterXml(); // Route tested (x3).

			// Nobody did anything with the current source node, so just copy it to output.
			// This case is handled, above.
			//writer.WriteNode(reader, false);
		}

		private static void ProcessCurrentElement(IMergeStrategy mergeStrategy, MergeOrder mergeOrder, string currentKey, string winnerId, string loserId, IMergeEventListener listener, XmlWriter writer, string elementMarker,
			IDictionary<string, byte[]> parentIndex,
			IDictionary<string, ChangedElement> loserChangedObjects, IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> winnerChangedObjects, IDictionary<string, XmlNode> winnerGoners,
			ref bool transferUntouched)
		{
			if (winnerGoners.ContainsKey(currentKey))
			{
				// Route used.
				transferUntouched = false;
				ProcessDeletedRecordFromWinningData(mergeStrategy, mergeOrder, listener, parentIndex, winnerGoners, currentKey, loserId, elementMarker, loserGoners, loserChangedObjects, writer);
			}

			if (winnerChangedObjects.ContainsKey(currentKey))
			{
				//Route used.
				transferUntouched = false;
				ProcessWinnerEditedRecord(mergeStrategy, mergeOrder, listener, currentKey, winnerId, elementMarker, loserGoners, loserChangedObjects, winnerChangedObjects, writer);
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
			if (!loserChangedObjects.ContainsKey(currentKey))
				return; // Route used.

			// Loser changed it, but winner did nothing to it.
			// Route tested (x2-optional first elment, x2-main record)
			transferUntouched = false;
			// Make change report(s) the hard way.
			var changedElement = loserChangedObjects[currentKey];
			// Since winner did nothing, it ought to be the same as parent.
			var oursIsWinner = mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin;
			var ours = oursIsWinner ? changedElement.ParentNode : changedElement.ChildNode;
			var theirs = oursIsWinner ? changedElement.ChildNode : changedElement.ParentNode;
			mergeStrategy.MakeMergedEntry(listener, ours, theirs, changedElement.ParentNode);
			ReplaceCurrentNode(writer, loserChangedObjects, currentKey);
			// ReplaceCurrentNode removes currentKey from loserChangedObjects.
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
			IDictionary<string, ChangedElement> winnerChangedObjects,
			IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> loserChangedObjects,
			XmlReader reader, XmlWriter writer,
			string id, string winnerId, string recordElementName, string loserId)
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
				if (currentKey == null)
					break;

				ProcessCurrentElement(mergeStrategy, mergeOrder, currentKey, winnerId, loserId, listener, writer, recordElementName,
					parentIndex,
					loserChangedObjects, loserGoners,
					winnerChangedObjects, winnerGoners, ref transferUntouched);

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
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserChangedObjects,
			IDictionary<string, ChangedElement> winnerChangedObjects,
			XmlWriter writer)
		{
			if (loserGoners.ContainsKey(currentKey))
			{
				// Route tested (x2).
				// Winner edited it, but loser deleted it.
				// Make a conflict report.
				var changedObjectChangedElement = winnerChangedObjects[currentKey];
				var elementStrategy = mergeStrategy.GetElementStrategy(changedObjectChangedElement.ParentNode);
				var generator = elementStrategy.ContextDescriptorGenerator;
				if (generator != null)
				{
					listener.EnteringContext(generator.GenerateContextDescriptor(changedObjectChangedElement.ParentNode.OuterXml, mergeOrder.pathToOurs));
				}
				AddConflictToListener(
					listener,
					new EditedVsRemovedElementConflict(
						recordElementName,
						changedObjectChangedElement.ChildNode,
						null,
						changedObjectChangedElement.ParentNode,
						mergeOrder.MergeSituation,
						elementStrategy,
						winnerId),
					changedObjectChangedElement.ChildNode,
					loserGoners[currentKey],
					changedObjectChangedElement.ParentNode);

				ReplaceCurrentNode(writer, changedObjectChangedElement.ChildNode);
				winnerChangedObjects.Remove(currentKey);
				loserGoners.Remove(currentKey);
			}
			else
			{
				var oursIsWinner = mergeOrder.MergeSituation.ConflictHandlingMode == MergeOrder.ConflictHandlingModeChoices.WeWin;
				XmlNode ours;
				XmlNode theirs;
				XmlNode commonAncestor;
				if (loserChangedObjects.ContainsKey(currentKey))
				{
					// Both edited it.
					// Route tested (x2-optional first elment, x2-main record).
					var changedObjectChangedElement = winnerChangedObjects[currentKey];
					ours = oursIsWinner ? changedObjectChangedElement.ChildNode : loserChangedObjects[currentKey].ChildNode;
					theirs = oursIsWinner ? loserChangedObjects[currentKey].ChildNode : changedObjectChangedElement.ChildNode;
					commonAncestor = changedObjectChangedElement.ParentNode;
					loserChangedObjects.Remove(currentKey);
				}
				else
				{
					// Winner edited it. Loser did nothing with it. Loser is the same as parent.
					// Route tested (x2-optional first elment, x2-main record)
					var changedObjectChangedElement = winnerChangedObjects[currentKey];
					ours = oursIsWinner ? changedObjectChangedElement.ChildNode : changedObjectChangedElement.ParentNode;
					theirs = oursIsWinner ? changedObjectChangedElement.ParentNode : changedObjectChangedElement.ChildNode;
					commonAncestor = changedObjectChangedElement.ParentNode;
					winnerChangedObjects.Remove(currentKey);
				}
				var mergedResult = mergeStrategy.MakeMergedEntry(listener, ours, theirs, commonAncestor);
				ReplaceCurrentNode(writer, mergedResult);
			}
		}

		private static void ReplaceCurrentNode(XmlWriter writer, IDictionary<string, ChangedElement> loserChangedObjects, string currentKey)
		{
			ReplaceCurrentNode(writer, loserChangedObjects[currentKey].ChildNode);
			loserChangedObjects.Remove(currentKey);
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
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, byte[]> parentIndex,
			IDictionary<string, XmlNode> winnerGoners,
			string currentKey, string loserId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserChangedObjects,
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
				if (loserChangedObjects.ContainsKey(currentKey))
				{
					var changedObject = loserChangedObjects[currentKey];
					// Winner deleted it, but loser edited it.
					// Make a conflict report.
					// Route tested (x2).
					var elementStrategy = mergeStrategy.GetElementStrategy(changedObject.ParentNode);
					var generator = elementStrategy.ContextDescriptorGenerator;
					if (generator != null)
					{
						listener.EnteringContext(generator.GenerateContextDescriptor(changedObject.ParentNode.OuterXml, mergeOrder.pathToOurs));
					}
					AddConflictToListener(
						listener,
						new RemovedVsEditedElementConflict(
							recordElementName,
							null,
							changedObject.ChildNode,
							changedObject.ParentNode,
							mergeOrder.MergeSituation,
							elementStrategy,
							loserId),
						winnerGoners[currentKey],
						changedObject.ChildNode,
						changedObject.ParentNode);
					// Write out edited node, under the least loss principle.
					ReplaceCurrentNode(writer, loserChangedObjects, currentKey);
					loserChangedObjects.Remove(currentKey);
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

		private static void Do2WayDiffX(Dictionary<string, byte[]> parentIndex, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> ChangedObjects, IDictionary<string, XmlNode> newbies,
			string firstElementMarker,
			string recordElementName, string id, IMergeEventListener mergeEventListener)
		{
			try
			{
				var warningListener = new ChangeAndConflictAccumulator();

				foreach (var winnerDif in Xml2WayDiffService.ReportDifferences(
					parentIndex, childPathname,
					warningListener,
					firstElementMarker,
					recordElementName, id))
				{
					Do2WayDiffCore(id, winnerDif, goners, ChangedObjects, newbies);
				}

				//What's going on here: all the changes found by the differ aren't supposed to be reported to the listener, but
				//when we added duplicate guid detection to the differ, we need a way to get warning out
				foreach(var warning in warningListener.Warnings)
				{
					mergeEventListener.WarningOccurred(warning);
				}
			}
			catch
			{ }
		}

		private static Dictionary<string, byte[]> Do2WayDiff(string parentPathname, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> ChangedObjects, IDictionary<string, XmlNode> newbies,
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
					Do2WayDiffCore(id, winnerDif, goners, ChangedObjects, newbies);
				}
			}
			catch
			{ }
			return parentIndex;
		}

		private static void Do2WayDiffCore(string id, IChangeReport winnerDif, IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> ChangedObjects, IDictionary<string, XmlNode> newbies)
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
					ChangedObjects.Add(GetKey(originalNode, id), new ChangedElement
																		{
																			ParentNode = originalNode,
																			ChildNode = updatedNode
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
			internal XmlNode ParentNode;
			internal XmlNode ChildNode;
		}
	}
}