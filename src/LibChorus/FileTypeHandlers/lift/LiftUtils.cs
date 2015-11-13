using System;
using System.Xml;
using SIL.Xml;

namespace Chorus.FileTypeHandlers.lift
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
			var fileNameEscaped = (fileNameUnescaped == null) ? "unknown" : Uri.EscapeDataString(fileNameUnescaped);
			string url = string.Format("lift://{0}?type=entry&", fileNameEscaped);

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
				// Q: Should this be "$form=", since the caller wants to add its own "&label=" part of the query?
				url += "label=" + Uri.EscapeDataString(form) + "&";
			}
			url = url.Trim('&');
			return url;
		}

		public static string GetUrl(XmlNode child, string unescaped, string label)
		{
			var url = GetUrl(child, unescaped);
			if(string.IsNullOrEmpty(label))
				return url;

			// The call, above, to GetUrl, adds "&label=", and the provided "label" returns the same 'form' of the entry in its xpath,
			// as is used in the above call to GetUrl.
			// Do URLs support two duplicate parts of the query? Even if the contents of "&label=" are not the same, is that supported?
			if (!url.Contains("&label="))
				url = url + "&label=" + Uri.EscapeDataString(label);
			return url;
		}
	}
}