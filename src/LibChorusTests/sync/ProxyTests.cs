using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

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
		private const string CloneableTestProjectUrl = "https://hg-public.languageforge.org/testing-clone";
		private readonly ConsoleProgress _progress = new ConsoleProgress{ShowVerbose = true};


		[SetUp]
		public void Setup()
		{
			HgRunner.TimeoutSecondsOverrideForUnitTests = 10000;//reset it in between tests
		}

		[Test, Ignore("By Hand Only")]
		public void Clone_Test()
		{
		   // RobustNetworkOperation.ClearCredentialSettings();
			using (var f = new TemporaryFolder("clonetest"))
			{
				HgRepository.Clone(new HttpRepositoryPath("cloneableTestProjectUrl", CloneableTestProjectUrl, false, false), f.Path, _progress);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
			}
		}


		[Test, Ignore("By Hand Only")]
		public void Pull_Test()
		{
			//RobustNetworkOperation.ClearCredentialSettings();
			using (var f = new TemporaryFolder("pulltest"))
			{
				var repo = HgRepository.CreateOrUseExisting(f.Path, _progress);
				var address = new HttpRepositoryPath("default", CloneableTestProjectUrl, false, false);
				repo.Pull(address, CloneableTestProjectUrl);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
			}
		}

		[Test, Ignore("By Hand Only")]
		public void PullThenPush_Test()
		{
		  //  RobustNetworkOperation.ClearCredentialSettings();
			using (var f = new TemporaryFolder("pulltest"))
			{
				var repo = HgRepository.CreateOrUseExisting(f.Path, _progress);
				var address = new HttpRepositoryPath("default", CloneableTestProjectUrl, false, false);
				repo.Pull(address, CloneableTestProjectUrl);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));

				//nb: this is safe to do over an over, because it will just say "no changes found", never actually add a changeset

				repo.Push(address, CloneableTestProjectUrl);
			}
		}
	}
}