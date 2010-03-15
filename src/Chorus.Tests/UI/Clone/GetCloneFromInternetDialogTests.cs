using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.Utilities.UsbDrive;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

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

//        [Test, Ignore("Run by hand only")]
//        public void LaunchDialog_GoodAddressLargeRepot()
//        {
//            Launch("http://hg-public.languagedepot.org/tpi");
//        }
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
		public void LaunchDialog_ProjectWontbeFound()//gives HTTP Error 404: Not Found
		{
			Launch(@"C:\Users\tim\Documents\WeSay\ThaiFood");
		}

		private void Launch(string url)
		{
			using (var targetComputer = new TempFolder("clonetest-targetComputer"))
			using (var usb = new TempFolder("clonetest-Usb"))
			{
				Directory.CreateDirectory(usb.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(usb.Combine("repo1"), new NullProgress());

				//ok, the point here is that we already haved something called "repo1"
				Directory.CreateDirectory(targetComputer.Combine("repo1"));

				using (var dlg = new GetCloneFromInternetDialog(targetComputer.Path))
				{

			 //       dlg.URL = url;

					if (DialogResult.OK != dlg.ShowDialog())
						return;
				}
			}
		}

		[Test,Ignore("By hand only")]
		public void LaunchUI()
		{
			Launch();
		}


		private void Launch()
		{
			using (var targetComputer = new TempFolder("clonetest-targetComputer"))
			using (var dest = new TempFolder("clonetest"))
			{
				Directory.CreateDirectory(dest.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(dest.Combine("repo1"), new NullProgress());

				//ok, the point here is that we already haved something called "repo1"
				Directory.CreateDirectory(targetComputer.Combine("repo1"));

				using (var dlg = new GetCloneFromInternetDialog(targetComputer.Path))
				{

					if (DialogResult.OK != dlg.ShowDialog())
						return;
				}
			}
		}

	}
}