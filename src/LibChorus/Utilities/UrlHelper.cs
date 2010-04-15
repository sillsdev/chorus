using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chorus.Utilities
{
	public class UrlHelper
	{
		public static string GetEscapedUrl(string url)
		{
			string xml = url;
			if (!string.IsNullOrEmpty(xml))
			{
				xml = xml.Replace("&", "&amp;");
				xml = xml.Replace("\"", "&quot;");
				xml = xml.Replace("'", "&apos;");
				xml = xml.Replace("<", "&lt;");
				xml = xml.Replace(">", "&gt;");
			}
			return xml;

		}

		public static string GetUnEscapedUrl(string attributeValue)
		{
			string url = attributeValue;
			if (!string.IsNullOrEmpty(url))
			{
				url = url.Replace("&apos;", "'");
				url = url.Replace("&quot;", "\"");
				url = url.Replace("&amp;", "&");
				url = url.Replace("&lt;", "<");
				url = url.Replace("&gt;", ">");
			}
			return url;
		}

		/// <summary>
		/// get at the value in a URL, which are listed the collection of name=value pairs after the ?
		/// </summary>
		/// <example>GetValueFromQueryStringOfRef("id", ""lift://blah.lift?id=foo") returns "foo"</example>
		public static string GetValueFromQueryStringOfRef(string url, string name, string defaultIfCannotGetIt)
		{
			try
			{
				Uri uri;
				if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
				{
					throw new ApplicationException("Could not parse the url " + url);
				}

				//Could not parse the url lift://FTeam.lift?type=entry&label=نویس&id=e824f0ae-6d36-4c52-b30b-eb845d6c120a

				var parse = System.Web.HttpUtility.ParseQueryString(uri.Query);

				var r = parse.GetValues(name);
				var label = r == null ? defaultIfCannotGetIt : r.First();
				return string.IsNullOrEmpty(label) ? defaultIfCannotGetIt : label;
			}
			catch (Exception)
			{
				return defaultIfCannotGetIt;
			}
		}

		public static string GetPathOnly(string url)
		{
// DIDN"T WORK
//			Uri uri;
//				if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
//				{
//					throw new ApplicationException("Could not parse the url " + url);
//				}
//
//			return uri.AbsolutePath;

			int locationOfQuestionMark = url.IndexOf('?');
			if(locationOfQuestionMark > 0)
			{
				return url.Substring(0, locationOfQuestionMark);
			}
			return url;
		}
	}
}
