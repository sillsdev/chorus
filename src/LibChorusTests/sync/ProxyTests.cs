using System.Diagnostics;
using System.IO;
using Chorus.FileTypeHanders.test;
using Chorus.merge;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.Tests.merge;
using LibChorus.Tests.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests.sync
{
	/// <summary>
	/// Note, these are only going to test the poxy situation if you're working behind a proxy. You can put yourself
	/// behind one using fiddler (http://fiddler2.com)
	/// </summary>
	[TestFixture]
	[Category("Sync")]
	public class ProxyTests
	{

		[SetUp]
		public void Setup()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 10000;//reset it in between tests
		}

		[Test, Ignore("By Hand Only")]
		public void Test()
		{
			using (var f = new TempFolder("clonetest"))
			{
				HgRepository.Clone("http://chorus:notasecret@hg-public.languagedepot.org/testing-clone", f.Path, new NullProgress());
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
			}
		}

	}
}