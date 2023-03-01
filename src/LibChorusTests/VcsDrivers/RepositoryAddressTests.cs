using System;
using System.Collections.Generic;
using System.Reflection;
using Chorus.Properties;
using Chorus.VcsDrivers;
using LibChorus.Tests.Model;
using LibChorus.Tests.VcsDrivers.Mercurial;
using NUnit.Framework;
using SIL.Progress;

namespace LibChorus.Tests.VcsDrivers
{
	[TestFixture]
	class RepositoryAddressTests
	{
		private ChorusHubRepositorySource _source;
		private IEnumerable<RepositoryInformation> _repositoryInformations;
		private RepositoryInformation _normalRepo, _duplicateRepo, _newRepo;
		private string _chorusHubURL = "http://chorushub@127.0.0.1:5913/";
		private readonly IProgress _progress = new NullProgress();

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

		[TestCase("1", ExpectedResult = true)]
		[TestCase("2", ExpectedResult = true)]
		[TestCase("3", ExpectedResult = true)]
		[TestCase("54", ExpectedResult = true)]
		[TestCase("NaN", ExpectedResult = false)]
		public bool TestIsMatchingName(string num)
		{
			const string name = "RepoName";
			const string id = "RepoID";
			return InvokeIsMatchingName(new RepositoryInformation(name + num, id), name);
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
			Assert.That(InvokeTryGetBestRepoMatch(_normalRepo.RepoID, _normalRepo.RepoName, out matchName, out warningMessage), Is.True);
			Assert.AreEqual(_normalRepo.RepoName, matchName);
			Assert.That(warningMessage, Is.Null);

			// Case 2: Repo ID matches, but name does not
			Assert.That(InvokeTryGetBestRepoMatch(_normalRepo.RepoID, "DifferentProjectName", out matchName, out warningMessage), Is.True);
			Assert.AreEqual(_normalRepo.RepoName, matchName);
			Assert.That(warningMessage, Is.Null);

			// Case 3: There is a new repo with the correct name
			Assert.That(InvokeTryGetBestRepoMatch("AnyIDWillDo", _newRepo.RepoName, out matchName, out warningMessage), Is.True);
			Assert.AreEqual(_newRepo.RepoName, matchName);
			Assert.That(warningMessage, Is.Null);

			// Case 4: There is a new repo with a properly-derived name
			Assert.That(InvokeTryGetBestRepoMatch("AnyIDWillDo", _duplicateRepo.RepoName, out matchName, out warningMessage), Is.True);
			Assert.AreEqual(_newRepo.RepoName, matchName);
			Assert.That(warningMessage, Is.Not.Null.Or.Empty);

			// Case 5: There is no matching repo
			Assert.That(InvokeTryGetBestRepoMatch("DoesNotExist", "DoesNotExist", out matchName, out warningMessage), Is.False);
			Assert.That(warningMessage, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void TestGetPotentialRepoUri()
		{
			// Case 1: Repo name and ID both match
			var uri = _source.GetPotentialRepoUri(_normalRepo.RepoID, _normalRepo.RepoName, _progress);
			Assert.AreEqual(_chorusHubURL + _normalRepo.RepoName, uri);

			// Case 2: Repo ID matches, but name does not
			uri = _source.GetPotentialRepoUri(_normalRepo.RepoID, "DifferentProjectName", _progress);
			Assert.AreEqual(_chorusHubURL + _normalRepo.RepoName, uri);

			// Case 3: There is a new repo with the correct name
			uri = _source.GetPotentialRepoUri("AnyIDWillDo", _newRepo.RepoName, _progress);
			Assert.AreEqual(_chorusHubURL + _newRepo.RepoName, uri);

			// Case 4: There is a new repo with a properly-derived name
			uri = _source.GetPotentialRepoUri("AnyIDWillDo", _duplicateRepo.RepoName, _progress);
			Assert.AreEqual(_chorusHubURL + _newRepo.RepoName, uri);

			// Case 5: There is no matching repo
			uri = _source.GetPotentialRepoUri("DoesNotExist", "DoesNotExist", _progress);
			Assert.AreEqual(_chorusHubURL + "DoesNotExist", uri);

			// Case 6: We are cloning a new repo and can't calculate the ID locally yet
			var progress = new ProgressForTest();
			uri = _source.GetPotentialRepoUri(null, null, progress);
			Assert.AreEqual(_chorusHubURL + RepositoryAddress.ProjectNameVariable, uri);
			Assert.IsEmpty(progress.Warnings);
		}

		[TestCase("https://hg-public.languageforge.org/ngl-flex", typeof(HttpRepositoryPath))]
		[TestCase("//DinosaurShare/OldChorus/Lom", typeof(DirectoryRepositorySource))]
		public void Create_CorrectType(string uri, Type expected)
		{
			Assert.AreEqual(expected, RepositoryAddress.Create("name", uri).GetType());
		}
	}

	[TestFixture]
	class HttpRepositoryPathTests
	{
		private const string UrlBase = ".languageforge.org/project/";
		private const string ResumableDomainPlus = "resumable" + UrlBase;
		private const string NonResDomainPlus = "public" + UrlBase;
		private const string ProjectName = "tpi-flex";
		private const string UrlTemplate = "https://" + ResumableDomainPlus + RepositoryAddress.ProjectNameVariable;
		private const string ResumableUrlFull = "https://" + ResumableDomainPlus + ProjectName;
		private const string UrlWithCredentials = "https://usern%40me:pa5%24word@" + NonResDomainPlus + ProjectName;
		private const string UrlSansCredentials = "https://" + NonResDomainPlus + ProjectName;

		[Test]
		public void GetPotentialRepoUri_ReplacesProjectNameVariable()
		{
			var source = new HttpRepositoryPath("test", UrlTemplate, true);
			Assert.AreEqual(ResumableUrlFull, source.GetPotentialRepoUri("testing", ProjectName, null));
		}

		[Test]
		public void GetPotentialRepoUri_ToleratesNullProjectName()
		{
			var source = new HttpRepositoryPath("test", ResumableUrlFull, true);
			Assert.AreEqual(ResumableUrlFull, source.GetPotentialRepoUri("testing", null, null));
		}

		[Test]
		public void GetPotentialRepoUri_LeavesExistingUserInfo()
		{
			var source = new HttpRepositoryPath("test", UrlWithCredentials, false);
			Assert.AreEqual(UrlWithCredentials, source.GetPotentialRepoUri("testing", null, null));
		}

		[Test]
		public void GetPotentialRepoUri_InsertsEscapedCredentials()
		{
			using (new ServerSettingsModelTests.PasswordForSession("p@ss:w0rd"))
			{
				Settings.Default.LanguageForgeUser = "joe@doe.co";
				const string expected = "https://joe%40doe.co:p%40ss%3Aw0rd@" + NonResDomainPlus + ProjectName;
				var source = new HttpRepositoryPath("test", UrlSansCredentials, false);
				Assert.That(source.GetPotentialRepoUri("testing", null, null), Is.EqualTo(expected));
			}
		}
	}
}
