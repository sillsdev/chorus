using System;
using System.Linq;
using Chorus.FileTypeHandlers;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Test class for ProjectLexiconSettingsFileHandler.
	/// </summary>
	[TestFixture]
	public class ProjectLexiconSettingsFileHandlerTests
	{
		private IChorusFileTypeHandler _projectLexiconSettingsFileHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_projectLexiconSettingsFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "ProjectLexiconSettingsFileHandler")).First();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_projectLexiconSettingsFileHandler = null;
		}

		[Test]
		public void HandlerShouldProcessMaximumFileSize()
		{
			Assert.AreEqual(UInt32.MaxValue, _projectLexiconSettingsFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerOnlySupportsProjectLexiconSettingsExtension()
		{
			var extensions = _projectLexiconSettingsFileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Count() == 1);
			Assert.AreEqual("plsx", extensions.First());
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _projectLexiconSettingsFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneChangeNoMatterHowManyWereMade()
		{
			// There are actually more than one change, but we don't fret about that at this point.
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
</ProjectLexiconSettings>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
<WritingSystems addToSldr='true' />
</ProjectLexiconSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.plsx", parent);
				repositorySetup.ChangeFileAndCommit("some.plsx", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _projectLexiconSettingsFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual("Edited", result.First().ActionLabel);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneChangeEvenWhenNoneArePresent()
		{
			// One 'change' reported, even for the exact same file.
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<ProjectLexiconSettings>
</ProjectLexiconSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.plsx", parent);
				repositorySetup.ChangeFileAndCommit("some.plsx", parent, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _projectLexiconSettingsFileHandler.Find2WayDifferences(firstFiR, firstFiR, hgRepository);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual("Edited", result.First().ActionLabel);
			}
		}
	}
}
