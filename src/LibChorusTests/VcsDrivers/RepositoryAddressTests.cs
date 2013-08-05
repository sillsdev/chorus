using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress;

namespace LibChorus.Tests.VcsDrivers
{
	[TestFixture]
	class RepositoryAddressTests
	{
		private ChorusHubRepositorySource _source;
		private IEnumerable<RepositoryInformation> _repositoryInformations;
		private RepositoryInformation _normalRepo, _duplicateRepo, _newRepo;
		private string _chorusHubURL = "http://chorushub@127.0.0.1:5913/";
		private IProgress _progress = new NullProgress();

		[SetUp]
		public void SetUp()
		{
			// represents a normal repo
			_normalRepo = new RepositoryInformation("AnotherProjectName", "RepoId1");
			// represents a repo that already exists when a project with the same name is added
			_duplicateRepo = new RepositoryInformation("DuplicateProject", "RepoId3");
			// represents a new repo with the same name as the project being added;
			// also represents a new repo with a name derived from the name of the duplicate project
			_newRepo = new RepositoryInformation("DuplicateProject2", RepositoryInformation.NEW_REPO);

			_repositoryInformations = new List<RepositoryInformation>
				{
					new RepositoryInformation("ProjectName", "RepoId"),
					_normalRepo,
					new RepositoryInformation("DifferentProjectName", "RepoId2"),
					_duplicateRepo,
					new RepositoryInformation("DuplicateProject1", "RepoId4"),
					new RepositoryInformation("DuplicateProjectNot", RepositoryInformation.NEW_REPO),
					_newRepo
				};

			_source = new ChorusHubRepositorySource("localhost",
				_chorusHubURL + RepositoryAddress.ProjectNameVariable,
				false, _repositoryInformations);
		}

		// Allows us to access and test the private method
		private static bool InvokeIsMatchingName(RepositoryInformation repoInfo, string projectName)
		{
			return (bool)typeof(ChorusHubRepositorySource).InvokeMember("IsMatchingName", BindingFlags.DeclaredOnly |
					 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod,
					null, null, new object[] { repoInfo, projectName });
		}

		[Test]
		public void TestIsMatchingName()
		{
			string name = "RepoName";
			string id = "RepoID";
			foreach (string num in new List<string> {"1", "2", "3", "54"})
			{
				Assert.IsTrue(InvokeIsMatchingName(new RepositoryInformation(name + num, id), name));
			}
			Assert.IsFalse(InvokeIsMatchingName(new RepositoryInformation(name + "NaN", id), name));
		}

		// Allows us to access and test the private method
		private bool InvokeTryGetBestRepoMatch(
			string repoIdentifier, string projectName, out string matchName, out string warningMessage)
		{
			object[] args = new object[] {repoIdentifier, projectName, null, null};
			bool retval = (bool) typeof(ChorusHubRepositorySource).InvokeMember("TryGetBestRepoMatch",
					BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
					BindingFlags.Instance | BindingFlags.InvokeMethod, null, _source, args);
			matchName = (string) args[2];
			warningMessage = (string) args[3];
			return retval;
		}

		[Test]
		public void TestTryGetBestRepoMatch()
		{
			// output variables
			string matchName, warningMessage;

			// Case 1: Repo name and ID both match
			Assert.IsTrue(InvokeTryGetBestRepoMatch(
				_normalRepo.RepoID, _normalRepo.RepoName, out matchName, out warningMessage));
			Assert.AreEqual(_normalRepo.RepoName, matchName);
			Assert.IsNull(warningMessage);

			// Case 2: Repo ID matches, but name does not
			Assert.IsTrue(InvokeTryGetBestRepoMatch(
				_normalRepo.RepoID, "DifferentProjectName", out matchName, out warningMessage));
			Assert.AreEqual(_normalRepo.RepoName, matchName);
			Assert.IsNull(warningMessage);

			// Case 3: There is a new repo with the correct name
			Assert.IsTrue(InvokeTryGetBestRepoMatch(
				"AnyIDWillDo", _newRepo.RepoName, out matchName, out warningMessage));
			Assert.AreEqual(_newRepo.RepoName, matchName);
			Assert.IsNull(warningMessage);

			// Case 4: There is a new repo with a properly-derived name
			Assert.IsTrue(InvokeTryGetBestRepoMatch(
				"AnyIDWillDo", _duplicateRepo.RepoName, out matchName, out warningMessage));
			Assert.AreEqual(_newRepo.RepoName, matchName);
			Assert.IsNotNullOrEmpty(warningMessage);

			// Case 5: There is no matching repo
			Assert.IsFalse(InvokeTryGetBestRepoMatch(
				"DoesNotExist", "DoesNotExist", out matchName, out warningMessage));
			Assert.IsNotNullOrEmpty(warningMessage);
		}

		private class HgRepositoryProxy : HgRepository
		{
			new public string Identifier { get; private set; }

			public HgRepositoryProxy(string projectName, string repositoryIdentifier, IProgress progress)
				: base("C:\\temp\\" + projectName, progress)
			{
				Identifier = repositoryIdentifier;
			}

			public HgRepositoryProxy(RepositoryInformation info, IProgress progress)
				: base("C:\\temp\\" + info.RepoName, progress)
			{
				Identifier = info.RepoID;
			}
		}

		// TODO pH 2013.07: test same scenarios w/ GetPotentialRepoURI and CanConnect?
		//public override string GetPotentialRepoUri(string repoIdentifier, string projectName, IProgress progress)
		//public override bool CanConnect(HgRepository localRepository, string projectName, IProgress progress)
		[Test]
		public void testCanConnect()
		{
			// Case 1: Repo name and ID both match
			Assert.IsTrue(_source.CanConnect(new HgRepositoryProxy(_normalRepo, _progress),
				_normalRepo.RepoName, _progress));

			// Case 2: Repo ID matches, but name does not
			Assert.IsTrue(_source.CanConnect(new HgRepositoryProxy("DifferentRepoName", _normalRepo.RepoID, _progress),
				"DifferentProjectName", _progress));

			// Case 3: There is a new repo with the correct name
			Assert.IsTrue(_source.CanConnect(new HgRepositoryProxy(_newRepo.RepoName, "AnyIDWillDo", _progress),
				_newRepo.RepoName, _progress));

			// Case 4: There is a new repo with a properly-derived name
			Assert.IsTrue(_source.CanConnect(new HgRepositoryProxy(_duplicateRepo.RepoName, "AnyIDWillDo", _progress),
				_duplicateRepo.RepoName, _progress));

			// Case 5: There is no matching repo
			Assert.IsFalse(_source.CanConnect(new HgRepositoryProxy("DoesNotExist", "DoesNotExist", _progress),
				"DoesNotExist", _progress));
		}
	}
}
