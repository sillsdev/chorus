using System;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.merge.xml.lift
{
	public static class LiftUtils
	{
		static public string LiftTimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";

		public static bool GetIsMarkedAsDeleted(XmlNode entry)
		{
			return !string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(entry, "dateDeleted"));
		}

		public static string GetId(XmlNode e)
		{
			return e.GetOptionalStringAttribute("id", string.Empty);
		}

		public static string GetGuid(XmlNode e)
		{
			return e.GetOptionalStringAttribute("guid", string.Empty);
		}

		public static DateTime GetModifiedDate(XmlNode entry)
		{
			XmlAttribute d = entry.Attributes["dateModified"];
			if (d == null)
				return default(DateTime); //review
			return DateTime.Parse(d.Value);
		}

		public static string GetUrl(XmlNode entryNode, string fileName)
		{
			string url = "lift://" + fileName + "/navigate?type=entry&";
			if (!string.IsNullOrEmpty(LiftUtils.GetGuid(entryNode)))
			{
				url += "guid=" + LiftUtils.GetGuid(entryNode) + "&";
			}
			if (!string.IsNullOrEmpty(LiftUtils.GetId(entryNode)))
			{
				url += "id=" + LiftUtils.GetId(entryNode) + "&";
			}
			url = url.Trim('&');
			return url;
		}

		public static XmlNode FindEntryById(XmlNode doc, string id)
		{
				return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		}
		public static XmlNode FindEntryByGuid(XmlNode doc, string guid)
		{
				return doc.SelectSingleNode("lift/entry[@guid=\""+guid+"\"]");
		}
		public static bool AreTheSame(XmlNode ourEntry, XmlNode theirEntry)
		{
			//for now...
			if (GetModifiedDate(theirEntry) == GetModifiedDate(ourEntry)
				&& !(GetModifiedDate(theirEntry) == default(DateTime))
				&& !GetIsMarkedAsDeleted(ourEntry))
				return true;

			return XmlUtilities.AreXmlElementsEqual(ourEntry.OuterXml, theirEntry.OuterXml);
		}


	}
}