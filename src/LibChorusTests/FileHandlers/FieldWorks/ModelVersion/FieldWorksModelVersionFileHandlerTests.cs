using System;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.FieldWorks.ModelVersion;
using Chorus.merge;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.FieldWorks.ModelVersion
{
	/// <summary>
	/// Test the FW model version file handler
	/// </summary>
	[TestFixture]
	public class FieldWorksModelVersionFileHandlerTests
	{
		private IChorusFileTypeHandler _fwModelVersionFileHandler;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fwModelVersionFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
											  where handler.GetType().Name == "FieldWorksModelVersionFileHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fwModelVersionFileHandler = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fwModelVersionFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = _fwModelVersionFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("ModelVersion", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fwModelVersionFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			using (var tempModelVersionFile = new TempFile("{\"modelversion\": 7000037}"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(_fwModelVersionFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNotNull(_fwModelVersionFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("{\"modelversion\": 7000037}"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "ModelVersion");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNull(_fwModelVersionFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldMergeTheirModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000000}";
			const string ourData = "{\"modelversion\": 7000000}";
			const string theirData = "{\"modelversion\": 7000001}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation())
					{ EventListener = listener };
				_fwModelVersionFileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(theirData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void ShouldMergeOurModelNumber()
		{
			const string commonData = "{\"modelversion\": 7000000}";
			const string ourData = "{\"modelversion\": 7000002}";
			const string theirData = "{\"modelversion\": 7000001}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation())
					{EventListener = listener};
				_fwModelVersionFileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void BothDidSameUpgrade()
		{
			const string commonData = "{\"modelversion\": 7000000}";
			const string ourData = "{\"modelversion\": 7000002}";
			const string theirData = "{\"modelversion\": 7000002}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				_fwModelVersionFileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<FieldWorksModelVersionUpdatedReport>();
			}
		}

		[Test]
		public void ShouldRejectOurDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000009}";
			const string theirData = "{\"modelversion\": 7000010}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				Assert.Throws<InvalidOperationException>(() => _fwModelVersionFileHandler.Do3WayMerge(mergeOrder));
			}
		}

		[Test]
		public void ShouldRejectTheirDowngrade()
		{
			const string commonData = "{\"modelversion\": 7000010}";
			const string ourData = "{\"modelversion\": 7000010}";
			const string theirData = "{\"modelversion\": 7000009}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				Assert.Throws<InvalidOperationException>(() => _fwModelVersionFileHandler.Do3WayMerge(mergeOrder));
			}
		}

		[Test]
		public void ShouldHaveNoChanges()
		{
			const string commonData = "{\"modelversion\": 7000002}";
			const string ourData = "{\"modelversion\": 7000002}";
			const string theirData = "{\"modelversion\": 7000002}";
			using (var commonTempFile = new TempFile("Common.ModelVersion"))
			using (var ourTempFile = new TempFile("Our.ModelVersion"))
			using (var theirTempFile = new TempFile("Their.ModelVersion"))
			{
				File.WriteAllText(commonTempFile.Path, commonData);
				File.WriteAllText(ourTempFile.Path, ourData);
				File.WriteAllText(theirTempFile.Path, theirData);

				var listener = new ListenerForUnitTests();
				var mergeOrder = new MergeOrder(ourTempFile.Path, commonTempFile.Path, theirTempFile.Path, new NullMergeSituation()) { EventListener = listener };
				_fwModelVersionFileHandler.Do3WayMerge(mergeOrder);
				var mergedData = File.ReadAllText(ourTempFile.Path);
				Assert.AreEqual(ourData, mergedData);
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneAddition()
		{
			const string parent = "{\"modelversion\": 7000002}";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.ModelVersion", parent);
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var result = _fwModelVersionFileHandler.Find2WayDifferences(null, firstFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.IsInstanceOf(typeof(FieldWorksModelVersionAdditionChangeReport), result[0]);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportOneChange()
		{
			const string parent = "{\"modelversion\": 7000000}";
			// One change.
			const string child = "{\"modelversion\": 7000002}";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.ModelVersion", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.ModelVersion", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _fwModelVersionFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(1, result.Count);
				Assert.IsInstanceOf(typeof(FieldWorksModelVersionUpdatedReport), result[0]);
			}
		}
	}
}