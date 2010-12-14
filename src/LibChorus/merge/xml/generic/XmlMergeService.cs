using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.xml;
using Chorus.Utilities;

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

			var newbies = new Dictionary<string, XmlNode>();
			// Do diff between winner and common
			var winnerGoners = new Dictionary<string, XmlNode>();
			var winnerDirtballs = new Dictionary<string, ChangedElement>();
			var parentIndex = Do2WayDiff(commonAncestorPathname, pathToWinner, winnerGoners, winnerDirtballs, newbies,
				firstElementMarker,
				recordElementName, id);

			// Do diff between loser and common
			var loserGoners = new Dictionary<string, XmlNode>();
			var loserDirtballs = new Dictionary<string, ChangedElement>();
			Do2WayDiff(parentIndex, pathToLoser, loserGoners, loserDirtballs, newbies,
				firstElementMarker,
				recordElementName, id);

			// At this point we have two sets of diffs, but we need to merge them.
			// Newbies from both get added.
			// A conflict has 'winner' stay, but with a report.
			using (var writer = XmlWriter.Create(outputPathname, new XmlWriterSettings
			{
				OmitXmlDeclaration = false,
				CheckCharacters = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = new UTF8Encoding(false),
				Indent = true,
				IndentChars = (""),
				NewLineOnAttributes = false
			}))
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
							newbies,
							winnerGoners, winnerDirtballs,
							loserGoners, loserDirtballs,
							reader, writer,
							winnerId, firstElementMarker);
					}

					ProcessMainRecordElements(
						mergeStrategy, mergeOrder, mergeOrder.EventListener,
						winnerGoners, winnerDirtballs, loserGoners, loserDirtballs,
						reader, writer,
						id, winnerId, recordElementName);

					WriteOutNewObjects(newbies.Values, writer);

					writer.WriteEndElement();
				}
			}
		}

		private static void ProcessFirstElement(
			IMergeStrategy mergeStrategy, MergeOrder mergeOrder, IMergeEventListener listener,
			IDictionary<string, XmlNode> newbies,
			IDictionary<string, XmlNode> winnerGoners, IDictionary<string, ChangedElement> winnerDirtballs,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			XmlReader reader, XmlWriter writer,
			string winnerId, string firstElementMarker)
		{
			XmlNode currentNode;
			if (newbies.TryGetValue(firstElementMarker, out currentNode))
			{
				// Brand new, so write it out and quit.
				newbies.Remove(firstElementMarker);
				WriteNode(currentNode, writer);

				// This should never have them, but make sure.
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
								  loserDirtballs, loserGoners,
								  winnerDirtballs, winnerGoners, ref transferUntouched);

			if (!transferUntouched)
			{
				// Read to next main element,
				// Which skips writing our the current element.
				reader.ReadOuterXml();
				return;
			}

			// Nobody did anything with the current source node, so just copy it to output.
			writer.WriteNode(reader, false);
		}

		private static void ProcessCurrentElement(MergeOrder mergeOrder, string currentKey, IMergeStrategy mergeStrategy, string winnerId, IMergeEventListener listener, XmlWriter writer, string elementMarker,
			IDictionary<string, ChangedElement> loserDirtballs, IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> winnerDirtballs, IDictionary<string, XmlNode> winnerGoners,
			ref bool transferUntouched)
		{
			if (winnerGoners.ContainsKey(currentKey))
			{
				transferUntouched = false;
				ProcessDeletedRecordFromWinningData(mergeOrder, listener, winnerGoners, currentKey, winnerId, elementMarker, loserGoners, loserDirtballs, writer);
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
				transferUntouched = false;
				loserGoners.Remove(currentKey);
			}
			if (loserDirtballs.ContainsKey(currentKey))
			{
				// Loser changed it, but winner did nothing to it.
				transferUntouched = false;
				ReplaceCurrentNode(writer, loserDirtballs, currentKey);
			}
		}

		private static void WriteOutNewObjects(IEnumerable<XmlNode> newbies, XmlWriter writer)
		{
			// Note: If we need to put in a XmlAdditionChangeReport for newbies from 'loser',
			// Note: then we will need two 'newbie' lists.
			foreach (var newby in newbies)
				WriteNode(newby, writer);
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
					loserDirtballs, loserGoners,
					winnerDirtballs, winnerGoners, ref transferUntouched);

				if (!transferUntouched)
				{
#if DEBUG
					FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
#endif
					// Read to next <rt> element,
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
												dirtballElement.m_childNode,
												loserGoners[currentKey],
												dirtballElement.m_parentNode,
												mergeOrder.MergeSituation,
												new ElementStrategy(false),
												winnerId));

				ReplaceCurrentNode(writer, dirtballElement.m_childNode);
				winnerDirtballs.Remove(currentKey);
				loserGoners.Remove(currentKey);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentKey))
				{
					// Both edited it. Check it out.
					var mergedResult = winnerDirtballs[currentKey].m_childNode.OuterXml;
					if (!XmlUtilities.AreXmlElementsEqual(winnerDirtballs[currentKey].m_childNode, loserDirtballs[currentKey].m_childNode))
					{
						var dirtballElement = winnerDirtballs[currentKey];
						mergedResult = mergeStrategy.MakeMergedEntry(listener, dirtballElement.m_childNode,
																	 loserDirtballs[currentKey].m_childNode, dirtballElement.m_parentNode);
					}
					ReplaceCurrentNode(writer, mergedResult);
					loserDirtballs.Remove(currentKey);
				}
				else
				{
					// Winner edited it. Loser did nothing with it.
					ReplaceCurrentNode(writer, winnerDirtballs[currentKey].m_childNode);
					winnerDirtballs.Remove(currentKey);
				}
			}
		}

		private static void ReplaceCurrentNode(XmlWriter writer, IDictionary<string, ChangedElement> loserDirtballs, string currentKey)
		{
			ReplaceCurrentNode(writer, loserDirtballs[currentKey].m_childNode);
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
			IDictionary<string, XmlNode> winnerGoners,
			string currentKey, string winnerId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			XmlWriter writer)
		{
			if (loserGoners.ContainsKey(currentKey))
			{
				// Both deleted it.
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
													dirtball.m_childNode,
													dirtball.m_parentNode,
													mergeOrder.MergeSituation,
													new ElementStrategy(false),
													winnerId));
					// Write out edited node, under the least loss principle.
					ReplaceCurrentNode(writer, loserDirtballs, currentKey);
					loserDirtballs.Remove(currentKey);
				}
				// else // Winner deleted it and loser did nothing with it.
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
																			m_parentNode = originalNode,
																			m_childNode = updatedNode
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
				throw new ArgumentException("File does not exist.", "pathname");
		}

		private class ChangedElement
		{
			internal XmlNode m_parentNode;
			internal XmlNode m_childNode;
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