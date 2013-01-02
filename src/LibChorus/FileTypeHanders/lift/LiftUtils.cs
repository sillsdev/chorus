using System;
using System.Xml;
using Palaso.Xml;

namespace Chorus.FileTypeHanders.lift
{
	public static class LiftUtils
	{
		static public string LiftTimeFormatNoTimeZone = "yyyy-MM-ddTHH:mm:ssZ";

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
				url += "label=" + Uri.EscapeDataString(form) + "&";
			}
			url = url.Trim('&');
			return url;
		}

		public static string GetUrl(XmlNode child, string unescaped, string label)
		{
			var x = GetUrl(child, unescaped);
			if(string.IsNullOrEmpty(label))
				return x;
			if (!x.Contains("&label="))
				x = x + "&label=" + Uri.EscapeDataString(label);
			return x;
		}
	}
}