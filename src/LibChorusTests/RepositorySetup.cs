using System;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests
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
			_progress = new MultiProgress(new IProgress[] { new ConsoleProgress(){ShowVerbose=true}, _stringBuilderProgress });
			RootFolder = new TempFolder("ChorusTest-" + name);
		}

		public RepositorySetup(string userName)
		{
			Init(userName);

			ProjectFolder = new TempFolder(RootFolder, ProjectName);

			RepositorySetup.MakeRepositoryForTest(ProjectFolder.Path, userName,_progress);
			ProjectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);

		}


		public RepositorySetup(string cloneName, RepositorySetup sourceToClone)
		{
			Init(cloneName);
			string pathToProject = RootFolder.Combine(ProjectName);
			ProjectFolderConfig = sourceToClone.ProjectFolderConfig.Clone();
			ProjectFolderConfig.FolderPath = pathToProject;

			sourceToClone.CreateSynchronizer().MakeClone(pathToProject, true, _progress);
			ProjectFolder = TempFolder.TrackExisting(RootFolder.Combine(ProjectName));

			var hg = new HgRepository(pathToProject, new NullProgress());
			hg.SetUserNameInIni(cloneName, new NullProgress());

		}

		public string GetProgressString()
		{
			return _stringBuilderProgress.ToString();
		}

		public Synchronizer CreateSynchronizer()
		{
			return Synchronizer.FromProjectConfiguration(ProjectFolderConfig, new NullProgress());
		}


		public HgRepository Repository
		{
			get { return new HgRepository(ProjectFolderConfig.FolderPath, _progress); }
		}
		public void Dispose()
		{
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
			options.DoPushToLocalSources = false;

			CreateSynchronizer().SyncNow(options, _progress);
		}

		public SyncResults CheckinAndPullAndMerge(RepositorySetup otherUser)
		{
			SyncOptions options = new SyncOptions();
			options.DoMergeWithOthers = true;
			options.DoPullFromOthers = true;
			options.DoPushToLocalSources = true;

			options.RepositorySourcesToTry.Add(otherUser.GetRepositoryAddress());
			return CreateSynchronizer().SyncNow(options, _progress);
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
			var hg = new HgRepository(newRepositoryPath, new NullProgress());
			hg.SetUserNameInIni(userId, new NullProgress());
		}


		private static string ProjectName
		{
			get { return "foo project"; }//nb: important that it have a space, as this helps catch failure to enclose in quotes
		}

		public IDisposable GetFileLockForReading(string localPath)
		{
			return new StreamWriter(ProjectFolder.Combine(localPath));
		}
		public IDisposable GetFileLockForWriting(string localPath)
		{
			return new StreamReader(ProjectFolder.Combine(localPath));
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
	}

}