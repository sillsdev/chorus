using System;
using System.IO;
using System.Linq;
using Chorus.ChorusHub;
using Chorus.VcsDrivers.Mercurial;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace ChorusHubTests
{
	[TestFixture]
	[System.ComponentModel.Category("KnownMonoIssue")] // cross-process comms doesn't work in mono.
	public class ChorusHubClientTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test, Ignore("Run by hand only, after the service is stopped")]
		public void FindServer_NoServerFound_Null()
		{
			Assert.IsNull(ChorusHubServerInfo.FindServerInformation());
		}

		[Test]
		public void GetRepostoryNames_EmptyHubFolder_EmptyList()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
		   {
			   var chorusHubServerInfo = ChorusHubServerInfo.FindServerInformation();
			   Assert.IsNull(chorusHubServerInfo);
				//normally we shouldn't call this if none was found
				Assert.Throws<ApplicationException>(()=>
				{
					var client = new ChorusHubClient(null);
					Assert.AreEqual(0, client.GetRepositoryInformation(string.Empty).Count());
				});
			}
		}

		[Test, Ignore("Run by hand only, after the service is running")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetOneItemWithProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.someExt"
			const string queryString = "filePattern=*.someExt";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
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

				ChorusHubParameters.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubService())
				{
					service.Start(true);

					ChorusHubServerInfo.ClearServerInfoForTests();
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

		[Test, Ignore("Run by hand only, after the service is running")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetTwoItems()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
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

				ChorusHubParameters.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubService())
				{
					// hg server side is now involved in deciding what repos are available
					service.Start(true);
					ChorusHubServerInfo.ClearServerInfoForTests();
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

		[Test, Ignore("Run by hand only, after the service is running")]
		public void GetRepostoryNames_ThreeItemsInHubFolder_GetTwoItemsWithProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.ext1" or "randomName.Ext2"
			const string queryString = @"filePattern=*.ext1|*.Ext2";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
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

				ChorusHubParameters.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubService())
				{
					service.Start(true);

					// Make sure filter works
					ChorusHubServerInfo.ClearServerInfoForTests();
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

		[Test, Ignore("Run by hand only, after the service is running")]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetNoneForProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.ext1"
			// but there aren't any.
			const string queryString = "filePattern=*.ext1";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
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
				ChorusHubParameters.RootDirectory = chorusHubSourceFolder.Path;
				using (var service = new ChorusHubService())
				{
					service.Start(true);

					// Make sure filter works
					ChorusHubServerInfo.ClearServerInfoForTests();
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