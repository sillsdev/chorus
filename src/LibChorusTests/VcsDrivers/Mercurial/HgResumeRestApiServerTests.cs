using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chorus.Model;
using Chorus.Properties;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	class HgResumeRestApiServerTests
	{
		private string _savedUser;

		[SetUp]
		public void SetUp()
		{
			_savedUser = Settings.Default.LanguageForgeUser;
		}

		[TearDown]
		public void TearDown()
		{
			Settings.Default.LanguageForgeUser = _savedUser;
			ServerSettingsModel.PasswordForSession = null;
		}

		[Test]
		public void Constructor_PrivateLanguageForgeUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("https://hg-private.languageforge.org/kyu-dictionary");
			Assert.That(api.Host, Is.EqualTo("hg-private.languageforge.org"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}

		[Test]
		public void Constructor_LanguageForgeUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("https://hg.languageforge.org/projects/kyu-dictionary");
			Assert.That(api.Host, Is.EqualTo("hg.languageforge.org"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}

		[Test]
		public void FormatUrl_FormatsCorrectlyWithEmptyParameters()
		{
			Assert.That(HgResumeRestApiServer.FormatUrl(
					new Uri("https://hg.languageforge.org:1234/projects/kyu-dictionary"),
					"foo",
					new HgResumeApiParameters()),
				Is.EqualTo("https://hg.languageforge.org:1234/api/v03/foo"));
		}

		[Test]
		public void FormatUrl_FormatsCorrectlyWithParameters()
		{
			//this test is overly specific as the order of the parameters is not important, however this is much simpler to write.
			Assert.That(HgResumeRestApiServer.FormatUrl(
					new Uri("https://hg.languageforge.org:1234/projects/kyu-dictionary"),
					"foo",
					new HgResumeApiParameters
					{
						StartOfWindow = 10,
						ChunkSize = 20,
						BundleSize = 30,
						Quantity = 40,
						TransId = "transId",
						BaseHashes = new[] { "baseHash1", "baseHash2" },
						RepoId = "repoId"
					}),
				Is.EqualTo(
					"https://hg.languageforge.org:1234/api/v03/foo?offset=10&chunkSize=20&bundleSize=30&quantity=40&transId=transId&baseHashes[]=baseHash1&baseHashes[]=baseHash2&repoId=repoId"));
		}

		[Test]
		public void Execute_SendsUtf8BasicAuthOnFirstRequest()
		{
			SetSession("jösé", "pässwörd");
			string authorization = null;
			using (var server = new LoopbackServer(ctx =>
			{
				authorization = ctx.Request.Headers["Authorization"];
				ctx.Response.StatusCode = 200;
			}))
			{
				var response = new HgResumeRestApiServer(server.BaseUrl + "proj")
					.Execute("isAvailable", new HgResumeApiParameters(), 10);

				Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
				Assert.That(authorization,
					Is.EqualTo("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("jösé:pässwörd"))));
			}
		}

		/// <summary>
		/// HttpClient drops our Authorization header when it follows a redirect; the handler's credentials
		/// must answer the new location's challenge. 307 keeps the method and body, as a push chunk needs.
		/// ASCII credentials because on .NET Framework the handler encodes challenge answers as Latin-1
		/// (as the old HttpWebRequest code always did), not the UTF-8 we send up front.
		/// </summary>
		[Test]
		public void Execute_RedirectedPostToSameHost_AuthenticatesAtNewLocationAndKeepsBody()
		{
			SetSession("user", "password");
			string authorization = null;
			string body = null;
			using (var target = new LoopbackServer(ctx =>
			{
				authorization = ctx.Request.Headers["Authorization"];
				if (authorization == null)
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=\"hgresume\"");
					return;
				}
				using (var reader = new StreamReader(ctx.Request.InputStream))
					body = reader.ReadToEnd();
				ctx.Response.StatusCode = 200;
			}))
			using (var redirector = new LoopbackServer(ctx =>
			{
				ctx.Response.StatusCode = 307;
				ctx.Response.RedirectLocation = target.BaseUrl.TrimEnd('/') + ctx.Request.RawUrl;
			}))
			{
				var response = new HgResumeRestApiServer(redirector.BaseUrl + "proj")
					.Execute("pushBundleChunk", new HgResumeApiParameters(), Encoding.UTF8.GetBytes("chunk"), 10);

				Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
				Assert.That(authorization,
					Is.EqualTo("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:password"))));
				Assert.That(body, Is.EqualTo("chunk"));
			}
		}

		[Test]
		public void Execute_WithSink_StreamsOkBodyToSinkAndReportsProgress()
		{
			SetSession("user", "password");
			var body = new byte[300000];
			new Random(1).NextBytes(body);
			using (var server = new LoopbackServer(ctx =>
			{
				ctx.Response.StatusCode = 200;
				ctx.Response.AddHeader("X-HgR-Status", "SUCCESS");
				ctx.Response.AddHeader("X-HgR-BundleSize", body.Length.ToString());
				ctx.Response.ContentLength64 = body.Length;
				ctx.Response.OutputStream.Write(body, 0, body.Length);
			}))
			using (var api = new HgResumeRestApiServer(server.BaseUrl + "proj"))
			{
				var sink = new MemoryStream();
				var reported = new List<long>();
				long reportedLength = -1;
				var response = api.Execute("pullBundleChunk", new HgResumeApiParameters(), sink,
					(received, length) => { reported.Add(received); reportedLength = length; }, 10);

				Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.OK));
				Assert.That(response.ResumableResponse.BundleSize, Is.EqualTo(body.Length));
				Assert.That(response.Content, Is.Empty, "a streamed body is not also buffered");
				Assert.That(sink.ToArray(), Is.EqualTo(body));
				Assert.That(reported, Is.Ordered.Ascending);
				Assert.That(reported.Last(), Is.EqualTo(body.Length));
				Assert.That(reportedLength, Is.EqualTo(body.Length));
			}
		}

		[Test]
		public void Execute_WithSink_NonOkResponse_BodyIsBufferedNotStreamed()
		{
			SetSession("user", "password");
			using (var server = new LoopbackServer(ctx =>
			{
				ctx.Response.StatusCode = 503;
				ctx.Response.AddHeader("X-HgR-Status", "NOTAVAILABLE");
				var text = Encoding.UTF8.GetBytes("down for maintenance");
				ctx.Response.OutputStream.Write(text, 0, text.Length);
			}))
			using (var api = new HgResumeRestApiServer(server.BaseUrl + "proj"))
			{
				var sink = new MemoryStream();
				var response = api.Execute("pullBundleChunk", new HgResumeApiParameters(), sink, null, 10);

				Assert.That(response.HttpStatus, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
				Assert.That(Encoding.UTF8.GetString(response.Content), Is.EqualTo("down for maintenance"));
				Assert.That(sink.Length, Is.EqualTo(0));
			}
		}

		/// <summary>
		/// Served from a raw socket rather than LoopbackServer: on .NET Framework, HttpListener (http.sys)
		/// never puts the partial body on the wire before Abort, so the client would see no bytes at all.
		/// </summary>
		[Test]
		public void Execute_WithSink_ConnectionLostMidBody_KeepsBytesReceivedAndThrows()
		{
			SetSession("user", "password");
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			try
			{
				var server = Task.Run(() =>
				{
					using (var client = listener.AcceptTcpClient())
					{
						var stream = client.GetStream();
						var reader = new StreamReader(stream, Encoding.ASCII);
						while (!string.IsNullOrEmpty(reader.ReadLine()))
						{
							// skip the request headers
						}
						var head = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 1000\r\n\r\n");
						stream.Write(head, 0, head.Length);
						stream.Write(new byte[400], 0, 400);
						Thread.Sleep(300); // let the client take those bytes before the connection goes away under it
						client.Client.LingerState = new LingerOption(true, 0); // close with a reset, as a dropped link does
						client.Client.Close();
					}
				});
				using (var api = new HgResumeRestApiServer($"http://127.0.0.1:{((IPEndPoint) listener.LocalEndpoint).Port}/proj"))
				{
					var sink = new MemoryStream();
					Assert.That(() => api.Execute("pullBundleChunk", new HgResumeApiParameters(), sink, null, 10),
						Throws.TypeOf<HttpRequestException>());
					Assert.That(sink.Length, Is.EqualTo(400));
				}
				server.Wait(TimeSpan.FromSeconds(5));
			}
			finally
			{
				listener.Stop();
			}
		}

		/// <summary>
		/// The timeout bounds each wait for data, not the whole download: a body that keeps coming, however
		/// slowly, is never cut off, but one that stops is given up on (as null, like any other timeout)
		/// with what arrived kept.
		/// </summary>
		[Test]
		public void Execute_WithSink_BodyStalls_TimesOutKeepingBytesReceived()
		{
			SetSession("user", "password");
			using (var release = new ManualResetEventSlim())
			using (var server = new LoopbackServer(ctx =>
			{
				ctx.Response.StatusCode = 200;
				ctx.Response.ContentLength64 = 1000;
				for (var i = 0; i < 4; i++)
				{
					// four pieces 0.5 s apart: more than the 1 s timeout in all, but never a 1 s silence
					ctx.Response.OutputStream.Write(new byte[100], 0, 100);
					ctx.Response.OutputStream.Flush();
					Thread.Sleep(500);
				}
				release.Wait(TimeSpan.FromSeconds(20)); // then silence
				ctx.Response.Abort(); // the body is short of its Content-Length, so it can't be closed normally
			}))
			using (var api = new HgResumeRestApiServer(server.BaseUrl + "proj"))
			{
				var sink = new MemoryStream();
				var response = api.Execute("pullBundleChunk", new HgResumeApiParameters(), sink, null, 1);
				release.Set();

				Assert.That(response, Is.Null);
				Assert.That(sink.Length, Is.EqualTo(400));
			}
		}

		[Test]
		public void SessionCredentials_OnlyGivenToTheServersHost()
		{
			SetSession("user", "pass");
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials(new Uri("https://hg.example.org/proj"));

			Assert.That(credentials.GetCredential(new Uri("https://hg.example.org/api/v03/x"), "Basic")?.Password, Is.EqualTo("pass"));
			Assert.That(credentials.GetCredential(new Uri("https://HG.EXAMPLE.ORG:8443/api/v03/x"), "Basic"), Is.Not.Null);
			Assert.That(credentials.GetCredential(new Uri("https://evil.example.org/api/v03/x"), "Basic"), Is.Null);
		}

		[Test]
		public void SessionCredentials_NotGivenOnDowngradeToHttp()
		{
			SetSession("user", "pass");
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials(new Uri("https://hg.example.org/proj"));

			Assert.That(credentials.GetCredential(new Uri("http://hg.example.org/api/v03/x"), "Basic"), Is.Null);
		}

		[Test]
		public void SessionCredentials_HttpServer_GivenOverHttpAndHttps()
		{
			SetSession("user", "pass");
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials(new Uri("http://local.example.org/proj"));

			Assert.That(credentials.GetCredential(new Uri("http://local.example.org/api/v03/x"), "Basic"), Is.Not.Null);
			Assert.That(credentials.GetCredential(new Uri("https://local.example.org/api/v03/x"), "Basic"), Is.Not.Null);
		}

		[Test]
		public void SessionCredentials_ReadsCurrentSession()
		{
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials(new Uri("https://hg.example.org/proj"));
			var uri = new Uri("https://hg.example.org/api/v03/x");

			SetSession("first", "one");
			Assert.That(credentials.GetCredential(uri, "Basic").UserName, Is.EqualTo("first"));
			SetSession("second", "two");
			Assert.That(credentials.GetCredential(uri, "Basic").UserName, Is.EqualTo("second"));
		}

		private static void SetSession(string user, string password)
		{
			Settings.Default.LanguageForgeUser = user;
			ServerSettingsModel.PasswordForSession = password;
		}

		/// <summary>An HttpListener on a free loopback port that answers every request with the given handler.</summary>
		private sealed class LoopbackServer : IDisposable
		{
			private readonly HttpListener _listener;
			private readonly Task _serving;

			public string BaseUrl { get; }

			public LoopbackServer(Action<HttpListenerContext> handle)
			{
				// another process can grab the probed port before the listener binds it, so retry a few times
				for (var attempt = 1; ; attempt++)
				{
					BaseUrl = $"http://127.0.0.1:{GetFreePort()}/";
					_listener = new HttpListener();
					_listener.Prefixes.Add(BaseUrl);
					try
					{
						_listener.Start();
						break;
					}
					catch (HttpListenerException) when (attempt < 5)
					{
						_listener.Close();
					}
				}
				_serving = Task.Run(async () =>
				{
					while (_listener.IsListening)
					{
						HttpListenerContext context;
						try
						{
							context = await _listener.GetContextAsync();
						}
						catch (Exception) // listener stopped
						{
							return;
						}
						try
						{
							handle(context);
						}
						finally
						{
							try
							{
								context.Response.Close();
							}
							catch (ObjectDisposedException) // the handler aborted the response
							{
							}
						}
					}
				});
			}

			private static int GetFreePort()
			{
				var probe = new TcpListener(IPAddress.Loopback, 0);
				probe.Start();
				var port = ((IPEndPoint)probe.LocalEndpoint).Port;
				probe.Stop();
				return port;
			}

			public void Dispose()
			{
				_listener.Stop();
				_listener.Close();
				_serving.Wait(TimeSpan.FromSeconds(5));
			}
		}
	}
}
