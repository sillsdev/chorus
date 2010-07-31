#define USENEWCODE
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;

namespace Chorus.FileTypeHanders.lift
{
	/// <summary>
	/// This is used by version control systems to do an intelligent 3-way merge of lift files.
	///
	/// This class is lift-specific.  It walks through each entry, applying a merger
	/// (which is currenlty, normally, something that uses the Chorus xmlMerger).
	///
	/// TODO: A confusing part here is the mix of levels we got from how this was built historically:
	/// file, lexentry, ultimately the chorus xml merger on the parts of the lexentry.  Each level seems to have some strategies.
	/// I (JH) wonder if we could move more down to the generic level.
	///
	/// Eventually, we may want a non-dom way to handle the top level, in which case having this class would be handy.
	/// </summary>
	public class LiftMerger
	{
		private IMergeStrategy _mergingStrategy;
		public IMergeEventListener EventListener = new NullMergeEventListener();
		private readonly MergeOrder _mergeOrder;
		private readonly string _pathToWinner;
		private readonly string _pathToLoser;
		private readonly string _pathToCommonAncestor;
		private readonly string _winnerId;

		/// <summary>
		/// Here, "alpha" is the guy who wins when there's no better way to decide, and "beta" is the loser.
		/// </summary>
		public LiftMerger(MergeOrder mergeOrder, string alphaLiftPath, string betaLiftPath, IMergeStrategy mergeStrategy, string ancestorLiftPath, string winnerId)
		{
			_mergeOrder = mergeOrder;
			_pathToWinner = alphaLiftPath;
			_pathToLoser = betaLiftPath;
			_pathToCommonAncestor = ancestorLiftPath;
			_winnerId = winnerId;
			_mergingStrategy = mergeStrategy;
		}

		private class ChangedElement
		{
			internal XmlNode m_parentNode;
			internal XmlNode m_childNode;
		}

		public void DoMerge(string outputPathname)
		{
			var newbies = new Dictionary<string, XmlNode>();
			// Do diff between winner and common
			var winnerGoners = new Dictionary<string, XmlNode>();
			var winnerDirtballs = new Dictionary<string, ChangedElement>();
			Do2WayDiff(_pathToCommonAncestor, _pathToWinner, winnerGoners, winnerDirtballs, newbies);

			// Do diff between loser and common
			var loserGoners = new Dictionary<string, XmlNode>();
			var loserDirtballs = new Dictionary<string, ChangedElement>();
			Do2WayDiff(_pathToCommonAncestor, _pathToLoser, loserGoners, loserDirtballs, newbies);

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
				using (var reader = XmlReader.Create(new FileStream(_pathToCommonAncestor, FileMode.Open), readerSettings))
				{
					WritePreliminaryInformation(reader, writer);

					ProcessMainRecordElements(winnerGoners, winnerDirtballs, loserGoners, loserDirtballs, reader, writer);

					WriteOutNewObjects(newbies, enc, writer);

					// ReSharper disable PossibleNullReferenceException
					writer.WriteEndElement();
					// ReSharper restore PossibleNullReferenceException
				}
			}
		}

		private void ProcessMainRecordElements(IDictionary<string, XmlNode> winnerGoners,
			IDictionary<string, ChangedElement> winnerDirtballs,
			IDictionary<string, XmlNode> loserGoners,
			IDictionary<string, ChangedElement> loserDirtballs,
			XmlReader reader, XmlWriter writer)
		{
			var keepReading = reader.Read();
			while (keepReading)
			{
				if (reader.EOF || !reader.IsStartElement())
					break;

				// 'entry' node is current node in reader.
				// Fetch id from 'entry' node and see if it is in either
				// of the modified/deleted dictionaries.
				var transferUntouched = true;
				var currentGuid = reader.GetAttribute("id");

				if (winnerGoners.ContainsKey(currentGuid))
				{
					transferUntouched = false;
					ProcessDeletedRecordFromWinningData(winnerGoners, currentGuid, loserGoners, loserDirtballs);
				}

				if (winnerDirtballs.ContainsKey(currentGuid))
				{
					transferUntouched = false;
					ProcessWinnerEditedRecord(currentGuid, loserGoners, loserDirtballs, winnerDirtballs, writer);
				}

				if (loserGoners.ContainsKey(currentGuid))
				{
					// Loser deleted it but winner did nothing to it.
					// If winner had either deleted or edited it,
					// then the code above here would have been involved,
					// and currentGuid would have been removed from loserGoners.
					// The net effect is that it will be removed.
					transferUntouched = false;
					loserGoners.Remove(currentGuid);
				}
				if (loserDirtballs.ContainsKey(currentGuid))
				{
					// Loser changed it, but winner did nothing to it.
					transferUntouched = false;
					ReplaceCurrentNode(writer, loserDirtballs, currentGuid);
				}

				if (!transferUntouched)
				{
					// Read to next <rt> element,
					// Which skips writing our the current element.
					FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
					reader.ReadOuterXml();
					keepReading = reader.IsStartElement();
					continue;
				}

				// Nobody did anything with the current source node, so just copy it to output.
				writer.WriteNode(reader, false);
				keepReading = reader.IsStartElement();
			}
		}

		private void ProcessWinnerEditedRecord(string currentGuid, IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs, IDictionary<string, ChangedElement> winnerDirtballs, XmlWriter writer)
		{
			if (loserGoners.ContainsKey(currentGuid))
			{
				// Winner edited it, but loser deleted it.
				// Make a conflict report.
				var dirtballElement = winnerDirtballs[currentGuid];
				EventListener.ConflictOccurred(new EditedVsRemovedElementConflict(
												"entry",
												dirtballElement.m_childNode,
												loserGoners[currentGuid],
												dirtballElement.m_parentNode,
												_mergeOrder.MergeSituation,
												new ElementStrategy(false),
												_winnerId));

				ReplaceCurrentNode(writer, dirtballElement.m_childNode);
				winnerDirtballs.Remove(currentGuid);
				loserGoners.Remove(currentGuid);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentGuid))
				{
					// Both edited it. Check it out.
					var mergedResult = winnerDirtballs[currentGuid].m_childNode.OuterXml;
					if (!XmlUtilities.AreXmlElementsEqual(winnerDirtballs[currentGuid].m_childNode, loserDirtballs[currentGuid].m_childNode))
					{
						var dirtballElement = winnerDirtballs[currentGuid];
						mergedResult = _mergingStrategy.MakeMergedEntry(EventListener, dirtballElement.m_childNode,
																	 loserDirtballs[currentGuid].m_childNode, dirtballElement.m_parentNode);
					}
					ReplaceCurrentNode(writer, mergedResult);
					loserDirtballs.Remove(currentGuid);
				}
				else
				{
					// Winner edited it. Loser did nothing with it.
					ReplaceCurrentNode(writer, winnerDirtballs[currentGuid].m_childNode);
					winnerDirtballs.Remove(currentGuid);
				}
			}
		}

		private void ProcessDeletedRecordFromWinningData(IDictionary<string, XmlNode> winnerGoners, string currentGuid, IDictionary<string, XmlNode> loserGoners, IDictionary<string, ChangedElement> loserDirtballs)
		{
			if (loserGoners.ContainsKey(currentGuid))
			{
				// Both deleted it.
				loserGoners.Remove(currentGuid);
			}
			else
			{
				if (loserDirtballs.ContainsKey(currentGuid))
				{
					var dirtball = loserDirtballs[currentGuid];
					// Winner deleted it, but loser edited it.
					// Make a conflict report.
					EventListener.ConflictOccurred(new RemovedVsEditedElementConflict(
													"entry",
													winnerGoners[currentGuid],
													dirtball.m_childNode,
													dirtball.m_parentNode,
													_mergeOrder.MergeSituation,
													new ElementStrategy(false),
													_winnerId));
					loserDirtballs.Remove(currentGuid);
				}
				// else // Winner deleted it and loser did nothing with it.
			}
			winnerGoners.Remove(currentGuid);
		}

		private static void ReplaceCurrentNode(XmlWriter writer, IDictionary<string, ChangedElement> loserDirtballs, string currentGuid)
		{
			ReplaceCurrentNode(writer, loserDirtballs[currentGuid].m_childNode);
			loserDirtballs.Remove(currentGuid);
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

		private static void WritePreliminaryInformation(XmlReader reader, XmlWriter writer)
		{
			reader.MoveToContent();
			writer.WriteStartElement("lift");
			if (reader.MoveToAttribute("version"))
				writer.WriteAttributeString("version", reader.Value);
			if (reader.MoveToAttribute("producer"))
				writer.WriteAttributeString("producer", reader.Value);
			reader.MoveToElement();
		}

		private static void Do2WayDiff(string parentPathname, string childPathname,
			IDictionary<string, XmlNode> goners, IDictionary<string, ChangedElement> dirtballs, IDictionary<string, XmlNode> newbies)
		{
			try
			{
				foreach (var winnerDif in Xml2WayDiffService.ReportDifferences(
					parentPathname, childPathname,
					new ChangeAndConflictAccumulator(),
					"entry", "id"))
				{
					if (!(winnerDif is IXmlChangeReport))
						continue; // It could be ErrorDeterminingChangeReport, so what to do with it?

					var asXmlReport = (IXmlChangeReport)winnerDif;
					switch (winnerDif.GetType().Name)
					{
						case "XmlDeletionChangeReport":
							var gonerNode = asXmlReport.ParentNode;
							goners.Add(gonerNode.Attributes["id"].Value, gonerNode);
							break;
						case "XmlChangedRecordReport":
							var originalNode = asXmlReport.ParentNode;
							var updatedNode = asXmlReport.ChildNode;
							dirtballs.Add(originalNode.Attributes["id"].Value, new ChangedElement
							{
								m_parentNode = originalNode,
								m_childNode = updatedNode
							});
							break;
						case "XmlAdditionChangeReport":
							var newbieNode = asXmlReport.ChildNode;
							newbies.Add(newbieNode.Attributes["id"].Value, newbieNode);
							break;
					}
				}
			}
			catch
			{}
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