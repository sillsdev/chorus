using System.IO;
using Chorus.UI.Misc;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.UI.Misc
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
			Assert.AreEqual("hg-public.languagedepot.org", m.SelectedServerPath);
		}
		[Test]
		public void InitFromUri_FullTypicalLangDepot_SelectedServerLabel()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-public.languagedepot.org/tpi");
			Assert.AreEqual("languageDepot.org", m.SelectedServerLabel);
		}
		[Test]
		public void InitFromUri_FullPrivateLangDepot_SelectedServerLabel()
		{
			var m = new ServerSettingsModel();
			m.InitFromUri("http://joe:pass@hg-private.languagedepot.org/tpi");
			Assert.AreEqual("private.LanguageDepot.org".ToLower(), m.SelectedServerLabel.ToLower());
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
			using(var folder = new TempFolder("ServerSettingsModel"))
			{
				var m = new ServerSettingsModel();
				var url = "http://joe:pass@hg-public.languagedepot.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				m.SaveSettings();
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg","hgrc")));
				var repo =HgRepository.CreateOrLocate(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("default", address.Name);
				Assert.AreEqual(url, address.URI);
			}
		}

		[Test]
		public void SaveSettings_PrexistsButWeChangePasswordAndSave_ChangesPassword()
		{
			using (var folder = new TempFolder("ServerSettingsModel"))
			{
				var original = HgRepository.CreateOrLocate(folder.Path, new NullProgress());
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("default", "http://joe:oldPassword@hg-public.languagedepot.org/tpi", false) });

				var m = new ServerSettingsModel();
				m.InitFromProjectPath(folder.Path);
				m.Password = "newPassword";
				m.SaveSettings();
				Assert.IsTrue(Directory.Exists(folder.Combine(".hg")));
				Assert.IsTrue(File.Exists(folder.Combine(".hg", "hgrc")));
				var repo = HgRepository.CreateOrLocate(folder.Path, new NullProgress());
				var address = repo.GetDefaultNetworkAddress<HttpRepositoryPath>();
				Assert.AreEqual("newPassword", address.Password);
			}
		}

		[Test]
		public void SetUrlToUseIfSettingsAreEmpty_RepoAlreadyExistsWithAServerAddress_IgnoresOfferedUrl()
		{
			using (var folder = new TempFolder("ServerSettingsModel"))
			{
				var original = HgRepository.CreateOrLocate(folder.Path, new NullProgress());
				var existing = "http://abc.com";
				original.SetKnownRepositoryAddresses(new[] { new HttpRepositoryPath("default", existing, false) });

				var m = new ServerSettingsModel();
				var url = "http://joe:pass@hg-public.languagedepot.org/tpi";
				m.InitFromProjectPath(folder.Path);
				m.SetUrlToUseIfSettingsAreEmpty(url);
				Assert.AreEqual(existing,m.URL);
			}
		}
	}
}