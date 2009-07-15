using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using System.Linq;

namespace Chorus.Tests.VcsDrivers.Mercurial
{
	/// <summary>
	/// Mercurial has repository and global settings.  This fixture is for testing
	/// access to those.
	/// </summary>
	[TestFixture]
	public class HgSettingsTests
	{

		[Test] public void GetKnownRepositories_NoneKnown_GivesEmptyList()
		{
			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path);
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				var sources = repo.GetKnownPeerRepositories();
				Assert.AreEqual(0, sources.Count());
			}
		}

		[Test]
		public void GetKnownRepositories_TwoInRepoSettings_GivesThem()
		{
			using (var testRoot = new TempFolder("ChorusHgSettingsTest"))
			{
				HgRepository.CreateRepositoryInExistingDir(testRoot.Path);
				File.WriteAllText(testRoot.Combine(Path.Combine(".hg","hgrc")), @"
[paths]
one = c:\intentionally bogus
two = http://foo.com");
				var repo = new HgRepository(testRoot.Path, new NullProgress());
				var sources = repo.GetKnownPeerRepositories();
				Assert.AreEqual(2, sources.Count());
				Assert.AreEqual(@"c:\intentionally bogus" ,sources.First().URI);
				Assert.AreEqual(@"http://foo.com", sources.Last().URI);
				Assert.AreEqual(@"one" ,sources.First().Name);
				Assert.AreEqual(@"two", sources.Last().Name);
			}
		}

		[Test]
		public void GetUserName_NameInLocalReop_GetsName()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.WriteIniContents(@"[ui]
username = joe
");
				var repository = setup.Repo.GetRepository(new StringBuilderProgress());
				Assert.AreEqual("joe", repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void GetUserName_EmptyHgrc_ReturnsEmpty()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.WriteIniContents(@"");
				var repository = setup.Repo.GetRepository(new StringBuilderProgress());
				Assert.AreEqual(string.Empty, repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void GetUserName_NoHgrcYet_ReturnsEmpty()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.Repo.GetRepository(new StringBuilderProgress());
				Assert.AreEqual(string.Empty, repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void SetUserNameInIni_SetsName()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.Repo.GetRepository(new StringBuilderProgress());
				repository.SetUserNameInIni("bill", new NullProgress());
				Assert.AreEqual("bill", repository.GetUserNameFromIni(new NullProgress()));

				//this time, the hgrc does exist
				repository.SetUserNameInIni("sue", new NullProgress());
				Assert.AreEqual("sue", repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void SetAddresses()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.Repo.GetRepository(new StringBuilderProgress());
				var x = new RepositoryAddress("one", @"c:\one");
				var y = new RepositoryAddress("two", @"http://two.org");
				repository.SetKnownPeerAddresses(new List<RepositoryAddress>(new RepositoryAddress[]{x,y}));
				Assert.AreEqual(2, repository.GetKnownPeerRepositories().Count());
				Assert.AreEqual(x.Name, repository.GetKnownPeerRepositories().First().Name);
				Assert.AreEqual(x.URI, repository.GetKnownPeerRepositories().First().URI);
				Assert.AreEqual(y.Name, repository.GetKnownPeerRepositories().ToArray()[1].Name);
				Assert.AreEqual(y.URI, repository.GetKnownPeerRepositories().ToArray()[1].URI);

				var z = new RepositoryAddress("three", @"http://three.org");
				//this time, the hgrc does exist
				repository.SetKnownPeerAddresses(new List<RepositoryAddress>(new RepositoryAddress[]{z}));
				Assert.AreEqual(1, repository.GetKnownPeerRepositories().Count());
				Assert.AreEqual(z.Name, repository.GetKnownPeerRepositories().First().Name);
				Assert.AreEqual(z.URI, repository.GetKnownPeerRepositories().First().URI);
			}
		}
	}
}