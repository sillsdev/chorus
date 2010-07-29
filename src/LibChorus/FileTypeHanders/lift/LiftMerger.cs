using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.FileTypeHanders.xml;
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
		private readonly string _alphaLift;
		private readonly string _betaLift;
		private readonly string _ancestorLift;
		// only one index for now...
		private readonly Dictionary<string, XmlNode> _betaIdToNodeIndex;
		private readonly Dictionary<string,bool> _processedIds;
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
			_processedIds = new Dictionary<string,bool>();
			_alphaLift = File.ReadAllText(alphaLiftPath);
			_betaLift =  File.ReadAllText(betaLiftPath);
			_ancestorLift = File.ReadAllText(ancestorLiftPath);
			_betaIdToNodeIndex = new Dictionary<string, XmlNode>();
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
			_processedIds = new Dictionary<string,bool>();
			_alphaLift = alphaLiftContents;
			_betaLift = betaLiftContents;
			_ancestorLift = ancestorLiftContents;
			_betaIdToNodeIndex = new Dictionary<string, XmlNode>();
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


			Encoding utf8NoBom = new UTF8Encoding(false);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = utf8NoBom;//the lack of a bom is probably no big deal either way

			//this, rather than a string builder, is needed to avoid utf-16 coming out
			using (MemoryStream memoryStream = new MemoryStream())
			{
				foreach (XmlNode b in _betaDom.SafeSelectNodes("lift/entry"))
				{
					_betaIdToNodeIndex[LiftUtils.GetId(b)] = b;
				}
				using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
				{
					WriteStartOfLiftElement(writer);
					foreach (XmlNode e in _alphaDom.SafeSelectNodes("lift/entry"))
					{
						ProcessEntry(writer, e);
					}

					//now process any remaining elements in "betas"
					foreach (XmlNode e in _betaDom.SafeSelectNodes("lift/entry"))
					{
						string id = LiftUtils.GetId(e);
						if (!_processedIds.ContainsKey(id))
						{
							ProcessEntryWeKnowDoesntNeedMerging(e, id, writer);
						}
					}
					writer.WriteEndElement();
					writer.Close();
				}

				//don't use GetBuffer()!!! it pads the results with nulls:  return Encoding.UTF8.GetString(memoryStream.ToArray());
				//this works but doubles the ram use: return Encoding.UTF8.GetString(memoryStream.ToArray());
				return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
			}
		}


//        public void Do2WayDiffOfLift()
//        {
//            _alphaDom.LoadXml(_alphaLift);
//            _betaDom.LoadXml(_ancestorLift);//nb: putting the ancestor in there is intentional
//            _ancestorDom.LoadXml(_ancestorLift);
//
//            //NB: we dont' actually have any interest in writing anything,
//            //but for now, this lets us reuse the merger code
//
//            using(MemoryStream memoryStream = new MemoryStream())
//            using (XmlWriter writer = XmlWriter.Create(memoryStream))
//            {
//                WriteStartOfLiftElement(writer);
//
//                foreach (XmlNode e in _alphaDom.SelectNodes("lift/entry"))
//                {
//                        ProcessEntry(writer, e);
//                }
//
//                //now detect any deleted elements
//                foreach (XmlNode e in _ancestorDom.SelectNodes("lift/entry"))
//                {
//                    if (!_processedIds.Contains(LiftUtils.GetId(e)))
//                    {
//                        EventListener.ChangeOccurred(new XmlDeletionChangeReport(e));
//                    }
//                }
//                writer.WriteEndElement();
//            }
//        }

		private static XmlNode FindEntry(XmlNode doc, string id)
		{
			FailureSimulator.IfTestRequestsItThrowNow("LiftMerger.FindEntryById");
			return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		}

		private void ProcessEntry(XmlWriter writer, XmlNode alphaEntry)
		{
			string id = LiftUtils.GetId(alphaEntry);
			XmlNode betaEntry;

			if(!_betaIdToNodeIndex.TryGetValue(id, out betaEntry))
			{
				//enchance: we know this at this point only in the 2-way diff mode
				EventListener.ChangeOccurred(new XmlAdditionChangeReport("hackFixThis.lift", alphaEntry));
				ProcessEntryWeKnowDoesntNeedMerging(alphaEntry, id, writer);
			}
			else if (AreTheSame(alphaEntry, betaEntry))//unchanged or both made same change
			{
				writer.WriteRaw(alphaEntry.OuterXml);
			}
			else //one or both changed
			{

				/* TODO: put this back after figuring out the exact situation it was for an adding a unit test
				 if (!LiftUtils.GetIsMarkedAsDeleted(alphaEntry))
				{
					EventListener.ChangeOccurred(new XmlChangedRecordReport("hackFixThis.lift",  FindEntryById(_ancestorDom, id), alphaEntry));
				}
				 */

				XmlNode commonEntry = FindEntry(_ancestorDom, id);

				writer.WriteRaw(_mergingStrategy.MakeMergedEntry(this.EventListener, alphaEntry, betaEntry, commonEntry));
			}
			_processedIds[id] = true;
		}


		private void ProcessEntryWeKnowDoesntNeedMerging(XmlNode entry, string id, XmlWriter writer)
		{
			if(FindEntry(_ancestorDom,id) ==null)
			{
				writer.WriteRaw(entry.OuterXml); //it's new
			}
			else
			{
				// it must have been deleted by the other guy
			}
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

			// REVIEW JohnH(RandyR): Please look this over to see which of the three overloads of
			// XmlUtilities.AreXmlElementsEqual ought to be used here.
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