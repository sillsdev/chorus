using System;
using System.Linq;
using Chorus.FileTypeHandlers;
using LibChorus.TestUtilities;
using NUnit.Framework;

namespace LibChorus.Tests.FileHandlers.ldml
{
	/// <summary>
	/// Test class for LdmlFileHandler.
	/// </summary>
	[TestFixture]
	public class LdmlFileHandlerTests
	{
		private IChorusFileTypeHandler _ldmlFileHandler;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			_ldmlFileHandler = (ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers.Where(
				handler => handler.GetType().Name == "LdmlFileHandler")).First();
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_ldmlFileHandler = null;
		}

		[Test]
		public void HandlerShouldProcessMaximumFileSize()
		{
			Assert.AreEqual(UInt32.MaxValue, _ldmlFileHandler.MaximumFileSize);
		}

		[Test]
		public void HandlerOnlySupportsldmlExtension()
		{
			var extensions = _ldmlFileHandler.GetExtensionsOfKnownTextFileTypes();
			Assert.IsTrue(extensions.Count() == 1);
			Assert.AreEqual("ldml", extensions.First());
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _ldmlFileHandler.DescribeInitialContents(null, null);
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
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<ldml>
<identity />
<special xmlns:fw='urn://fieldworks.sil.org/ldmlExtensions/v1' />
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.ldml", parent);
				repositorySetup.ChangeFileAndCommit("some.ldml", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _ldmlFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
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
<ldml>
<special xmlns:palaso='urn://palaso.org/ldmlExtensions/v1' />
</ldml>";
			using (var repositorySetup = new RepositorySetup("randy-" + Guid.NewGuid()))
			{
				repositorySetup.AddAndCheckinFile("some.ldml", parent);
				repositorySetup.ChangeFileAndCommit("some.ldml", parent, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _ldmlFileHandler.Find2WayDifferences(firstFiR, firstFiR, hgRepository);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual("Edited", result.First().ActionLabel);
			}
		}
	}
}
