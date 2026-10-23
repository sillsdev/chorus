using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Chorus;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace LibChorus.IntegrationTests
{
	/// <summary>
	/// Runs one hgresume server container for every test in this assembly. hgresume does no
	/// authentication of its own (the platform in front of it does), so any non-empty credentials work.
	/// Repos are created and inspected with `hg` inside the container, because this image predates the
	/// server's /api/manage endpoints.
	/// Set HGRESUME_URL and HGRESUME_REPO_ROOT to use a server that is already running on this machine
	/// instead (e.g. one started from the hgresume source), with repos created under that root by the
	/// `hg` that Chorus itself uses.
	/// </summary>
	[SetUpFixture]
	public class HgResumeServer
	{
		private const ushort ContainerPort = 80;
		private const string DefaultImage = "ghcr.io/sillsdev/hgresume:v2026-08-24";

		private static IContainer _container;
		private static string _repoRoot = "/var/vcs/public";

		/// <summary>Base URL of the server, e.g. http://127.0.0.1:49153</summary>
		public static string BaseUrl { get; private set; }

		[OneTimeSetUp]
		public async Task StartServer()
		{
			var externalUrl = Environment.GetEnvironmentVariable("HGRESUME_URL");
			var externalRepoRoot = Environment.GetEnvironmentVariable("HGRESUME_REPO_ROOT");
			if (!string.IsNullOrWhiteSpace(externalUrl) && !string.IsNullOrWhiteSpace(externalRepoRoot))
			{
				BaseUrl = externalUrl.TrimEnd('/');
				_repoRoot = externalRepoRoot;
				return;
			}
			var image = Environment.GetEnvironmentVariable("HGRESUME_IMAGE");
			_container = new ContainerBuilder(string.IsNullOrWhiteSpace(image) ? DefaultImage : image)
				.WithPortBinding(ContainerPort, assignRandomHostPort: true)
				.WithWaitStrategy(Wait.ForUnixContainer()
					.UntilHttpRequestIsSucceeded(r => r.ForPort(ContainerPort).ForPath("/api/v03/isAvailable")))
				.Build();
			try
			{
				await _container.StartAsync();
			}
			catch (Exception e) when (Environment.GetEnvironmentVariable("CI") == null)
			{
				// CI must fail loudly; a developer without Docker just doesn't get these tests.
				Assert.Ignore($"Could not start the hgresume container (is Docker running?): {e.Message}");
			}
			BaseUrl = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(ContainerPort)}";
		}

		[OneTimeTearDown]
		public async Task StopServer()
		{
			if (_container != null)
				await _container.DisposeAsync();
		}

		/// <summary>
		/// Creates an empty repo on the server and returns its URL. The code contains "resumable" because
		/// that's how Chorus decides to use the resumable transport.
		/// </summary>
		public static string CreateRepo(out string code)
		{
			code = "chorus-resumable-" + Guid.NewGuid().ToString("N").Substring(0, 8);
			Exec($"hg init {_repoRoot}/{code}");
			return $"{BaseUrl}/{code}";
		}

		/// <summary>Commits a file directly in the server's repo, as another user's push would.</summary>
		public static void CommitOnServer(string code, string fileName, string contents)
		{
			Exec($"cd {_repoRoot}/{code} && printf '%s' '{contents}' > {fileName} && " +
				$"hg add {fileName} && hg commit -u server -m 'server edit {fileName}'");
		}

		/// <summary>Short hashes of every changeset in the server's repo, newest first.</summary>
		public static string[] GetRevisions(string code)
		{
			return Exec($"hg -R {_repoRoot}/{code} log --template '{{node|short}}\\n'")
				.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string Exec(string command)
		{
			if (_container == null)
			{
				return ExecLocally(command);
			}
			var result = _container.ExecAsync(new[] { "sh", "-c", command }).GetAwaiter().GetResult();
			if (result.ExitCode != 0)
			{
				throw new Exception($"`{command}` failed ({result.ExitCode}):\n{result.Stdout}\n{result.Stderr}");
			}
			return result.Stdout;
		}

		/// <summary>Runs the command in a local shell, with Chorus's own `hg` first on the PATH.</summary>
		private static string ExecLocally(string command)
		{
			var startInfo = new ProcessStartInfo("sh", $"-c \"{command.Replace("\"", "\\\"")}\"")
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};
			var hgFolder = MercurialLocation.PathToMercurialFolder;
			if (!string.IsNullOrEmpty(hgFolder))
			{
				startInfo.Environment["PATH"] = hgFolder + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
			}
			using (var process = Process.Start(startInfo))
			{
				var stdout = process.StandardOutput.ReadToEnd();
				var stderr = process.StandardError.ReadToEnd();
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new Exception($"`{command}` failed ({process.ExitCode}):\n{stdout}\n{stderr}");
				}
				return stdout;
			}
		}
	}
}
