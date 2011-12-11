using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.FileTypeHanders;
using Chorus.FileTypeHanders.xml;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.FileHandlers.FieldWorks.Linguistics.Reversals
{
	public class FieldWorksReversalTypeHandlerTests
	{
		private IChorusFileTypeHandler _fwReversalFileHandler;
		private ListenerForUnitTests _eventListener;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			_fwReversalFileHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
									  where handler.GetType().Name == "FieldWorksReversalTypeHandler"
											  select handler).First();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			_fwReversalFileHandler = null;
		}

		[SetUp]
		public void TestSetup()
		{
			_eventListener = new ListenerForUnitTests();
		}

		[TearDown]
		public void TestTearDown()
		{
			_eventListener = null;
		}

		[Test]
		public void DescribeInitialContentsShouldHaveAddedForLabel()
		{
			var initialContents = _fwReversalFileHandler.DescribeInitialContents(null, null);
			Assert.AreEqual(1, initialContents.Count());
			var onlyOne = initialContents.First();
			Assert.AreEqual("Added", onlyOne.ActionLabel);
		}

		[Test]
		public void ExtensionOfKnownFileTypesShouldBeReversal()
		{
			var extensions = _fwReversalFileHandler.GetExtensionsOfKnownTextFileTypes().ToArray();
			Assert.AreEqual(1, extensions.Count(), "Wrong number of extensions.");
			Assert.AreEqual("reversal", extensions[0]);
		}

		[Test]
		public void ShouldNotBeAbleToValidateIncorrectFormatFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsFalse(_fwReversalFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateInProperlyFormatedFile()
		{
			const string data = @"<Reversal>
</Reversal>";
			using (var tempModelVersionFile = new TempFile(data))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(_fwReversalFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToDoAllCanOperations()
		{
			const string data = @"<Reversal>
</Reversal>";
			using (var tempModelVersionFile = new TempFile(data))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsTrue(_fwReversalFileHandler.CanValidateFile(newpath));
				Assert.IsTrue(_fwReversalFileHandler.CanDiffFile(newpath));
				Assert.IsTrue(_fwReversalFileHandler.CanMergeFile(newpath));
				Assert.IsTrue(_fwReversalFileHandler.CanPresentFile(newpath));
				Assert.IsTrue(_fwReversalFileHandler.CanValidateFile(newpath));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldNotBeAbleToValidateFile()
		{
			using (var tempModelVersionFile = new TempFile("<classdata />"))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNotNull(_fwReversalFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void ShouldBeAbleToValidateFile()
		{
			const string data =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
</Reversal>";
			using (var tempModelVersionFile = new TempFile(data))
			{
				var newpath = Path.ChangeExtension(tempModelVersionFile.Path, "reversal");
				File.Copy(tempModelVersionFile.Path, newpath, true);
				Assert.IsNull(_fwReversalFileHandler.ValidateFile(newpath, null));
				File.Delete(newpath);
			}
		}

		[Test]
		public void NewEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, _eventListener,
					"header",
					"ReversalIndexEntry",
					"guid");
				differ.ReportDifferencesToListener();
				_eventListener.AssertExpectedChangesCount(1);
				_eventListener.AssertFirstChangeType<XmlAdditionChangeReport>();
			}
		}

		[Test]
		public void NewNestedEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66' >
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66' >
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
	<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
	</ReversalIndexEntry>
</ReversalIndexEntry>
</Reversal>";
			// c1ed46b9-e382-11de-8a39-0800200c9a66

			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, _eventListener,
					"header",
					"ReversalIndexEntry",
					"guid");
				differ.ReportDifferencesToListener();
				_eventListener.AssertExpectedChangesCount(1);
				_eventListener.AssertFirstChangeType<XmlChangedRecordReport>();
			}
		}

		[Test]
		public void DeletedEntryInChildReported()
		{
			const string parent =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
<ReversalIndexEntry guid='c1ed46ba-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			const string child =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='c1ed46b9-e382-11de-8a39-0800200c9a66'>
</ReversalIndexEntry>
</Reversal>";

			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, _eventListener,
					"header",
					"ReversalIndexEntry",
					"guid");
				differ.ReportDifferencesToListener();
				_eventListener.AssertExpectedChangesCount(1);
				_eventListener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void WinnerAndLoserEachAddedNewElement()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieOurs'/></Reversal>");
			var theirContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieTheirs'/></Reversal>");

			FieldWorksTestServices.DoMerge(
				_fwReversalFileHandler,
				commonAncestor, ourContent, theirContent,
				new List<string> { @"Reversal/ReversalIndexEntry[@guid=""oldie""]", @"Reversal/ReversalIndexEntry[@guid=""newbieOurs""]", @"Reversal/ReversalIndexEntry[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void WinnerAndLoserEachAddedNewSubentry()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</ReversalIndexEntry>", "<Subentries><ReversalIndexEntry guid='newbieOurs'/></Subentries></ReversalIndexEntry>");
			var theirContent = commonAncestor.Replace("</ReversalIndexEntry>", "<Subentries><ReversalIndexEntry guid='newbieTheirs'/></Subentries></ReversalIndexEntry>");

			FieldWorksTestServices.DoMerge(
				_fwReversalFileHandler,
				commonAncestor, ourContent, theirContent,
				new List<string> { @"Reversal/ReversalIndexEntry/Subentries/ReversalIndexEntry[@guid=""newbieOurs""]", @"Reversal/ReversalIndexEntry/Subentries/ReversalIndexEntry[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void WinnerAddedNewEntryLoserAddedNewSubentry()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("</Reversal>", "<ReversalIndexEntry guid='newbieOurs'/></Reversal>");
			var theirContent = commonAncestor.Replace("</ReversalIndexEntry>", "<Subentries><ReversalIndexEntry guid='newbieTheirs'/></Subentries></ReversalIndexEntry>");

			FieldWorksTestServices.DoMerge(
				_fwReversalFileHandler,
				commonAncestor, ourContent, theirContent,
				new List<string> { @"Reversal/ReversalIndexEntry[@guid=""oldie""]", @"Reversal/ReversalIndexEntry[@guid=""newbieOurs""]", @"Reversal/ReversalIndexEntry[@guid=""oldie""]/Subentries/ReversalIndexEntry[@guid=""newbieTheirs""]" }, null,
				0, new List<Type>(),
				2, new List<Type> { typeof(XmlChangedRecordReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothEditedACatInConflictingWay()
		{
			const string commonAncestor =
@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<header>
<ReversalIndex guid='c1ed46b8-e382-11de-8a39-0800200c9a66'>
	<PartsOfSpeech>
		<CmPossibilityList guid ='c1ed46bb-e382-11de-8a39-0800200c9a66' >
			<Possibilities>
				<PartOfSpeech guid ='c1ed6db0-e382-11de-8a39-0800200c9a66'>
					<Name>
						<AUni
							ws='en'>commonName</AUni>
					</Name>
				</PartOfSpeech>
			</Possibilities>
		</CmPossibilityList>
	</PartsOfSpeech>
</ReversalIndex>
</header>
<ReversalIndexEntry guid='oldie'>
</ReversalIndexEntry>
</Reversal>";
			var ourContent = commonAncestor.Replace("commonName", "OurName");
			var theirContent = commonAncestor.Replace("commonName", "TheirName");

			var result = FieldWorksTestServices.DoMerge(
				_fwReversalFileHandler,
				commonAncestor, ourContent, theirContent,
				null, null,
				1, new List<Type> { typeof(BothEditedTheSameElement) },
				0, new List<Type>());

			Assert.IsTrue(result.Contains("OurName"));
		}
	}
}