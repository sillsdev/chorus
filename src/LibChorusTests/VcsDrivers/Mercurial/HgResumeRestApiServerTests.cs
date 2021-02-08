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
	}
}
