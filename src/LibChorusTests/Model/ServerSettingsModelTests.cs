using System.IO;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;
using Chorus.Model;
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
			Chorus.Properties.Settings.Default.LanguageForgeUser = user;
			Chorus.Properties.Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword(pass);
			var m = new ServerSettingsModel();
			m.InitFromUri("https://hg.languageforge.org/tpi");
			Assert.AreEqual(user, m.Username);
			Assert.AreEqual(pass, m.Password);
			Assert.AreEqual("tpi", m.ProjectId);
		}

		[Test]
		public void InitFromUri_UsernameAndPass_OverwritesSettings()
		{
			Chorus.Properties.Settings.Default.LanguageForgeUser = "from";
			Chorus.Properties.Settings.Default.LanguageForgePass = ServerSettingsModel.EncryptPassword("settings");
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
		public void SaveSettings_PreexistsAndWeSave_MovesCredentials()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				const string user = "joe";
				const string pass = "pass";
				const string oldHost = "hg-public.languagedepot.org/tpi";
				const string url = "https://" + user + ":" + pass + "@" + oldHost;
				// Precondition is some url that is not our default from the ServerSettingsModel
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				original.SetKnownRepositoryAddresses(new[]
				{
					new HttpRepositoryPath("languageForge.org [legacy sync]", url, false)
				});

				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.SaveSettings();
				Assert.AreEqual(user, Chorus.Properties.Settings.Default.LanguageForgeUser);
				Assert.AreEqual(pass, ServerSettingsModel.DecryptPassword(Chorus.Properties.Settings.Default.LanguageForgePass));
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg", "hgrc")));
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("https://hg-public.languageforge.org/tpi", address.URI);
				Assert.AreEqual("https://hg-public.languageforge.org/tpi", address.GetPotentialRepoUri(null, null, null));
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
	}
}