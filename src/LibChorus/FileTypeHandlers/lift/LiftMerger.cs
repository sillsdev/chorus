using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.Utilities;
using Palaso.Xml;
using Palaso.Providers;
using Chorus.FileTypeHandlers.lift;
using Chorus.FileTypeHandlers.xml;

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
	/// [ANS RBR: You get a non-dom way in 'default', but the result is these FooMerger classes all go away. :-)]
	/// </summary>
	public class LiftMerger
	{
		private readonly string _alphaLift;
		private readonly string _betaLift;
		private readonly string _ancestorLift;
		private readonly IMergeStrategy _mergingStrategy;
		public IMergeEventListener EventListener = new NullMergeEventListener();

		/// <summary>
		/// Here, "alpha" is the guy who wins when there's no better way to decide, and "beta" is the loser.
		/// </summary>
		public LiftMerger(IMergeStrategy mergeStrategy, string alphaLiftPath, string betaLiftPath, string ancestorLiftPath)
			: this(File.ReadAllText(alphaLiftPath), File.ReadAllText(betaLiftPath), File.ReadAllText(ancestorLiftPath), mergeStrategy)
		{
		}

		/// <summary>
		/// Used by tests (and public constructor), which prefer to give us raw contents rather than paths
		/// </summary>
		internal LiftMerger(string alphaLiftContents, string betaLiftContents, string ancestorLiftContents, IMergeStrategy mergeStrategy)
		{
			_alphaLift = alphaLiftContents;
			_betaLift = betaLiftContents;
			_ancestorLift = ancestorLiftContents;
			_mergingStrategy = mergeStrategy;
		}

		public string GetMergedLift()
		{
//            string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//                                    @"chorusMergeResult" + Path.GetFileName(_alphaLiftPath) + ".txt");
//
//            File.WriteAllText(path, "ENter GetMergedLift()");

			var ancestorDom = new XmlDocument();
			ancestorDom.LoadXml(_ancestorLift);
			//var comonIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);

			var alphaDom = new XmlDocument();
			alphaDom.LoadXml(_alphaLift);
			var alphaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);

			var betaDom = new XmlDocument();
			betaDom.LoadXml(_betaLift);
			var betaIdToNodeIndex = new Dictionary<string, XmlNode>(StringComparer.OrdinalIgnoreCase);

			var processedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// The memory stream, rather than a string builder, is needed to avoid utf-16 coming out
			using (var memoryStream = new MemoryStream())
			{
				//foreach (XmlNode commonNode in _ancestorDom.SafeSelectNodes("lift/entry"))
				//	comonIdToNodeIndex[LiftUtils.GetId(commonNode)] = commonNode;
				foreach (XmlNode alphaNode in alphaDom.SafeSelectNodes("lift/entry"))
					alphaIdToNodeIndex[LiftUtils.GetId(alphaNode)] = alphaNode;
				foreach (XmlNode betaNode in betaDom.SafeSelectNodes("lift/entry"))
					betaIdToNodeIndex[LiftUtils.GetId(betaNode)] = betaNode;

				var settings = CanonicalXmlSettings.CreateXmlWriterSettings();
				settings.CloseOutput = false;
				using (var writer = XmlWriter.Create(memoryStream, settings))
				{
					WriteStartOfLiftElement(writer, alphaDom);

					ProcessHeaderNodeHACK(writer, alphaDom, betaDom);
					// Process alpha's entries
					ProcessEntries(writer, EventListener, _mergingStrategy, ancestorDom, processedIds,
								   alphaDom, "alpha", _alphaLift,
								   betaIdToNodeIndex, "beta", _betaLift);
					// Process beta's entries
					ProcessEntries(writer, EventListener, _mergingStrategy, ancestorDom, processedIds,
								   betaDom, "beta", _betaLift,
								   alphaIdToNodeIndex, "alpha", _alphaLift);
					writer.WriteEndElement();
					writer.Close();
				}

				// Don't use GetBuffer()!!! it pads the results with nulls:  return Encoding.UTF8.GetString(memoryStream.ToArray());
				// This works but doubles the ram use: return Encoding.UTF8.GetString(memoryStream.ToArray());
				return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
			}
		}

		private static void ProcessHeaderNodeHACK(XmlWriter writer, XmlNode alphaDom, XmlNode betaDom)
		{
			// Without a principled merge system for the header element,
			// just pick the longest one from alpha/beta.
			// NB: This has *no* tests, since it is such a hack.
			var alphaHeader = alphaDom.SelectSingleNode("lift/header");
			var betaHeader = betaDom.SelectSingleNode("lift/header");
			XmlNode winningHeader;
			if (alphaHeader == null && betaHeader == null)
				winningHeader = null;
			else if (alphaHeader == null & betaHeader != null)
				winningHeader = betaHeader;
			else if (betaHeader == null)
				winningHeader = alphaHeader;
			else if (alphaHeader.ChildNodes.Count > betaHeader.ChildNodes.Count)
				winningHeader = alphaHeader;
			else
				winningHeader = betaHeader;
			if (winningHeader != null)
				writer.WriteNode(winningHeader.CreateNavigator(), false);
		}

		private static void ProcessEntries(XmlWriter writer, IMergeEventListener eventListener, IMergeStrategy mergingStrategy,
			XmlNode ancestorDom, HashSet<string> processedIds,
			XmlNode sourceDom, string sourceLabel, string sourcePath,
			IDictionary<string, XmlNode> otherIdNodeIndex, string otherLabel, string otherPath)
		{
			foreach (XmlNode sourceEntry in sourceDom.SafeSelectNodes("lift/entry"))
			{
				ProcessEntry(writer, eventListener, mergingStrategy, ancestorDom, processedIds,
							 sourceEntry, sourceLabel, sourcePath,
							 otherIdNodeIndex, otherLabel, otherPath);
			}
		}

		private static void ProcessEntry(XmlWriter writer, IMergeEventListener eventListener, IMergeStrategy mergingStrategy,
			XmlNode ancestorDom, HashSet<string> processedIds,
			XmlNode sourceEntry, string sourceLabel, string sourcePath,
			IDictionary<string, XmlNode> otherIdNodeIndex, string otherLabel, string otherPath)
		{
			var id = LiftUtils.GetId(sourceEntry);
			if (processedIds.Contains(id))
				return; // Already handled the id.
			XmlNode otherEntry;
			var commonEntry = FindEntry(ancestorDom, id);
			if (!otherIdNodeIndex.TryGetValue(id, out otherEntry))
			{
				// It is in source, but not in target, so it clearly has been deleted by target (new style deletion).
				if (LiftUtils.GetIsMarkedAsDeleted(sourceEntry))
				{
					if (!LiftUtils.GetIsMarkedAsDeleted(commonEntry))
					{
						// source and target both deleted but in different ways, so make deletion change report.
						// There is no need to write anything.
						eventListener.ChangeOccurred(new XmlDeletionChangeReport(sourcePath, commonEntry, sourceEntry));
					}
					//else
					//{
					//    // Make it go away without fanfare by doing nothing to writer, since target actually removed the dead entry.
					//}
					processedIds.Add(id);
					return;
				}

				if (commonEntry == null)
				{
					// source added it
					eventListener.ChangeOccurred(new XmlAdditionChangeReport(sourcePath, sourceEntry));
					writer.WriteNode(sourceEntry.CreateNavigator(), false);
				}
				else
				{
					if (AreTheSame(sourceEntry, commonEntry))
					{
						// target's deletion wins with a plain vanilla deletion report.
						eventListener.ChangeOccurred(new XmlDeletionChangeReport(sourcePath, commonEntry, otherEntry));
					}
					else
					{
						// source wins over target's new style deletion on the least loss priciple.
						// Add an edit vs remove conflict report.
						eventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", sourceEntry, otherEntry, commonEntry, new MergeSituation(sourcePath, sourceLabel, "", otherLabel, "", MergeOrder.ConflictHandlingModeChoices.WeWin), null, sourceLabel));
						writer.WriteNode(sourceEntry.CreateNavigator(), false);
					}
					processedIds.Add(id);
					return;
				}
			}
			else if (AreTheSame(sourceEntry, otherEntry))
			{
				//unchanged or both made same change
				writer.WriteNode(sourceEntry.CreateNavigator(), false);
			}
			else if (LiftUtils.GetIsMarkedAsDeleted(otherEntry))
			{
				// source edited, target deleted (old style deletion using dateDeleted attr).
				eventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", sourceEntry, otherEntry, commonEntry, new MergeSituation(sourcePath, sourceLabel, "", otherLabel, "", MergeOrder.ConflictHandlingModeChoices.WeWin), null, sourceLabel));
			}
			else if (LiftUtils.GetIsMarkedAsDeleted(sourceEntry))
			{
				// source deleted (old style), but target edited.
				// target wins with the least loss principle.
				// But generate a conflict report.
				eventListener.ConflictOccurred(new RemovedVsEditedElementConflict("entry", otherEntry, sourceEntry, commonEntry, new MergeSituation(otherPath, otherLabel, "", sourceLabel, "", MergeOrder.ConflictHandlingModeChoices.TheyWin), null, sourceLabel));
				writer.WriteNode(otherEntry.CreateNavigator(), false);
			}
			else
			{
				// One or both of them edited it, so merge the hard way.
				using (var reader = XmlReader.Create(new StringReader(
					mergingStrategy.MakeMergedEntry(eventListener, sourceEntry, otherEntry, commonEntry)
				)))
				{
					writer.WriteNode(reader, false);
				}
			}
			processedIds.Add(id);
		}

		private static XmlNode FindEntry(XmlNode doc, string id)
		{
			FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
			return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		}

		internal static void AddDateCreatedAttribute(XmlNode elementNode)
		{
			AddAttribute(elementNode, "dateCreated", DateTimeProvider.Current.Now.ToString(LiftUtils.LiftTimeFormatNoTimeZone));
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
				&& LiftUtils.GetModifiedDate(betaEntry) != default(DateTime))
				return true;

			return XmlUtilities.AreXmlElementsEqual(alphaEntry.OuterXml, betaEntry.OuterXml);
		}

		private static void WriteStartOfLiftElement(XmlWriter writer, XmlNode alphaDom)
		{
			XmlNode liftNode = alphaDom.SelectSingleNode("lift");

			writer.WriteStartElement(liftNode.Name);
			foreach (XmlAttribute attribute in liftNode.Attributes)
			{
				writer.WriteAttributeString(attribute.Name, attribute.Value);
			}
		}
	}
}