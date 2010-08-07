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
		/// <summary>
		/// Perform the 3-way merge.
		/// </summary>
		public static void Do3WayMerge(MergeOrder mergeOrder, IMergeStrategy mergeStrategy, // Get from mergeOrder: IMergeEventListener listener,
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
			CheckParameters(mergeStrategy, mergeOrder, mergeOrder.EventListener, pathToWinner, pathToLoser, commonAncestorPathname, recordElementName, id, writePreliminaryInformationDelegate);

			var newbies = new Dictionary<string, XmlNode>();
			// Do diff between winner and common
			var winnerGoners = new Dictionary<string, XmlNode>();
			var winnerDirtballs = new Dictionary<string, ChangedElement>();
			var parentIndex = Do2WayDiff(commonAncestorPathname, pathToWinner, winnerGoners, winnerDirtballs, newbies, recordElementName, id);

			// Do diff between loser and common
			var loserGoners = new Dictionary<string, XmlNode>();
			var loserDirtballs = new Dictionary<string, ChangedElement>();
			Do2WayDiff(parentIndex, pathToLoser, loserGoners, loserDirtballs, newbies, recordElementName, id);

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
				var enc = Encoding.UTF8;
				using (var reader = XmlReader.Create(new FileStream(commonAncestorPathname, FileMode.Open), readerSettings))
				{
					// This must be client specific behavior.
					writePreliminaryInformationDelegate(reader, writer);

					ProcessMainRecordElements(
						mergeStrategy, mergeOrder, mergeOrder.EventListener,
						winnerGoners, winnerDirtballs, loserGoners, loserDirtballs,
						reader, writer,
						id, winnerId, recordElementName);

					WriteOutNewObjects(newbies, enc, writer);

					writer.WriteEndElement();
				}
			}
		}

		private static void WriteOutNewObjects(Dictionary<string, XmlNode> newbies, Encoding enc, XmlWriter writer)
		{
			var readerSettings = new XmlReaderSettings
			{
				CheckCharacters = false,
				ConformanceLevel = ConformanceLevel.Fragment,
				ProhibitDtd = true,
				ValidationType = ValidationType.None,
				CloseInput = true,
				IgnoreWhitespace = true
			};
			foreach (var newby in newbies.Values)
			{
				// Note: If we need to put in a XmlAdditionChangeReport for newbies from 'loser',
				// Note: then we will need two 'newbie' lists.
				using (var nodeReader = XmlReader.Create(new MemoryStream(enc.GetBytes(newby.OuterXml)), readerSettings))
				{
					writer.WriteNode(nodeReader, false);
				}
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
				var currentId = reader.GetAttribute(id);

				if (winnerGoners.ContainsKey(currentId))
				{
					transferUntouched = false;
					ProcessDeletedRecordFromWinningData(mergeOrder, listener, winnerGoners, currentId, winnerId, recordElementName, loserGoners, loserDirtballs);
				}

				if (winnerDirtballs.ContainsKey(currentId))
				{
					transferUntouched = false;
					ProcessWinnerEditedRecord(mergeStrategy, mergeOrder, listener, currentId, winnerId, recordElementName, loserGoners, loserDirtballs, winnerDirtballs, writer);
				}

				if (loserGoners.ContainsKey(currentId))
				{
					// Loser deleted it but winner did nothing to it.
					// If winner had either deleted or edited it,
					// then the code above here would have been involved,
					// and currentId would have been removed from loserGoners.
					// The net effect is that it will be removed.
					transferUntouched = false;
					loserGoners.Remove(currentId);
				}
				if (loserDirtballs.ContainsKey(currentId))
				{
					// Loser changed it, but winner did nothing to it.
					transferUntouched = false;
					ReplaceCurrentNode(writer, loserDirtballs, currentId);
				}

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
			string currentId, string winnerId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs,
			IDictionary<string, ChangedElement> winnerDirtballs,
			XmlWriter writer)
		{
			if (loserGoners.ContainsKey(currentId))
			{
				// Winner edited it, but loser deleted it.
				// Make a conflict report.
				var dirtballElement = winnerDirtballs[currentId];
				listener.ConflictOccurred(new EditedVsRemovedElementConflict(
												recordElementName,
												dirtballElement.m_childNode,
												loserGoners[currentId],
												dirtballElement.m_parentNode,
												mergeOrder.MergeSituation,
												new ElementStrategy(false),
												winnerId));

				ReplaceCurrentNode(writer, dirtballElement.m_childNode);
				winnerDirtballs.Remove(currentId);
				loserGoners.Remove(currentId);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentId))
				{
					// Both edited it. Check it out.
					var mergedResult = winnerDirtballs[currentId].m_childNode.OuterXml;
					if (!XmlUtilities.AreXmlElementsEqual(winnerDirtballs[currentId].m_childNode, loserDirtballs[currentId].m_childNode))
					{
						var dirtballElement = winnerDirtballs[currentId];
						mergedResult = mergeStrategy.MakeMergedEntry(listener, dirtballElement.m_childNode,
																	 loserDirtballs[currentId].m_childNode, dirtballElement.m_parentNode);
					}
					ReplaceCurrentNode(writer, mergedResult);
					loserDirtballs.Remove(currentId);
				}
				else
				{
					// Winner edited it. Loser did nothing with it.
					ReplaceCurrentNode(writer, winnerDirtballs[currentId].m_childNode);
					winnerDirtballs.Remove(currentId);
				}
			}
		}

		private static void ReplaceCurrentNode(XmlWriter writer, IDictionary<string, ChangedElement> loserDirtballs, string currentId)
		{
			ReplaceCurrentNode(writer, loserDirtballs[currentId].m_childNode);
			loserDirtballs.Remove(currentId);
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
			string currentId, string winnerId, string recordElementName,
			IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs)
		{
			if (loserGoners.ContainsKey(currentId))
			{
				// Both deleted it.
				loserGoners.Remove(currentId);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentId))
				{
					var dirtball = loserDirtballs[currentId];
					// Winner deleted it, but loser edited it.
					// Make a conflict report.
					listener.ConflictOccurred(new RemovedVsEditedElementConflict(
													recordElementName,
													winnerGoners[currentId],
													dirtball.m_childNode,
													dirtball.m_parentNode,
													mergeOrder.MergeSituation,
													new ElementStrategy(false),
													winnerId));
					loserDirtballs.Remove(currentId);
				}
				// else // Winner deleted it and loser did nothing with it.
			}
			winnerGoners.Remove(currentId);
		}

		private static void Do2WayDiff(Dictionary<string, byte[]> parentIndex, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> dirtballs, IDictionary<string, XmlNode> newbies,
			string recordElementName, string id)
		{
			try
			{
				foreach (var winnerDif in Xml2WayDiffService.ReportDifferences(
					parentIndex, childPathname,
					new ChangeAndConflictAccumulator(),
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
			string recordElementName, string id)
		{
			Dictionary<string, byte[]> parentIndex = null;
			try
			{
				foreach (var winnerDif in Xml2WayDiffService.ReportDifferencesForMerge(
					parentPathname, childPathname,
					new ChangeAndConflictAccumulator(),
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
					goners.Add(gonerNode.Attributes[id].Value, gonerNode);
					break;
				case "XmlChangedRecordReport":
					var originalNode = asXmlReport.ParentNode;
					var updatedNode = asXmlReport.ChildNode;
					dirtballs.Add(originalNode.Attributes[id].Value, new ChangedElement
																		{
																			m_parentNode = originalNode,
																			m_childNode = updatedNode
																		});
					break;
				case "XmlAdditionChangeReport":
					var newbieNode = asXmlReport.ChildNode;
					newbies.Add(newbieNode.Attributes[id].Value, newbieNode);
					break;
			}
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