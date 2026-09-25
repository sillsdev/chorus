using System;
using System.IO;
using System.Linq;
using Chorus.Model;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.IntegrationTests
{
	/// <summary>
	/// Send/receive through the real resumable transport against the hgresume container.
	/// </summary>
	[TestFixture]
	public class HgResumeSendReceiveTests
	{
		private TemporaryFolder _folder;
		private StringBuilderProgress _progress;

		[SetUp]
		public void SetUp()
		{
			_folder = new TemporaryFolder("HgResumeSendReceiveTests");
			// the transport reports progress through ProgressIndicator, which is null by default
			_progress = new StringBuilderProgress { ShowVerbose = true, ProgressIndicator = new NullProgressIndicator() };
			new ServerSettingsModel { Username = "test", Password = "test", RememberPassword = false }.SaveUserSettings();
		}

		[TearDown]
		public void TearDown()
		{
			if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
				TestContext.WriteLine("--- Chorus progress ---\n" + _progress.Text);
			_folder.Dispose();
		}

		[Test]
		public void Push_NewRepo_ServerHasEveryRevision()
		{
			var url = HgResumeServer.CreateRepo(out var code);
			var local = CreateLocalRepo("local", commits: 3);

			local.Push(Address(url), url);

			Assert.That(HgResumeServer.GetRevisions(code), Is.EquivalentTo(LocalRevisions(local)));
		}

		[Test]
		public void Push_BundleLargerThanOneChunk_ServerHasEveryRevision()
		{
			var url = HgResumeServer.CreateRepo(out var code);
			var local = CreateLocalRepo("local", commits: 1);
			// random bytes don't compress, so the bundle stays bigger than the transport's first chunks
			var bytes = new byte[3 * 1024 * 1024];
			new Random(1).NextBytes(bytes);
			var bigFile = Path.Combine(local.PathToRepo, "big.bin");
			File.WriteAllBytes(bigFile, bytes);
			local.AddAndCheckinFile(bigFile);

			local.Push(Address(url), url);

			Assert.That(HgResumeServer.GetRevisions(code), Is.EquivalentTo(LocalRevisions(local)));
		}

		[Test]
		public void Clone_AfterPush_HasSameRevisionsAndFiles()
		{
			var url = HgResumeServer.CreateRepo(out _);
			var local = CreateLocalRepo("local", commits: 2);
			local.Push(Address(url), url);

			var clonePath = HgRepository.Clone(Address(url), Path.Combine(_folder.Path, "clone"), _progress);
			var clone = new HgRepository(clonePath, _progress);

			Assert.That(LocalRevisions(clone), Is.EquivalentTo(LocalRevisions(local)));
			Assert.That(File.ReadAllText(Path.Combine(clonePath, "file1.txt")), Is.EqualTo("contents 1"));
		}

		[Test]
		public void Pull_ServerHasNewChange_LocalGetsIt()
		{
			var url = HgResumeServer.CreateRepo(out var code);
			var local = CreateLocalRepo("local", commits: 1);
			local.Push(Address(url), url);
			HgResumeServer.CommitOnServer(code, "fromServer.txt", "server contents");

			Assert.That(local.Pull(Address(url), url), Is.True, "Pull should report that it received changes");

			Assert.That(LocalRevisions(local), Is.EquivalentTo(HgResumeServer.GetRevisions(code)));
		}

		[Test]
		public void Pull_NothingNew_ReturnsFalse()
		{
			var url = HgResumeServer.CreateRepo(out _);
			var local = CreateLocalRepo("local", commits: 1);
			local.Push(Address(url), url);

			Assert.That(local.Pull(Address(url), url), Is.False);
		}

		private HgRepository CreateLocalRepo(string name, int commits)
		{
			var path = Path.Combine(_folder.Path, name);
			Directory.CreateDirectory(path);
			var repo = HgRepository.CreateRepositoryInExistingDir(path, _progress);
			for (var i = 1; i <= commits; i++)
			{
				var file = Path.Combine(path, $"file{i}.txt");
				File.WriteAllText(file, $"contents {i}");
				repo.AddAndCheckinFile(file);
			}
			return repo;
		}

		private static RepositoryAddress Address(string url) => RepositoryAddress.Create("hgresume", url);

		private static string[] LocalRevisions(HgRepository repo) =>
			repo.GetAllRevisions().Select(r => r.Number.Hash).ToArray();
	}
}
