using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Chorus.VcsDrivers.Mercurial
{
	public class HgResumeRestApiServer : IApiServer
	{
		private string _baseUrl;

		public HgResumeRestApiServer(string baseUrl)
		{
			_baseUrl = baseUrl;
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout = 10)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout = 10)
		{
			string queryString = BuildQueryString(parameters);
			string apiUrl = _baseUrl + "/" + method + "?" + queryString;
			var req = WebRequest.Create(apiUrl) as HttpWebRequest;
			req.UserAgent = "HgResume";
			req.Timeout = secondsBeforeTimeout * 1000; // timeout is in milliseconds
			if (contentToSend.Length == 0)
			{
				req.Method = "GET";
			}
			else
			{
				req.Method = "POST";
				req.ContentLength = contentToSend.Length;
				req.ContentType = "text/plain";  // i'm not sure this is really what we want.  The other possibility is "application/x-www-form-urlencoded"
				using (Stream reqStream = req.GetRequestStream())
				{
					reqStream.Write(contentToSend, 0, contentToSend.Length);
				}
			}

			var apiResponse = new HgResumeApiResponse();
			using (var res = (HttpWebResponse)req.GetResponse())
			{
				for (int i = 0; i < res.Headers.Count; i++)
				{
					apiResponse.Headers[res.Headers.Keys[i]] = res.Headers[i];
				}
				apiResponse.StatusCode = res.StatusCode;

				// customized from Paratext's GetStreaming method in their RESTClient.cs
				apiResponse.Content = new byte[res.ContentLength];
				var responseStream = res.GetResponseStream();
				int offset = 0;
				int bytesRead;
				do
				{
					bytesRead = responseStream.Read(apiResponse.Content, offset,
														Math.Min(apiResponse.Content.Length - offset, (int)res.ContentLength));
					offset += bytesRead;
				} while (bytesRead > 0 && offset < res.ContentLength);
			}
			return apiResponse;
		}

		public string GetIdentifier()
		{
			Match match = Regex.Match(_baseUrl, @"(://|:\\)([^/]+)");
			if (match.Success)
			{
				return match.Groups[2].Value;
			}
			return _baseUrl;
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