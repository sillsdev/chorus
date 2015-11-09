using System;
using System.Linq;
using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.merge.xml.lift
{
	/// <summary>
	/// NB: this uses dummy strategies because the tests are not testing if the internals of the entries are merged
	/// </summary>
	[TestFixture]
	public class FileLevelDiffTests
	{
		[Test]
		public void NewEntryFromUs_Reported()
		{
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='newGuy'/>
						<entry id='old2'/>
					</lift>";
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
					"entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlAdditionChangeReport>();
			}
		}

		[Test]
		public void WeRemovedEntry_Reported()
		{
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
					</lift>";
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
															 "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void WeMarkedEntryAsDeleted_ReportedAsDeletion()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
															 "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void DeletionReport_Not_ProducedForDeletionInParentAndChild()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry	id='old1'	dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
															 "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
			}
		}

		/// <summary>
		/// well, there is an exception lower down, but not way up at this level
		/// </summary>
		[Test]
		public void DuplicateIdInParentEntry_EmitsWarning()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old1'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry	id='old1'	dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
															 "entry", "id");
				differ.ReportDifferencesToListener();
				Assert.AreEqual(1, listener.Warnings.Count);
			}
		}

		[Test]
		public void DuplicateIdInChildEntryEmitsWarning()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry	id='old1'	dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old1'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header","entry", "id");
				differ.ReportDifferencesToListener();
				Assert.AreEqual(1, listener.Warnings.Count);
			}
		}

		[Test]
		public void Deletion_WasTombstoneNowMissing_NoDeletionReport()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
															 "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
			}
		}
		[Test]
		public void GuidAttrBeforeIdAttrDoesNotGenerateReports()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='fuzz-old1' guid='old1'/>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry guid='old1' id='fuzz-old1'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header", "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void IdHasEntityDoesNotGenerateReports()
		{
			var parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
	<entry
		id=""Id'dPrematurely_18d66025-59bc-4bd0-b59c-0f01ae09dede""
		dateCreated='2009-09-14T10:02:26Z'
		dateModified='2009-09-14T10:26:21Z'
		guid='18d66025-59bc-4bd0-b59c-0f01ae09dede'>
	</entry>
					</lift>";
			var child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
<entry dateCreated='2009-09-14T10:02:26Z' dateModified='2009-09-14T10:26:21Z' guid='18d66025-59bc-4bd0-b59c-0f01ae09dede' id=""Id&apos;dPrematurely_18d66025-59bc-4bd0-b59c-0f01ae09dede"">
</entry>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header", "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void DeletionReport_Not_ProducedForDeletedAnnotationUsingNotesHandler()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<notes version='0'>
						<annotation guid='old1'/>
						<annotation guid='soonToBeGoner'/>
					</notes>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<notes version='0'>
						<annotation guid='old1'/>
					</notes>";

			// Make sure the common differ code does produce the deletion report.
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path,
					listener,
					null,
					"annotation",
					"guid");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
			// Now make sure the ChorusNotesFileHandler filters it out, and does not return it,
			// as per the original notes differ code.
			var notesHandler = (from handler in ChorusFileTypeHandlerCollection.CreateWithInstalledHandlers().Handlers
								where handler.GetType().Name == "ChorusNotesFileHandler"
								select handler).First();
			using (var repositorySetup = new RepositorySetup("randy"))
			{
				repositorySetup.AddAndCheckinFile("notestest.ChorusNotes", parent);
				repositorySetup.ChangeFileAndCommit("notestest.ChorusNotes", child, "change it");
				var hgRepository = repositorySetup.Repository;
				var allRevisions = (from rev in hgRepository.GetAllRevisions()
									orderby rev.Number.LocalRevisionNumber
									select rev).ToList();
				var first = allRevisions[0];
				var second = allRevisions[1];
				var firstFiR = hgRepository.GetFilesInRevision(first).First();
				var secondFiR = hgRepository.GetFilesInRevision(second).First();
				var result = notesHandler.Find2WayDifferences(firstFiR, secondFiR, hgRepository);
				Assert.AreEqual(0, result.Count());
			}
		}
	}
}
