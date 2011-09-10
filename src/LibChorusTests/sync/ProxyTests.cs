using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

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
		private string _cloneableTestProjectUrl = "http://chorus:notasecret@hg-public.languagedepot.org/testing-clone";
		private ConsoleProgress _progress = new ConsoleProgress(){ShowVerbose = true};


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
				HgRepository.Clone(_cloneableTestProjectUrl, f.Path, _progress);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
			}
		}


		[Test, Ignore("By Hand Only")]
		public void Pull_Test()
		{
			//RobustNetworkOperation.ClearCredentialSettings();
			using (var f = new TemporaryFolder("pulltest"))
			{
				var repo = HgRepository.CreateOrLocate(f.Path, _progress);
				repo.TryToPull("default", _cloneableTestProjectUrl);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
			}
		}

		[Test, Ignore("By Hand Only")]
		public void PullThenPush_Test()
		{
		  //  RobustNetworkOperation.ClearCredentialSettings();
			using (var f = new TemporaryFolder("pulltest"))
			{
				var repo = HgRepository.CreateOrLocate(f.Path, _progress);
				repo.TryToPull("default", _cloneableTestProjectUrl);
				Assert.IsTrue(Directory.Exists(f.Combine(f.Path, ".hg")));
				var address =RepositoryAddress.Create("default", _cloneableTestProjectUrl);

				//nb: this is safe to do over an over, because it will just say "no changes found", never actually add a changeset

				repo.Push(address, _cloneableTestProjectUrl, _progress);
			}
		}
	}
}