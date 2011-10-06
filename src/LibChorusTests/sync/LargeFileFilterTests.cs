using System;
using System.IO;
using System.Linq;
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
		private ChorusFileTypeHandlerCollection _handlersColl;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_handlersColl = ChorusFileTypeHandlerCollection.CreateWithTestHandlerOnly();
			var testHandler = (from handler in _handlersColl.Handlers
							   select handler).First();

			_goodData = "good" + Environment.NewLine;
			for (var i = 1; i < testHandler.MaximumFileSize; i += _goodData.Length)
				_goodData += _goodData;

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
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				bob.Repository.AddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
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
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				bob.Repository.AddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
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
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				bob.Repository.AddAndCheckinFile(fullPathname);
				bob.AssertLocalRevisionNumber(0);
				bob.AssertFileContents(fullPathname, _goodData);

				bob.ChangeFile(fileName, _longData);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				bob.Repository.Commit(false, "test");
				bob.AssertLocalRevisionNumber(1); // 'forget' marks it as deleted in the repo.
				bob.AssertFileContents(fullPathname, _longData);

				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
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
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				//bob.Repository.AddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
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
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				//bob.Repository.AddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void MegabyteIs1024X1024()
		{
			Assert.AreEqual(1048576, LargeFileFilter.Megabyte);
		}


		/// <summary>
		/// Regression test: WS-34181
		/// </summary
		[Test, Ignore("Fails due to mercurial")]
		public void FileWithSpecialCharacterIsAllowed()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "ŭburux.wav";
				bob.ChangeFile(fileName, _goodData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				var pathToRepo = bob.Repository.PathToRepo + Path.PathSeparator;
				bob.Repository.AddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.wav");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl,
					bob.Progress);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}
	}
}