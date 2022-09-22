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
		private readonly FastZip _zip = new FastZip();

		[Test]
		public void HgMerge_LaunchesChorusMerge()
		{
			using (var tempFolder = new TemporaryFolder("ChorusRepos"))
			{
				// prep the two repo's
				var localRepoPath = Path.Combine(tempFolder.Path, "localRepo");
				UnzipRepo(localRepoPath, "simplerepo.zip");
				var remoteRepoPath = Path.Combine(tempFolder.Path, "remoteRepo");
				UnzipRepo(remoteRepoPath, "simplerepo_remotechange.zip");
				var localRepo = new HgRepository(localRepoPath, new ConsoleProgress { ShowVerbose = true });
				localRepo.CheckAndUpdateHgrc();

				// make a local change
				var textFilePath = Path.Combine(localRepoPath, "file.txt");
				File.WriteAllText(textFilePath, @"local changes");
				localRepo.AddAndCheckinFile(textFilePath);

				// pull remote changes
				localRepo.PullFromTarget("remote", remoteRepoPath);
				var heads = localRepo.GetHeads();

				Assert.That(() => localRepo.Merge("unused", heads.First().Number.LocalRevisionNumber),
					Throws.Exception.TypeOf<ApplicationException>().With.Property("Message").Contains("ChorusMerge Error:"));
			}
		}

		private void UnzipRepo(string desiredRepoPath, string repoZipFile)
		{
			var baseDir = PathHelper.NormalizePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			baseDir = PathHelper.StripFilePrefix(baseDir);

			var zipPath = Path.Combine(baseDir,
				Path.Combine("VcsDrivers", Path.Combine("TestData", repoZipFile)));
			_zip.ExtractZip(zipPath, desiredRepoPath, null);
		}
	}
}