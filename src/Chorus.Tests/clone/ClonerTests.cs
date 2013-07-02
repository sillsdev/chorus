using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Chorus.UI.Clone;
using Chorus.Utilities.UsbDrive;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests.clone
{
	[TestFixture]
	public class ClonerTests
	{
		[Test]
		public void MakeClone_NoProblems_MakesClone()
		{
			using(var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new CloneFromUsb();
				var progress = new ConsoleProgress();
				progress.ShowVerbose = true;
				model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.IsTrue(Directory.Exists(f.Combine(RepositorySetup.ProjectName, ".hg")));
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void MakeClone_TargetExists_CreatesCloneInAnotherFolder()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new CloneFromUsb();
				var progress = new ConsoleProgress();
				progress.ShowVerbose = true;
				var extantFolder = f.Combine(RepositorySetup.ProjectName);
				Directory.CreateDirectory(extantFolder);
				// Make a subfolder, which will force it to make a new folder, since an empty folder is deleted.
				var extantSubfolderPath = Path.Combine(extantFolder, "ChildFolder");
				Directory.CreateDirectory(extantSubfolderPath);

				var cloneFolder = model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.AreEqual(extantFolder + "1", cloneFolder);
				Assert.IsTrue(Directory.Exists(extantFolder + "1"));
			}
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void MakeClone_TargetExists_CreatesCloneInWhenTargetIsEmpty()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new CloneFromUsb();
				var progress = new ConsoleProgress();
				progress.ShowVerbose = true;
				var extantFolder = f.Combine(RepositorySetup.ProjectName);
				Directory.CreateDirectory(extantFolder);

				var cloneFolder = model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.AreEqual(extantFolder, cloneFolder);
				Assert.IsTrue(Directory.Exists(extantFolder));
				Assert.IsFalse(Directory.Exists(extantFolder + "1"));
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_NoDrivesFound_ReturnsEmptyList()
		{
				var model = new CloneFromUsb();
				var drives = new List<IUsbDriveInfo>();
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(0, model.GetDirectoriesWithMecurialRepos().Count());
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_OneDriveAndDirectoryButNotRep_ReturnsEmptyList()
		{
			using (var usb = new TemporaryFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("tests"));
				var model = new CloneFromUsb();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(0, model.GetDirectoriesWithMecurialRepos().Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_OneDriveOneRepo_ReturnsRepoPath()
		{
			using (var usb = new TemporaryFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("test"));
				Directory.CreateDirectory(usb.Combine("testrepo",".hg"));
				var model = new CloneFromUsb();
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
			using (var usb = new TemporaryFolder("clonetestUsb"))
			{
				Directory.CreateDirectory(usb.Combine("test1"));
				Directory.CreateDirectory(usb.Combine("test1", ".hg"));
				Directory.CreateDirectory(usb.Combine("testSKIP"));
				Directory.CreateDirectory(usb.Combine("testSKIP", ".hg"));
				var model = new CloneFromUsb();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				model.ProjectFilter = path => !path.Contains("SKIP");
				Assert.AreEqual(1, model.GetDirectoriesWithMecurialRepos().Count());
			}
		}

		// As of 12 December, 2011, JohnH and I decided to remove the search at the second level.
		// Seems that will work, but then the next attempt to sync, will not be able to find the second level repo.
		//[Test]
		//public void GetDirectoriesWithMecurialRepos_TwoDrivesEachWithRepo2Deep_FindsAllRepos()
		//{
		//    using (var usb1 = new TemporaryFolder("clonetestUsb1"))
		//    using (var usb2 = new TemporaryFolder("clonetestUsb2"))
		//    {
		//        Directory.CreateDirectory(usb1.Combine("a", "repo1", ".hg"));
		//        Directory.CreateDirectory(usb2.Combine("a", "repo2", ".hg"));
		//        var model = new CloneFromUsb();
		//        var drives = new List<IUsbDriveInfo>();
		//        drives.Add(new UsbDriveInfoForTests(usb1.Path));
		//        drives.Add(new UsbDriveInfoForTests(usb2.Path));
		//        model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
		//        Assert.AreEqual(2, model.GetDirectoriesWithMecurialRepos().Count());
		//        var repos = model.GetDirectoriesWithMecurialRepos().ToArray();
		//        Assert.AreEqual(usb1.Combine("a", "repo1"), repos[0]);
		//        Assert.AreEqual(usb2.Combine("a", "repo2"), repos[1]);
		//    }
		//}

		[Test]
		public void GetDirectoriesWithMecurialRepos_WithRepo2DeepIsNotFound()
		{
			using (var usb1 = new TemporaryFolder("clonetestUsb1"))
			{
				Directory.CreateDirectory(usb1.Combine("a", "repo1", ".hg"));
				var model = new CloneFromUsb();
				var drives = new List<IUsbDriveInfo>();
				drives.Add(new UsbDriveInfoForTests(usb1.Path));
				model.DriveInfoRetriever = new RetrieveUsbDriveInfoForTests(drives);
				Assert.AreEqual(0, model.GetDirectoriesWithMecurialRepos().Count());
			}
		}

		[Test]
		public void MakeListItem_MakesNormalItem()
		{
			using (var usb = new TemporaryFolder("clonetestUsb"))
			{
				var path = usb.Combine("test1");
				Directory.CreateDirectory(path);
				Directory.CreateDirectory(usb.Combine("test1", ".hg"));
				var model = new CloneFromUsb();
				var item = model.CreateListItemFor(path);
				Assert.That(item, Is.Not.Null, "model should have made a list item");
				Assert.That(item.Text, Is.EqualTo("test1"));
				Assert.That(item.Tag, Is.EqualTo(path));
				var last = File.GetLastWriteTime(path);
				string expectedSubitem = last.ToShortDateString() + " " + last.ToShortTimeString(); // Not a great test, basically duplicates the impl
				Assert.That(item.SubItems[1].Text, Is.EqualTo(expectedSubitem));
				Assert.That(item.ToolTipText, Is.EqualTo(path));
				Assert.That(item.ImageIndex, Is.EqualTo(0));
				Assert.That(item.BackColor, Is.EqualTo(Color.FromKnownColor(KnownColor.Window)));
				Assert.That(item.ForeColor, Is.EqualTo(Color.FromKnownColor(KnownColor.WindowText)));
			}
		}

		[Test]
		public void MakeListItemWhenAlreadyHaveProjectName_MakesDisabledItem()
		{
			using (var usb = new TemporaryFolder("clonetestUsb"))
			{
				var path = usb.Combine("test1");
				Directory.CreateDirectory(path);
				Directory.CreateDirectory(usb.Combine("test1", ".hg"));
				var model = new CloneFromUsb();
				model.ExistingProjects = new HashSet<string> {"test1"};
				var item = model.CreateListItemFor(path);
				Assert.That(item, Is.Not.Null, "model should have made a list item");
				Assert.That(item.Text, Is.EqualTo("test1"));
				Assert.That(item.Tag, Is.EqualTo(path));
				var last = File.GetLastWriteTime(path);
				string expectedSubitem = last.ToShortDateString() + " " + last.ToShortTimeString();
					// Not a great test, basically duplicates the impl
				Assert.That(item.SubItems[1].Text, Is.EqualTo(expectedSubitem));
				Assert.That(item.ToolTipText, Is.EqualTo(CloneFromUsb.ProjectWithSameNameExists));
				Assert.That(item.ImageIndex, Is.EqualTo(2));
				Assert.That(item.ForeColor, Is.EqualTo(CloneFromUsb.DisabledItemForeColor));
			}
		}

		[Test]
		public void MakeListItemWhenRepoInUse_MakesDisabledItem()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				// Set a project up sufficiently to have an ID.
				var tempFilePath = setup.ProjectFolder.Combine("test.1w1");
				File.WriteAllText(tempFilePath, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.1w1");
				setup.AddAndCheckIn(); // Need to have one commit.
				var id = setup.Repository.Identifier;
				var path = setup.Repository.PathToRepo;

				var model = new CloneFromUsb();
				model.ReposInUse = new Dictionary<string, string>();
				model.ReposInUse[id] = "myname";
				var item = model.CreateListItemFor(path);
				Assert.That(item, Is.Not.Null, "model should have made a list item");
				Assert.That(item.Text, Is.EqualTo(Path.GetFileName(path)));
				Assert.That(item.Tag, Is.EqualTo(path));
				var last = File.GetLastWriteTime(path);
				string expectedSubitem = last.ToShortDateString() + " " + last.ToShortTimeString();
				// Not a great test, basically duplicates the impl
				Assert.That(item.SubItems[1].Text, Is.EqualTo(expectedSubitem));
				Assert.That(item.ToolTipText, Is.EqualTo(string.Format(CloneFromUsb.ProjectInUseTemplate, "myname")));
				Assert.That(item.ImageIndex, Is.EqualTo(1));
				Assert.That(item.ForeColor, Is.EqualTo(CloneFromUsb.DisabledItemForeColor));
			}
		}
	}
}