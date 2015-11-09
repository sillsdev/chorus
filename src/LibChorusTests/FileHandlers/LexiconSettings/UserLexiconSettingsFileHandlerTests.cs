using System;
using System.Linq;
using Chorus.FileTypeHandlers;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.LexiconSettings
{
	/// <summary>
	/// Test class for UserLexiconSettingsFileHandler.
	/// </summary>
	[TestFixture]
	public class UserLexiconSettingsFileHandlerTests
	{
		private IChorusFileTypeHandler _userLexiconSettingsFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_userLexiconSettingsFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "UserLexiconSettingsFileHandler")).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_userLexiconSettingsFileHandler = null;
		}

		[Test]
		public void HandlerShouldProcessMaximumFileSize()
		{
			Assert.AreEqual(UInt32.MaxValue, _userLexiconSettingsFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerOnlySupportsUserLexiconSettingsExtension()
		{
			var extensions = _userLexiconSettingsFileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Count() == 1);
			Assert.AreEqual("ulsx", extensions.First());
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _userLexiconSettingsFileHandler.DescribeInitialContents(null, null);
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
<UserLexiconSettings>
</UserLexiconSettings>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<UserLexiconSettings>
<WritingSystems />
</UserLexiconSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.ulsx", parent);
				repositorySetup.ChangeFileAndCommit("some.ulsx", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _userLexiconSettingsFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
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
<UserLexiconSettings>
</UserLexiconSettings>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.ulsx", parent);
				repositorySetup.ChangeFileAndCommit("some.ulsx", parent, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _userLexiconSettingsFileHandler.Find2WayDifferences(firstFiR, firstFiR, hgRepository);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual("Edited", result.First().ActionLabel);
			}
		}
	}
}
