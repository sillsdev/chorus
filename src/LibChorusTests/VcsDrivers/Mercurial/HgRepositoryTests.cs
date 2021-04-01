using System;
using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;
// ReSharper disable LocalizableElement

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class HgRepositoryTests
	{
		private readonly IProgress _progress = new NullProgress();

		[Test]
		public void RepositoryURIForLog_ContainsNoPassword()
		{
			const string uri = "https://hg-public.languageforge.org/project/tpi-flex";
			var address = new HttpRepositoryPath("whatever", uri, false);

			// SUT
			var result = new HgRepository("ssh://nowhere", _progress).RepositoryURIForLog(address);
			Assert.AreEqual(uri, result);
		}

		[Test]
		public void RepositoryURIForLog_ChorusHubWithVariable_DoesNotThrow()
		{
			const string uri = "http://chorushub@172.20.3.170:5913/" + RepositoryAddress.ProjectNameVariable;
			var address = new HttpRepositoryPath("whatever", uri, false);

			// SUT
			var result = new HgRepository("ssh://nowhere", _progress).RepositoryURIForLog(address);
			Assert.AreEqual(uri, result);
		}

		[Test]
		public void GetUserIdInUse_Local()
		{
			const string username = "UserInIni";
			using (var tempDir = new TemporaryFolder("HgRepoURI"))
			{
				var repo = new HgRepository(tempDir.Path, _progress);
				var iniPath = repo.GetPathToHgrc();
				// ReSharper disable once AssignNullToNotNullAttribute
				Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
				File.WriteAllText(iniPath, $"[ui]{Environment.NewLine}username = {username}");

				// SUT
				Assert.AreEqual(username, repo.GetUserIdInUse());
			}
		}

		[Test]
		public void GetUserIdInUse_Internet()
		{
			const string username = "UserInUri";
			var repo = new HgRepository("https://" + username + ":secret@example.com", _progress);

			// SUT
			Assert.AreEqual(username, repo.GetUserIdInUse());
		}

		[Test]
		public void GetUserIdInUse_Fallback()
		{
			var username = Environment.UserName.Replace(" ", "");
			using (var tempDir = new TemporaryFolder("HgRepoURI"))
			{
				// SUT
				Assert.AreEqual(username, new HgRepository(tempDir.Path, _progress).GetUserIdInUse());
			}
			// SUT
			Assert.AreEqual(username, new HgRepository("http://example.com", _progress).GetUserIdInUse());
		}
	}
}