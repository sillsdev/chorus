using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Chorus.VcsDrivers.Mercurial
{

	public class HgResumeRestApiServer : IApiServer
	{
		public const string APIVERSION = "01";

		private string _baseUrl;
		private string _server;
		private string _projectId;
		private string _urlExecuted;

		public HgResumeRestApiServer(string url)
		{
			ParseComponentsInUrl(url);
			_urlExecuted = "";
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
				if (_baseUrl.EndsWith("projects"))
				{
					// remove "projects" from URL if necessary
					_baseUrl = _baseUrl.Substring(0, _baseUrl.IndexOf("/projects"));
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


		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, int secondsBeforeTimeout)
		{
			return Execute(method, parameters, new byte[0], secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, IDictionary<string, string> parameters, byte[] contentToSend, int secondsBeforeTimeout)
		{
			string queryString = BuildQueryString(parameters);
			_urlExecuted = _baseUrl + String.Format("/api/v{0}/", APIVERSION) + method + "?" + queryString;
			var req = WebRequest.Create(_urlExecuted) as HttpWebRequest;
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


			HttpWebResponse res;
			HgResumeApiResponse apiResponse;
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			try
			{
				using (res = (HttpWebResponse)req.GetResponse())
				{
					apiResponse = HandleResponse(res);
				}

			}
			catch(WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					using (res = (HttpWebResponse)e.Response)
					{
						apiResponse = HandleResponse(res);
					}
				}
				else if (e.Status == WebExceptionStatus.Timeout)
				{
					apiResponse = null;
				}
				else
				{
					throw; // throw for other types of network errors (see WebExceptionStatus for the full list of errors)
				}
			}
			finally
			{
				stopwatch.Stop();
			}
			if (apiResponse != null)
			{
				apiResponse.ResponseTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
			}
			return apiResponse;
		}

		private HgResumeApiResponse HandleResponse(HttpWebResponse res)
		{
			var apiResponse = new HgResumeApiResponse();
			for (int i = 0; i < res.Headers.Count; i++)
			{
				apiResponse.Headers[res.Headers.Keys[i]] = res.Headers[i];
			}
			apiResponse.StatusCode = res.StatusCode;

			var responseStream = res.GetResponseStream();
			if (responseStream != null && apiResponse.Headers.ContainsKey("Content-Length"))
			{
				apiResponse.Content = ReadStream(responseStream, Convert.ToInt32(apiResponse.Headers["Content-Length"]));
			}
			else
			{
				apiResponse.Content = new byte[0];
			}

			return apiResponse;
		}

		private static byte[] ReadStream(Stream stream, int length)
		{
			var buffer = new byte[length];
			int offset = 0;
			int bytesRead;
			do
			{
				bytesRead = stream.Read(buffer, offset, length - offset);
				offset += bytesRead;
			} while (bytesRead > 0 && offset < length);
			return buffer;
		}

		public string Identifier
		{
			get { return _server; }
		}

		public string ProjectId
		{
			get { return _projectId; }
		}

		public string Url
		{
			get { return _urlExecuted; }
		}

		private static string BuildQueryString(IDictionary<string, string> urlParameters)
		{
			string queryString = "";
			foreach (KeyValuePair<string, string> param in urlParameters)
			{
				queryString += string.Format("{0}={1}&", param.Key, System.Web.HttpUtility.UrlEncode(param.Value));
			}
			return queryString.TrimEnd('&');
		}
	}
}