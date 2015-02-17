using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chorus.UI.Clone;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;
using SIL.UsbDrive;
using Palaso.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromUsbDialogTests
	{
		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_LiveTest_FilterOutParatext()
		{
			using (var f = new TemporaryFolder("clonetest"))
			{
				using (var dlg = new GetCloneFromUsbDialog(f.Path))
				{
					dlg.Model.ProjectFilter = dir => !dir.Contains("Shared Paratext Projects");
					if(DialogResult.OK != dlg.ShowDialog())
						return;
				}
			}
		}

		[Test, Ignore("Run by hand only")]
		public void LaunchDialog_SimulatedUsb_ProjectAlreadyExists()
		{
			using (var targetComputer = new TemporaryFolder("clonetest-targetComputer"))
			using (var usb = new TemporaryFolder("clonetest-Usb"))
			{
				Directory.CreateDirectory(usb.Combine("repo1"));
				HgRepository.CreateRepositoryInExistingDir(usb.Combine("repo1"), new NullProgress());

				//ok, the point here is that we already haved something called "repo1"
				Directory.CreateDirectory(targetComputer.Combine("repo1"));

				using (var dlg = new GetCloneFromUsbDialog(targetComputer.Path))
				{
					var drives = new List<IUsbDriveInfo>();
					drives.Add(new UsbDriveInfoForTests(usb.Path));

					//don't look at the actual drives, look at our simulations
					dlg.Model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);

					if (DialogResult.OK != dlg.ShowDialog())
						return;
				}
			}
		}
	}
}