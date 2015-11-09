using Chorus.FileTypeHandlers;
using Chorus.FileTypeHandlers.xml;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.merge.xml.generic.xmldiff
{
	[TestFixture]
	public class Xml2WayDifferTests
	{
		[Test]
		public void OurNewEntryReported()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";

			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='newGuy'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header",
					"entry", "id");
				differ.ReportDifferencesToListener(); ;
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlAdditionChangeReport>();
			}
		}

		[Test]
		public void OurDeletedEntryReported()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";

			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
					</lift>";

			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header", "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void WeMarkedEntryAsDeleted_ReportedAsDeletion()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1'/>
						<entry id='old2'/>
					</lift>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header", "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlDeletionChangeReport>();
			}
		}

		[Test]
		public void DeletionReport_Not_ProducedForDeletionInParentAndChild()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry	id='old1'	dateDeleted='2009-06-16T06:14:20Z'/>
						<entry id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					"header", "entry", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void Deletion_WasTombstoneNowMissing_NoDeletionReport()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<item id='old1' dateDeleted='2009-06-16T06:14:20Z'/>
						<item id='old2'/>
					</lift>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<item id='old2'/>
					</lift>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null, "item", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void GuidAttrBeforeIdAttrDoesNotGenerateReports()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<root>
						<item id='fuzz-old1' guid='old1'/>
					</root>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<root>
						<item guid='old1' id='fuzz-old1'/>
					</root>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null, "item", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void SimpleChangeGeneratesReport()
		{
			const string parent = @"<?xml version='1.0' encoding='utf-8'?>
					<root>
						<item id='old1'/>
					</root>";
			const string child = @"<?xml version='1.0' encoding='utf-8'?>
					<root>
						<item id='old1' newAttr='newValue' />
					</root>";
			using (var parentTempFile = new TempFile(parent))
			using (var childTempFile = new TempFile(child))
			{
				var listener = new ListenerForUnitTests();
				var differ = Xml2WayDiffer.CreateFromFiles(parentTempFile.Path, childTempFile.Path, listener,
					null, "item", "id");
				differ.ReportDifferencesToListener();
				listener.AssertExpectedChangesCount(1);
				listener.AssertFirstChangeType<XmlChangedRecordReport>();
				listener.AssertExpectedConflictCount(0);
			}
		}
	}
}