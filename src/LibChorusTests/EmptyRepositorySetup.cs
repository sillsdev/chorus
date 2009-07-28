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
		public RepositoryManager RepoMan;
		public RepositoryAddress RepoPath;
		public ProjectFolderConfiguration ProjectFolderConfig;


		public EmptyRepositorySetup()
		{

			var userName = "Dan";

			RootFolder = new TempFolder("ChorusTest-"+userName);
			ProjectFolder = new TempFolder(RootFolder, "foo project");

			RepositoryManager.MakeRepositoryForTest(ProjectFolder.Path, userName);
			ProjectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);
			RepoMan = RepositoryManager.FromRootOrChildFolder(ProjectFolderConfig);
		}


		public HgRepository Repository
		{
			get { return RepoMan.Repository; }
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

			RepoMan.SyncNow(options, _progress);
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
	}
}