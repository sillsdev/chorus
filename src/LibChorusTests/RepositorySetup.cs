using System;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace LibChorus.Tests
{
	/// <summary>
	/// Provides temporary directories and repositories.
	/// </summary>
	public class RepositorySetup :IDisposable
	{
		private readonly StringBuilderProgress _stringBuilderProgress = new StringBuilderProgress();
		private IProgress _progress;
		public TempFolder RootFolder;
		public TempFolder ProjectFolder;
		public ProjectFolderConfiguration ProjectFolderConfig;

		private void Init(string name)
		{
			Progress = new MultiProgress(new IProgress[] { new ConsoleProgress(){ShowVerbose=true}, _stringBuilderProgress });
			RootFolder = new TempFolder("ChorusTest-" + name);
		}

		public RepositorySetup(string userName)
		{
			Init(userName);

			ProjectFolder = new TempFolder(RootFolder, ProjectName);

			RepositorySetup.MakeRepositoryForTest(ProjectFolder.Path, userName,Progress);
			ProjectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);

		}


		public RepositorySetup(string cloneName, RepositorySetup sourceToClone)
		{
			Init(cloneName);
			string pathToProject = RootFolder.Combine(ProjectName);
			ProjectFolderConfig = sourceToClone.ProjectFolderConfig.Clone();
			ProjectFolderConfig.FolderPath = pathToProject;

			sourceToClone.CreateSynchronizer().MakeClone(pathToProject, true);
			ProjectFolder = TempFolder.TrackExisting(RootFolder.Combine(ProjectName));

			var hg = new HgRepository(pathToProject, Progress);
			hg.SetUserNameInIni(cloneName, Progress);

		}

		public string GetProgressString()
		{
			return _stringBuilderProgress.ToString();
		}

		public Synchronizer CreateSynchronizer()
		{
			return Synchronizer.FromProjectConfiguration(ProjectFolderConfig, Progress);
		}


		public HgRepository Repository
		{
			get { return new HgRepository(ProjectFolderConfig.FolderPath, Progress); }
		}
		public void Dispose()
		{
			if (Repository != null)
			{
				Assert.IsFalse(Repository.GetHasLocks(), "A lock was left over, after the test.");
			}
			ProjectFolder.Dispose();
			RootFolder.Dispose();
		}

		public void WriteIniContents(string s)
		{
			File.WriteAllText(PathToHgrc, s);
		}

		private string PathToHgrc
		{
			get { return Path.Combine(Path.Combine(ProjectFolder.Path, ".hg"), "hgrc"); }
		}

		public void EnsureNoHgrcExists()
		{
			if (File.Exists(PathToHgrc))
				File.Delete(PathToHgrc);
		}

		public void AddAndCheckinFile(string fileName, string contents)
		{
			var p = ProjectFolder.Combine(fileName);
			File.WriteAllText(p, contents);
			Repository.AddAndCheckinFile(p);
		}

		public void AddAndCheckIn()
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = false;
			options.DoPullFromOthers = false;
			options.DoSendToOthers = false;

			CreateSynchronizer().SyncNow(options);
		}

		public SyncResults CheckinAndPullAndMerge(RepositorySetup otherUser)
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoPullFromOthers = true;
			options.DoSendToOthers = true;

			options.RepositorySourcesToTry.Add(otherUser.GetRepositoryAddress());
			return CreateSynchronizer().SyncNow(options);
		}

		public RepositoryAddress GetRepositoryAddress()
		{
			var x =   RepositoryAddress.Create("unknownname", ProjectFolder.Path, false);
			x.Enabled = true;
			return x;
		}

		public void AssertFileExistsRelativeToRoot(string relativePath)
		{
			Assert.IsTrue(File.Exists(RootFolder.Combine(relativePath)));
		}

		public void AssertFileExistsInRepository(string pathRelativeToRepositoryRoot)
		{
			Assert.IsTrue(Repository.GetFileExistsInRepo(pathRelativeToRepositoryRoot));
		}

		public void AssertFileDoesNotExistInRepository(string pathRelativeToRepositoryRoot)
		{
			Assert.IsFalse(Repository.GetFileExistsInRepo(pathRelativeToRepositoryRoot));
		}

		public static void MakeRepositoryForTest(string newRepositoryPath, string userId, IProgress progress)
		{
			HgRepository.CreateRepositoryInExistingDir(newRepositoryPath,progress);
			var hg = new HgRepository(newRepositoryPath, progress);
			hg.SetUserNameInIni(userId,  progress);
		}


		public static string ProjectName
		{
			get { return "foo project"; }//nb: important that it have a space, as this helps catch failure to enclose in quotes
		}

		public IProgress Progress
		{
			get { return _progress; }
			set { _progress = value; }
		}

		public IDisposable GetFileLockForReading(string localPath)
		{
			return new StreamWriter(ProjectFolder.Combine(localPath));
		}
		public IDisposable GetFileLockForWriting(string localPath)
		{
#if MONO
			// This doesn't work.  A mono bug perhaps? (CP)
			FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
			// This didn't work either
			//f.Lock(0, f.Length - 1);
			//FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Open, FileAccess.Write, FileShare.None);
			// This locked the file, but also deleted it (as expected) which isn't what the test expects
			//FileStream f = new FileStream(ProjectFolder.Combine(localPath), FileMode.Create, FileAccess.Write, FileShare.None);
			return f;
#else
			return new StreamReader(ProjectFolder.Combine(localPath));
#endif
		}


		public void AssertSingleHead()
		{
			var actual = Repository.GetHeads().Count;
			Assert.AreEqual(1, actual, "There should be on only one head, but there are " + actual.ToString());
		}

		public void AssertHeadCount(int count)
		{
			var actual = Repository.GetHeads().Count;
			Assert.AreEqual(count, actual, "Wrong number of heads");
		}

		public void AssertFileExists(string relativePath)
		{
			Assert.IsTrue(File.Exists(ProjectFolder.Combine(relativePath)));
		}

		public void AssertFileContents(string relativePath, string expectedContents)
		{
			Assert.AreEqual(expectedContents, File.ReadAllText(ProjectFolder.Combine(relativePath)));
		}

		/// <summary>
		/// Obviously, don't leave this in a unit test... it's only for debugging
		/// </summary>
		public void ShowInTortoise()
		{
			var start = new System.Diagnostics.ProcessStartInfo("hgtk", "log");
			start.WorkingDirectory = ProjectFolder.Path;
			System.Diagnostics.Process.Start(start);
		}
	}

}