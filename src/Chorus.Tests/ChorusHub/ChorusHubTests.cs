using System;
using System.Collections.Generic;
using System.IO;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;
using System.Linq;

namespace Chorus.Tests.ChorusHub
{
	[TestFixture]
	[Category("KnownMonoIssue")] // cross-process comms doesn't work in mono.
	public class ChorusHubClientTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[Test]
		public void FindServer_NoServerFound_Null()
		{
			var client = new ChorusHubClient();
			Assert.IsNull(client.FindServer());
		}

		[Test]
		public void GetRepostoryNames_EmptyHubFolder_EmptyList()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
		   {
				var client = new ChorusHubClient();
				Assert.IsNull(client.FindServer());
				//normally we shouldn't call this if none was found
				Assert.Throws<ApplicationException>(()=>
					{
						Assert.AreEqual(0, client.GetRepositoryNames(string.Empty).Count());
					});
			}
		}

		[Test]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetOneItemWithProjectFilter()
		{
			// only accept folders containing a file with the name "randomName.someExt"
			const string queryString = "fileExtension=someExt";

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var tempFile = TempFile.WithExtension("someExt"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			{
				tempFile.MoveTo(Path.Combine(repo1.Path, Path.GetFileName(tempFile.Path)));
				using (var writer = new StreamWriter(tempFile.Path))
					writer.Write("Some random text.");

				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				using (var writer = new StreamWriter(tempFile.Path))
					writer.Write("Another line of random text.");

				using (var service = new ChorusHubService(new ChorusHubParameters() { RootDirectory = chorusHubSourceFolder.Path }))
				{
					service.Start(true);

					// Make sure all repos are there first
					var client1 = new ChorusHubClient();
					Assert.NotNull(client1.FindServer());
					IEnumerable<string> allRepoNames = client1.GetRepositoryNames(string.Empty);
					Assert.AreEqual(2, allRepoNames.Count());
					Assert.IsTrue(allRepoNames.Contains("repo2"));

					// Make sure filter works
					// In order to have a hope of getting a different result to GetRepositoryNames
					// we have to start over with a new client
					var client2 = new ChorusHubClient();
					Assert.NotNull(client2.FindServer());
					IEnumerable<string> repositoryNames = client2.GetRepositoryNames(queryString);
					Assert.AreEqual(1, repositoryNames.Count());
					Assert.IsTrue(repositoryNames.Contains("repo1"));
					Assert.IsFalse(repositoryNames.Contains("repo2"));
				}
			}
		}

		[Test]
		public void GetRepostoryNames_TwoItemsInHubFolder_GetTwoItems()
		{
			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "repo2"))
			{
				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				using (var service = new ChorusHubService(new ChorusHubParameters(){RootDirectory = chorusHubSourceFolder.Path}))
				{
					// hg server side is now involved in deciding what repos are available
					service.Start(true);
					var client = new ChorusHubClient();
					Assert.NotNull(client.FindServer());
					IEnumerable<string> repositoryNames = client.GetRepositoryNames(string.Empty);
					Assert.AreEqual(2, repositoryNames.Count());
					Assert.IsTrue(repositoryNames.Contains("repo1"));
					Assert.IsTrue(repositoryNames.Contains("repo2"));
				}
			}
		}
	}
}