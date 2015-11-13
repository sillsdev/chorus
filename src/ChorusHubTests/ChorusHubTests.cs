using System;
using System.IO;
using System.Linq;
using Chorus;
using Chorus.ChorusHub;
using Chorus.VcsDrivers.Mercurial;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.Progress;
using SIL.TestUtilities;

namespace ChorusHubTests
{
	[TestFixture]
	[NUnit.Framework.Category("KnownMonoIssue")] // cross-process comms doesn't work in mono.
	public class ChorusHubClientTests
	{
		private string _originalPath;

		[SetUp]
		public void Setup()
		{
			_originalPath = ChorusHubOptions.RootDirectory;
			ChorusHubServerInfo.ClearServerInfoForTests();
		}

		[TearDown]
		public void TestTeardown()
		{
			ChorusHubOptions.RootDirectory = _originalPath;
			if (!Directory.GetDirectories(_originalPath, "*.*").Any() && !Directory.GetFiles(_originalPath, "*.*").Any())
				Directory.Delete(_originalPath);
		}

		[Test, Ignore("Run by hand.")]
		public void FindServer_NoServerFound_Null()
		{
			Assert.IsNull(ChorusHubServerInfo.FindServerInformation());
		}

		[Test]
		public void NullServerInfoThrowsOnCreateChorusHubClientAttempt()
		{
			Assert.Throws<ArgumentNullException>(() => new ChorusHubClient(null));
		}

		[Test, Ignore("Run by hand.")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetOneItemWithProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.someExt"
			const string queryString = "filePattern=*.someExt";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest_" + Guid.NewGuid()))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var tempFile = TempFile.WithExtension("someExt"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			using (var tempFile2 = TempFile.WithExtension("someOtherExt"))
			{
				tempFile.MoveTo(Path.Combine(repo1.Path, Path.GetFileName(tempFile.Path)));
				using (var writer = new StreamWriter(tempFile.Path))
					writer.Write("Some random text.");
				tempFile2.MoveTo(Path.Combine(repo2.Path, Path.GetFileName(tempFile2.Path)));

				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				var r1 = HgRepository.CreateOrUseExisting(repo1.Path, new ConsoleProgress());
				r1.AddAndCheckinFile(tempFile.Path); // need this to create store/data/files
				var r2 = HgRepository.CreateOrUseExisting(repo2.Path, new ConsoleProgress());
				r2.AddAndCheckinFile(tempFile2.Path); // need this to create store/data/files

				ChorusHubOptions.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubServer())
				{
					Assert.IsTrue(service.Start(true));

					var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
					Assert.NotNull(chorusHubServerInfo);
					// Make sure all repos are there first
					var client1 = new ChorusHubClient(chorusHubServerInfo);
					var allRepoInfo = client1.GetRepositoryInformation(string.Empty);
					Assert.AreEqual(2, allRepoInfo.Count());
					Assert.IsTrue(allRepoInfo.Select(ri => ri.RepoName.Contains("repo2")).Any());

					// Make sure filter works
					// In order to have a hope of getting a different result to GetRepositoryInformation
					// we have to start over with a new client
					chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
					Assert.NotNull(ChorusHubServerInfo.FindServerInformation());
					var client2 = new ChorusHubClient(chorusHubServerInfo);
					var repoInfo = client2.GetRepositoryInformation(queryString);
					Assert.AreEqual(1, repoInfo.Count());
					var info = repoInfo.First();
					Assert.IsTrue(info.RepoName == "repo1");
				}
			}
		}

		[Test, Ignore("Run by hand.")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetTwoItems()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest_" + Guid.NewGuid()))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var tempFile1 = TempFile.WithExtension("ext1"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			using (var tempFile2 = TempFile.WithExtension("ext2"))
			{
				tempFile1.MoveTo(Path.Combine(repo1.Path, Path.GetFileName(tempFile1.Path)));
				tempFile2.MoveTo(Path.Combine(repo2.Path, Path.GetFileName(tempFile2.Path)));
				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				var r1 = HgRepository.CreateOrUseExisting(repo1.Path, new ConsoleProgress());
				r1.AddAndCheckinFile(tempFile1.Path); // need this to create store/data/files
				var r2 = HgRepository.CreateOrUseExisting(repo2.Path, new ConsoleProgress());
				r2.AddAndCheckinFile(tempFile2.Path); // need this to create store/data/files

				ChorusHubOptions.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubServer())
				{
					// hg server side is now involved in deciding what repos are available
					Assert.IsTrue(service.Start(true));
					var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
					Assert.NotNull(chorusHubServerInfo);
					var client = new ChorusHubClient(chorusHubServerInfo);
					var repoInfo = client.GetRepositoryInformation(string.Empty);
					Assert.AreEqual(2, repoInfo.Count());
					var info1 = repoInfo.First();
					var info2 = repoInfo.Last();
					Assert.IsTrue(info1.RepoName == "repo1");
					Assert.IsTrue(info2.RepoName == "repo2");
				}
			}
		}

		[Test, Ignore("Run by hand.")]
		public void GetRepostoryNames_ThreeItemsInHubFolder_GetTwoItemsWithProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.ext1" or "randomName.Ext2"
			const string queryString = @"filePattern=*.ext1|*.Ext2";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest_" + Guid.NewGuid()))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var tempFile1 = TempFile.WithExtension("ext1"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			using (var tempFile2 = TempFile.WithExtension("unwanted"))
			using (var repo3 = new TemporaryFolder(chorusHubSourceFolder, "repo3"))
			using (var tempFile3 = TempFile.WithExtension("Ext2"))
			{
				tempFile1.MoveTo(Path.Combine(repo1.Path, Path.GetFileName(tempFile1.Path)));
				using (var writer = new StreamWriter(tempFile1.Path))
					writer.Write("Some random text.");

				tempFile2.MoveTo(Path.Combine(repo2.Path, Path.GetFileName(tempFile2.Path)));
				tempFile3.MoveTo(Path.Combine(repo3.Path, Path.GetFileName(tempFile3.Path)));

				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo3.Path, "bob", new ConsoleProgress());
				var r1 = HgRepository.CreateOrUseExisting(repo1.Path, new ConsoleProgress());
				r1.AddAndCheckinFile(tempFile1.Path); // need this to create store/data/files
				var r2 = HgRepository.CreateOrUseExisting(repo2.Path, new ConsoleProgress());
				r2.AddAndCheckinFile(tempFile2.Path); // need this to create store/data/files
				var r3 = HgRepository.CreateOrUseExisting(repo3.Path, new ConsoleProgress());
				r3.AddAndCheckinFile(tempFile3.Path); // need this to create store/data/files

				ChorusHubOptions.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubServer())
				{
					Assert.IsTrue(service.Start(true));

					// Make sure filter works
					var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
					Assert.NotNull(chorusHubServerInfo);
					var client = new ChorusHubClient(chorusHubServerInfo);
					var repoInfo = client.GetRepositoryInformation(queryString);
					Assert.AreEqual(2, repoInfo.Count());
					var info1 = repoInfo.First();
					var info2 = repoInfo.Last();
					Assert.IsTrue(info1.RepoName == "repo1");
					Assert.IsTrue(info2.RepoName == "repo3");
				}
			}
		}

		[Test, Ignore("Run by hand.")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetNoneForProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.ext1"
			// but there aren't any.
			const string queryString = "filePattern=*.ext1";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest_" + Guid.NewGuid()))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var tempFile1 = TempFile.WithExtension("other"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			using (var tempFile2 = TempFile.WithExtension("unwanted"))
			{
				tempFile1.MoveTo(Path.Combine(repo1.Path, Path.GetFileName(tempFile1.Path)));
				using (var writer = new StreamWriter(tempFile1.Path))
					writer.Write("Some random text.");
				// Does it work if the file is empty?
				tempFile2.MoveTo(Path.Combine(repo2.Path, Path.GetFileName(tempFile2.Path)));
				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());
				var r1 = HgRepository.CreateOrUseExisting(repo1.Path, new ConsoleProgress());
				r1.AddAndCheckinFile(tempFile1.Path); // need this to create store/data/files
				var r2 = HgRepository.CreateOrUseExisting(repo2.Path, new ConsoleProgress());
				r2.AddAndCheckinFile(tempFile2.Path); // need this to create store/data/files
				ChorusHubOptions.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubServer())
				{
					Assert.IsTrue(service.Start(true));

					// Make sure filter works
					var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
					Assert.NotNull(chorusHubServerInfo);
					var client = new ChorusHubClient(chorusHubServerInfo);
					var repositoryInfo = client.GetRepositoryInformation(queryString);
					Assert.AreEqual(0, repositoryInfo.Count());
				}
			}
		}
	}
}