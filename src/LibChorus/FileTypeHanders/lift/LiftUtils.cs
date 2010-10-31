using System;
using System.Xml;
using Chorus.merge.xml.generic;

namespace Chorus.FileTypeHanders.lift
{
	public static class LiftUtils
	{
		static public string LiftTimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";

		// Not used after Randy's overhaul.
		//public static bool GetIsMarkedAsDeleted(XmlNode entry)
		//{
		//    return !string.IsNullOrEmpty(XmlUtilities.GetOptionalAttributeString(entry, "dateDeleted"));
		//}

		public static string GetId(XmlNode e)
		{
			return e.GetOptionalStringAttribute("id", string.Empty);
		}

		public static string GetGuid(XmlNode e)
		{
			return e.GetOptionalStringAttribute("guid", string.Empty);
		}
		public static string GetFormForEntry(XmlNode e)
		{
			return e.SelectTextPortion("lexical-unit/form/text");
		}

		public static DateTime GetModifiedDate(XmlNode entry)
		{
			XmlAttribute d = entry.Attributes["dateModified"];
			if (d == null)
				return default(DateTime); //review
			return DateTime.Parse(d.Value);
		}

		public static string GetUrl(XmlNode entryNode, string fileNameUnescaped)
		{
			fileNameUnescaped = fileNameUnescaped==null?"unknown": Uri.EscapeDataString(fileNameUnescaped);
			string url = string.Format("lift://{0}?type=entry&", fileNameUnescaped);

			var guid = GetGuid(entryNode);
			if (!string.IsNullOrEmpty(guid))
			{
				url += "id=" + guid + "&";
			}
			else
			{
				var id = GetId(entryNode);
				if (!string.IsNullOrEmpty(id))
				{
					url += "id=" + id + "&";
				}
			}


			var form = GetFormForEntry(entryNode);
			if (!string.IsNullOrEmpty(form))
			{
				url += "label=" + form + "&";
			}
			url = url.Trim('&');
			return url;
		}

		// Not used after Randy's overhaul.
		//public static XmlNode FindEntryById(XmlNode doc, string id)
		//{
		//        return doc.SelectSingleNode("lift/entry[@id=\""+id+"\"]");
		//}
		//public static XmlNode FindEntryByGuid(XmlNode doc, string guid)
		//{
		//        return doc.SelectSingleNode("lift/entry[@guid=\""+guid+"\"]");
		//}
		//public static bool AreTheSame(XmlNode ourEntry, XmlNode theirEntry)
		//{
		//    //for now...
		//    if (GetModifiedDate(theirEntry) == GetModifiedDate(ourEntry)
		//        && !(GetModifiedDate(theirEntry) == default(DateTime))
		//        && !GetIsMarkedAsDeleted(ourEntry))
		//        return true;

		//    return XmlUtilities.AreXmlElementsEqual(ourEntry.OuterXml, theirEntry.OuterXml);
		//}


		public static string GetUrl(XmlNode child, string unescaped, string label)
		{
			var x = GetUrl(child, unescaped);
			if(string.IsNullOrEmpty(label))
				return x;
			return x + "&label=" + Uri.EscapeDataString(label);
		}
	}
}