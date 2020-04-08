using System;
using System.IO;
using Chorus.VcsDrivers;
using Chorus.VcsDrivers.Mercurial;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.PlatformUtilities;
using SIL.Progress;
using SIL.TestUtilities;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class Utf8Tests
	{
		class MercurialExtensionHider : IDisposable
		{
			private readonly string _extensionPath;
			private readonly string _extensionPathRenamed;

			public MercurialExtensionHider()
			{
				_extensionPath = FileLocationUtilities.GetDirectoryDistributedWithApplication(false, "MercurialExtensions", "fixutf8");
				_extensionPathRenamed = _extensionPath + "-HidingForTest";
				Directory.Move(_extensionPath, _extensionPathRenamed);
			}

			public void Dispose()
			{
				Directory.Move(_extensionPathRenamed, _extensionPath);
			}
		}

		[Test]
		public void AddUtf8FileName_CloneUpdatedFileExists()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				const string utf8FilePath = "açesbsun.wav";
				setup.ChangeFile(utf8FilePath, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				setup.AddAndCheckIn();

				using (var other = new RepositorySetup("Bob", setup))
				{
					other.AssertFileExists(utf8FilePath);
				}

			}
		}

		[Test]
		public void ChangedUtf8File_FileCanBePulledAndUpdated()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				const string utf8FilePath = "açesbsun.wav";
				setup.ChangeFile(utf8FilePath, "hello1");
				setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				setup.AddAndCheckIn();

				using (var other = new RepositorySetup("Bob", setup))
				{
					setup.ChangeFile(utf8FilePath, "hello2");
					setup.Repository.Commit(false, "update");
					other.CheckinAndPullAndMerge(setup); // Fix: Currently this modifies Dan adding bogus file unexpectedly.
					other.AssertFileExists(utf8FilePath);
					string[] fileNames = Directory.GetFiles(other.ProjectFolder.Path, "*.wav");
					Assert.AreEqual(1, fileNames.Length);
				}

			}
		}

		[Test]
		[Platform(Exclude = "Linux")]
		public void Utf8ExtensionNotPresent_MercurialOperationReportsError()
		{
			using (new MercurialExtensionHider())
			using (var setup = new RepositorySetup("Dan", false))
			{
				Assert.Throws<ApplicationException>(
					() =>
					RepositorySetup.MakeRepositoryForTest(
						setup.ProjectFolder.Path, "Dan", setup.Progress
					)
				);
				//const string utf8FilePath = "açesbsun.wav";
				//setup.ChangeFile(utf8FilePath, "hello1");
				//setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				//setup.AddAndCheckIn();
				//setup.AssertFileDoesNotExistInRepository(utf8FilePath);
				//Assert.IsTrue(setup.GetProgressString().Contains("Failed to set up extensions"));
			}
		}

		[Test]
		public void Utf8ExtensionNotPresent_CloneLocalWithoutUpdateThrows()
		{
			// The mercurial ini is only checked once by use of a static variable.
			// Passing false to the makeRepository in RepositorySetup avoids any checking before we run the CloneWithoutLocalUpdate
			using(var setup = new RepositorySetup("Dan", false))
			{
				using(new MercurialExtensionHider())
				using(var other = new RepositorySetup("Bob", false))
				{
					var exception = Assert.Throws<ApplicationException>(
						() => setup.Repository.CloneLocalWithoutUpdate(other.ProjectFolder.Path)
					);

					if (!Platform.IsMono)
					{
						// On mono a different exception is thrown, which is fine, the rest of this test is still useful
						Assert.That(exception.Message.Contains("fixutf8"),
							"Expected fixutf8 in:" + exception.Message);
					}
				}
			}
		}

		/// <summary>
		/// The local clone works as it uses the settings of the source repo. i.e. It is a clone to not a clone from.
		/// </summary>
		[Test]
		public void Utf8ExtensionPresent_CloneLocalWithUpdateDoesNotHaveBogusFiles()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				const string utf8FilePath = "açesbsun.wav";
				setup.ChangeFile(utf8FilePath, "hello1");
				setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				setup.AddAndCheckIn();

				using (var other = new RepositorySetup("Bob", false))
				{
					setup.Repository.CloneLocalWithoutUpdate(other.ProjectFolder.Path); // Somewhat surprisingly this works as it is using the settings of the source hgrc during the clone
					other.Repository.Update();

					other.AssertFileExists(utf8FilePath);
					string[] fileNames = Directory.GetFiles(other.ProjectFolder.Path, "*.wav");
					Assert.AreEqual(1, fileNames.Length);

					//Assert.IsTrue(setup.GetProgressString().Contains());
				}

			}
		}

		/// <summary>
		/// The local clone works as it uses the settings of the source repo. i.e. It is a clone to not a clone from.
		/// </summary>
		[Test]
		public void Utf8ExtensionPresent_CloneDoesNotHaveBogusFiles()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				const string utf8FilePath = "açesbsun.wav";
				setup.ChangeFile(utf8FilePath, "hello1");
				setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				setup.AddAndCheckIn();

				using (var other = new RepositorySetup("Bob", false))
				{
					//var uri = new Uri(String.Format("file:///{0}", setup.ProjectFolder.Path));
					HgRepository.Clone(new HttpRepositoryPath("utf test repo", setup.ProjectFolder.Path, false), other.ProjectFolder.Path, other.Progress);
					other.Repository.Update();

					other.AssertFileExists(utf8FilePath);
					string[] fileNames = Directory.GetFiles(other.ProjectFolder.Path, "*.wav");
					Assert.AreEqual(1, fileNames.Length);

					//Assert.IsTrue(setup.GetProgressString().Contains());
				}

			}
		}

		/// <summary>
		/// The local clone works as it uses the settings of the source repo. i.e. It is a clone to not a clone from.
		/// </summary>
		[Test, Ignore("May not be able to test it, if the ini file is created by the installer.")]
		public void Utf8ExtensionPresent_LocalMercurialIniIncorrect_MercurialOpStillWorks()
		{
			using (new MercurialIniHider())
			using (var setup = new RepositorySetup("Dan"))
			{
				const string utf8FilePath = "açesbsun.wav";
				setup.ChangeFile(utf8FilePath, "hello1");
				setup.ProjectFolderConfig.IncludePatterns.Add("*.wav");
				setup.AddAndCheckIn();

				setup.AssertFileExistsInRepository("açesbsun.wav");

			}
		}



		[Test]
		public void CreateOrLocate_FolderHasThaiAndAccentedLetter2_FindsIt()
		{
			using (var testRoot = new TemporaryFolder("chorus utf8 folder test"))
			{
				//string path = Path.Combine(testRoot.Path, "Abé Books");
				string path = Path.Combine(testRoot.Path, "ไก่ projéct");
				Directory.CreateDirectory(path);

				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
			}
		}

		[Test]
		public void CreateOrLocate_FolderHasAccentedLetter2_FindsIt()
		{
			using (var testRoot = new TemporaryFolder("chorus utf8 folder test"))
			{
				//string path = Path.Combine(testRoot.Path, "Abé Books");
				string path = Path.Combine(testRoot.Path, "projéct");
				Directory.CreateDirectory(path);

				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
			}

		}

		[Test]
		public void CreateOrLocate_FolderHasAccentedLetterAbeBooks_FindsIt()
		{
			using (var testRoot = new TemporaryFolder("chorus utf8 folder test"))
			{
				string path = Path.Combine(testRoot.Path, "Abé Books");
				//string path = Path.Combine(testRoot.Path, "projéct");
				Directory.CreateDirectory(path);

				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
				Assert.NotNull(HgRepository.CreateOrUseExisting(path, new ConsoleProgress()));
			}

		}

		// Regression: http://jira.palaso.org/issues/browse/CHR-32
		/* When applications are passed to windows console apps they are passed as CP1252 strings if at all possible,
		 * failing that they are passed as UCS2. e.g. projéct would be CP1252 and ไก่ would be UCS2.
		 *
		 * The CreateOrLocate tests did not capture this distinction, so these 'Recover' tests have been added.
		 */
		[Test]
		public void Recover_UnicodeProject_NoThrow()
		{
			using (var repo = new RepositorySetup("Dan", "ไก่", true))
			{
				repo.Repository.RecoverFromInterruptedTransactionIfNeeded();
				Assert.True(File.Exists(repo.PathToHgrc));
			}

		}

		// Regression: http://jira.palaso.org/issues/browse/CHR-32
		[Test]
		public void Recover_Cp1252ProjectPath_NoThrow()
		{
			using (var repo = new RepositorySetup("Dan", "projéct", true))
			{
				repo.Repository.RecoverFromInterruptedTransactionIfNeeded();
				Assert.True(File.Exists(repo.PathToHgrc));
			}
		}

	}


}
