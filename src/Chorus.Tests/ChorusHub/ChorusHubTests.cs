using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Chorus.UI.Clone;
using ChorusHub;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;
using System.Linq;

namespace Chorus.Tests.ChorusHub
{
	[TestFixture]
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
															Assert.AreEqual(0, client.GetRepositoryNames().Count());
														});
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

				using (var service = new ChorusHubService(chorusHubSourceFolder.Path))
				{
					service.Start(false/* hg server plays no role in telling us what repositories are available */);
					var client = new ChorusHubClient();
					Assert.NotNull(client.FindServer());
					IEnumerable<string> repositoryNames = client.GetRepositoryNames();
					Assert.AreEqual(2, repositoryNames.Count());
					Assert.IsTrue(repositoryNames.Contains("repo1"));
					Assert.IsTrue(repositoryNames.Contains("repo2"));
				}
			}
		}
	}
}