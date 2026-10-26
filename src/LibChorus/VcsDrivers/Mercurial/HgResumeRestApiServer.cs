using System;
using System.Diagnostics;
using System.IO;
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

	public class HgResumeRestApiServer : IApiServer, IDisposable
	{
		public const string ApiVersion = "03";

		// One HttpClient per instance, and so per push, pull or clone (HgRepository makes a new server for
		// each). That buys keep-alive connection reuse across the many small requests one operation makes,
		// without a process-wide client that would hold connections, and their DNS answers, forever.
		// Timeout is left infinite here and enforced per call with a CancellationTokenSource, because each
		// API call carries its own secondsBeforeTimeout.
		private readonly HttpClient _client;

		private readonly Uri _url;

		public HgResumeRestApiServer(string url)
		{
			_url = new Uri(url);
			Url = "";
			_client = new HttpClient(new HttpClientHandler
			{
				// We send the Authorization header ourselves (see Execute), but HttpClient drops it when it
				// follows a redirect. These let the handler answer the 401 challenge at the new location,
				// then keep sending credentials there up front.
				Credentials = new SessionCredentials(_url),
				PreAuthenticate = true
			}, disposeHandler: true)
			{
				Timeout = Timeout.InfiniteTimeSpan
			};

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
			return Send(method, parameters, contentToSend, null, null, secondsBeforeTimeout);
		}

		public HgResumeApiResponse Execute(string method, HgResumeApiParameters parameters, Stream bodySink,
			Action<long, long> onBodyProgress, int secondsBeforeTimeout)
		{
			if (bodySink == null)
			{
				throw new ArgumentNullException(nameof(bodySink));
			}
			return Send(method, parameters, new byte[0], bodySink, onBodyProgress, secondsBeforeTimeout);
		}

		private HgResumeApiResponse Send(string method, HgResumeApiParameters parameters, byte[] contentToSend,
			Stream bodySink, Action<long, long> onBodyProgress, int secondsBeforeTimeout)
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
#if !NETFRAMEWORK
			// HttpClient asks for HTTP/1.1 unless told otherwise. Asking for 2.0 lets an https server agree
			// to it (via ALPN); anything else, including plain http, falls back to 1.1. .NET Framework's
			// handler only speaks 1.x and throws if asked for 2.0.
			req.Version = new Version(2, 0);
#endif
			// Send credentials pre-emptively so we never pay for a 401 challenge round trip. The
			// resumable server accepts Basic auth on the first request, so PreAuthenticate's
			// challenge-then-cache dance (what the old HttpWebRequest code relied on) is pure overhead here.
			// The handler's SessionCredentials only come into play after a redirect drops this header.
			req.Headers.Authorization = BasicAuthHeader(
				Properties.Settings.Default.LanguageForgeUser, ServerSettingsModel.PasswordForSession);

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
			long bytesReceived = 0;
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			try
			{
				// timeout is per-call; the token cancels the whole send-and-read. Without a sink,
				// ResponseContentRead buffers the body inside SendAsync, so the deadline covers the download
				// too. With a sink, the deadline covers the headers and is then restarted before each wait
				// for body data (see CopyBody), so it bounds a stall rather than the whole download.
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(secondsBeforeTimeout));
				try
				{
					var completion = bodySink == null
						? HttpCompletionOption.ResponseContentRead
						: HttpCompletionOption.ResponseHeadersRead;
					using var res = _client.SendAsync(req, completion, cts.Token).GetAwaiter().GetResult();
					// HttpClient does not throw on a non-2xx status, so RESET/FAIL/etc. responses land
					// here just like the old code's ProtocolError branch did.
					byte[] body;
					if (bodySink == null)
					{
						body = res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
					}
					else
					{
						// .NET Framework's response stream takes no notice of the token while it waits for
						// data, so a stalled read would never return; disposing the response is what breaks
						// it (CopyBody maps the resulting exception back to a timeout). Redundant on .NET 5+.
						using var abortOnTimeout = cts.Token.Register(res.Dispose);
						if (res.StatusCode == HttpStatusCode.OK)
						{
							bytesReceived = CopyBody(res, bodySink, onBodyProgress, cts, secondsBeforeTimeout);
							body = new byte[0];
						}
						else
						{
							try
							{
								body = res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
							}
							catch (Exception e) when (cts.IsCancellationRequested)
							{
								// the read was broken by the timeout disposing the response
								throw new OperationCanceledException(e.Message, e, cts.Token);
							}
						}
					}
					if (bodySink == null || res.StatusCode != HttpStatusCode.OK)
					{
						bytesReceived = body.Length;
					}
					apiResponse = HandleResponse(res, body);
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
				activity?.SetTag("app.hgresume.bytes-received", bytesReceived);
			}
			return apiResponse;
		}

		// Size of the reads that move a streamed body into its sink; also how often the progress callback
		// runs and the idle timeout is re-armed.
		private const int StreamBufferSize = 81920;

		/// <summary>
		/// Copies the body of <paramref name="res"/> into <paramref name="sink"/> as it arrives, re-arming
		/// <paramref name="cts"/> before each read so that the timeout bounds the wait for data rather than
		/// the whole download. Whatever was received stays in the sink however this returns.
		/// </summary>
		/// <returns>the number of bytes copied</returns>
		/// <exception cref="OperationCanceledException">no data arrived within the timeout</exception>
		/// <exception cref="HttpRequestException">the connection was lost before the body was complete</exception>
		/// <exception cref="IOException">writing to the sink failed (e.g. the disk is full); not retryable, so
		/// it is passed on as it is rather than reported as a lost connection</exception>
		private static long CopyBody(HttpResponseMessage res, Stream sink, Action<long, long> onProgress,
			CancellationTokenSource cts, int idleTimeoutInSeconds)
		{
			long expected = res.Content.Headers.ContentLength ?? -1;
			long received = 0;
			bool reading = true; // only failures reading the response are about the connection
			try
			{
				using var body = res.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
				var buffer = new byte[StreamBufferSize];
				while (true)
				{
					cts.CancelAfter(TimeSpan.FromSeconds(idleTimeoutInSeconds));
					reading = true;
					int read = body.ReadAsync(buffer, 0, buffer.Length, cts.Token).GetAwaiter().GetResult();
					reading = false;
					if (read == 0)
					{
						break;
					}
					sink.Write(buffer, 0, read);
					received += read;
					onProgress?.Invoke(received, expected);
				}
			}
			catch (Exception e) when (reading && cts.IsCancellationRequested)
			{
				// the read was broken by the timeout disposing the response (see Send)
				throw new OperationCanceledException(e.Message, e, cts.Token);
			}
			catch (Exception e) when (reading && (e is IOException || e is WebException || e is ObjectDisposedException))
			{
				throw new HttpRequestException(
					$"The connection was lost after {received} bytes of the response body: {e.Message}", e);
			}
			finally
			{
				sink.Flush();
			}
			if (expected >= 0 && received < expected)
			{
				throw new HttpRequestException(
					$"The connection was closed after {received} of the {expected} bytes of the response body");
			}
			return received;
		}

		internal static AuthenticationHeaderValue BasicAuthHeader(string user, string password)
		{
			return new AuthenticationHeaderValue("Basic",
				Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")));
		}

		/// <summary>
		/// Credentials for one server's handler. They are read from the current session settings on every
		/// challenge rather than captured, and only handed to that server's host, so a redirect to some other
		/// server can't collect the password. A redirect from https to http gets nothing either.
		/// Derives from CredentialCache only because both HttpClient handlers (and .NET Framework's
		/// HttpWebRequest underneath) refuse to use any other ICredentials after following a redirect;
		/// the cache itself stays empty and our re-implementation of ICredentials.GetCredential answers.
		/// </summary>
		internal sealed class SessionCredentials : CredentialCache, ICredentials
		{
			private readonly Uri _server;

			public SessionCredentials(Uri server)
			{
				_server = server;
			}

			NetworkCredential ICredentials.GetCredential(Uri uri, string authType)
			{
				if (!string.Equals(uri.Host, _server.Host, StringComparison.OrdinalIgnoreCase) ||
					(uri.Scheme != Uri.UriSchemeHttps && _server.Scheme == Uri.UriSchemeHttps))
				{
					return null;
				}
				var user = Properties.Settings.Default.LanguageForgeUser;
				var password = ServerSettingsModel.PasswordForSession;
				if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
				{
					return null;
				}
				return new NetworkCredential(user, password);
			}
		}

		private static HgResumeApiResponse HandleResponse(HttpResponseMessage res, byte[] body)
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
				Content = body
			};
			return apiResponse;
		}

		public void Dispose()
		{
			_client.Dispose();
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
