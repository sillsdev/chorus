using System;
using System.IO;
using System.Reflection;
using Chorus.VcsDrivers.Mercurial;
using Ionic.Zip;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class RepositoryTests
	{
		[Test]
		public void AddingRepositoryWithinAnotherRepositoryFromDirectoryNameIsDifferentRepository()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var dirInfo = Directory.CreateDirectory(Path.Combine(parentFolder, "Child"));
				var childRepo = HgRepository.CreateOrUseExisting(dirInfo.FullName, new NullProgress());
				Assert.AreNotEqual(parentFolder, childRepo.PathToRepo);
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryFromFileNameIsDifferentRepository()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var dirInfo = Directory.CreateDirectory(Path.Combine(parentFolder, "Child"));
				var childPathname = Path.Combine(dirInfo.FullName, "Child.txt");
				File.WriteAllText(childPathname, "New child content.");
				var childRepo = HgRepository.CreateOrUseExisting(childPathname, new NullProgress());
				Assert.AreNotEqual(parentFolder, childRepo.PathToRepo);
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNonexistantDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var nonexistantDirectory = Path.Combine(parentFolder, "Child");
				Assert.Throws<InvalidOperationException>(() => HgRepository.CreateOrUseExisting(nonexistantDirectory, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNonexistantFileThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				var parentFolder = tempParent.Path;
				var nonexistantFile = Path.Combine(parentFolder, "bogusfile.txt");
				Assert.Throws<InvalidOperationException>(() => HgRepository.CreateOrUseExisting(nonexistantFile, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithNullDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				Assert.Throws<ArgumentNullException>(() => HgRepository.CreateOrUseExisting(null, new NullProgress()));
			}
		}

		[Test]
		public void AddingRepositoryWithinAnotherRepositoryWithEmptyStringDirectoryThrows()
		{
			using (var tempParent = new TemporaryFolder("ChorusParent"))
			{
				var parentRepo = new HgRepository(tempParent.Path, new NullProgress());
				parentRepo.Init();
				var parentFile = tempParent.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				parentRepo.AddAndCheckinFile(parentFile.Path);

				Assert.Throws<ArgumentNullException>(() => HgRepository.CreateOrUseExisting("", new NullProgress()));
			}
		}

		[Test]
		public void RepositoryRecoversFromIncompleteMerge()
		{
			using (var tempRepo = new TemporaryFolder("ChorusIncompleteMerge"))
			{
				var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
#if MONO
				baseDir = baseDir.Replace(@"file:", null);
#else
				baseDir = baseDir.Replace(@"file:\", null);
#endif
				var zipFile = new ZipFile(Path.Combine(baseDir, Path.Combine("VcsDrivers", Path.Combine("TestData", "incompletemergerepo.zip"))));
				zipFile.ExtractAll(tempRepo.Path);
				var hgRepo = new HgRepository(tempRepo.Path, new NullProgress());
				hgRepo.CheckAndUpdateHgrc();
				var parentFile = tempRepo.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				var exception = Assert.Throws<ApplicationException>(() => hgRepo.AddAndCheckinFile(parentFile.Path));
				Assert.That(exception.Message.Contains("Unable to recover") && !exception.Message.Contains("unresolved merge"), "Repository should have conflict in retrying the merge, but not have an incomplete merge");
			}
		}
	}
}