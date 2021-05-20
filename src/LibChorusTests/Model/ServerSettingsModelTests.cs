using System.IO;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;
using Chorus.Model;
using Chorus.Properties;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using Newtonsoft.Json;

namespace LibChorus.Tests.Model
{
	[TestFixture]
	public class ServerSettingsModelTests
	{
		[Test]
		public void InitFromUri_FullTypicalLangForge_AccountNameCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.AreEqual("joe", m.Username);
		}

		[Test]
		public void InitFromUri_FullTypicalLangForge_PasswordCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.AreEqual("pass", m.Password);
		}

		[Test]
		public void InitFromUri_FullTypicalLangForge_ProjectIdCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.AreEqual("tpi", m.ProjectId);
		}

		[Test]
		public void InitFromUri_FullTypicalLangForge_ExistingProjectIdDisplayedOnLoad(
			[Values(true, false)] bool hasProjId)
		{
			const string proj = "tpi";
			var m = new ServerSettingsModel();
			m.InitFromUri($"https://joe:pass@hg-public.languageforge.org/{(hasProjId ? proj : string.Empty)}");
			Assert.AreEqual(hasProjId, m.HasLoggedIn);
		}

		[Test]
		public void InitFromUri_FullTypicalLangForge_DomainAndBandwidthCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.AreEqual("hg-public.languageforge.org", m.Host);
			Assert.AreEqual(ServerSettingsModel.BandwidthEnum.High, m.Bandwidth.Value);
		}

		[Test]
		public void InitFromUri_ResumableLangForge_DomainAndBandwidthCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://resumable.languageforge.org/tpi");
			Assert.AreEqual("resumable.languageforge.org", m.Host);
			Assert.AreEqual(ServerSettingsModel.BandwidthEnum.Low, m.Bandwidth.Value);
		}

		[Test]
		public void InitFromUri_FullTypicalLangForge_CustomUrlFalse()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.IsFalse(m.IsCustomUrl);
		}

		[TestCase("resumable", true)]
		[TestCase("hg-public", false)]
		[TestCase("hg-private", false)]
		public void InitFromUri_LanguageDepot_ConvertedToLanguageForge(string subdomain, bool isResumable)
		{
			var expectedNewHost = $"{subdomain}.languageforge.org";
			var expectedBandwidth = isResumable
				? ServerSettingsModel.BandwidthEnum.Low
				: ServerSettingsModel.BandwidthEnum.High;
			var m = new ServerSettingsModel();
			// SUT
			// ReSharper disable once StringLiteralTypo - the old server used to be called Language Depot
			m.InitFromUri($"http://joe:cool@{subdomain}.languagedepot.org/mcx");

			if (subdomain.Equals("hg-private"))
			{
				Assert.True(m.IsCustomUrl, "not really custom, but no longer stores nicely in our memory model");
			}
			else
			{
				Assert.False(m.IsCustomUrl);
			}
			Assert.AreEqual(expectedNewHost, m.Host);
			Assert.AreEqual("joe", m.Username);
			Assert.AreEqual("cool", m.Password);
			Assert.AreEqual(expectedBandwidth, m.Bandwidth.Value);
			Assert.AreEqual("mcx", m.ProjectId);
			Assert.AreEqual($"https://{expectedNewHost}/mcx", m.URL);
		}

		[Test]
		public void InitFromUri_HasFolderDesignator_IdIsCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi?localFolder=foo");
			Assert.AreEqual("tpi", m.ProjectId);
		}

		[Test]
		public void InitFromUri_UnknownHttpGiven_InitializesEverything()
		{
			const string user = "Sally";
			const string pass = "Guggenheim";
			const string proj = "ngl";
			const string host = "chorus.elsewhere.net";
			const string hostAndProj = host + "/" + proj;
			// URL is intentionally insecure: just in case some self-hosting user hasn't implemented security
			const string url = "http://" + user + ":" + pass + "@" + hostAndProj;
			const string urlSansCredentials = "http://" + hostAndProj;
			var m = new ServerSettingsModel();
			// SUT
			m.InitFromUri(url);

			Assert.IsTrue(m.IsCustomUrl);
			Assert.AreEqual(host, m.Host);
			Assert.AreEqual(user, m.Username);
			Assert.AreEqual(pass, m.Password);
			Assert.AreEqual(ServerSettingsModel.BandwidthEnum.High, m.Bandwidth.Value, "Custom URL's aren't known to be resumable");
			Assert.AreEqual(proj, m.ProjectId);
			Assert.AreEqual(urlSansCredentials, m.URL);
		}

		[Test]
		public void InitFromUri_UnknownHttpGiven_CustomUrlIsTrue()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://somewhereelse.net/xyz");
			Assert.IsTrue(m.IsCustomUrl);
		}

		[Test]
		public void InitFromUri_LANPathGiven_CustomUrlIsTrue()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("\\mybox\tpi");
			Assert.IsTrue(m.IsCustomUrl);
		}

		[Test]
		public void InitFromUri_NoUsernameOrPass_UsesSettings()
		{
			const string user = "john";
			const string pass = "settings";
			Settings.Default.LanguageForgeUser = user;
			Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(pass);
			var m = new ServerSettingsModel();
			m.InitFromUri("https://hg.languageforge.org/tpi");
			Assert.AreEqual(user, m.Username);
			Assert.AreEqual(pass, m.Password);
			Assert.AreEqual("tpi", m.ProjectId);
		}

		[Test]
		public void InitFromUri_UsernameAndPass_OverwritesSettings()
		{
			Settings.Default.LanguageForgeUser = "from";
			Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword("settings");
			var m = new ServerSettingsModel();
			m.InitFromUri("https://jan:pass@hg-public.languageforge.org/tps");
			Assert.AreEqual("jan", m.Username);
			Assert.AreEqual("pass", m.Password);
			Assert.AreEqual("tps", m.ProjectId);
		}

		[Test]
		public void PopulateAvailableProjects()
		{
			const string id1 = "nko";
			const string id2 = "atinlay-dictionary";
			const string id3 = "wra-ramo-dict";
			const string json = @"[
			{
				 ""identifier"":""" + id1 + @""",
				 ""name"":""Nkonya 2011"",
				 ""repository"":""http:\/\/public.languageforge.org"",
				 ""role"":""unknown""
			},
			{
				 ""identifier"":""" + id2 + @""",
				 ""name"":""Atinlay Dictionary"",
				 ""repository"":""http:\/\/public.languageforge.org"",
				 ""role"":""manager""
			},
			{
				 ""identifier"":""" + id3 + @""",
				 ""name"":""Ramo Dictionary"",
				 ""repository"":""http:\/\/public.languageforge.org"",
				 ""role"":""unknown""
			}]";

			var m = new ServerSettingsModel();

			// SUT
			m.PopulateAvailableProjects(json);

			Assert.AreEqual(3, m.AvailableProjects.Count, "number of available projects");
			CollectionAssert.Contains(m.AvailableProjects, id1);
			CollectionAssert.Contains(m.AvailableProjects, id2);
			CollectionAssert.Contains(m.AvailableProjects, id3);
		}

		[Test]
		public void PopulateAvailableProjects_ToleratesMissingProperties()
		{
			const string id = "nko";
			const string json = @"[{
				 ""identifier"":""" + id + @"""
			}]";

			var m = new ServerSettingsModel();

			// SUT
			m.PopulateAvailableProjects(json);

			Assert.AreEqual(1, m.AvailableProjects.Count, "number of available projects");
			CollectionAssert.Contains(m.AvailableProjects, id);
		}

		[Test]
		public void PopulateAvailableProjects_ToleratesExtraProperties()
		{
			const string id = "nko";
			const string json = @"[
			{
				 ""identifier"":""" + id + @""",
				 ""name"":""Nkonya 2011"",
				 ""repository"":""http:\/\/public.languageforge.org"",
				 ""role"":""unknown"",
				 ""owner"":""nko-admin""
			}]";

			var m = new ServerSettingsModel();

			// SUT
			m.PopulateAvailableProjects(json);

			Assert.AreEqual(1, m.AvailableProjects.Count, "number of available projects");
			CollectionAssert.Contains(m.AvailableProjects, id);
		}

		[Test]
		public void PopulateAvailableProjects_NoProjects([Values("", "[]")] string json)
		{
			var m = new ServerSettingsModel();

			// SUT
			m.PopulateAvailableProjects(json);

			CollectionAssert.IsEmpty(m.AvailableProjects);
		}

		[Test]
		public void PopulateAvailableProjects_ThrowsBadJson()
		{
			const string badJson = @"[{""identifier"":(}]";
			var m = new ServerSettingsModel();

			// SUT
			Assert.Throws<JsonReaderException>(() => m.PopulateAvailableProjects(badJson));
		}

		[TestCase(null, "rememberMe", true)]
		[TestCase(null, null, false)]
		[TestCase(null, "", false)]
		[TestCase("temporary", "", false)]
		public void RememberPassword(string cachedPassword, string savedPassword, bool expectedRememberPassword)
		{
			Settings.Default.LanguageForgeUser = "User";
			Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(savedPassword);
			ServerSettingsModel.PasswordForSession = cachedPassword;

			var sut = new ServerSettingsModel();
			Assert.AreEqual(expectedRememberPassword, sut.RememberPassword);
			if (expectedRememberPassword)
			{
				Assert.AreEqual(savedPassword, sut.Password);
			}
			else
			{
				Assert.IsNullOrEmpty(sut.Password);
			}
		}

		[Test]
		public void RememberPasswordByDefault()
		{
			Settings.Default.LanguageForgeUser = Settings.Default.LanguageForgePass = string.Empty;
			Assert.True(new ServerSettingsModel().RememberPassword);
		}

		[Test]
		public void SaveSettings_NoHgFolderExists_CreatesOneWithCorrectPath()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				const string user = "joe";
				const string pass = "pass";
				const string host = "hg-public.languageforge.org/tpi";
				const string url = "https://" + user + ":" + pass + "@" + host;
				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				m.SaveSettings();
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg","hgrc")));
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual(new ServerSettingsModel.BandwidthItem(ServerSettingsModel.BandwidthEnum.High), m.Bandwidth);
				Assert.AreEqual("https://" + host, address.URI);
				Assert.AreEqual(user, m.Username);
				Assert.AreEqual(pass, m.Password);
			}
		}

		[Test]
		public void SaveSettings_PreexistsAndWeSave_MovesCredentials([Values(true, false)] bool isResumable)
		{
			ServerSettingsModel.PasswordForSession = Settings.Default.LanguageForgePass = Settings.Default.LanguageForgeUser = null;
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				const string user = "joe";
				const string pass = "pass";
				var subdomain = isResumable ? "resumable" : "hg-public";
				var oldHost = $"{subdomain}.languagedepot.org/tpi";
				var oldUrl = $"https://{user}:{pass}@{oldHost}";
				const string newDomainAndProj = ".languageforge.org/tpi";
				var newUrl = $"https://{subdomain}{newDomainAndProj}";
				var newUrlWithCredentials = $"https://{user}:{pass}@{subdomain}{newDomainAndProj}";
				// Precondition is some url that is not our default from the ServerSettingsModel
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				original.SetKnownRepositoryAddresses(new[]
				{
					new HttpRepositoryPath("languageForge.org [legacy sync]", oldUrl, false)
				});

				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.SaveSettings();
				Assert.AreEqual(user, Settings.Default.LanguageForgeUser);
				Assert.AreEqual(pass, ServerSettingsModel.DecryptPassword(Settings.Default.LanguageForgePass));
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg", "hgrc")));
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual(newUrl, address.URI);
				Assert.AreEqual(isResumable ? newUrl : newUrlWithCredentials, address.GetPotentialRepoUri(null, null, null),
					"The new 'potential' URI should contain credentials only when non-resumable");
			}
		}

		[Test]
		public void SaveSettings_ForgetsPassword()
		{
			ServerSettingsModel.PasswordForSession = null;
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				const string user = "joe";
				const string pass = "passwordForShortTermMemory";
				var m = new ServerSettingsModel {Username = user, Password = pass, RememberPassword = false};
				m.InitFromProjectPath(folder.Path);
				m.SaveSettings();
				Assert.AreEqual(user, Settings.Default.LanguageForgeUser);
				Assert.Null(Settings.Default.LanguageForgePass);
				Assert.AreEqual(pass, ServerSettingsModel.PasswordForSession);
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.That(address.URI, Is.Not.StringContaining(pass));
			}
		}

		[Test]
		public void SetUrlToUseIfSettingsAreEmpty_RepoAlreadyExistsWithAServerAddress_IgnoresOfferedUrl()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var existing = "https://abc.com";
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("languageforge.org [Safe Mode]", existing, false) });

				var m = new ServerSettingsModel();
				var url = "https://joe:pass@hg-public.languageforge.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				Assert.AreEqual(existing, m.URL);
			}
		}

		/// <summary>
		/// We want disk URLs identified as 'default' to be ignored (since they are not ones we added ourselves)
		/// </summary>
		[Test]
		public void DefaultUrlsAreIgnored()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("default", "c://abc.com", false) });

				var m = new ServerSettingsModel();
				const string url = "unclickable://hg-private.languageforge.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				Assert.AreEqual(url, m.URL);
			}
		}

		/// <summary>
		/// The new default (as of 8 Nov 2012) is resumable.
		/// </summary>
		[Test]
		public void DefaultIsResumable()
		{
			var m = new ServerSettingsModel();
			Assert.AreEqual("resumable.languageforge.org", m.Host);
		}

		[Test]
		public void EncryptPassword_NullAndEmptyDoNotCrash()
		{
			Assert.DoesNotThrow(() => ServerSettingsModel.EncryptPassword(null));
			Assert.DoesNotThrow(() => ServerSettingsModel.EncryptPassword(string.Empty));
		}

		[Test]
		public void DecryptPassword_NullAndEmptyDoNotCrash()
		{
			Assert.DoesNotThrow(() => ServerSettingsModel.DecryptPassword(null));
			Assert.DoesNotThrow(() => ServerSettingsModel.DecryptPassword(string.Empty));
		}

		[Test]
		public void EncryptPassword_RoundTrips()
		{
			const string password = "P@55w0rd";
			var encrypted = ServerSettingsModel.EncryptPassword(password);
			var decrypted = ServerSettingsModel.DecryptPassword(encrypted);
			Assert.AreEqual(password, decrypted);
			Assert.AreNotEqual(password, encrypted);
		}

		[Test]
		public void PasswordForSession_UsesSaved([Values(null, "", "myPass")] string password)
		{
			Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(password);
			ServerSettingsModel.PasswordForSession = null;
			Assert.AreEqual(password, ServerSettingsModel.PasswordForSession);
		}

		[Test]
		public void PasswordForSession_UsesCached()
		{
			try
			{
				const string pass = "yourPass";
				ServerSettingsModel.PasswordForSession = pass;
				Settings.Default.LanguageForgePass = null;
				Assert.AreEqual(pass, ServerSettingsModel.PasswordForSession);
			}
			finally
			{
				ServerSettingsModel.PasswordForSession = null;
			}
		}

		/// <remarks>
		/// The preference is unimportant in the real world, as the session cache will be populated only if the user initiates
		/// a Send and Receive when there is no saved password. However, other unit tests depend on this preference to be robust.
		/// </remarks>
		[Test]
		public void PasswordForSession_PrefersCached()
		{
			try
			{
				const string pass = "cachedPass";
				ServerSettingsModel.PasswordForSession = pass;
				Settings.Default.LanguageForgePass = "something-else";
				Assert.AreEqual(pass, ServerSettingsModel.PasswordForSession);
			}
			finally
			{
				ServerSettingsModel.PasswordForSession = null;
			}
		}

		[Test]
		public void RemovePasswordForLog_NullAndEmptyDoNotCrash()
		{
			Assert.DoesNotThrow(() => ServerSettingsModel.RemovePasswordForLog(null));
			Assert.DoesNotThrow(() => ServerSettingsModel.RemovePasswordForLog(string.Empty));
		}

		[Test]
		public void RemovePasswordForLog_RemovesThePassword()
		{
			try
			{
				const string password = "patchworkQu11+";
				const string logFormat = "Cannot connect to https://someone:{0}@hg-public.languageforge.org/flex-proj; check your password and try again.";
				ServerSettingsModel.PasswordForSession = password;
				// SUT
				var scrubbed = ServerSettingsModel.RemovePasswordForLog(string.Format(logFormat, password));
				Assert.AreEqual(string.Format(logFormat, ServerSettingsModel.PasswordAsterisks), scrubbed);
				StringAssert.DoesNotContain(password, scrubbed);
			}
			finally
			{
				ServerSettingsModel.PasswordForSession = null;
			}
		}

		[Test]
		public void RemovePasswordForLog_RemovesOnlyThePassword()
		{
			try
			{
				const string password = "password";
				const string logFormat = "Cannot connect to https://someone:{0}@hg-public.languageforge.org/flex-proj; check your {1} and try again.";
				ServerSettingsModel.PasswordForSession = password;
				// SUT
				var scrubbed = ServerSettingsModel.RemovePasswordForLog(string.Format(logFormat, password, password));
				Assert.AreEqual(string.Format(logFormat, ServerSettingsModel.PasswordAsterisks, password), scrubbed);
			}
			finally
			{
				ServerSettingsModel.PasswordForSession = null;
			}
		}
	}
}