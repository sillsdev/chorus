using System;
using System.IO;
using System.Reflection;
using Chorus;
using Chorus.VcsDrivers.Mercurial;
using Ionic.Zip;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;
using Palaso.PlatformUtilities;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class RepositoryTests
	{
		[TearDown]
		public void TearDown()
		{
			MercurialLocation.PathToMercurialFolder = null;
		}

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
				var baseDir = FileUtils.NormalizePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
				baseDir = FileUtils.StripFilePrefix(baseDir);
				var zipFile = new ZipFile(Path.Combine(baseDir, Path.Combine("VcsDrivers", Path.Combine("TestData", "incompletemergerepo.zip"))));
				zipFile.ExtractAll(tempRepo.Path);
				var hgRepo = new HgRepository(tempRepo.Path, new NullProgress());
				hgRepo.CheckAndUpdateHgrc();
				var parentFile = tempRepo.GetNewTempFile(true);
				File.WriteAllText(parentFile.Path, "New Content");
				var exception = Assert.Throws<ApplicationException>(() => hgRepo.AddAndCheckinFile(parentFile.Path));
				Assert.That(exception.Message.Contains("Unable to recover") && !exception.Message.Contains("unresolved merge"),
					String.Format("Repository should have conflict in retrying the merge, but not have an incomplete merge: {0}", exception.Message));
			}
		}

		[Test]
		public void CloneToUsbWithoutUpdateFollowedByIdentifierDoesNotAffectHgrc()
		{
			using(var repo = new RepositorySetup("source"))
			using(var f = new TemporaryFolder("clonetest"))
			{
				// The MakeCloneFromLocalToLocal with false on alsoDoCheckout is the core of the usb clone operation.
				// We need to make sure that this clone is bare of extensions, and remains so after the identifier is checked.
				HgHighLevel.MakeCloneFromLocalToUsb(repo.ProjectFolder.Path, f.Path, new NullProgress());
				var cloneRepo = new HgRepository(f.Path, new NullProgress());
				var hgFolderPath = Path.Combine(f.Path, ".hg");
				Assert.IsTrue(Directory.Exists(hgFolderPath));
				var hgrcLines = File.ReadAllLines(Path.Combine(hgFolderPath, "hgrc"));
				//SUT
				CollectionAssert.DoesNotContain(hgrcLines, "[extensions]", "extensions section created in bare clone");
				var id = cloneRepo.Identifier;
				CollectionAssert.DoesNotContain(hgrcLines, "[extensions]", "extensions section created after Identifier property read");
			}
		}

		private static string GetExtensionsSection(string pathToMercurialFolder)
		{
#if !MONO
			return string.Format(@"[extensions]
eol=
hgext.graphlog=
convert=
fixutf8={0}\MercurialExtensions\fixutf8\fixutf8.py", Path.GetDirectoryName(pathToMercurialFolder));
#else
			return string.Format(@"[extensions]
hgext.graphlog=
convert=
fixutf8={0}/MercurialExtensions/fixutf8/fixutf8.py", Path.GetDirectoryName(pathToMercurialFolder));
#endif
		}

		[Test]
		public void CheckExtensions_IniFileMissing()
		{
			using (var tempRepo = new TemporaryFolder("CheckExtensions_IniFileMissing"))
			{
				// remember original value of Mercurial directory
				var pathToMercurialFolder = MercurialLocation.PathToMercurialFolder;
				// then set a dummy location that we can modify
				File.WriteAllText(Path.Combine(tempRepo.Path, Platform.IsWindows ? "hg.exe" : "hg"), string.Empty);
				MercurialLocation.PathToMercurialFolder = tempRepo.Path;

				var doc = HgRepository.GetMercurialConfigInMercurialFolder();
				var extensionsRequiredInIni = HgRepository.HgExtensions;

				Assert.That(HgRepository.CheckExtensions(doc, extensionsRequiredInIni), Is.False);
			}
		}

		[Test]
		public void CheckExtensions_ExtensionsSectionMissing()
		{
			using (var tempRepo = new TemporaryFolder("CheckExtensions_ExtensionsSectionMissing"))
			{
				// remember original value of Mercurial directory
				var pathToMercurialFolder = MercurialLocation.PathToMercurialFolder;
				// then set a dummy location that we can modify
				File.WriteAllText(Path.Combine(tempRepo.Path, Platform.IsWindows ? "hg.exe" : "hg"), string.Empty);
				MercurialLocation.PathToMercurialFolder = tempRepo.Path;
				File.WriteAllText(Path.Combine(tempRepo.Path, "mercurial.ini"), string.Empty);

				var doc = HgRepository.GetMercurialConfigInMercurialFolder();
				var extensionsRequiredInIni = HgRepository.HgExtensions;

				Assert.That(HgRepository.CheckExtensions(doc, extensionsRequiredInIni), Is.False);
			}
		}

		[Test]
		public void CheckExtensions_ExtensionsAreMissing()
		{
			using (var tempRepo = new TemporaryFolder("CheckExtensions_ExtensionsAreMissing"))
			{
				// remember original value of Mercurial directory
				var pathToMercurialFolder = MercurialLocation.PathToMercurialFolder;
				// then set a dummy location that we can modify
				File.WriteAllText(Path.Combine(tempRepo.Path, Platform.IsWindows ? "hg.exe" : "hg"), string.Empty);
				MercurialLocation.PathToMercurialFolder = tempRepo.Path;
				File.WriteAllText(Path.Combine(tempRepo.Path, "mercurial.ini"), "[extensions]");

				var doc = HgRepository.GetMercurialConfigInMercurialFolder();
				var extensionsRequiredInIni = HgRepository.HgExtensions;

				Assert.That(HgRepository.CheckExtensions(doc, extensionsRequiredInIni), Is.False);
			}
		}

		[Test]
		public void CheckExtensions_AllExtensionsListedInIni()
		{
			using (var tempRepo = new TemporaryFolder("CheckExtensions_AllExtensionsListedInIni"))
			{
				// remember original value of Mercurial directory
				var pathToMercurialFolder = MercurialLocation.PathToMercurialFolder;
				// then set a dummy location that we can modify
				File.WriteAllText(Path.Combine(tempRepo.Path, Platform.IsWindows ? "hg.exe" : "hg"), string.Empty);
				MercurialLocation.PathToMercurialFolder = tempRepo.Path;
				File.WriteAllText(Path.Combine(tempRepo.Path, "mercurial.ini"),
					GetExtensionsSection(pathToMercurialFolder));

				var doc = HgRepository.GetMercurialConfigInMercurialFolder();
				var extensionsRequiredInIni = HgRepository.HgExtensions;

				Assert.That(HgRepository.CheckExtensions(doc, extensionsRequiredInIni), Is.True);
			}
		}

		[Test]
		public void CheckExtensions_DisallowsAdditionalExtensions()
		{
			using (var tempRepo = new TemporaryFolder("CheckExtensions_DisallowsAdditionalExtensions"))
			{
				// remember original value of Mercurial directory
				var pathToMercurialFolder = MercurialLocation.PathToMercurialFolder;
				// then set a dummy location that we can modify
				File.WriteAllText(Path.Combine(tempRepo.Path, Platform.IsWindows ? "hg.exe" : "hg"), string.Empty);
				MercurialLocation.PathToMercurialFolder = tempRepo.Path;
				File.WriteAllText(Path.Combine(tempRepo.Path, "mercurial.ini"),
					GetExtensionsSection(pathToMercurialFolder) + "\nfoo=");

				var doc = HgRepository.GetMercurialConfigInMercurialFolder();
				var extensionsRequiredInIni = HgRepository.HgExtensions;

				Assert.That(HgRepository.CheckExtensions(doc, extensionsRequiredInIni), Is.False);
			}
		}

	}
}
