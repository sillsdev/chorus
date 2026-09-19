using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web;
using Chorus.Model;
using Chorus.Utilities;

namespace Chorus.VcsDrivers.Mercurial
{

	public class HgResumeRestApiServer : IApiServer
	{
		public const string ApiVersion = "03";

		// One HttpClient (and one handler) for the whole process: this is what buys us keep-alive
		// connection reuse across the many small requests a resumable push/pull makes. A per-request
		// HttpWebRequest shares ServicePoint pools on .NET Framework, but on modern .NET a shared
		// HttpClient over SocketsHttpHandler is where the reuse actually happens. Timeout is left
		// infinite here and enforced per call with a CancellationTokenSource, because each API call
		// carries its own secondsBeforeTimeout.
		private static readonly HttpClient Client = CreateClient();

		private static HttpClient CreateClient()
		{
			var handler = new HttpClientHandler();
			return new HttpClient(handler, disposeHandler: true)
			{
				Timeout = Timeout.InfiniteTimeSpan
			};
		}

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

		// TODO (Hasso) 2021.01: remove UserName and Password from this API
		[Obsolete] public string UserName => null;
		[Obsolete] public string Password => null;

		public static string FormatUrl(Uri uri, string method, HgResumeApiParameters parameters)
		{
			string queryString = parameters.BuildQueryString();
			return $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)}/api/v{ApiVersion}/{method}{queryString}";
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters parameters, byte[] contentToSend, int secondsBeforeTimeout)
		{
			using var activity = LibChorusActivitySource.Value.StartActivity();
			activity?.SetTag("app.hgresume.method", method);
			activity?.SetTag("app.hgresume.bytes-sent", contentToSend.Length);
			Url = FormatUrl(_url, method, parameters);

			if (string.IsNullOrEmpty(Properties.Settings.Default.LanguageForgeUser) ||
				string.IsNullOrEmpty(ServerSettingsModel.PasswordForSession))
			{
				throw new HgResumeException("Missing username or password");
			}

			using var req = new HttpRequestMessage(
				contentToSend.Length == 0 ? HttpMethod.Get : HttpMethod.Post, Url);
			// Keep the exact legacy value ("HgResume v03"); the space makes it an invalid product token,
			// so add it unvalidated rather than through UserAgent.ParseAdd.
			req.Headers.TryAddWithoutValidation("User-Agent", $"HgResume v{ApiVersion}");
			// Send credentials pre-emptively so we never pay for a 401 challenge round trip. The
			// resumable server accepts Basic auth on the first request, so PreAuthenticate's
			// challenge-then-cache dance (what the old HttpWebRequest code relied on) is pure overhead here.
			var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(
				$"{Properties.Settings.Default.LanguageForgeUser}:{ServerSettingsModel.PasswordForSession}"));
			req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

			if (contentToSend.Length > 0)
			{
				// ByteArrayContent sets Content-Length for us; the server needs it (it reads a chunked
				// body as empty). Skip the 100-continue handshake: it costs a round trip before every
				// chunk body, which measured as ~17% of the time a large push spends in pushBundleChunk.
				req.Headers.ExpectContinue = false;
				var content = new ByteArrayContent(contentToSend);
				content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // i'm not sure this is really what we want.  The other possibility is "application/x-www-form-urlencoded"
				req.Content = content;
			}

			HgResumeApiResponse apiResponse;
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			try
			{
				// timeout is per-call; the token cancels the whole send-and-read (ResponseContentRead
				// buffers the body inside SendAsync, so the deadline covers the download too).
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(secondsBeforeTimeout));
				try
				{
					using var res = Client.SendAsync(req, HttpCompletionOption.ResponseContentRead, cts.Token)
						.GetAwaiter().GetResult();
					// HttpClient does not throw on a non-2xx status, so RESET/FAIL/etc. responses land
					// here just like the old code's ProtocolError branch did.
					apiResponse = HandleResponse(res);
				}
				catch (OperationCanceledException)
				{
					// treat a client-side timeout the way the old code treated WebExceptionStatus.Timeout
					apiResponse = null;
				}
			}
			finally
			{
				stopwatch.Stop();
			}
			if (apiResponse != null)
			{
				apiResponse.ResponseTimeInMilliseconds = stopwatch.ElapsedMilliseconds;
				activity?.SetTag("app.hgresume.http-status", (int) apiResponse.HttpStatus);
				activity?.SetTag("app.hgresume.bytes-received", apiResponse.Content?.Length ?? 0);
			}
			return apiResponse;
		}

		private static HgResumeApiResponse HandleResponse(HttpResponseMessage res)
		{
			// HgResumeApiResponseHeaders only looks at the x-hgr-* headers, so copy just those into the
			// WebHeaderCollection it expects. Filtering to that prefix also sidesteps WebHeaderCollection
			// throwing on restricted response headers.
			var headers = new WebHeaderCollection();
			foreach (var header in res.Headers)
			{
				if (header.Key.StartsWith(HgResumeApiResponseHeaders.headerPrefix, StringComparison.OrdinalIgnoreCase))
				{
					headers.Set(header.Key, string.Join(",", header.Value));
				}
			}
			var apiResponse = new HgResumeApiResponse
			{
				ResumableResponse = new HgResumeApiResponseHeaders(headers),
				HttpStatus = res.StatusCode,
				Content = res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
			};
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
					return HttpUtility.ParseQueryString(_url.Query).Get("repoId");
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
