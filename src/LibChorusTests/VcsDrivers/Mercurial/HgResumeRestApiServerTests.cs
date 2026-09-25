using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
		/// </summary>
		[Test]
		public void Execute_RedirectedPostToSameHost_AuthenticatesAtNewLocationAndKeepsBody()
		{
			SetSession("user", "pässwörd");
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
					Is.EqualTo("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pässwörd"))));
				Assert.That(body, Is.EqualTo("chunk"));
			}
		}

		[Test]
		public void SessionCredentials_OnlyGivenToKnownHosts()
		{
			SetSession("user", "pass");
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials();
			((HgResumeRestApiServer.SessionCredentials)credentials).AllowServer(new Uri("https://hg.example.org/proj"));

			Assert.That(credentials.GetCredential(new Uri("https://hg.example.org/api/v03/x"), "Basic")?.Password, Is.EqualTo("pass"));
			Assert.That(credentials.GetCredential(new Uri("https://HG.EXAMPLE.ORG:8443/api/v03/x"), "Basic"), Is.Not.Null);
			Assert.That(credentials.GetCredential(new Uri("https://evil.example.org/api/v03/x"), "Basic"), Is.Null);
		}

		[Test]
		public void SessionCredentials_NotGivenOnDowngradeToHttp()
		{
			SetSession("user", "pass");
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials();
			((HgResumeRestApiServer.SessionCredentials)credentials).AllowServer(new Uri("https://hg.example.org/proj"));
			((HgResumeRestApiServer.SessionCredentials)credentials).AllowServer(new Uri("http://local.example.org/proj"));

			Assert.That(credentials.GetCredential(new Uri("http://hg.example.org/api/v03/x"), "Basic"), Is.Null);
			Assert.That(credentials.GetCredential(new Uri("http://local.example.org/api/v03/x"), "Basic"), Is.Not.Null);
			Assert.That(credentials.GetCredential(new Uri("https://local.example.org/api/v03/x"), "Basic"), Is.Not.Null);
		}

		[Test]
		public void SessionCredentials_ReadsCurrentSession()
		{
			ICredentials credentials = new HgResumeRestApiServer.SessionCredentials();
			((HgResumeRestApiServer.SessionCredentials)credentials).AllowServer(new Uri("https://hg.example.org/proj"));
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
							context.Response.Close();
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
