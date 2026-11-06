using System;
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
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			// Removing this variable from the process's environment so that tests use the default server
			Environment.SetEnvironmentVariable(ServerSettingsModel.ServerEnvVar, null, EnvironmentVariableTarget.Process);
		}

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
		public void InitFromUri_FullTypicalLangForge_DomainAndBandwidthCorrectAndNotCustom()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
			Assert.AreEqual("hg-public.languageforge.org", m.Host);
			Assert.AreEqual(HgTransportKind.HgWeb, m.Bandwidth.Transport);
			Assert.False(m.IsCustomUrl);
		}

		[Test]
		public void InitFromUri_LegacyResumableLangForge_MovedToLexboxHostAndResumableAndNotCustom()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://resumable.languageforge.org/tpi");
			Assert.AreEqual("hg-public.languageforge.org", m.Host);
			Assert.AreEqual("https://hg-public.languageforge.org/tpi", m.URL);
			Assert.AreEqual(HgTransportKind.Resumable, m.Bandwidth.Transport);
			Assert.AreEqual(HgTransportKind.Resumable, m.Transport);
			Assert.False(m.IsCustomUrl);
		}

		[Test]
		public void InitFromUri_PrivateLangForge_DomainAndBandwidthCorrectAndNotCustom()
		{
			using (new PrivateServer())
			{
				var m = new ServerSettingsModel();
				m.InitFromUri("https://joe:pass@hg-private.languageforge.org/tpi");
				Assert.AreEqual("hg-private.languageforge.org", m.Host);
				Assert.AreEqual(HgTransportKind.HgWeb, m.Bandwidth.Transport);
				Assert.False(m.IsCustomUrl);
			}
		}

		[Test]
		public void InitFromUri_PrivateLegacyResumableLangForge_MovedToLexboxHostAndResumableAndNotCustom()
		{
			using (new PrivateServer())
			{
				var m = new ServerSettingsModel();
				m.InitFromUri("https://resumable.languageforge.org/tpi");
				Assert.AreEqual("hg-private.languageforge.org", m.Host);
				Assert.AreEqual(HgTransportKind.Resumable, m.Bandwidth.Transport);
				Assert.False(m.IsCustomUrl);
			}
		}

		[TestCase("resumable", true)]
		[TestCase("hg-public", false)]
		[TestCase("hg-private", false)]
		public void InitFromUri_LanguageDepot_ConvertedToLanguageForge(string subdomain, bool isResumable)
		{
			try
			{
				if (subdomain.Equals("hg-private"))
				{
					// Setting IsPrivateServer is necessary to prevent hg-private.languageforge.org being marked as custom.
					// If IsPrivateServer is set incorrectly (either way), the URL will be imported and scrubbed correctly,
					// but marked as custom (tested in other tests)
					ServerSettingsModel.IsPrivateServer = true;
				}

				// Lexbox serves both transports from hg-public/hg-private; resumable is saved as a separate setting
				var expectedNewHost = isResumable ? "hg-public.languageforge.org" : $"{subdomain}.languageforge.org";
				var expectedTransport = isResumable ? HgTransportKind.Resumable : HgTransportKind.HgWeb;
				var m = new ServerSettingsModel();
				// SUT
				// ReSharper disable once StringLiteralTypo - the old server used to be called Language Depot
				m.InitFromUri($"http://joe:cool@{subdomain}.languagedepot.org/mcx");

				Assert.False(m.IsCustomUrl);

				Assert.AreEqual(expectedNewHost, m.Host);
				Assert.AreEqual("joe", m.Username);
				Assert.AreEqual("cool", m.Password);
				Assert.AreEqual(expectedTransport, m.Bandwidth.Transport);
				Assert.AreEqual("mcx", m.ProjectId);
				Assert.AreEqual($"https://{expectedNewHost}/mcx", m.URL);
			}
			finally
			{
				ServerSettingsModel.IsPrivateServer = false;
			}
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
			Assert.AreEqual(HgTransportKind.HgWeb, m.Bandwidth.Transport, "Custom URL's aren't known to be resumable");
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
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
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
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
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
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
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
				Assert.That(sut.Password, Is.Null.Or.Empty);
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
				Assert.That(folder.Combine(".hg"), Does.Exist);
				Assert.That(folder.Combine(".hg","hgrc"), Does.Exist);
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual(new ServerSettingsModel.BandwidthItem(HgTransportKind.HgWeb), m.Bandwidth);
				Assert.AreEqual("https://" + host, address.URI);
				Assert.AreEqual(user, m.Username);
				Assert.AreEqual(pass, m.Password);
			}
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
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
				// Lexbox serves both transports from one host; the transport is saved separately
				const string newUrl = "https://hg-public.languageforge.org/tpi";
				const string newUrlWithCredentials = "https://" + user + ":" + pass + "@hg-public.languageforge.org/tpi";
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
				Assert.That(folder.Combine(".hg"), Does.Exist);
				Assert.That(folder.Combine(".hg", "hgrc"), Does.Exist);
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual(newUrl, address.URI);
				Assert.AreEqual("Lexbox", address.Name);
				var expectedTransport = isResumable ? HgTransportKind.Resumable : HgTransportKind.HgWeb;
				Assert.AreEqual(expectedTransport, repo.GetTransport());
				Assert.AreEqual(expectedTransport, address.Transport);
				Assert.AreEqual(isResumable ? newUrl : newUrlWithCredentials, address.GetPotentialRepoUri(null, null, null),
					"The new 'potential' URI should contain credentials only when non-resumable");
			}
		}

		[Test]
		public void SaveSettings_TransportRoundTripsIndependentOfUrl([Values] HgTransportKind transport)
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.InitFromUri("https://joe:pass@hg-public.languageforge.org/tpi");
				m.Bandwidth = new ServerSettingsModel.BandwidthItem(transport);
				m.SaveSettings();

				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				Assert.AreEqual(transport, repo.GetTransport());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("https://hg-public.languageforge.org/tpi", address.URI, "the URL should not depend on the transport");
				Assert.AreEqual(transport, address.Transport);

				var reloaded = new ServerSettingsModel();
				reloaded.InitFromProjectPath(folder.Path);
				Assert.AreEqual(transport, reloaded.Bandwidth.Transport);
				Assert.False(reloaded.IsCustomUrl);
			}
		}

		[Test]
		public void InitFromProjectPath_SavedTransportOverridesUrl()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				repo.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("Lexbox", "https://resumable.languageforge.org/tpi", false) });
				repo.SetTransport(HgTransportKind.HgWeb);

				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				Assert.AreEqual(HgTransportKind.HgWeb, m.Bandwidth.Transport);
				Assert.AreEqual("https://hg-public.languageforge.org/tpi", m.URL);
			}
		}

		[Test]
		public void Transport_CustomUrl_FollowsBandwidthChoice([Values] HgTransportKind transport)
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://chorus.elsewhere.net/tpi");
			m.Bandwidth = new ServerSettingsModel.BandwidthItem(transport);
			Assert.True(m.IsCustomUrl);
			Assert.AreEqual(transport, m.Transport);
		}

		[Test]
		public void InitFromUri_CustomResumableUrl_DefaultsToResumable()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("https://chorus.elsewhere.net/resumable/tpi");
			Assert.True(m.IsCustomUrl);
			Assert.AreEqual(HgTransportKind.Resumable, m.Transport, "legacy custom URLs keep their transport until the user changes it");
		}

		[Test]
		public void SaveSettings_CustomUrl_TransportRoundTripsIndependentOfUrl([Values] HgTransportKind transport)
		{
			const string url = "https://chorus.elsewhere.net/tpi";
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.InitFromUri(url);
				m.Bandwidth = new ServerSettingsModel.BandwidthItem(transport);
				m.SaveSettings();

				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual(url, address.URI);
				Assert.AreEqual(transport, address.Transport);

				var reloaded = new ServerSettingsModel();
				reloaded.InitFromProjectPath(folder.Path);
				Assert.True(reloaded.IsCustomUrl);
				Assert.AreEqual(transport, reloaded.Bandwidth.Transport);
			}
		}

		[Test]
		public void AliasName_Lexbox_DoesNotIncludeBandwidth([Values] HgTransportKind transport)
		{
			var m = new ServerSettingsModel { Bandwidth = new ServerSettingsModel.BandwidthItem(transport) };
			Assert.AreEqual("Lexbox", m.AliasName);
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
				Assert.That(address.URI, Does.Not.Contain(pass));
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
			Assert.AreEqual("hg-public.languageforge.org", m.Host);
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
		public void DecryptPassword_InvalidBase64_ReturnsNull()
		{
			Assert.That(ServerSettingsModel.DecryptPassword("not-valid-base64!!!"), Is.Null);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
		public void EncryptPassword_RoundTrips()
		{
			const string password = "P@55w0rd";
			var encrypted = ServerSettingsModel.EncryptPassword(password);
			var decrypted = ServerSettingsModel.DecryptPassword(encrypted);
			Assert.AreEqual(password, decrypted);
			Assert.AreNotEqual(password, encrypted);
		}

		[Test]
		[Platform(Exclude = "Linux", Reason = "Windows Data Protection API (DPAPI) is not supported on this platform")]
		public void PasswordForSession_UsesSaved([Values(null, "", "myPass")] string password)
		{
			Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(password);
			ServerSettingsModel.PasswordForSession = null;
			Assert.AreEqual(password, ServerSettingsModel.PasswordForSession);
		}

		[Test]
		public void PasswordForSession_UsesCached()
		{
			const string pass = "yourPass";
			using (new PasswordForSession(pass))
			{
				Settings.Default.LanguageForgePass = null;
				Assert.AreEqual(pass, ServerSettingsModel.PasswordForSession);
			}
		}

		/// <remarks>
		/// The preference is unimportant in the real world, as the session cache will be populated only if the user initiates
		/// a Send and Receive when there is no saved password. However, other unit tests depend on this preference to be robust.
		/// </remarks>
		[Test]
		public void PasswordForSession_PrefersCached()
		{
			const string pass = "cachedPass";
			using (new PasswordForSession(pass))
			{
				Settings.Default.LanguageForgePass = "something-else";
				Assert.AreEqual(pass, ServerSettingsModel.PasswordForSession);
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
			const string password = "patch-work_Qu11t";
			const string logFormat = "Cannot connect to https://someone:{0}@hg-public.languageforge.org/flex-proj; check your password and try again.";
			using (new PasswordForSession(password))
			{
				// SUT
				var scrubbed = ServerSettingsModel.RemovePasswordForLog(string.Format(logFormat, password));
				Assert.AreEqual(string.Format(logFormat, ServerSettingsModel.PasswordAsterisks), scrubbed);
				StringAssert.DoesNotContain(password, scrubbed);
			}
		}

		[Test]
		public void RemovePasswordForLog_RemovesOnlyThePassword()
		{
			const string password = "password";
			const string logFormat = "Cannot connect to https://someone:{0}@hg-public.languageforge.org/flex-proj; check your {1} and try again.";
			using(new PasswordForSession(password))
			{
				// SUT
				var scrubbed = ServerSettingsModel.RemovePasswordForLog(string.Format(logFormat, password, password));
				Assert.AreEqual(string.Format(logFormat, ServerSettingsModel.PasswordAsterisks, password), scrubbed);
			}
		}

		[Test]
		public void RemovePasswordForLog_RemovesTheEscapedPassword()
		{
			const string password = "p@tchwork:Qu11+";
			using (new PasswordForSession(password))
			{
				var source = new HttpRepositoryPath("test", "https://public.languageforge.org", true);
				var url = source.GetPotentialRepoUri("test", "test", null);
				// SUT
				var scrubbed = ServerSettingsModel.RemovePasswordForLog(url);
				ServerSettingsModel.PasswordForSession = ServerSettingsModel.PasswordAsterisks; // this will be cleared when we exit the using block
				var expected = source.GetPotentialRepoUri("test", "test", null);
				Assert.That(scrubbed, Is.EqualTo(expected));
			}
		}

		internal class PasswordForSession : IDisposable
		{
			public PasswordForSession(string password)
			{
				ServerSettingsModel.PasswordForSession = password;
			}

			public void Dispose()
			{
				ServerSettingsModel.PasswordForSession = null;
			}
		}

		private class PrivateServer : IDisposable
		{
			public PrivateServer()
			{
				ServerSettingsModel.IsPrivateServer = true;
			}

			public void Dispose()
			{
				ServerSettingsModel.IsPrivateServer = false;
			}
		}
	}
}