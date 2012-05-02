using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.lift;
using Chorus.sync;
using LibChorus.TestUtilities;
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
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				bob.Repository.TestOnlyAddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl);
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
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				bob.Repository.TestOnlyAddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl);
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
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
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
					_handlersColl);
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
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				//bob.Repository.TestOnlyAddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl);
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
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				//bob.Repository.TestOnlyAddSansCommit(fullPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.chorusTest");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl);
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
		/// </summary>
		[Test]
		public void FileWithSpecialCharacterIsAllowed()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName =  "ŭburux.wav";
				bob.ChangeFile(fileName, _goodData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				bob.Repository.TestOnlyAddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.wav");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					_handlersColl);
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void NormallyExcludedFwdataFileIsNotAddedByLargeFileFilter()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				const string largeFwdataFilename = "whopper.fwdata";
				var largeFwdataPathname = Path.Combine(pathToRepo, largeFwdataFilename);
				bob.ChangeFile(largeFwdataPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeFwdataPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add("*.fwdata");
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("*.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));

				var shortpath = largeFwdataPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LargeFileInExcludedPathIsNotFilteredOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				var nestedFolder = Path.Combine(bob.Repository.PathToRepo, "Cache");
				Directory.CreateDirectory(nestedFolder);
				const string largeVideoFilename = "whopper.mov";
				var largeVideoPathname = Path.Combine("Cache", largeVideoFilename);
				bob.ChangeFile(largeVideoPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeVideoPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add(Path.Combine("**", "Cache"));
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));

				var shortpath = largeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LargeFileInExcludedNestedPathIsNotFilteredOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				var firstLayer = Path.Combine(bob.Repository.PathToRepo, "SomeLayer");
				Directory.CreateDirectory(firstLayer);
				var nestedFolder = Path.Combine(firstLayer, "Cache");
				Directory.CreateDirectory(nestedFolder);
				const string largeVideoFilename = "whopper.mov";
				var largeVideoPathname = Path.Combine(nestedFolder, largeVideoFilename);
				bob.ChangeFile(largeVideoPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeVideoPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add(Path.Combine("**", "Cache"));
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));

				var shortpath = largeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LargeFileInNonExcludedFolderNotFilteredByExclusionAtDeeperNesting()
		{
			//Put a large file in [repo]\Cache and in [repo]\foo\SomeLayer\Cache
			//exclude \foo\**\Cache, make sure that [repo]\Cache\largeFile was not filtered out.
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				var firstLayer = Path.Combine(bob.Repository.PathToRepo, "foo");
				Directory.CreateDirectory(firstLayer);
				var coFirstLayer = Path.Combine(bob.Repository.PathToRepo, "Cache");
				Directory.CreateDirectory(coFirstLayer);
				var secondLayer = Path.Combine(bob.Repository.PathToRepo, "SomeLayer");
				Directory.CreateDirectory(secondLayer);
				var nestedFolder = Path.Combine(secondLayer, "Cache");
				Directory.CreateDirectory(nestedFolder);
				const string largeVideoFilename = "whopper.mov";
				var largeVideoPathname = Path.Combine(nestedFolder, largeVideoFilename);
				bob.ChangeFile(largeVideoPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeVideoPathname);
				var nonExcludedVideoPath = Path.Combine(coFirstLayer, largeVideoFilename);
				bob.ChangeFile(nonExcludedVideoPath, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(nonExcludedVideoPath);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add(Path.Combine("foo", Path.Combine("**", "Cache")));
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsFalse(string.IsNullOrEmpty(result), @"Cache folder at root was improperly filtered by foo\**\Cache");

				var shortpath = largeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LargeFileInExcludedDeeplyNestedPathIsNotFilteredOut()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				var firstLayer = Path.Combine(bob.Repository.PathToRepo, "FirstLayer");
				Directory.CreateDirectory(firstLayer);
				var secondLayer = Path.Combine(firstLayer, "SecondLayer");
				Directory.CreateDirectory(secondLayer);
				var nestedFolder = Path.Combine(secondLayer, "Cache");
				Directory.CreateDirectory(nestedFolder);
				const string largeVideoFilename = "whopper.mov";
				var largeVideoPathname = Path.Combine(nestedFolder, largeVideoFilename);
				bob.ChangeFile(largeVideoPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeVideoPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add(Path.Combine("FirstLayer", Path.Combine("**", "Cache")));
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));

				var shortpath = largeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void NormallyExcludedNestedFileIsNotAddedByLargeFileFilter()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				var nestedFolder = Path.Combine(bob.Repository.PathToRepo, "nestedFolder");
				Directory.CreateDirectory(nestedFolder);
				const string largeVideoFilename = "whopper.mov";
				var largeVideoPathname = Path.Combine("nestedFolder", largeVideoFilename);
				bob.ChangeFile(largeVideoPathname, megabyteLongData);
				bob.Repository.TestOnlyAddSansCommit(largeVideoPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add("**.mov");
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("**.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));

				var shortpath = largeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void NormallyExcludedFileIsNotAddedByLargeFileFilter()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string dictionaryFilename = "defaultDictionary.css";
				const string oldDictionaryFilename = "defaultDictionary.old";
				const string largeVideoFilename = "whopper.mov";

				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;

				bob.ChangeFile(dictionaryFilename, megabyteLongData);
				var fullDictionaryPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, dictionaryFilename);
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				bob.Repository.TestOnlyAddSansCommit(fullDictionaryPathname);

				bob.ChangeFile(oldDictionaryFilename, megabyteLongData);
				var fullOldDictionaryPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, oldDictionaryFilename);
				bob.Repository.TestOnlyAddSansCommit(fullOldDictionaryPathname);

				bob.ChangeFile(largeVideoFilename, megabyteLongData);
				var fullLargeVideoPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, largeVideoFilename);
				bob.Repository.TestOnlyAddSansCommit(fullLargeVideoPathname);

				var config = bob.ProjectFolderConfig;
				config.ExcludePatterns.Clear();
				config.ExcludePatterns.Add("defaultDictionary.css");
				config.ExcludePatterns.Add("*.old");
				config.ExcludePatterns.Add("*.mov");
				config.IncludePatterns.Clear();
				config.IncludePatterns.Add("*.*");

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsTrue(string.IsNullOrEmpty(result));
				var shortpath = fullDictionaryPathname.Replace(pathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));

				shortpath = fullOldDictionaryPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));

				shortpath = fullLargeVideoPathname.Replace(pathToRepo, "");
				Assert.IsFalse(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}

		[Test]
		public void LargeMp3FileIsNotAllowed()
		{
			using (var bob = new RepositorySetup("bob"))
			{
				const string fileName = "whopper.Mp3";
				var megabyteLongData = "long" + Environment.NewLine;
				while (megabyteLongData.Length < LargeFileFilter.Megabyte)
					megabyteLongData += megabyteLongData;
				bob.ChangeFile(fileName, megabyteLongData);
				var fullPathname = Path.Combine(bob.ProjectFolderConfig.FolderPath, fileName);
				var pathToRepo = bob.Repository.PathToRepo + Path.DirectorySeparatorChar;
				bob.Repository.TestOnlyAddSansCommit(fullPathname);
				var config = bob.ProjectFolderConfig;
				LiftFolder.AddLiftFileInfoToFolderConfiguration(config);

				var result = LargeFileFilter.FilterFiles(
					bob.Repository,
					config,
					ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers());
				Assert.IsFalse(string.IsNullOrEmpty(result));
				var shortpath = fullPathname.Replace(pathToRepo, "");
				Assert.IsTrue(config.ExcludePatterns.Contains(shortpath));
				Assert.IsFalse(config.IncludePatterns.Contains(shortpath));
			}
		}
	}
}