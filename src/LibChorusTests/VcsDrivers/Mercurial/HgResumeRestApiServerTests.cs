using System;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	class HgResumeRestApiServerTests
	{
		[Test]
		public void Constructor_PrivateLanguageForgeUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("https://hg-private.languageforge.org/kyu-dictionary");
			Assert.That(api.Host, Is.EqualTo("hg-private.languageforge.org"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}

		[Test]
		public void Constructor_LanguageForgeUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("https://hg.languageforge.org/projects/kyu-dictionary");
			Assert.That(api.Host, Is.EqualTo("hg.languageforge.org"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}

		[Test]
		public void FormatUrl_FormatsCorrectlyWithEmptyParameters()
		{
			Assert.That(HgResumeRestApiServer.FormatUrl(
					new Uri("https://hg.languageforge.org:1234/projects/kyu-dictionary"),
					"foo",
					new HgResumeApiParameters()),
				Is.EqualTo("https://hg.languageforge.org:1234/api/v03/foo"));
		}

		[Test]
		public void FormatUrl_FormatsCorrectlyWithParameters()
		{
			//this test is overly specific as the order of the parameters is not important, however this is much simpler to write.
			Assert.That(HgResumeRestApiServer.FormatUrl(
					new Uri("https://hg.languageforge.org:1234/projects/kyu-dictionary"),
					"foo",
					new HgResumeApiParameters
					{
						StartOfWindow = 10,
						ChunkSize = 20,
						BundleSize = 30,
						Quantity = 40,
						TransId = "transId",
						BaseHashes = new[] { "baseHash1", "baseHash2" },
						RepoId = "repoId"
					}),
				Is.EqualTo(
					"https://hg.languageforge.org:1234/api/v03/foo?offset=10&chunkSize=20&bundleSize=30&quantity=40&transId=transId&baseHashes[]=baseHash1&baseHashes[]=baseHash2&repoId=repoId"));
		}
	}
}
