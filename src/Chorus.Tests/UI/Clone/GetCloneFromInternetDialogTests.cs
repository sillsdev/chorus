using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromInterentDialogTests
	{
		[SetUp]
		public void Setup()
		{
			Application.EnableVisualStyles();//make progress bar work correctly
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_GoodAddressNoFolder()
		{
			LaunchCustomUrl("http://hg-public.languagedepot.org/tpi");
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_GoodAddressWithFolderName()
		{
			LaunchCustomUrl("http://hg-public.languagedepot.org/tpi?localFolder=TokPisin");
		}
//
//        [Test, Ignore("Run by hand only")]
//        public void LaunchDialog_GoodAddressSmallRepot()
//        {
//            Launch("http://hg.palaso.org/chorusdemo");
//        }
//
//        [Test, Ignore("Run by hand only")]
//        public void LaunchDialog_IllFormedAddress()//gives abort: repository htt://a73fsz.org/tpi not found!
//        {
//            Launch("htt://a73fsz.org/tpi");
//        }
//        [Test, Ignore("Run by hand only")]
//        public void LaunchDialog_BogusAddress()//(in Ukarumpa) gives : HTTP Error 502: Proxy Error ( The host was not found. )
//        {
//            Launch("http://a73fsz.org/tpi");
//        }
//        [Test, Ignore("Run by hand only")]
//        public void LaunchDialog_ProjectWontbeFound()//gives HTTP Error 404: Not Found
//        {
//            Launch("http://hg-public.languagedepot.org/NOTHERE");
//        }

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_CustomUrlSourceWontBeFound()//gives HTTP Error 404: Not Found
		{
			using (var source = new TemporaryFolder("CloneDialogTest"))
			{
				Directory.CreateDirectory(source.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(source.Combine("repo1"), new NullProgress());
				LaunchCustomUrl(@"somewhereElse");
			}
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_CustomSourceWillBeFound()
		{
			using (var source = new TemporaryFolder("CloneDialogTest"))
			{
				Directory.CreateDirectory(source.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(source.Combine("repo1"), new NullProgress());
				LaunchCustomUrl(source.Combine("repo1"));
			}
		}

		private void LaunchCustomUrl(string url)
		{
			using (var targetComputer = new TemporaryFolder("clonetest-targetComputer"))
			{
				var model = new GetCloneFromInternetModel(targetComputer.Path);
				model.InitFromUri(url);
				using (var dlg = new GetCloneFromInternetDialog(model))
				{
					dlg.ShowDialog();
				}
			}
		}

		[Test,Ignore("By hand only")]
		public void LaunchUI_Blank()
		{
			Launch();
		}

		[Test, Ignore("By hand only")]
		public void LaunchWithPreformedSettings()
		{
			Launch();
		}


		private void Launch()
		{
			using (var targetComputer = new TemporaryFolder("clonetest-targetComputer"))
			using (var dest = new TemporaryFolder("clonetest"))
			{
				Directory.CreateDirectory(dest.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(dest.Combine("repo1"), new NullProgress());

				//ok, the point here is that we already haved something called "repo1"
				Directory.CreateDirectory(targetComputer.Combine("repo1"));

				using (var dlg = new GetCloneFromInternetDialog(targetComputer.Path))
				{

					dlg.ShowDialog();
				}
			}
		}

	}
}