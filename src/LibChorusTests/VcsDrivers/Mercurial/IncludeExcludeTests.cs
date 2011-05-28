using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
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
	}

}
