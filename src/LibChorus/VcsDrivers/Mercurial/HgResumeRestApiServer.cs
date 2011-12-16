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
		private string _server;
		private string _projectId;

		public HgResumeRestApiServer(string url)
		{
			ParseComponentsInUrl(url);
		}

		private void ParseComponentsInUrl(string url)
		{
			Match match = Regex.Match(url, @"(\w+)(://|:\\)([^/]+)(.*)");
			if (match.Success)
			{
				string protocol = match.Groups[1].Value + match.Groups[2].Value;
				_server = match.Groups[3].Value;
				_baseUrl = protocol + _server;
				string path = match.Groups[4].Value;
				_projectId = path;
				if (path.Contains("/"))
				{
					int slashIndexInPath = path.LastIndexOf("/");
					_projectId = path.Substring(slashIndexInPath+1);
					_baseUrl += path.Substring(0, slashIndexInPath);
				}
			}
			else
			{
				// fallback in case we can't parse the URL
				_baseUrl = url;
				_server = url;
				_projectId = url;
			}
		}


		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout = 10)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout = 10)
		{
			string queryString = BuildQueryString(parameters);
			string apiUrl = _baseUrl + "/api/" + method + "?" + queryString;
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

		public string Identifier
		{
			get { return _server; }
		}

		public string ProjectId
		{
			get { return _projectId; }
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