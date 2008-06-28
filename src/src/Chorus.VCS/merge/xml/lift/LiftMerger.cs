using System;
using System.Collections.Generic;
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
	/// file, lexentry, ultimately generic.  Each level seems to have some strategies.
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


		public LiftMerger(IMergeStrategy mergeStrategy, string ourLiftPath, string theirLiftPath, string ancestorLiftPath)
		{

			_ourLift = File.ReadAllText(ourLiftPath);
			_theirLift =  File.ReadAllText(theirLiftPath);
			_ancestorLift = File.ReadAllText(ancestorLiftPath);
			_ourDom = new XmlDocument();
			_theirDom = new XmlDocument();
			_ancestorDom = new XmlDocument();

			_mergingStrategy = mergeStrategy;
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
			_ourDom.LoadXml(_ourLift);
			_theirDom.LoadXml(_theirLift);
			_ancestorDom.LoadXml(_ancestorLift);

			StringBuilder builder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(builder))
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
			return builder.ToString();
		}

		private static XmlNode FindEntry(XmlNode doc, string id)
		{
			return doc.SelectSingleNode("lift/entry[@id='"+id+"']");
		}

		private void ProcessEntry(XmlWriter writer, XmlNode ourEntry)
		{
			string id = GetId(ourEntry);
			XmlNode theirEntry = FindEntry(_theirDom, id);
			if (theirEntry == null)
			{
				ProcessEntryWeKnowDoesntNeedMerging(ourEntry, id, writer);
			}
			else if (AreTheSame(ourEntry, theirEntry))
			{
				writer.WriteRaw(ourEntry.OuterXml);
			}
			else
			{
				XmlNode commonEntry = FindEntry(_ancestorDom, id);
				writer.WriteRaw(_mergingStrategy.MakeMergedEntry(ourEntry, theirEntry, commonEntry));
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