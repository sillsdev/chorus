using System;
using System.IO;
using System.Linq;
using Chorus.Model;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;
// ReSharper disable AssignNullToNotNullAttribute - we're sure the directory won't be null (
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
			using (var tempDir = new TemporaryFolder("HgRepoUID"))
			{
				var repo = new HgRepository(tempDir.Path, _progress);
				var iniPath = repo.GetPathToHgrc();
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
			var repo = new HgRepository("https://" + username + ":secret@www.example.com", _progress);

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

		[TestCase("LanguageDepot", "https://user:pass@hg-public.languageforge.org/ngl-flex", "https://hg-public.languageforge.org/ngl-flex")]
		[TestCase("LanguageDepot [QA] [Resumable]", "https://bob:hid@resumable-qa.languageforge.org/ngl-flex", "https://resumable-qa.languageforge.org/ngl-flex")]
		public void RemoveCredentialsFromIniIfNecessary(string name, string oldUri, string newUri)
		{
			using (var tempDir = new TemporaryFolder("HgRepoURI"))
			{
				var repo = new HgRepository(tempDir.Path, _progress);
				var iniPath = repo.GetPathToHgrc();
				Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
				var ini = repo.GetMercurialConfigForRepository();
				ini.Sections.GetOrCreate("paths").Set(name, oldUri);
				ini.Save();

				// SUT
				repo.RemoveCredentialsFromIniIfNecessary();

				ini = repo.GetMercurialConfigForRepository();
				var newPaths = ini.Sections["paths"];
				Assert.AreEqual(1, newPaths.ItemCount);
				Assert.AreEqual(newUri, newPaths.GetItem(0).Value);
			}
		}

		[TestCase("dan", "remembered", "shibboleth")]
		[TestCase("naphtali", null, null)]
		[TestCase(null, null, "shibboleth")]
		public void RemoveCredentialsFromIniIfNecessary_PreservesOtherData(string savedUser, string savedPass, string newSavedPass)
		{
			const string iniUsername = "issachar";
			const string urlEnd = "hg-private.languageforge.org/auc-flex";
			const string oldUrl = "https://" + "dinah:shibboleth@" + urlEnd;
			const string newUrl = "https://" + urlEnd;
			const string networkDirName = "Old Network Share";
			const string networkDirRepo = "//chorus-box/projects/Wao";

			Chorus.Properties.Settings.Default.LanguageForgeUser = savedUser;
			Chorus.Properties.Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(savedPass);

			using (var tempDir = new TemporaryFolder("HgRepoURI"))
			{
				var repo = new HgRepository(tempDir.Path, _progress);
				var iniPath = repo.GetPathToHgrc();
				Directory.CreateDirectory(Path.GetDirectoryName(iniPath));
				var ini = repo.GetMercurialConfigForRepository();
				ini.Sections.GetOrCreate("ui").Set("username", iniUsername);
				var paths = ini.Sections.GetOrCreate("paths");
				paths.Set(networkDirName, networkDirRepo);
				paths.Set("LanguageForge", oldUrl);
				ini.Save();

				// SUT
				repo.RemoveCredentialsFromIniIfNecessary();

				ini = repo.GetMercurialConfigForRepository();
				Assert.AreEqual(iniUsername, ini.Sections["ui"].GetValue("username"), "Other data should be preserved");
				var newPaths = ini.Sections["paths"];
				Assert.AreEqual(2, newPaths.ItemCount);
				Assert.AreEqual(networkDirRepo, newPaths.GetValue(networkDirName));
				var actualUrl = newPaths.GetValue(newPaths.GetKeys().First(k => !k.Equals(networkDirName)));
				Assert.AreEqual(newUrl, actualUrl);
				Assert.AreEqual("dinah", Chorus.Properties.Settings.Default.LanguageForgeUser, "username is always saved");
				Assert.AreEqual("shibboleth", ServerSettingsModel.PasswordForSession, "should have read password from file");
				if (string.IsNullOrEmpty(newSavedPass))
				{
					Assert.IsNullOrEmpty(Chorus.Properties.Settings.Default.LanguageForgePass);
				}
				else
				{
					Assert.AreEqual(newSavedPass, ServerSettingsModel.DecryptPassword(Chorus.Properties.Settings.Default.LanguageForgePass),
						"should have saved the password to user settings");
				}
			}
		}
	}
}