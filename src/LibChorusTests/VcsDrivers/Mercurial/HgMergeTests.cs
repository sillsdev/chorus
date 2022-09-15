using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Chorus.VcsDrivers.Mercurial;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgMergeTests
	{
		[Test]
		public void HgMerge_LaunchesChorusMerge()
		{
			using (var tempFolder = new TemporaryFolder("ChorusRepos"))
			{
				Console.WriteLine("------------------------------------------------");
				Console.WriteLine(Environment.Version);
				Console.WriteLine("------------------------------------------------");

				var baseDir = PathHelper.NormalizePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
				baseDir = PathHelper.StripFilePrefix(baseDir);
				FastZip zipFile = new FastZip();

				var localRepoPath = Path.Combine(tempFolder.Path, "localRepo");
				string zipPath = Path.Combine(baseDir, Path.Combine("VcsDrivers", Path.Combine("TestData", "simplerepo.zip")));
				zipFile.ExtractZip(zipPath, localRepoPath, null);

				var remoteRepoPath = Path.Combine(tempFolder.Path, "remoteRepo");
				string remoteZipPath = Path.Combine(baseDir, Path.Combine("VcsDrivers", Path.Combine("TestData", "simplerepo_remotechange.zip")));
				zipFile.ExtractZip(remoteZipPath, remoteRepoPath, null);

				var hgRepo = new HgRepository(localRepoPath, new ConsoleProgress { ShowVerbose = true });
				hgRepo.CheckAndUpdateHgrc();

				var textFilePath = Path.Combine(localRepoPath, "file.txt");
				File.WriteAllText(textFilePath, @"local changes");
				hgRepo.AddAndCheckinFile(textFilePath);

				hgRepo.PullFromTarget("remote", remoteRepoPath);
				var heads = hgRepo.GetHeads();
				var exception = Assert.Throws<ApplicationException>(() => hgRepo.Merge("unused", heads.First().Number.LocalRevisionNumber));

				Assert.IsNotNull(exception);
				Assert.That(exception.Message.Contains("ChorusMerge Error:"));
			}
		}
	}
}