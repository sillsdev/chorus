using System;
using System.Linq;
using Chorus.FileTypeHanders;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Test class for LexiconProjectSettingsFileHandler.
	/// </summary>
	[TestFixture]
	public class LexiconProjectSettingsFileHandlerTests
	{
		private IChorusFileTypeHandler _lexiconProjectSettingsFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_lexiconProjectSettingsFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "LexiconProjectSettingsFileHandler")).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_lexiconProjectSettingsFileHandler = null;
		}

		[Test]
		public void HandlerShouldProcessMaximumFileSize()
		{
			Assert.AreEqual(UInt32.MaxValue, _lexiconProjectSettingsFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerOnlySupportsLexiconProjectSettingsExtension()
		{
			var extensions = _lexiconProjectSettingsFileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Count() == 1);
			Assert.AreEqual("lpsx", extensions.First());
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _lexiconProjectSettingsFileHandler.DescribeInitialContents(null, null);
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
<LexiconProjectSettings>
</LexiconProjectSettings>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<LexiconProjectSettings>
<WritingSystems addToSldr='true' />
</LexiconProjectSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.lpsx", parent);
				repositorySetup.ChangeFileAndCommit("some.lpsx", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _lexiconProjectSettingsFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
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
<LexiconProjectSettings>
</LexiconProjectSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.lpsx", parent);
				repositorySetup.ChangeFileAndCommit("some.lpsx", parent, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _lexiconProjectSettingsFileHandler.Find2WayDifferences(firstFiR, firstFiR, hgRepository);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual("Edited", result.First().ActionLabel);
			}
		}
	}
}
