using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	class HgResumeRestApiServerTests
	{
		[Test]
		public void Constructor_languageDepotUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("http://hg-private.languagedepot.org/kyu-dictionary");
			Assert.That(api.Identifier, Is.EqualTo("hg-private.languagedepot.org"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}

		[Test]
		public void Constructor_languageForgeUrl_IdentityAndProjectIdSetCorrectly()
		{
			var api = new HgResumeRestApiServer("http://hg.languageforge.com/projects/kyu-dictionary");
			Assert.That(api.Identifier, Is.EqualTo("hg.languageforge.com"));
			Assert.That(api.ProjectId, Is.EqualTo("kyu-dictionary"));
		}
	}
}
