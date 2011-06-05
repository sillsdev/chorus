using System.IO;
using Chorus.FileTypeHanders;
using Chorus.sync;
using NUnit.Framework;

namespace LibChorus.Tests.sync
{
	/// <summary>
	/// Test the LargeFileFilter class.
	/// </summary>
	[TestFixture]
	public class LargeFileFilterTests
	{
		private string _goodData = "";
		private string _longData;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			for (var i = 0; i < 190; ++i)
				_goodData += "a";
			_longData = _goodData + _goodData;
		}

		[Test]
		public void ShortAddedFileIsAllowed()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "test.chorusTest";
				bob.ChangeFile(fileName, _goodData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				bob.Repository.AddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(),
					bob.Progress);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(bob.Repository.PathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LongAddedFileIsfilteredOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "test.chorusTest";
				bob.ChangeFile(fileName, _longData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				bob.Repository.AddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(),
					bob.Progress);
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(bob.Repository.PathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LongModifiedFileIsfilteredOut()
		{
			// File is in repo in its shorter version, but now it has grown too large.
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "test.chorusTest";
				bob.ChangeFile(fileName, _goodData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				bob.Repository.AddAndCheckinFile(fullPathname);

				bob.ChangeFile(fileName, _longData);
				bob.Repository.AddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(),
					bob.Progress);
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(bob.Repository.PathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void ShortUnknownFileIsAllowed()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "test.chorusTest";
				bob.ChangeFile(fileName, _goodData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				//bob.Repository.AddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(),
					bob.Progress);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(bob.Repository.PathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LongUnknownFileIsfilteredOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "test.chorusTest";
				bob.ChangeFile(fileName, _longData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				//bob.Repository.AddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly(),
					bob.Progress);
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(bob.Repository.PathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}
	}
}