using System;
using System.IO;
using Chorus.sync;
using Chorus.Utilities;

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
		public RepositoryManager Repo;
		public RepositoryPath RepoPath;


		public EmptyRepositorySetup()
		{

			var userName = "Dan";

			RootFolder = new TempFolder("ChorusTest-"+userName);
			ProjectFolder = new TempFolder(RootFolder, "foo project");

			RepositoryManager.MakeRepositoryForTest(ProjectFolder.Path, userName);
			var projectFolderConfig = new ProjectFolderConfiguration(ProjectFolder.Path);
			Repo = RepositoryManager.FromRootOrChildFolder(projectFolderConfig);
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
	}
}