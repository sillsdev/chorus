using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.FieldWorks.CustomProperties
{
	/// <summary>
	/// Test the FW custom property file handler.
	/// </summary>
	[TestFixture]
	public class FieldWorksCustomPropertyFileHandlerTests
	{
		private IChorusFileTypeHandler _fwCustomPropertiesFileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fwCustomPropertiesFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
											  where handler.GetType().Name == "FieldWorksCustomPropertyFileHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fwCustomPropertiesFileHandler = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fwCustomPropertiesFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeCustomProperties()
		{
			var extensions = _fwCustomPropertiesFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("CustomProperties", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "CustomProperties");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fwCustomPropertiesFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>";
			using (var tempModelVersionFile = new TempFile(data))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "CustomProperties");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(_fwCustomPropertiesFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "someext");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNotNull(_fwCustomPropertiesFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data = @"<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>";
			using (var tempModelVersionFile = new TempFile(data))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "CustomProperties");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNull(_fwCustomPropertiesFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void Find2WayDifferencesShouldReportThreeChanges()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformGoner' name='Goner' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Integer' />
<CustomField class='WfiWordform' key='WfiWordformNewby' name='Newby' type='Boolean' />
</AdditionalFields>";
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("fwtest.CustomProperties", parent);
				repositorySetup.ChangeFileAndCommit("fwtest.CustomProperties", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = _fwCustomPropertiesFileHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository).ToList();
				Assert.AreEqual(3, result.Count);
			}
		}

		[Test]
		public void PropertyDeletedReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformGoner' name='Goner' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void PropertyChangedReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformDirtball' name='Dirtball' type='Integer' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlChangedRecordReport>();
			}
		}

		[Test]
		public void NoChangesReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			// One deletion, one change, and one insertion, and one unchanged.
			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null,
					"CustomField",
					"key");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='OurNewClass' key='OurNewClassOurCertified' name='OurCertified' type='Boolean' /></AdditionalFields>");
			var theirContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='TheirNewClass' key='TheirNewClassTheirCertified' name='TheirCertified' type='Boolean' /></AdditionalFields>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""OurNewClassOurCertified""]", @"AdditionalFields/CustomField[@key=""TheirNewClassTheirCertified""]" }, null,
				0, 2);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void WinnerAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' /></AdditionalFields>");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" }, null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void LoserAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("</AdditionalFields>", "<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' /></AdditionalFields>");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" }, null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
		}

		[Test]
		public void WinnerDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void LoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, 0);
		}

		[Test]
		public void WinnerAndLoserBothDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Boolean' />", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]" },
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
		}

		[Test]
		public void WinnerAndLoserBothMadeSameChangeToElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Boolean""]" },
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void WinnerAndLoserBothChangedElementButInDifferentWays()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			var theirContent = commonAncestor.Replace("Boolean", "Binary");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				new List<string> { @"AdditionalFields/CustomField[@type=""Binary""]" },
				1, 0);
			_eventListener.AssertFirstConflictType<BothEditedAttributeConflict>();
		}

		[Test]
		public void WinnerChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Boolean", "Integer");
			const string theirContent = commonAncestor;

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void LoserChangedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
</AdditionalFields>";
			const string ourContent = commonAncestor;
			var theirContent = commonAncestor.Replace("Boolean", "Integer");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@type=""Integer""]" },
				null,
				0, 1);
			_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
		}

		[Test]
		public void WinnerEditedButLoserDeletedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("Binary", "Integer");
			var theirContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />", null);

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				1, 0);
			_eventListener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
		}

		[Test]
		public void WinnerDeletedButLoserEditedElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<AdditionalFields>
<CustomField class='WfiWordform' key='WfiWordformCertified' name='Certified' type='Boolean' />
<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />
</AdditionalFields>";
			var ourContent = commonAncestor.Replace("<CustomField class='WfiWordform' key='WfiWordformAttested' name='Attested' type='Binary' />", null);
			var theirContent = commonAncestor.Replace("Binary", "Integer");

			DoMerge(commonAncestor, ourContent, theirContent,
				new List<string> { @"AdditionalFields/CustomField[@key=""WfiWordformCertified""]", @"AdditionalFields/CustomField[@key=""WfiWordformAttested""]" },
				null,
				1, 0);
			_eventListener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		private string DoMerge(string commonAncestor, string ourContent, string theirContent,
			IEnumerable<string> matchesExactlyOne, IEnumerable<string> isNull,
			int expectedConflictCount, int expectedChangesCount)
		{
			string result;
			using (var ours = new TempFile(ourContent))
			using (var theirs = new TempFile(theirContent))
			using (var ancestor = new TempFile(commonAncestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(ours.Path, ancestor.Path, theirs.Path, situation);
				_eventListener = new ListenerForUnitTests();
				mergeOrder.EventListener = _eventListener;

				_fwCustomPropertiesFileHandler.Do3WayMerge(mergeOrder);
				result = File.ReadAllText(ours.Path);
				foreach (var query in matchesExactlyOne)
					XmlTestHelper.AssertXPathMatchesExactlyOne(result, query);
				if (isNull != null)
				{
					foreach (var query in isNull)
						XmlTestHelper.AssertXPathIsNull(result, query);
				}
				_eventListener.AssertExpectedConflictCount(expectedConflictCount);
				_eventListener.AssertExpectedChangesCount(expectedChangesCount);
			}
			return result;
		}
	}
}
