using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.xml;
using Chorus.Properties;
using Chorus.Utilities;
using Palaso.Xml;

namespace Chorus.merge.xml.generic
{
	/// <summary>
	/// Service that manages Xml merging.
	/// </summary>
	public static class XmlMergeService
	{
		private static readonly XmlReaderSettings _readerSettings = new XmlReaderSettings
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Fragment,
					ProhibitDtd = true,
					ValidationType = ValidationType.None,
					CloseInput = true,
					IgnoreWhitespace = true
				};

		private static readonly Encoding _utf8 = Encoding.UTF8;

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
				default:
					throw new ArgumentException("The merge service cannot handle the requested conflict handling mode");
				case MergeOrder.ConflictHandlingModeChoices.WeWin:
					pathToWinner = mergeOrder.pathToOurs;
					pathToLoser = mergeOrder.pathToTheirs;
					winnerId = mergeOrder.MergeSituation.AlphaUserId;
					break;
				case MergeOrder.ConflictHandlingModeChoices.TheyWin:
					pathToWinner = mergeOrder.pathToTheirs;
					pathToLoser = mergeOrder.pathToOurs;
					winnerId = mergeOrder.MergeSituation.BetaUserId;
					break;
			}
			var commonAncestorPathname = mergeOrder.pathToCommonAncestor;
			// Do not change outputPathname, or be ready to fix SyncScenarioTests.CanCollaborateOnLift()!
			var outputPathname = mergeOrder.pathToOurs;
			CheckParameters(mergeStrategy, mergeOrder, mergeOrder.EventListener, pathToWinner, pathToLoser, commonAncestorPathname,
				recordElementName, id, writePreliminaryInformationDelegate);

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

					WriteOutNewObjects(mergeOrder.EventListener, winnerNewbies.Values, pathToWinner, writer);
					WriteOutNewObjects(mergeOrder.EventListener, loserNewbies.Values, pathToLoser, writer);

					writer.WriteEndElement();
				}
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
						var results = mergeStrategy.MakeMergedEntry(listener, currentNode, loserFirstElement, null);
						var doc = new XmlDocument();
						doc.LoadXml(results);
						WriteNode(doc, writer);
					}
					else
					{
						// Technically, both of them added the same thing.
						// Review: JohnH(RandyR) Should there be a special 'both added same thing' change report for this case?
						listener.ChangeOccurred(new XmlAdditionChangeReport(pathToWinner, currentNode));
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
					listener.ChangeOccurred(new XmlAdditionChangeReport(pathToWinner, currentNode));
					WriteNode(currentNode, writer);

					winnerNewbies.Remove(firstElementMarker);
					// These should never have them, but make sure.
					winnerGoners.Remove(firstElementMarker);
					winnerDirtballs.Remove(firstElementMarker);
					loserGoners.Remove(firstElementMarker);
					loserDirtballs.Remove(firstElementMarker);
				}

				return;
			}
			if (loserNewbies.TryGetValue(firstElementMarker, out currentNode))
			{
				// Brand new, so write it out and quit.
				// Loser added it.
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

			// Maybe it is not anywhere.
			if (((!winnerGoners.ContainsKey(firstElementMarker) && !winnerDirtballs.ContainsKey(firstElementMarker)) &&
				 !loserGoners.ContainsKey(firstElementMarker)) && !loserDirtballs.ContainsKey(firstElementMarker))
			{
				if (reader.LocalName == firstElementMarker)
					writer.WriteNode(reader, false);
				return;
			}

			// Do it the hard way for the others.
			var transferUntouched = true;

			ProcessCurrentElement(mergeOrder, firstElementMarker, mergeStrategy, winnerId, listener, writer, firstElementMarker,
									parentIndex,
								  loserDirtballs, loserGoners,
								  winnerDirtballs, winnerGoners, ref transferUntouched);

			if (!transferUntouched)
			{
				// Read to next main element,
				// Which skips writing out the current element.
				reader.ReadOuterXml();
				return;
			}

			// Nobody did anything with the current source node, so just copy it to output.
			writer.WriteNode(reader, false);
		}

		private static void ProcessCurrentElement(MergeOrder mergeOrder, string currentKey, IMergeStrategy mergeStrategy, string winnerId, IMergeEventListener listener, XmlWriter writer, string elementMarker,
			IDictionary<string, byte[]> parentIndex,
			IDictionary<string, ChangedElement> loserDirtballs, IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> winnerDirtballs, IDictionary<string, XmlNode> winnerGoners,
			ref bool transferUntouched)
		{
			if (winnerGoners.ContainsKey(currentKey))
			{
				transferUntouched = false;
				ProcessDeletedRecordFromWinningData(mergeOrder, listener, parentIndex, winnerGoners, currentKey, winnerId, elementMarker, loserGoners, loserDirtballs, writer);
			}

			if (winnerDirtballs.ContainsKey(currentKey))
			{
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
				AddDeletionReport(mergeOrder.pathToTheirs, currentKey, listener, parentIndex, loserGoners);
				transferUntouched = false;
				loserGoners.Remove(currentKey);
			}
			if (!loserDirtballs.ContainsKey(currentKey))
				return;

			// Loser changed it, but winner did nothing to it.
			transferUntouched = false;
			// Make change report(s) the hard way.
			var changedElement = loserDirtballs[currentKey];
			mergeStrategy.MakeMergedEntry(listener, null, changedElement._childNode, changedElement._parentNode);
			// No. Too high a level.
			//listener.ChangeOccurred((new XmlChangedRecordReport(null, null, loserDirtballs[currentKey]._parentNode,
			//                                                    loserDirtballs[currentKey]._childNode)));
			ReplaceCurrentNode(writer, loserDirtballs, currentKey);
		}

		private static void AddDeletionReport(string pathforRemover, string currentKey, IMergeEventListener listener,
											  IDictionary<string, byte[]> parentIndex, IDictionary<string, XmlNode> goners)
		{
			var doc = new XmlDocument();
			doc.LoadXml(Encoding.UTF8.GetString(parentIndex[currentKey.ToLowerInvariant()]));
			listener.ChangeOccurred(new XmlDeletionChangeReport(pathforRemover, doc.DocumentElement, goners[currentKey]));
		}

		private static void WriteOutNewObjects(IMergeEventListener listener, IEnumerable<XmlNode> newbies, string pathname, XmlWriter writer)
		{
			foreach (var newby in newbies)
			{
				listener.ChangeOccurred(new XmlAdditionChangeReport(pathname, newby));
				WriteNode(newby, writer);
			}
		}

		private static void WriteNode(XmlNode nodeToWrite, XmlWriter writer)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(_utf8.GetBytes(nodeToWrite.OuterXml)), _readerSettings))
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
					break;

				// 'entry' node is current node in reader.
				// Fetch id from 'entry' node and see if it is in either
				// of the modified/deleted dictionaries.
				var transferUntouched = true;
				var currentKey = reader.GetAttribute(id);

				ProcessCurrentElement(mergeOrder, currentKey, mergeStrategy, winnerId, listener, writer, recordElementName,
					parentIndex,
					loserDirtballs, loserGoners,
					winnerDirtballs, winnerGoners, ref transferUntouched);

				if (!transferUntouched)
				{
#if DEBUG
					FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
#endif
					// Read to next record element,
					// Which skips writing our the current element.
					reader.ReadOuterXml();
					keepReading = reader.IsStartElement();
					continue;
				}

				// Nobody did anything with the current source node, so just copy it to output.
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
				// Winner edited it, but loser deleted it.
				// Make a conflict report.
				var dirtballElement = winnerDirtballs[currentKey];
				listener.ConflictOccurred(new EditedVsRemovedElementConflict(
												recordElementName,
												dirtballElement._childNode,
												loserGoners[currentKey],
												dirtballElement._parentNode,
												mergeOrder.MergeSituation,
												new ElementStrategy(false),
												winnerId));

				ReplaceCurrentNode(writer, dirtballElement._childNode);
				winnerDirtballs.Remove(currentKey);
				loserGoners.Remove(currentKey);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentKey))
				{
					// Both edited it. Check it out.
					// Too high up.
					//var mergedResult = winnerDirtballs[currentKey]._childNode.OuterXml;
					//if (XmlUtilities.AreXmlElementsEqual(winnerDirtballs[currentKey]._childNode, loserDirtballs[currentKey]._childNode))
					//{
					//    // Both made the same change.
					//    listener.ChangeOccurred(new XmlChangedRecordReport(null, null, winnerDirtballs[currentKey]._parentNode,
					//                                                       winnerDirtballs[currentKey]._childNode));
					//}
					//else
					//{
					//    var dirtballElement = winnerDirtballs[currentKey];
					//    mergedResult = mergeStrategy.MakeMergedEntry(listener, dirtballElement._childNode,
					//                                                 loserDirtballs[currentKey]._childNode, dirtballElement._parentNode);
					//}
					var dirtballElement = winnerDirtballs[currentKey];
					var mergedResult = mergeStrategy.MakeMergedEntry(listener, dirtballElement._childNode,
																 loserDirtballs[currentKey]._childNode, dirtballElement._parentNode);
					ReplaceCurrentNode(writer, mergedResult);
					loserDirtballs.Remove(currentKey);
				}
				else
				{
					// Winner edited it. Loser did nothing with it.
					// Too high up.
					//listener.ChangeOccurred(new XmlChangedRecordReport(null, null, winnerDirtballs[currentKey]._parentNode,
					//												   winnerDirtballs[currentKey]._childNode));
					var dirtballElement = winnerDirtballs[currentKey];
					var mergedResult = mergeStrategy.MakeMergedEntry(listener, dirtballElement._childNode, null, dirtballElement._parentNode);
					ReplaceCurrentNode(writer, mergedResult);
					winnerDirtballs.Remove(currentKey);
				}
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
					listener.ConflictOccurred(new RemovedVsEditedElementConflict(
													recordElementName,
													winnerGoners[currentKey],
													dirtball._childNode,
													dirtball._parentNode,
													mergeOrder.MergeSituation,
													new ElementStrategy(false),
													winnerId));
					// Write out edited node, under the least loss principle.
					ReplaceCurrentNode(writer, loserDirtballs, currentKey);
					loserDirtballs.Remove(currentKey);
				}
				else
				{
					// Winner deleted it and loser did nothing with it.
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

		private static void CheckParameters(IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			string winnerPathname, string loserPathname, string commonAncestorPathname,
			string recordElementName, string id,
			Action<XmlReader, XmlWriter> writePreliminaryInformationDelegate)
		{
			if (mergeStrategy == null) throw new ArgumentNullException("mergeStrategy");
			if (mergeOrder == null) throw new ArgumentNullException("mergeOrder");
			if (listener == null) throw new ArgumentNullException("listener");
			if (writePreliminaryInformationDelegate == null)
				throw new ArgumentNullException("writePreliminaryInformationDelegate");

			CheckThatFileExists(winnerPathname);
			CheckThatFileExists(loserPathname);
			CheckThatFileExists(commonAncestorPathname);

			CheckParameterForContent(recordElementName);
			CheckParameterForContent(id);
		}

		private static void CheckParameterForContent(string parameter)
		{
			if (String.IsNullOrEmpty(parameter))
				throw new ArgumentNullException("parameter");
		}

		private static void CheckThatFileExists(string pathname)
		{
			if (!File.Exists(pathname))
				throw new ArgumentException(AnnotationImages.kFileDoesNotExist, "pathname");
		}

		private class ChangedElement
		{
			internal XmlNode _parentNode;
			internal XmlNode _childNode;
		}

		internal static void AddDateCreatedAttribute(XmlNode elementNode)
		{
			AddAttribute(elementNode, "dateCreated", DateTime.Now.ToString(LiftUtils.LiftTimeFormatNoTimeZone));
		}

		internal static void AddAttribute(XmlNode element, string name, string value)
		{
			XmlAttribute attr = element.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			element.Attributes.Append(attr);
		}
	}
}