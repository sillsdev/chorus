using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	/// <summary>
	/// The exception thrown by HgRepository.Execute carries hg's stderr followed by the full hg command line
	/// (including local paths) and the hg version. The HgCommonException matchers must only look at the
	/// HTTP status hg itself reported, not at digits that happen to appear elsewhere in that text.
	/// </summary>
	[TestFixture]
	public class HgExceptionsTests
	{
		private static Exception HgFailure(string stderr, string localPath = "/tmp/SR_Tests/19-2324045f/sena-3")
		{
			return new ApplicationException(stderr + Environment.NewLine +
				"hg command was" + Environment.NewLine +
				"clone -U  \"http://hg.localhost:6579/sena-3\" \"" + localPath + "\" " + Environment.NewLine +
				"hg version is Mercurial Distributed SCM (version 6.5.1)" + Environment.NewLine);
		}

		[Test]
		public void AuthorizationFailed_WithDigitsInLocalPath_IsAuthorizationError_NotProjectLabelError()
		{
			// "19-2324045f" contains "404"
			var error = HgFailure("abort: authorization failed", "/tmp/SR_Tests/19-2324045f/sena-3");

			Assert.That(RepositoryAuthorizationException.ErrorMatches(error), Is.True);
			Assert.That(ProjectLabelErrorException.ErrorMatches(error), Is.False);
		}

		[TestCase("/tmp/SR_Tests/400/proj")]
		[TestCase("/tmp/SR_Tests/403/proj")]
		[TestCase("/tmp/SR_Tests/500/proj")]
		[TestCase("/tmp/SR_Tests/502/proj")]
		[TestCase("/tmp/SR_Tests/503/proj")]
		public void DigitsInLocalPath_DoNotMatchAnyHttpStatusError(string localPath)
		{
			var error = HgFailure("abort: error: Connection refused", localPath);

			Assert.That(RepositoryAuthorizationException.ErrorMatches(error), Is.False);
			Assert.That(FirewallProblemSuspectedException.ErrorMatches(error), Is.False);
			Assert.That(ServerErrorException.ErrorMatches(error), Is.False);
			Assert.That(UriProblemException.ErrorMatches(error), Is.False);
			Assert.That(ProjectLabelErrorException.ErrorMatches(error), Is.False);
			Assert.That(PortProblemException.ErrorMatches(error), Is.True);
		}

		[Test]
		public void HttpError404_IsProjectLabelError()
		{
			var error = HgFailure("abort: HTTP Error 404: Not Found");

			Assert.That(ProjectLabelErrorException.ErrorMatches(error), Is.True);
			Assert.That(RepositoryAuthorizationException.ErrorMatches(error), Is.False);
		}

		[Test]
		public void HttpError403_IsAuthorizationError()
		{
			Assert.That(RepositoryAuthorizationException.ErrorMatches(HgFailure("abort: HTTP Error 403: Forbidden")), Is.True);
		}

		[Test]
		public void HttpError400_IsFirewallProblem()
		{
			Assert.That(FirewallProblemSuspectedException.ErrorMatches(HgFailure("abort: HTTP Error 400: Bad Request")), Is.True);
		}

		[TestCase("abort: HTTP Error 500: Internal Server Error")]
		[TestCase("abort: HTTP Error 503: Service Unavailable")]
		public void HttpError500And503_AreServerErrors(string stderr)
		{
			Assert.That(ServerErrorException.ErrorMatches(HgFailure(stderr)), Is.True);
		}

		[Test]
		public void HttpError502_IsUriProblem()
		{
			Assert.That(UriProblemException.ErrorMatches(HgFailure("abort: HTTP Error 502: Bad Gateway")), Is.True);
		}

		[Test]
		public void HttpError4040_DoesNotMatch404()
		{
			Assert.That(ProjectLabelErrorException.ErrorMatches(HgFailure("abort: HTTP Error 4040: nonsense")), Is.False);
		}

		[Test]
		public void HttpErrorWithNonAsciiDigits_DoesNotMatchOrThrow()
		{
			// Arabic-Indic digits: \d matches them but int.Parse throws on them
			Assert.That(ProjectLabelErrorException.ErrorMatches(HgFailure("abort: HTTP Error ٤٠٤: Not Found")), Is.False);
		}

		/// <summary>
		/// End-to-end check that the text the matchers look for is what the bundled hg really prints:
		/// clone from a local server that answers every request with the given status.
		/// </summary>
		[TestCase(400, typeof(FirewallProblemSuspectedException))]
		[TestCase(403, typeof(RepositoryAuthorizationException))]
		[TestCase(404, typeof(ProjectLabelErrorException))]
		[TestCase(500, typeof(ServerErrorException))]
		[TestCase(502, typeof(UriProblemException))]
		[TestCase(503, typeof(ServerErrorException))]
		public void CloneFromSource_ServerRespondsWithHttpStatus_ThrowsMatchingException(int statusCode, Type expectedException)
		{
			using (var server = new FixedStatusHttpServer(statusCode))
			using (var folder = new TemporaryFolder("HgExceptionsTests"))
			{
				// the folder name contains every status code we classify, to prove they are not picked up from the hg command line
				var repo = new HgRepository(Path.Combine(folder.Path, "400-403-404-500-502-503"), new NullProgress());

				Assert.That(() => repo.CloneFromSource("test", server.BaseUrl + "some-project"), Throws.TypeOf(expectedException));
			}
		}

		private sealed class FixedStatusHttpServer : IDisposable
		{
			private readonly HttpListener _listener;
			private readonly Task _serving;

			public string BaseUrl { get; }

			public FixedStatusHttpServer(int statusCode)
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
						context.Response.StatusCode = statusCode;
						context.Response.Close();
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
