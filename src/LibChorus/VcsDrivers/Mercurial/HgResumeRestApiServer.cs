using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgResumeRestApiServer : IApiServer
	{
		private string _baseUrl;

		public HgResumeRestApiServer(string baseUrl)
		{
			_baseUrl = baseUrl;
		}

		public HttpWebResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout = 10)
		{
			return Execute(method, parameters, "", secondsBeforeTimeout);
		}

		public HttpWebResponse Execute(string method, IDictionary<string, string> parameters, string contentToSend, int secondsBeforeTimeout = 10)
		{
			string queryString = BuildQueryString(parameters);
			string apiUrl = _baseUrl + "/" + method + "?" + queryString;
			var req = WebRequest.Create(apiUrl) as HttpWebRequest;
			req.UserAgent = "HgResume";
			req.Timeout = secondsBeforeTimeout * 1000; // timeout is in milliseconds
			if (string.IsNullOrEmpty(contentToSend))
			{
				req.Method = "GET";
			}
			else
			{
				req.Method = "POST";
				UTF8Encoding enc = new UTF8Encoding();
				byte[] postDataBytes = enc.GetBytes(contentToSend);
				req.ContentLength = postDataBytes.Length;
				req.ContentType = "text/plain";  // i'm not sure this is really what we want.  The other possibility is "application/x-www-form-urlencoded"
				using (Stream reqStream = req.GetRequestStream())
				{
					reqStream.Write(postDataBytes, 0, postDataBytes.Length);
				}
			}

			return (HttpWebResponse)req.GetResponse();
		}

		private static string BuildQueryString(IDictionary<string, string> urlParameters)
		{
			string queryString = "";
			foreach (KeyValuePair<string, string> param in urlParameters)
			{
				queryString += string.Format("{1}={2}&", param.Key, System.Web.HttpUtility.UrlEncode(param.Value));
			}
			queryString.TrimEnd('&');
			return queryString;
		}
	}
}