using System;
using System.Collections.Generic;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
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
			// only accept folders containing a file with the name "somename.repoType"
			const string searchFilter = "fileExtension=repoType";
			// TODO: Test needs more work

			using (var testRoot = new TemporaryFolder("ChorusHubCloneTest"))
			using (var chorusHubSourceFolder = new TemporaryFolder(testRoot, "ChorusHub"))
			using (var repo1 = new TemporaryFolder(chorusHubSourceFolder, "repo1"))
			using (var repo2 = new TemporaryFolder(chorusHubSourceFolder, "filterthisoneout"))
			{
				RepositorySetup.MakeRepositoryForTest(repo1.Path, "bob", new ConsoleProgress());
				RepositorySetup.MakeRepositoryForTest(repo2.Path, "bob", new ConsoleProgress());

				using (var service = new ChorusHubService(new ChorusHubParameters() { RootDirectory = chorusHubSourceFolder.Path }))
				{
					service.Start(true);
					var client = new ChorusHubClient();
					Assert.NotNull(client.FindServer());
					// Make sure all repos are there first
					IEnumerable<string> allRepoNames = client.GetRepositoryNames(string.Empty);
					Assert.AreEqual(2, allRepoNames.Count());
					Assert.IsTrue(allRepoNames.Contains("filterthisoneout"));
					// Make sure filter works
					IEnumerable<string> repositoryNames = client.GetRepositoryNames(searchFilter);
					Assert.AreEqual(1, repositoryNames.Count());
					Assert.IsTrue(repositoryNames.Contains("repo1"));
					Assert.IsFalse(repositoryNames.Contains("filterthisoneout"));
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