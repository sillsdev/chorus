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
				var sources = repo.GetKnownRepositorySources();
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
				var sources = repo.GetKnownRepositorySources();
				Assert.AreEqual(2, sources.Count());
				Assert.AreEqual(@"c:\intentionally bogus" ,sources.First().URI);
				Assert.AreEqual(@"http://foo.com", sources.Last().URI);
				Assert.AreEqual(@"one" ,sources.First().SourceLabel);
				Assert.AreEqual(@"two", sources.Last().SourceLabel);
			}
		}
	}
}