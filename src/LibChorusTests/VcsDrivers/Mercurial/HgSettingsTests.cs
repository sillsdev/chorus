using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using System.Linq;
using Palaso.Progress.LogBox;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	/// <summary>
	/// Mercurial has repository and global settings.  This fixture is for testing
	/// access to those.
	/// </summary>
	[TestFixture]
	public class HgSettingsTests
	{
		private ConsoleProgress _progress;

		[SetUp]
		public void Setup()
		{
			_progress = new ConsoleProgress();
		}

		[Test] public void GetKnownRepositories_NoneKnown_GivesNone()
		{
			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repo = new HgRepository(testRoot.Path, _progress);
				var sources = repo.GetRepositoryPathsInHgrc();
				Assert.AreEqual(0, sources.Count());
			}
		}

		[Test]
		public void GetKnownRepositories_TwoInRepoSettings_GivesThem()
		{
			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				File.WriteAllText(testRoot.Combine(Path.Combine(".hg","hgrc")), @"
[paths]
one = c:\intentionally bogus
two = http://foo.com");
				var repo = new HgRepository(testRoot.Path, _progress);
				var sources = repo.GetRepositoryPathsInHgrc();
				Assert.AreEqual(2, sources.Count());
				Assert.AreEqual(@"c:\intentionally bogus" ,sources.First().URI);
				Assert.AreEqual(@"http://foo.com", sources.Last().URI);
				Assert.AreEqual(@"one" ,sources.First().Name);
				Assert.AreEqual(@"two", sources.Last().Name);
			}
		}



		private bool GetIsReady(string pathsSectionContents)
		{
			string contents = @"[paths]" + Environment.NewLine + pathsSectionContents+Environment.NewLine;

			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				File.WriteAllText(testRoot.Combine(Path.Combine(".hg", "hgrc")), contents);
				var repo = new HgRepository(testRoot.Path, _progress);
				string msg;
				bool ready= repo.GetIsReadyForInternetSendReceive(out msg);
				Console.WriteLine(msg);
				return ready;
			}
		}


		[Test]
		public void GetIsReadyForInternetSendReceive_NoPaths_ReturnsFalse()
		{
			Assert.IsFalse(GetIsReady(@""));
		}

		[Test]
		public void GetIsReadyForInternetSendReceive_MissingUserName_ReturnsFalse()
		{
			Assert.IsFalse(GetIsReady(@"LanguageDepot = http://hg-public.languagedepot.org/xyz"));
		}

		[Test]
		public void GetIsReadyForInternetSendReceive_HasFullLangDepotUrl_ReturnsTrue()
		{
			Assert.IsTrue(GetIsReady(@"LanguageDepot = http://joe_user:xyz@hg-public.languagedepot.org/xyz"));
		}

		[Test]
		public void GetUserName_NameInLocalReop_GetsName()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.WriteIniContents(@"[ui]
username = joe
");
				var repository = setup.CreateSynchronizer().Repository;
				Assert.AreEqual("joe", repository.GetUserNameFromIni(_progress, "anonymous"));
			}
		}
		[Test]
		public void GetUserName_EmptyHgrc_ReturnsDefault()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.WriteIniContents(@"");
				var repository = setup.CreateSynchronizer().Repository;
				Assert.AreEqual("anonymous", repository.GetUserNameFromIni(_progress, "anonymous"));
			}
		}
		[Test]
		public void GetUserName_NoHgrcYet_ReturnsDefault()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.CreateSynchronizer().Repository;
				Assert.AreEqual("anonymous", repository.GetUserNameFromIni(_progress, "anonymous"));
			}
		}
		[Test]
		public void SetUserNameInIni_SetsName()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.CreateSynchronizer().Repository;
				repository.SetUserNameInIni("bill", _progress);
				Assert.AreEqual("bill", repository.GetUserNameFromIni(_progress, "anonymous"));

				//this time, the hgrc does exist
				repository.SetUserNameInIni("sue", _progress);
				Assert.AreEqual("sue", repository.GetUserNameFromIni(_progress, "anonymous"));
			}
		}

		[Test]
		public void SetGlobalProxyInfo_TODO()
		{
			///this test will have to wait until we have a way to *not* use the global mercurial.ini/.hgrc
			/// of this machine
		}
		[Test]
		public void GetGlobalProxyInfo_TODO()
		{
			///this test will have to wait until we have a way to *not* use the global mercurial.ini/.hgrc
			/// of this machine
		}


		[Test]
		public void SetRepositoryAliases()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.CreateSynchronizer().Repository;
				var x = RepositoryAddress.Create("one", @"c:\one");
				var y = RepositoryAddress.Create("two", @"http://two.org");
				repository.SetKnownRepositoryAddresses(new List<RepositoryAddress>(new RepositoryAddress[]{x,y}));
				Assert.AreEqual(2, repository.GetRepositoryPathsInHgrc().Count());
				Assert.AreEqual(x.Name, repository.GetRepositoryPathsInHgrc().First().Name);
				Assert.AreEqual(x.URI, repository.GetRepositoryPathsInHgrc().First().URI);
				Assert.AreEqual(y.Name, repository.GetRepositoryPathsInHgrc().ToArray()[1].Name);
				Assert.AreEqual(y.URI, repository.GetRepositoryPathsInHgrc().ToArray()[1].URI);

				var z = RepositoryAddress.Create("three", @"http://three.org");
				//this time, the hgrc does exist
				repository.SetKnownRepositoryAddresses(new List<RepositoryAddress>(new RepositoryAddress[]{z}));
				Assert.AreEqual(1, repository.GetRepositoryPathsInHgrc().Count());
				Assert.AreEqual(z.Name, repository.GetRepositoryPathsInHgrc().First().Name);
				Assert.AreEqual(z.URI, repository.GetRepositoryPathsInHgrc().First().URI);
			}
		}

		[Test]
		public void SetAndGetDefaultSyncRepositories()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.CreateSynchronizer().Repository;
				var x =  RepositoryAddress.Create("one", @"c:\one");
				var y = RepositoryAddress.Create("two", @"http://two.org");
				var z = RepositoryAddress.Create("three", @"http://three.org");
				repository.SetKnownRepositoryAddresses(new List<RepositoryAddress>(new RepositoryAddress[] { x, y,z }));

				repository.SetDefaultSyncRepositoryAliases(new string[] {"one", "three"});
				Assert.AreEqual(2, repository.GetDefaultSyncAliases().Count());


				repository.SetDefaultSyncRepositoryAliases(new string[] { "two" });
				Assert.AreEqual(1, repository.GetDefaultSyncAliases().Count());
			}
		}

		[Test]
		public void EnsureTheseExtensionAreEnabled_noExistingExtensions_AddsThem()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				setup.EnsureNoHgrcExists();

				setup.Repository.EnsureTheseExtensionAreEnabled(new string[] { "a","b" });
				Assert.AreEqual("a", setup.Repository.GetEnabledExtension().First());
				Assert.AreEqual("b", setup.Repository.GetEnabledExtension().ToArray()[1]);
			}
		}

		[Test]
		public void EnsureTheseExtensionAreEnabled_someOthersEnabledAlready_StayEnabled()
		{
			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path, _progress);
				var repository = new HgRepository(testRoot.Path, new ConsoleProgress());
				File.WriteAllText(testRoot.Combine(Path.Combine(".hg", "hgrc")), @"
[extensions]
a =
x =
");

				repository.EnsureTheseExtensionAreEnabled(new string[] { "a", "b" });
				Assert.AreEqual(3, repository.GetEnabledExtension().Count());
				Assert.AreEqual("a", repository.GetEnabledExtension().ToArray()[0]);
				Assert.AreEqual("x", repository.GetEnabledExtension().ToArray()[1]);
				Assert.AreEqual("b", repository.GetEnabledExtension().ToArray()[2]);
			}
		}
	}
}