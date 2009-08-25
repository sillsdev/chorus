using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.Utilities;
using Chorus.Utilities.UsbDrive;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests
{
	[TestFixture]
	public class GetCloneFromInterentDialogTests
	{


		[Test, Ignore("Run by hand only")]
		public void LaunchDialog()
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
					dlg.URL = "http://hg-public.languagedepot.org/tpi";

					if (DialogResult.OK != dlg.ShowDialog())
						return;
				}
			}
		}


	}
}
