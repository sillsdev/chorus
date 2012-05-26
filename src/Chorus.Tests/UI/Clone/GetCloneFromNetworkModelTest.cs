using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.UI.Clone;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace Chorus.Tests.UI.Clone
{
	[TestFixture]
	public class GetCloneFromNetworkModelTest
	{
		[Test]
		public void MakeClone_NoProblems_MakesClone()
		{
			using(var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromNetworkFolderModel(f.Path);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
				model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.IsTrue(Directory.Exists(f.Combine(RepositorySetup.ProjectName, ".hg")));
			}
		}

		[Test]
		public void MakeClone_TargetExists_CreatesCloneInAnotherFolder()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromNetworkFolderModel(f.Path);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
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
		//[Category("SkipOnTeamCity")]
		public void MakeClone_TargetExists_CreatesCloneInWhenTargetIsEmpty()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromNetworkFolderModel(f.Path);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
				var extantFolder = f.Combine(RepositorySetup.ProjectName);
				Directory.CreateDirectory(extantFolder);

				var cloneFolder = model.MakeClone(repo.ProjectFolder.Path, f.Path, progress);
				Assert.AreEqual(extantFolder, cloneFolder);
				Assert.IsTrue(Directory.Exists(extantFolder));
				Assert.IsFalse(Directory.Exists(extantFolder + "1"));
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_NoDirectories_ReturnsEmptyList()
		{
			using (var f = new TemporaryFolder("clonetest"))
			{
				var model = new GetCloneFromNetworkFolderModel(f.Path);
				List<string> nextFolders;
				Assert.AreEqual(0, model.GetRepositoriesAndNextLevelSearchFolders(new List<string>{f.Path}, out nextFolders, -1).Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_SeveralEmptyDirectories_ReturnsEmptyList()
		{
			using (var f = new TemporaryFolder("clonetest"))
			{
				MakeFolderTree(f.Path, "Folders", 4, 3, 4);
				var model = new GetCloneFromNetworkFolderModel(f.Path);
				List<string> nextFolders;
				var repoList = model.GetRepositoriesAndNextLevelSearchFolders(new List<string> { f.Path }, out nextFolders, -1);
				Assert.AreEqual(0, repoList.Count());
				Assert.AreEqual(0, nextFolders.Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_SeveralDirectoriesOneBuriedRepo_ReturnsListOfOne()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				MakeFolderTree(f.Path, "Folders", 4, 4, 4);
				var clonePath = Path.Combine(Path.Combine(Path.Combine(f.Path, "Folders_4"), "Folders_4_4"), "Folders_4_4_4");
				var model = new GetCloneFromNetworkFolderModel(clonePath);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
				model.MakeClone(repo.ProjectFolder.Path, clonePath, progress);
				List<string> nextFolders;
				var repoList = model.GetRepositoriesAndNextLevelSearchFolders(new List<string> {f.Path}, out nextFolders, -1);
				Assert.AreEqual(1, repoList.Count());
				Assert.AreEqual(0, nextFolders.Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_SeveralDirectoriesOneBuriedRepoLimitedRecursion_ReturnsEmptyList()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				MakeFolderTree(f.Path, "Folders", 4, 5, 6);
				var clonePath = Path.Combine(Path.Combine(Path.Combine(f.Path, "Folders_4"), "Folders_4_4"), "Folders_4_4_4");
				var model = new GetCloneFromNetworkFolderModel(clonePath);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
				model.MakeClone(repo.ProjectFolder.Path, clonePath, progress);
				List<string> nextFolders;
				var repoList = model.GetRepositoriesAndNextLevelSearchFolders(new List<string> { f.Path }, out nextFolders, 2);
				Assert.AreEqual(0, repoList.Count());
				Assert.AreEqual(4 * 5 * 6, nextFolders.Count());
			}
		}

		[Test]
		public void GetDirectoriesWithMecurialRepos_SeveralDirectoriesOneBuriedRepoLimitedRecursion_ReturnsListOfOne()
		{
			using (var repo = new RepositorySetup("source"))
			using (var f = new TemporaryFolder("clonetest"))
			{
				MakeFolderTree(f.Path, "Folders", 4, 4, 4);
				var clonePath = Path.Combine(Path.Combine(Path.Combine(f.Path, "Folders_4"), "Folders_4_4"), "Folders_4_4_4");
				var model = new GetCloneFromNetworkFolderModel(clonePath);
				var progress = new ConsoleProgress
								{
									ShowVerbose = true
								};
				model.MakeClone(repo.ProjectFolder.Path, clonePath, progress);
				List<string> nextFolders;
				var repoList = model.GetRepositoriesAndNextLevelSearchFolders(new List<string> { f.Path }, out nextFolders, 4);
				Assert.AreEqual(1, repoList.Count());
				Assert.AreEqual(0, nextFolders.Count());
			}
		}

		/// <summary>
		/// Create a tree of subfolders from initialPath.
		/// </summary>
		/// <param name="initialPath">Parent folder to begin in</param>
		/// <param name="nameHeader">Text to put at start of each folder's name</param>
		/// <param name="treeStructure">Specifies how many subfolders to create at each level in each subfolder.</param>
		private static void MakeFolderTree(string initialPath, string nameHeader, params int[] treeStructure)
		{
			if (treeStructure.Length == 0)
				return;

			var remainingStructure = new int[treeStructure.Length - 1];
			for (var i = 1; i < treeStructure.Length; i++)
				remainingStructure[i - 1] = treeStructure[i];

			var numFoldersToCreate = treeStructure[0];
			for (int i = 1; i <= numFoldersToCreate; i++)
			{
				var name = nameHeader + "_" + i;
				var newPath = Path.Combine(initialPath, name);
				Directory.CreateDirectory(newPath);
				MakeFolderTree(newPath, name, remainingStructure);
			}
		}
	}
}
