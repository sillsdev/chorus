using System;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests
{
	/// <summary>
	/// Provides temporary directories and repositories.
	/// </summary>
	public class EmptyRepositorySetup :IDisposable
	{
		private StringBuilderProgress _progress = new StringBuilderProgress();
		public TempFolder RootFolder;
		public TempFolder ProjectFolder;
		public Synchronizer Synchronizer;
		public RepositoryAddress RepoPath;
		public ProjectFolderConfiguration ProjectFolderConfig;


		public EmptyRepositorySetup()
		{

			var userName = "Dan";

			RootFolder = new TempFolder("ChorusTest-"+userName);
			ProjectFolder = new TempFolder(RootFolder, "foo project");

			EmptyRepositorySetup.MakeRepositoryForTest(ProjectFolder.Path, userName);
			ProjectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);
			Synchronizer = Synchronizer.FromProjectConfiguration(ProjectFolderConfig, new NullProgress());
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

			Synchronizer.SyncNow(options, _progress);
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

		public static void MakeRepositoryForTest(string newRepositoryPath, string userId)
		{
			HgRepository.CreateRepositoryInExistingDir(newRepositoryPath);
			var hg = new HgRepository(newRepositoryPath, new NullProgress());
			hg.SetUserNameInIni(userId, new NullProgress());
		}
	}
}