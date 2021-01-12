using System;
using System.Diagnostics;
using System.Net;
using Chorus.Utilities;

namespace Chorus.VcsDrivers.Mercurial
{

	public class HgResumeRestApiServer : IApiServer
	{
		public const string ApiVersion = "03";

		private readonly Uri _url;

		public HgResumeRestApiServer(string url)
		{
			_url = new Uri(url);
			Url = "";

			// http://jira.palaso.org/issues/browse/CHR-26
			// Fix to support HTTP/1.0 proxy servers (ipcop) that stand between the client an our server (and that fail with a HTTP 417 Expectation Failed error, if you don't have this fix)
			ServicePointManager.Expect100Continue = false;
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters request, int secondsBeforeTimeout)
		{
			return Execute(method, request, new byte[0], secondsBeforeTimeout);
		}

		public string UserName
		{
			get { return Uri.UnescapeDataString(_url.UserInfo.Split(':')[0]); }
		}

		public string Password
		{
			get { return Uri.UnescapeDataString(_url.UserInfo.Split(':')[1]); }
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters parameters, byte[] contentToSend, int secondsBeforeTimeout)
		{
			string queryString = parameters.BuildQueryString();
			Url = String.Format("{0}://{1}/api/v{2}/{3}?{4}", _url.Scheme, _url.Host, ApiVersion, method, queryString);
			var req = (HttpWebRequest) WebRequest.Create(Url);
			req.UserAgent = $"HgResume v{ApiVersion}";
			req.PreAuthenticate = true;
			if (!_url.UserInfo.Contains(":"))
			{
				throw new HgResumeException("Username or password were not supplied in custom location");
			}
			req.Credentials = new NetworkCredential(UserName, Password);
			req.Timeout = secondsBeforeTimeout * 1000; // timeout is in milliseconds
			if (contentToSend.Length == 0)
			{
				req.Method = WebRequestMethods.Http.Get;
			}
			else
			{
				req.Method = WebRequestMethods.Http.Post;
				req.ContentLength = contentToSend.Length;
				req.ContentType = "text/plain";  // i'm not sure this is really what we want.  The other possibility is "application/x-www-form-urlencoded"
				using (var reqStream = req.GetRequestStream())
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

		private static HgResumeApiResponse HandleResponse(HttpWebResponse res)
		{
			var apiResponse = new HgResumeApiResponse();
			apiResponse.ResumableResponse = new HgResumeApiResponseHeaders(res.Headers);
			apiResponse.HttpStatus = res.StatusCode;
			apiResponse.Content = WebResponseHelper.ReadResponseContent(res);
			return apiResponse;
		}

		public string Host
		{
			get { return _url.Host; }
		}

		public string ProjectId
		{
			get
			{
				if (_url.Query.Contains("repoId="))
				{
					return SIL.Network.HttpUtilityFromMono.ParseQueryString(_url.Query).Get("repoId");
				}
				if (_url.Segments[1].ToLower() != "projects/")
				{
					return _url.Segments[1].TrimEnd('/');
				}
				return _url.Segments[2].TrimEnd('/');
			}
		}

		public string Url { get; private set; }
	}
}