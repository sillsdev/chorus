using System.IO;
using System.Linq;
using Chorus.FileTypeHandlers.lift;
using Chorus.sync;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class IncludeExcludeTests
	{
		[Test]
		public void NoPatternsSpecified_FileIsNotAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var path = setup.ProjectFolder.Combine("test.1w1");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.AddAndCheckIn();
				setup.AssertFileDoesNotExistInRepository("test.1w1");

			}
		}

		[Test]
		public void StarDotExtensionPatternSpecified_FileAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var path = setup.ProjectFolder.Combine("test.1w1");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.1w1");
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.AddAndCheckIn();
				setup.AssertFileExistsInRepository("test.1w1");
			}
		}

		[Test]
		public void IncludeAllButExcludeOne_FileNotAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var path = setup.ProjectFolder.Combine("test.1w1");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.*");
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Add("*.1w1");
				setup.AddAndCheckIn();
				setup.AssertFileDoesNotExistInRepository("test.1w1");
			}
		}



		[Test]
		public void ExcludeLdmlInRoot_FileNotAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var path = setup.ProjectFolder.Combine("test.ldml");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.*");
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Add("*.ldml");
				setup.AddAndCheckIn();
				setup.AssertFileDoesNotExistInRepository("test.ldml");
			}
		}


		/// <summary>
		/// for LIFT, normally we want .lift, but not if it's in th export folder
		/// </summary>
		[Test]
		public void IncludeInGeneralButExcludeInSubfolder_FileNotAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var good = setup.ProjectFolder.Combine("good.lift");
				File.WriteAllText(good, "hello");

				var export = setup.ProjectFolder.Combine("export");
				Directory.CreateDirectory(export);
				var bad = Path.Combine(export, "bad.lift");
				File.WriteAllText(bad, "hello");

				var goodFontCss = Path.Combine(export, "customFonts.css");
				File.WriteAllText(goodFontCss, "hello");

				var goodLayoutCss = Path.Combine(export, "customLayout.css");
				File.WriteAllText(goodLayoutCss, "hello");

				var other = setup.ProjectFolder.Combine("other");
				Directory.CreateDirectory(other);
				var otherBad = Path.Combine(export, "otherBad.lift");
				File.WriteAllText(otherBad, "hello");

				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Clear();

				LiftFolder.AddLiftFileInfoToFolderConfiguration(setup.ProjectFolderConfig);

				setup.AddAndCheckIn();
				setup.AssertFileExistsInRepository("good.lift");
				setup.AssertFileExistsInRepository("export/customFonts.css");
				setup.AssertFileExistsInRepository("export/customLayout.css");
				setup.AssertFileDoesNotExistInRepository("export/bad.lift");
				setup.AssertFileDoesNotExistInRepository("other/otherBad.lift");
			}
		}

		[Test]
		public void ExcludedVideosFileNotAdded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var atRoot = setup.ProjectFolder.Combine("first.wmv");
				File.WriteAllText(atRoot, "hello");

				var pictures = setup.ProjectFolder.Combine("pictures");
				Directory.CreateDirectory(pictures);
				var videoExtensions = ProjectFolderConfiguration.VideoExtensions.ToList();
				foreach (var videoExtension in videoExtensions)
				{
					var bad = Path.Combine(pictures, "nested." + videoExtension);
					File.WriteAllText(bad, "hello");
				}

				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Clear();

				LiftFolder.AddLiftFileInfoToFolderConfiguration(setup.ProjectFolderConfig);

				setup.AddAndCheckIn();
				setup.AssertFileDoesNotExistInRepository("first.wmv");
				foreach (var videoExtension in videoExtensions)
					setup.AssertFileDoesNotExistInRepository("pictures/nested." + videoExtension);
			}
		}

		[Test]
		public void IncludeFilesInSubFolders()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var subpictures = setup.ProjectFolder.Combine("pictures", "subpictures");
				Directory.CreateDirectory(subpictures);
				var goodpicture = setup.ProjectFolder.Combine(subpictures, "good.picture");
				File.WriteAllText(goodpicture, "hello"); // Not a real jpeg file

				var subaudio = setup.ProjectFolder.Combine("audio", "subaudio");
				Directory.CreateDirectory(subaudio);
				var goodaudio = setup.ProjectFolder.Combine(subaudio, "good.audio");
				File.WriteAllText(goodaudio, "hello"); // Not a real mp3 file

				var subothers = setup.ProjectFolder.Combine("others", "subothers");
				Directory.CreateDirectory(subothers);
				var goodother = setup.ProjectFolder.Combine(subothers, "good.other");
				File.WriteAllText(goodother, "hello");

				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Clear();

				LiftFolder.AddLiftFileInfoToFolderConfiguration(setup.ProjectFolderConfig);

				setup.AddAndCheckIn();
				setup.AssertFileExistsInRepository("pictures/subpictures/good.picture");
				setup.AssertFileExistsInRepository("audio/subaudio/good.audio");
				setup.AssertFileExistsInRepository("others/subothers/good.other");
			}
		}

		// NB: This test should NOT pass, but it seems Hg will put it in, no matter what.
		[Test]
		public void WavFileInRepoEvenWhenExcluded()
		{
			using (var setup = new RepositorySetup("Dan"))
			{
				var path = setup.ProjectFolder.Combine("test.wav");
				File.WriteAllText(path, "hello");
				setup.ProjectFolderConfig.IncludePatterns.Clear();
				setup.ProjectFolderConfig.IncludePatterns.Add("*.*");
				setup.ProjectFolderConfig.ExcludePatterns.Clear();
				setup.ProjectFolderConfig.ExcludePatterns.Add("test.wav");
				setup.AddAndCheckIn();
				// TODO: If Hg is fixed to exclude "wav" files,
				// revise this test to assert it is *not* in repo.
				// Very important: Also fix the "wav" extension hacks in LargeFileFilter AND AudioFileTypeHandlerTests
				setup.AssertFileExistsInRepository("test.wav");
			}
		}
	}

}
