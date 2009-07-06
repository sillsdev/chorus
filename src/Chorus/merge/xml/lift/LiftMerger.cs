using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.merge.xml.generic;


namespace Chorus.merge.xml.lift
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
		private readonly string _ourLift;
		private readonly string _theirLift;
		private readonly string _ancestorLift;
		private readonly List<string> _processedIds = new List<string>();
		private readonly XmlDocument _ourDom;
		private readonly XmlDocument _theirDom;
		private readonly XmlDocument _ancestorDom;
		private IMergeStrategy _mergingStrategy;
		public IMergeEventListener EventListener = new NullMergeEventListener();


		public LiftMerger(IMergeStrategy mergeStrategy, string ourLiftPath, string theirLiftPath, string ancestorLiftPath)
		{
			_ourLiftPath = ourLiftPath;
			_ourLift = File.ReadAllText(ourLiftPath);
			_theirLift =  File.ReadAllText(theirLiftPath);
			_ancestorLift = File.ReadAllText(ancestorLiftPath);
			_ourDom = new XmlDocument();
			_theirDom = new XmlDocument();
			_ancestorDom = new XmlDocument();

			_mergingStrategy = mergeStrategy;

//            string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//                           @"chorusMergeOrder" + Path.GetFileName(_ourLiftPath) + ".txt");
//            File.WriteAllText(path, "Merging OURS\r\n" + _ourLift + "\r\n----------THEIRS\r\n" + _theirLift + "\r\n----------ANCESTOR\r\n" + _ancestorLift);
		}

		/// <summary>
		/// Used by tests, which prefer to give us raw contents rather than paths
		/// </summary>
		public LiftMerger(string ourLiftContents, string theirLiftContents, string ancestorLiftContents, IMergeStrategy mergeStrategy)
		{
			_ourLift = ourLiftContents;
			_theirLift = theirLiftContents;
			_ancestorLift = ancestorLiftContents;
			_ourDom = new XmlDocument();
			_theirDom = new XmlDocument();
			_ancestorDom = new XmlDocument();

			_mergingStrategy = mergeStrategy;
		}

		public string GetMergedLift()
		{
//            string path = Path.Combine(System.Environment.GetEnvironmentVariable("temp"),
//                                    @"chorusMergeResult" + Path.GetFileName(_ourLiftPath) + ".txt");
//
//            File.WriteAllText(path, "ENter GetMergedLift()");

			_ourDom.LoadXml(_ourLift);
			_theirDom.LoadXml(_theirLift);
			_ancestorDom.LoadXml(_ancestorLift);


			Encoding utf8NoBom = new UTF8Encoding(false);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = utf8NoBom;//the lack of a bom is probably no big deal either way

			//this, rather than a string builder, is needed to avoid utf-16 coming out
			MemoryStream memoryStream = new MemoryStream();

			using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
			{

				WriteStartOfLiftElement( writer);
				foreach (XmlNode e in _ourDom.SelectNodes("lift/entry"))
				{
					ProcessEntry(writer, e);
				}

				//now process any remaining elements in "theirs"
				foreach (XmlNode e in _theirDom.SelectNodes("lift/entry"))
				{
					string id = GetId(e);
					if (!_processedIds.Contains(id))
					{
						ProcessEntryWeKnowDoesntNeedMerging(e, id, writer);
					}
				}
				writer.WriteEndElement();

			}
			string xmlString = Encoding.UTF8.GetString(memoryStream.GetBuffer());
 //           File.WriteAllText(path, xmlString);

			return xmlString;
		}


		public void Do2WayDiffOfLift()
		{
			_ourDom.LoadXml(_ourLift);
			_theirDom.LoadXml(_ancestorLift);//nb: putting the ancestor in there is intentional
			_ancestorDom.LoadXml(_ancestorLift);

			//NB: we dont' actually have any interest in writing anything,
			//but for now, this lets us reuse the merger code

			using(MemoryStream memoryStream = new MemoryStream())
			using (XmlWriter writer = XmlWriter.Create(memoryStream))
			{
				WriteStartOfLiftElement(writer);

				foreach (XmlNode e in _ourDom.SelectNodes("lift/entry"))
				{
						ProcessEntry(writer, e);
				}

				//now detect any deleted elements
				foreach (XmlNode e in _ancestorDom.SelectNodes("lift/entry"))
				{
					if (!_processedIds.Contains(GetId(e)))
					{
						EventListener.ChangeOccurred(new DeletionChangeReport(e));
					}
				}
				writer.WriteEndElement();
			}
		}

		private static XmlNode FindEntry(XmlNode doc, string id)
		{
#if DEBUG
			//Debug.Fail("attach");

			string s = System.Environment.GetEnvironmentVariable("InduceChorusFailure");
			if(s!=null && s=="LiftMerger.FindEntry")
			{
				throw new Exception("Exception Induced By InduceChorusFailure Environment Variable");
			}
#endif
				return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		}

		private void ProcessEntry(XmlWriter writer, XmlNode ourEntry)
		{
			string id = GetId(ourEntry);
			XmlNode theirEntry = FindEntry(_theirDom, id);
			if (theirEntry == null) //it's new
			{
				//enchance: we know this at this point only in the 2-way diff mode
				EventListener.ChangeOccurred(new AdditionChangeReport(ourEntry));


				ProcessEntryWeKnowDoesntNeedMerging(ourEntry, id, writer);
			}
			else if (AreTheSame(ourEntry, theirEntry))//unchanged or both made same change
			{
				writer.WriteRaw(ourEntry.OuterXml);
			}
			else //one or both changed
			{
				if (!string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(ourEntry, "dateDeleted")))
				{
					EventListener.ChangeOccurred(new DeletionChangeReport(ourEntry));
				}

				XmlNode commonEntry = FindEntry(_ancestorDom, id);

				writer.WriteRaw(_mergingStrategy.MakeMergedEntry(this.EventListener, ourEntry, theirEntry, commonEntry));
			}
			_processedIds.Add(id);
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

		static public string LiftTimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";
		private string _ourLiftPath;


		internal static void AddDateCreatedAttribute(XmlNode elementNode)
		{
			AddAttribute(elementNode, "dateCreated", DateTime.Now.ToString(LiftTimeFormatNoTimeZone));
		}

		internal static void AddAttribute(XmlNode element, string name, string value)
		{
			XmlAttribute attr = element.OwnerDocument.CreateAttribute(name);
			attr.Value = value;
			element.Attributes.Append(attr);
		}

		private static bool AreTheSame(XmlNode ourEntry, XmlNode theirEntry)
		{
			//for now...
			if (GetModifiedDate(theirEntry) == GetModifiedDate(ourEntry)
				&& !(GetModifiedDate(theirEntry) == default(DateTime)))
				return true;

			return XmlUtilities.AreXmlElementsEqual(ourEntry.OuterXml, theirEntry.OuterXml);
		}

		private static DateTime GetModifiedDate(XmlNode entry)
		{
			XmlAttribute d = entry.Attributes["dateModified"];
			if (d == null)
				return default(DateTime); //review
			return DateTime.Parse(d.Value);
		}

		private void WriteStartOfLiftElement(XmlWriter writer)
		{
			XmlNode liftNode = _ourDom.SelectSingleNode("lift");

			writer.WriteStartElement(liftNode.Name);
			foreach (XmlAttribute attribute in liftNode.Attributes)
			{
				writer.WriteAttributeString(attribute.Name, attribute.Value);
			}
		}

		private static string GetId(XmlNode e)
		{
			return e.Attributes["id"].Value;
		}
	}
}