using System;
using System.Collections.Generic;
using System.IO;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;

namespace Chorus.Tests.VcsDrivers.Mercurial
{
	[TestFixture]
	public class IncludeExcludeTests
	{
		[Test]
		public void NoPatternsSpecified_FileIsNotAdded()
		{
			using (var setup = new EmptyRepositorySetup("Dan"))
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
			using (var setup = new EmptyRepositorySetup("Dan"))
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
			using (var setup = new EmptyRepositorySetup("Dan"))
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
	}

}
