using System.IO;
using System.Security.Principal;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;
using Chorus.Model;
using Chorus.Model;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;

namespace LibChorus.Tests.Model
{
	[TestFixture]
	public class ServerSettingsModelTests
	{
		[Test]
		public void InitFromUri_FullTypicalLangDepot_AccountNameCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("joe",m.AccountName);
		}

		[Test]
		public void InitFromUri_CredentialsOriginallySetFromModelWithSpecialCharacters_AbleToRoundTripCredentialsBackFromURIOK()
		{
			var model = new ServerSettingsModel();
			const string accountName = "joe@user.com";
			const string password = "pass@with%specials&";
			const string projectId = "projectId";
			model.AccountName = accountName;
			model.Password = password;
			model.ProjectId = projectId;
			var urlWithEncodedChars = model.URL;

			var newModel = new ServerSettingsModel();
			newModel.InitFromUri(urlWithEncodedChars);
			Assert.AreEqual(accountName, newModel.AccountName);
			Assert.AreEqual(password, newModel.Password);
			Assert.AreEqual(projectId, newModel.ProjectId);
		}

		[Test]
		public void InitFromUri_FullTypicalLangDepot_PasswordCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("pass", m.Password);
		}

		[Test]
		public void InitFromUri_FullTypicalLangDepot_ProjectIdCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("tpi", m.ProjectId);
		}
		[Test]
		public void InitFromUri_FullTypicalLangDepot_SelectedServerPathCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("hg-public.languagedepot.org", m.SelectedServerModel.DomainName);
		}
		[Test]
		public void InitFromUri_FullTypicalLangDepot_SelectedServerLabel()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("languagedepot.org [safe mode]", m.SelectedServerLabel.ToLower());
		}
		[Test]
		public void InitFromUri_FullPrivateLangDepot_SelectedServerLabel()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-private.languagedepot.org/tpi");
			Assert.AreEqual("languagedepot.org [private safe mode]", m.SelectedServerLabel.ToLower());
		}

		[Test]
		public void InitFromUri_FullTypicalLangDepot_CustomUrlFalse()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.IsFalse(m.CustomUrlSelected);
		}

		[Test]
		public void InitFromUri_HasFolderDesignator_IdIsCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi?localFolder=foo");
			Assert.AreEqual("tpi", m.ProjectId);
		}

		[Test]
		public void InitFromUri_UnknownHttpGiven_CustomUrlIsTrue()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://somewhereelse.net/xyz");
			Assert.IsTrue(m.CustomUrlSelected);
		}

		[Test]
		public void InitFromUri_LANPathGiven_CustomUrlIsTrue()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("\\mybox\tpi");
			Assert.IsTrue(m.CustomUrlSelected);
		}

		[Test]
		public void InitFromUri_LANPathGiven_SelectedServerLabelIsCorrect()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("\\mybox\tpi");
			Assert.IsTrue(m.SelectedServerLabel.ToLower().Contains("custom"));
		}

		[Test]
		public void SaveSettings_NoHgFolderExists_CreatesOneWithCorrectPath()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var m = new ServerSettingsModel();
				var url = "http://joe:pass@hg-public.languagedepot.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				m.SaveSettings();
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg","hgrc")));
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("languageDepot.org[safemode]".ToLower(), address.Name.ToLower());
				Assert.AreEqual(url, address.URI);
			}
		}

		[Test]
		public void SaveSettings_PrexistsButWeChangePasswordAndSave_ChangesPassword()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				// Precondition is some url that is not our default from the ServerSettingsModel
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("languagedepot.org [legacy sync]", "http://joe:oldPassword@hg-public.languagedepot.org/tpi", false) });

				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.Password = "newPassword";
				m.SaveSettings();
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg", "hgrc")));
				var repo = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("http://joe:newPassword@hg-public.languagedepot.org/tpi", address.URI);
				Assert.AreEqual("newPassword", address.Password);
			}
		}

		[Test]
		public void SetUrlToUseIfSettingsAreEmpty_RepoAlreadyExistsWithAServerAddress_IgnoresOfferedUrl()
		{
			using (var folder = new TemporaryFolder("ServerSettingsModel"))
			{
				var original = HgRepository.CreateOrUseExisting(folder.Path, new NullProgress());
				var existing = "http://abc.com";
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("languagedepot.org [Safe Mode]", existing, false) });

				var m = new ServerSettingsModel();
				var url = "http://joe:pass@hg-public.languagedepot.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				Assert.AreEqual(existing,m.URL);
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
				var existing = "c://abc.com";
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("default", existing, false) });

				var m = new ServerSettingsModel();
				var url = "c://joe:pass@hg-public.languagedepot.org/tpi";
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
			Assert.AreEqual("resumable.languagedepot.org", m.Servers[m.SelectedServerLabel].DomainName);
		}
	}
}