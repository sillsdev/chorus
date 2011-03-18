using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Palaso.Xml;

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
		private readonly string _alphaLift;
		private readonly string _betaLift;
		private readonly string _ancestorLift;
		private readonly Dictionary<string, XmlNode> _comonIdToNodeIndex;
		private readonly Dictionary<string, XmlNode> _alphaIdToNodeIndex;
		private readonly Dictionary<string, XmlNode> _betaIdToNodeIndex;
		private readonly HashSet<string> _processedIds;
		private readonly XmlDocument _alphaDom;
		private readonly XmlDocument _betaDom;
		private readonly XmlDocument _ancestorDom;
		private IMergeStrategy _mergingStrategy;
		public IMergeEventListener EventListener = new NullMergeEventListener();

		/// <summary>
		/// Here, "alpha" is the guy who wins when there's no better way to decide, and "beta" is the loser.
		/// </summary>
		public LiftMerger(IMergeStrategy mergeStrategy, string alphaLiftPath, string betaLiftPath, string ancestorLiftPath)
		{
			_processedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_alphaLift = File.ReadAllText(alphaLiftPath);
			_betaLift =  File.ReadAllText(betaLiftPath);
			_ancestorLift = File.ReadAllText(ancestorLiftPath);
			_comonIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_alphaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_betaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_alphaDom = new XmlDocument();
			_betaDom = new XmlDocument();
			_ancestorDom = new XmlDocument();

			_mergingStrategy = mergeStrategy;

//            string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//                           @"chorusMergeOrder" + Path.GetFileName(_alphaLiftPath) + ".txt");
//            File.WriteAllText(path, "Merging alphaS\r\n" + _alphaLift + "\r\n----------betaS\r\n" + _betaLift + "\r\n----------ANCESTOR\r\n" + _ancestorLift);
		}

		/// <summary>
		/// Used by tests, which prefer to give us raw contents rather than paths
		/// </summary>
		public LiftMerger(string alphaLiftContents, string betaLiftContents, string ancestorLiftContents, IMergeStrategy mergeStrategy)
		{
			_processedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_alphaLift = alphaLiftContents;
			_betaLift = betaLiftContents;
			_ancestorLift = ancestorLiftContents;
			_comonIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_alphaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_betaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);
			_alphaDom = new XmlDocument();
			_betaDom = new XmlDocument();
			_ancestorDom = new XmlDocument();

			_mergingStrategy = mergeStrategy;
		}

		public string GetMergedLift()
		{
//            string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//                                    @"chorusMergeResult" + Path.GetFileName(_alphaLiftPath) + ".txt");
//
//            File.WriteAllText(path, "ENter GetMergedLift()");

			_alphaDom.LoadXml(_alphaLift);
			_betaDom.LoadXml(_betaLift);
			_ancestorDom.LoadXml(_ancestorLift);

			// The memory stream, rather than a string builder, is needed to avoid utf-16 coming out
			using (var memoryStream = new MemoryStream())
			{
				foreach (XmlNode commonNode in _ancestorDom.SafeSelectNodes("lift/entry"))
					_comonIdToNodeIndex[LiftUtils.GetId(commonNode)] = commonNode;
				foreach (XmlNode alphaNode in _alphaDom.SafeSelectNodes("lift/entry"))
					_alphaIdToNodeIndex[LiftUtils.GetId(alphaNode)] = alphaNode;
				foreach (XmlNode betaNode in _betaDom.SafeSelectNodes("lift/entry"))
					_betaIdToNodeIndex[LiftUtils.GetId(betaNode)] = betaNode;

				var settings = CanonicalXmlSettings.CreateXmlWriterSettings();
				settings.CloseOutput = false;
				using (var writer = XmlWriter.Create(memoryStream, settings))
				{
					WriteStartOfLiftElement(writer);

					ProcessHeaderNodeHACK(writer);

					foreach (XmlNode alphaEntry in _alphaDom.SafeSelectNodes("lift/entry"))
						ProcessAlphaEntry(writer, alphaEntry);
					foreach (XmlNode betaEntry in _betaDom.SafeSelectNodes("lift/entry"))
						ProcessBetaEntry(writer, betaEntry);

					writer.WriteEndElement();
					writer.Close();
				}

				// Don't use GetBuffer()!!! it pads the results with nulls:  return Encoding.UTF8.GetString(memoryStream.ToArray());
				// This works but doubles the ram use: return Encoding.UTF8.GetString(memoryStream.ToArray());
				return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
			}
		}

		private void ProcessHeaderNodeHACK(XmlWriter writer)
		{
			// Without a principled merge system for the header element,
			// just pick the longest one from alpha/beta.
			// NB: This has *no* tests, since it is such a hack.
			var alphaHeader = _alphaDom.SelectSingleNode("lift/header");
			var betaHeader = _betaDom.SelectSingleNode("lift/header");
			XmlNode winningHeader = null;
			if (alphaHeader == null && betaHeader == null)
				winningHeader = null;
			else if (alphaHeader == null & betaHeader != null)
				winningHeader = betaHeader;
			else if (betaHeader == null && alphaHeader != null)
				winningHeader = alphaHeader;
			else if (alphaHeader.ChildNodes.Count > betaHeader.ChildNodes.Count)
				winningHeader = alphaHeader;
			else
				winningHeader = betaHeader;
			if (winningHeader != null)
				writer.WriteNode(winningHeader.CreateNavigator(), false);
		}

		private void ProcessBetaEntry(XmlWriter writer, XmlNode betaEntry)
		{
			var id = LiftUtils.GetId(betaEntry);
			if (_processedIds.Contains(id))
				return; // Already handled when ProcessAlphaEntry method called.
			XmlNode alphaEntry;
			var commonEntry = FindEntry(_ancestorDom, id);

			if (!_alphaIdToNodeIndex.TryGetValue(id, out alphaEntry))
			{
				// It is in beta, but not in alpha, so it clearly has been deleted by alpha (new style deletion).
				if (LiftUtils.GetIsMarkedAsDeleted(betaEntry))
				{
					if (!LiftUtils.GetIsMarkedAsDeleted(commonEntry))
					{
						// alpha and beta both deleted but in different ways, so make deletion change report.
						// There is no need to write anything.
						EventListener.ChangeOccurred(new XmlDeletionChangeReport(_betaLift, commonEntry, betaEntry));
					}
					//else
					//{
					//    // Make it go away without fanfare by doing nothing to writer, since alpha actually removed the dead entry.
					//}
					_processedIds.Add(id);
					return;
				}

				if (commonEntry == null)
				{
					// Beta added it.
					EventListener.ChangeOccurred(new XmlAdditionChangeReport(_betaLift, betaEntry));
					writer.WriteNode(betaEntry.CreateNavigator(), false);
				}
				else
				{
					if (AreTheSame(betaEntry, commonEntry))
					{
						// Alpha's deletion wins with a plain vanilla deletion report.
						EventListener.ChangeOccurred(new XmlDeletionChangeReport(_betaLift, commonEntry, alphaEntry));
					}
					else
					{
						// Beta wins with a edit vs remove conflict report,
						// as per the least loss priciple.
						EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", betaEntry, alphaEntry, commonEntry, new MergeSituation(_betaLift, "beta", "", "alpha", "", MergeOrder.ConflictHandlingModeChoices.TheyWin), null, "beta"));
						writer.WriteNode(betaEntry.CreateNavigator(), false);
					}
					_processedIds.Add(id);
					return;
				}
			}
			else if (AreTheSame(alphaEntry, betaEntry))//unchanged or both made same change
			{
				writer.WriteNode(alphaEntry.CreateNavigator(), false);
			}
			else if (LiftUtils.GetIsMarkedAsDeleted(alphaEntry))
			{
				// Beta edited, alpha deleted (old style deletion using dateDeleted attr).
				EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", betaEntry, alphaEntry, commonEntry, new MergeSituation(_betaLift, "beta", "", "alpha", "", MergeOrder.ConflictHandlingModeChoices.TheyWin), null, "beta"));
			}
			else
			{
#if false
				// One or both of them edited it, so merge the hard way.
				using (var reader = XmlReader.Create(new StringReader(
					_mergingStrategy.MakeMergedEntry(EventListener, alphaEntry, betaEntry, commonEntry)
				)))
				{
					writer.WriteNode(reader, false);
				}
#else
				if (LiftUtils.GetIsMarkedAsDeleted(betaEntry))
				{
					// Q (by RBR): This path is not used by the six new tests. Is that a problem?
					// ANS (by RBR): No, we edited, they deleted (old style) is covered by a test,
					// and the case is handled in the ProcessAlphaEntry method.
					// Leave the check in for histroiucal purposes,
					// the question doesn't come up again.

					// They deleted (old style), but we edited.
					// We win with the least loss principle.
					// But generate a conflict report.
					EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", alphaEntry, betaEntry, commonEntry, new MergeSituation(_alphaLift, "alpha", "", "beta", "", MergeOrder.ConflictHandlingModeChoices.WeWin), null, "alpha"));
					writer.WriteNode(betaEntry.CreateNavigator(), false);
				}
				else
				{
					// One or both of them edited it, so merge the hard way.
					using (var reader = XmlReader.Create(new StringReader(
						_mergingStrategy.MakeMergedEntry(EventListener, alphaEntry, betaEntry, commonEntry)
					)))
					{
						writer.WriteNode(reader, false);
					}
				}
#endif
			}
			_processedIds.Add(id);
		}

		private static XmlNode FindEntry(XmlNode doc, string id)
		{
			FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
			return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		}

		private void ProcessAlphaEntry(XmlWriter writer, XmlNode alphaEntry)
		{
			string id = LiftUtils.GetId(alphaEntry);
			XmlNode betaEntry;
			var commonEntry = FindEntry(_ancestorDom, id);
			if(!_betaIdToNodeIndex.TryGetValue(id, out betaEntry))
			{
				// It is in alpha, but not in beta, so it clearly has been deleted by beta (new style deletion).
				if (LiftUtils.GetIsMarkedAsDeleted(alphaEntry))
				{
					if (!LiftUtils.GetIsMarkedAsDeleted(commonEntry))
					{
						// alpha and beta both deleted but in different ways, so make deletion change report.
						// There is no need to write anything.
						EventListener.ChangeOccurred(new XmlDeletionChangeReport(_alphaLift, commonEntry, alphaEntry));
					}
					//else
					//{
					//    // Make it go away without fanfare by doing nothing to writer, since beta actually removed the dead entry.
					//}
					_processedIds.Add(id);
					return;
				}

				if (commonEntry == null)
				{
					// Alpha added it
					EventListener.ChangeOccurred(new XmlAdditionChangeReport(_alphaLift, alphaEntry));
					writer.WriteNode(alphaEntry.CreateNavigator(), false);
				}
				else
				{
					if (AreTheSame(alphaEntry, commonEntry))
					{
						// beta's deletion wins with a plain vanilla deletion report.
						EventListener.ChangeOccurred(new XmlDeletionChangeReport(_alphaLift, commonEntry, betaEntry));
					}
					else
					{
						// Alpha wins over beta's new style deletion on the least loss priciple.
						// Add an edit vs remove conflict report.
						EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", alphaEntry, betaEntry, commonEntry, new MergeSituation(_alphaLift, "alpha", "", "beta", "", MergeOrder.ConflictHandlingModeChoices.WeWin), null, "alpha"));
						writer.WriteNode(alphaEntry.CreateNavigator(), false);
					}
					_processedIds.Add(id);
					return;
				}
			}
			else if (AreTheSame(alphaEntry, betaEntry))//unchanged or both made same change
			{
				writer.WriteNode(alphaEntry.CreateNavigator(), false);
			}
			else if (LiftUtils.GetIsMarkedAsDeleted(betaEntry))
			{
				// We edited, they deleted (old style deletion using dateDeleted attr).
				EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", alphaEntry, betaEntry, commonEntry, new MergeSituation(_alphaLift, "alpha", "", "beta", "", MergeOrder.ConflictHandlingModeChoices.WeWin), null, "alpha"));
			}
			else
			{
				if (LiftUtils.GetIsMarkedAsDeleted(alphaEntry))
				{
					// We deleted (old style), but they edited.
					// They win with the least loss principle.
					// But generate a conflict report.
					EventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", betaEntry, alphaEntry, commonEntry, new MergeSituation(_betaLift, "beta", "", "alpha", "", MergeOrder.ConflictHandlingModeChoices.TheyWin), null, "beta"));
					writer.WriteNode(betaEntry.CreateNavigator(), false);
				}
				else
				{
					// One or both of them edited it, so merge the hard way.
					using (var reader = XmlReader.Create(new StringReader(
						_mergingStrategy.MakeMergedEntry(EventListener, alphaEntry, betaEntry, commonEntry)
					)))
					{
						writer.WriteNode(reader, false);
					}
				}
			}
			_processedIds.Add(id);
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

		private static bool AreTheSame(XmlNode alphaEntry, XmlNode betaEntry)
		{
			//review: why do we need to actually parse these dates?  Could we just do a string comparison?
			if (LiftUtils.GetModifiedDate(betaEntry) == LiftUtils.GetModifiedDate(alphaEntry)
				&& !(LiftUtils.GetModifiedDate(betaEntry) == default(DateTime)))
				return true;

			return XmlUtilities.AreXmlElementsEqual(alphaEntry.OuterXml, betaEntry.OuterXml);
		}



		private void WriteStartOfLiftElement(XmlWriter writer)
		{
			XmlNode liftNode = _alphaDom.SelectSingleNode("lift");

			writer.WriteStartElement(liftNode.Name);
			foreach (XmlAttribute attribute in liftNode.Attributes)
			{
				writer.WriteAttributeString(attribute.Name, attribute.Value);
			}
		}

	}
}