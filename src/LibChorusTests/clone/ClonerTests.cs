using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.clone;
using Chorus.Utilities;
using Chorus.Utilities.UsbDrive;
using LibChorus.Tests;
using NUnit.Framework;

namespace Chorus.Tests
{
	[TestFixture]
	public class ClonerTests
	{
		[Test]
		public void MakeClone_NoProblems_MakesClone()
		{
			using(var repo = new RepositorySetup("source"))
			using (var f = new TempFolder("clonetest"))
			{
				var model = new Cloner();
				var progress = new ConsoleProgress();
				progress.ShowVerbose = true;
				model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.IsTrue(Directory.Exists(f.Combine(RepositorySetup.ProjectName, ".hg")));
			}
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void MakeClone_TargetExists_Throws()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TempFolder("clonetest"))
			{
				var model = new Cloner();
				var progress = new ConsoleProgress();
				progress.ShowVerbose = true;
				Directory.CreateDirectory(f.Combine(RepositorySetup.ProjectName));
				model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_NoDrivesFound_ReturnsEmptyList()
		{
				var model = new Cloner();
				var drives = new List<IUsbDriveInfo>();
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(0, model.GetDirectoriesWithMecurialRepos().Count());
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_OneDriveAndDirectoryButNotRep_ReturnsEmptyList()
		{
			using (var usb = new TempFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("tests"));
				var model = new Cloner();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(0, model.GetDirectoriesWithMecurialRepos().Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_OneDriveOneRepo_ReturnsRepoPath()
		{
			using (var usb = new TempFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("test"));
				Directory.CreateDirectory(usb.Combine("testrepo",".hg"));
				var model = new Cloner();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(1, model.GetDirectoriesWithMecurialRepos().Count());
				Assert.AreEqual(usb.Combine("testrepo"), model.GetDirectoriesWithMecurialRepos().First());
			}
		}


		[Test]
		public void GetDirectoriesWithMecurialRepos_TwoRepos_ReturnsOnlyUnfilteredPath()
		{
			using (var usb = new TempFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("test1"));
				Directory.CreateDirectory(usb.Combine("test1", ".hg"));
				Directory.CreateDirectory(usb.Combine("testSKIP"));
				Directory.CreateDirectory(usb.Combine("testSKIP", ".hg"));
				var model = new Cloner();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				model.ProjectFilter = path => !path.Contains("SKIP");
				Assert.AreEqual(1, model.GetDirectoriesWithMecurialRepos().Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_TwoDrivesEachWithRepo2Deep_FindsAllRepos()
		{
			using (var usb1 = new TempFolder("clonetestUsb1"))
			using (var usb2 = new TempFolder("clonetestUsb2"))
			{
				Directory.CreateDirectory(usb1.Combine("a", "repo1", ".hg"));
				Directory.CreateDirectory(usb2.Combine("a", "repo2", ".hg"));
				var model = new Cloner();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb1.Path));
				drives.Add(new UsbDriveInfoForTests(usb2.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(2, model.GetDirectoriesWithMecurialRepos().Count());
				var repos = model.GetDirectoriesWithMecurialRepos().ToArray();
				Assert.AreEqual(usb1.Combine("a", "repo1"), repos[0]);
				Assert.AreEqual(usb2.Combine("a", "repo2"), repos[1]);
			}
		}
	}
}