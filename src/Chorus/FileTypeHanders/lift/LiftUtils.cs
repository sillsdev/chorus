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
			return e.Attributes["id"].Value;
		}

		public static DateTime GetModifiedDate(XmlNode entry)
		{
			XmlAttribute d = entry.Attributes["dateModified"];
			if (d == null)
				return default(DateTime); //review
			return DateTime.Parse(d.Value);
		}

		public static XmlNode FindEntry(XmlNode doc, string id)
		{
				return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
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