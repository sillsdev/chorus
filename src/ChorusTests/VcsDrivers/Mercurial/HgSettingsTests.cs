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
				var sources = repo.GetRepositoryPathsInHgrc();
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
				var sources = repo.GetRepositoryPathsInHgrc();
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
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
				Assert.AreEqual("joe", repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void GetUserName_EmptyHgrc_ReturnsEmpty()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.WriteIniContents(@"");
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
				Assert.AreEqual(string.Empty, repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void GetUserName_NoHgrcYet_ReturnsEmpty()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
				Assert.AreEqual(string.Empty, repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void SetUserNameInIni_SetsName()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
				repository.SetUserNameInIni("bill", new NullProgress());
				Assert.AreEqual("bill", repository.GetUserNameFromIni(new NullProgress()));

				//this time, the hgrc does exist
				repository.SetUserNameInIni("sue", new NullProgress());
				Assert.AreEqual("sue", repository.GetUserNameFromIni(new NullProgress()));
			}
		}
		[Test]
		public void SetRepositoryAliases()
		{
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
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
			using (var setup = new EmptyRepositorySetup())
			{
				setup.EnsureNoHgrcExists();
				var repository = setup.RepoMan.GetRepository(new StringBuilderProgress());
				var x =  RepositoryAddress.Create("one", @"c:\one");
				var y = RepositoryAddress.Create("two", @"http://two.org");
				var z = RepositoryAddress.Create("three", @"http://three.org");
				repository.SetKnownRepositoryAddresses(new List<RepositoryAddress>(new RepositoryAddress[] { x, y,z }));

				repository.SetDefaultSyncRepositoryAliases(new string[] {"one", "three"});
				Assert.AreEqual(2, repository.GetDefaultSyncAddresses().Count());


				repository.SetDefaultSyncRepositoryAliases(new string[] { "two" });
				Assert.AreEqual(1, repository.GetDefaultSyncAddresses().Count());
			}
		}
	}
}