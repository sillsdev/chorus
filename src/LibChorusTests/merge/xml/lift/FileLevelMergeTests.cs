using System;
using System.IO;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.merge.xml.lift
{
	/// <summary>
	/// NB: this uses dummy strategies because the tests are not testing if the internals of the entries are merged
	/// </summary>
	[TestFixture]
	public class FileLevelMergeTests
	{
		private string _ours;
		private string _theirs;
		private string _ancestor;

		[SetUp]
		public void Setup()
		{
			/*
			 * Bad idea to put this in both ours and theirs, since it causes a crash in a Dictionary.
			 * The likelihood of two apps adding the same entry with the same guid is precisely 0, after all.
						<entry id='sameInBoth'>
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
			*/
			_ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='usOnly' guid='c1ecf892-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByOther' guid='c1ecf893-e382-11de-8a39-0800200c9a66' />
						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>us</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			_theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='themOnly' guid='c1ecf895-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs' guid='c1ecf896-e382-11de-8a39-0800200c9a66' />

						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";
			_ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='doomedByOther' guid='c1ecf893-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUs' guid='c1ecf896-e382-11de-8a39-0800200c9a66' />
						<entry id='brewingConflict' guid='c1ecf894-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
		}

		[Test]
		public void ResultIsUtf8()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
		}

		[Test]
		public void NewEntryFromUs_Conveyed()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='usOnly']");
			}
		}

		[Test]
		public void WeAddedNewEntryInfileWithNoEntries()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
						<entry id='ourNew' guid='c1ecf898-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='ourNew']");
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void TheyAddedNewEntryInfileWithNoEntries()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header><ranges>alphastuff</ranges></header>
						<entry id='theirNew' guid='c1ecf898-e382-11de-8a39-0800200c9a66' />
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='theirNew']");
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void NewEntryFromUs_HasNoChangeReports()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='commonOldie' guid='c1ecf897-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='ourNew' guid='c1ecf898-e382-11de-8a39-0800200c9a66' />
						<entry id='commonOldie' guid='c1ecf897-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='commonOldie' guid='c1ecf897-e382-11de-8a39-0800200c9a66' />
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='ourNew']");
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void NewEntryFromThem_HasAdditionChangeReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='commonOldie' guid='c1ed1f90-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='commonOldie' guid='c1ed1f90-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='commonOldie' guid='c1ed1f90-e382-11de-8a39-0800200c9a66' />
						<entry id='theirNew' guid='c1ed1f91-e382-11de-8a39-0800200c9a66' />
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='theirNew']");
				listener.AssertExpectedConflictCount(0);
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void NewEntryFromThem_Conveyed()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='themOnly']");
			}
		}

		[Test]
		public void UnchangedEntryInBoth_NotDuplicated()
		{
			const string all = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='sameInBoth' guid='c1ed1f92-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			using (var oursTemp = new TempFile(all))
			using (var theirsTemp = new TempFile(all))
			using (var ancestorTemp = new TempFile(all))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='sameInBoth']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form/text");
			}
		}

		[Test]
		public void EntryRemovedByOther_Removed()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[not(entry/@id='doomedByOther')]");
			}
		}

		[Test]
		public void EntryRemovedByUs_Removed()
		{
			using (var oursTemp = new TempFile(_ours))
			using (var theirsTemp = new TempFile(_theirs))
			using (var ancestorTemp = new TempFile(_ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new PoorMansMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[not(entry/@id='doomedByUs')]");
			}
		}

		[Test]
		public void OnlyModificationDateChanged_NoConflictOrRecordedChange()
		{
			const string template = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='blah' guid='c1ed1f93-e382-11de-8a39-0800200c9a66' dateModified='theDate'/>
					</lift>";

			using (var oursTemp = new TempFile(template.Replace("theDate", "2009-07-08T01:47:02Z")))
			using (var theirsTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:03Z")))
			using (var ancestorTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:04Z")))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				Assert.AreEqual(0, listener.Conflicts.Count);
				Assert.AreEqual(0, listener.Changes.Count);
			}
		}

		[Test]
		public void Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
		{
			const string template = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='blah' guid='c1ed1f94-e382-11de-8a39-0800200c9a66' dateModified='theDate'>
						   <lexical-unit>
								<form lang='a'>
									<text>theForm</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";

			// NB: dateModified is set to ignore for LiftEntryMergingStrategy, thus no conflict report.
			using (var oursTemp = new TempFile(template.Replace("theDate",		"2009-07-08T01:47:06Z").Replace("theForm", "1")))
			using (var theirsTemp = new TempFile(template.Replace("theDate",	"2009-07-09T01:47:05Z").Replace("theForm", "2")))
			using (var ancestorTemp = new TempFile(template.Replace("theDate",	"2009-07-09T01:47:04Z").Replace("theForm", "3")))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				listener.AssertExpectedConflictCount(1);
				listener.AssertFirstConflictType<XmlTextBothEditedTextConflict>();
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void Empty_Ancestor_Adds_Children_From_Both()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='usOnly' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='themOnly' guid='c1ed1f96-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ancestor = @"<lift version='0.12'></lift>";
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new PoorMansMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				// REVIEW JohnT(RandyR): Should new entries from 'loser' register an addition change?
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[entry/@id='usOnly']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[entry/@id='themOnly']");
				listener.AssertExpectedChangesCount(0);
				listener.AssertExpectedConflictCount(0);
			}
		}

		[Test]
		public void OldStyle_DoomedByUsEditedByThem_HasOneConflict()
		{
			// Old Style means the deleted entry was just marked as deleted with the dateDeleted attr.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f97-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f97-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f97-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>newByThem</text>
								 </gloss>
							</sense>
						</entry>

					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		[Test]
		public void OldStyle_DoomedByThemEditedByUs_HasOneConflict()
		{
			// Old Style means the deleted entry was just marked as deleted with the dateDeleted attr.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f99-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9a-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f99-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9a-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>newByUs</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f99-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9a-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />

					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
		}

		[Test]
		public void NewStyle_DoomedByUsEditedByThem_HasOneConflict()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9b-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUsEditedByThem' guid='c1ed1f9c-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9b-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9b-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUsEditedByThem' guid='c1ed1f9c-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>newByThem</text>
								 </gloss>
							</sense>
						</entry>

					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<RemovedVsEditedElementConflict>();
		}

		[Test]
		public void NewStyle_DoomedByThemEditedByUs_HasOneConflict()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>newByUs</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedChangesCount(0);
			listener.AssertExpectedConflictCount(1);
			listener.AssertFirstConflictType<EditedVsRemovedElementConflict>();
		}

		[Test]
		public void DoomedByUs_NewWay_AndByThem_OldWay_HasNoChangeReports()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9f-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByBoth' guid='c1ed1fa0-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			// 'ours' does the newer removal.
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9f-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";
			// 'theirs' does the older dateDeleted marking.
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9f-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByBoth' guid='c1ed1fa0-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />
					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test]
		public void DoomedByUs_OldWay_AndByThem_NewWay_HasNoChangeReports()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1fa1-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByBoth' guid='c1ed1fa2-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			// 'ours' does the older dateDeleted marking.
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1fa1-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByBoth' guid='c1ed1fa2-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />
					</lift>";
			// 'theirs' does the newer removal.
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1fa1-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			var listener = new ListenerForUnitTests();
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new DropTheirsMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.ToLower().Contains("utf-8"));
			}
			listener.AssertExpectedConflictCount(0);
			listener.AssertExpectedChangesCount(0);
		}

		[Test, Ignore("Not implemented")]
		public void ReorderedEntry_Reordered()
		{
		}

		[Test, Ignore("Not implemented")]
		public void OlderLiftVersion_Handled()
		{//what to do?
		}

		[Test, Ignore("Not implemented")]
		public void NewerLiftVersion_Handled()
		{//what to do?
		}

		[Test, Ignore("Not implemented")]
		public void MetaData_Preserved()
		{
		}

		[Test, Ignore("Not implemented")]
		public void MetaData_Merged()
		{
		}

		[Test]
		public void BothAddedHeaderButWithDifferentContentInEach()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='parent' guid='c1ed1fa3-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form parent</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";
			var alpha = ancestor.Replace("<entry id", "<header><description>alphastuff</description></header><entry id");
			var beta = ancestor.Replace("<entry id", "<header><ranges>betastuff</ranges></header><entry id");

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry",
					"guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.Contains("<header>"));
				Assert.IsTrue(result.Contains("<description>"));
				Assert.IsTrue(result.Contains("<ranges>"));
				listener.AssertExpectedChangesCount(2);
			}
		}

		[Test]
		public void WinnerEditedLoserDidNothing()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='parent' guid='c1ed1fa3-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form parent</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";
			var alpha = ancestor.Replace("form parent", "form alpha");
			const string beta = ancestor;

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
											"header",
											"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.Contains("form alpha"));
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void LoserEditedWinnerDidNothing()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='parent' guid='c1ed1fa3-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form parent</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";
			const string alpha = ancestor;
			var beta = ancestor.Replace("form parent", "form beta");

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
											"header",
											"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Assert.IsTrue(result.Contains("form beta"));
				listener.AssertExpectedChangesCount(0);
			}
		}

		[Test]
		public void GetMergedLift_LiftHasNoConflicts_IndentingIsCorrect()
		{
			const string alpha = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa3-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string beta = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='beta' guid='c1ed1fa4-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form beta</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
					</lift>";

			string expectedResult =    ("<?xml version='1.0' encoding='utf-8'?>\r\n"
										+ "<lift\r\n"
										+ "\tproducer='WeSay 1.0.0.0'\r\n"
										+ "\tversion='0.10'>\r\n"
										+ "\t<entry\r\n"
										+ "\t\tid='alpha'\r\n"
										+ "\t\tguid='c1ed1fa3-e382-11de-8a39-0800200c9a66'>\r\n"
										+ "\t\t<lexical-unit>\r\n"
										+ "\t\t\t<form\r\n"
										+ "\t\t\t\tlang='a'>\r\n"
										+ "\t\t\t\t<text>form alpha</text>\r\n"
										+ "\t\t\t</form>\r\n"
										+ "\t\t</lexical-unit>\r\n"
										+ "\t</entry>\r\n"
										+ "\t<entry\r\n"
										+ "\t\tid='beta'\r\n"
										+ "\t\tguid='c1ed1fa4-e382-11de-8a39-0800200c9a66'>\r\n"
										+ "\t\t<lexical-unit>\r\n"
										+ "\t\t\t<form\r\n"
										+ "\t\t\t\tlang='a'>\r\n"
										+ "\t\t\t\t<text>form beta</text>\r\n"
										+ "\t\t\t</form>\r\n"
										+ "\t\t</lexical-unit>\r\n"
										+ "\t</entry>\r\n"
										+ "</lift>").Replace('\'', '\"');

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
											"header",
											"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Console.WriteLine(result);
				Assert.AreEqual(expectedResult, result);
			}

		}

		[Test]
		public void GetMergedLift_LiftConflicts_IndentingIsCorrect()
		{
			const string alpha = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa5-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha1</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string beta = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa5-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha2</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa5-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			string expectedResult =    ("<?xml version='1.0' encoding='utf-8'?>\r\n" +
										"<lift\r\n" +
										"\tproducer='WeSay 1.0.0.0'\r\n" +
										"\tversion='0.10'>\r\n" +
										"\t<entry\r\n" +
										"\t\tid='alpha'\r\n" +
										"\t\tguid='c1ed1fa5-e382-11de-8a39-0800200c9a66'>\r\n" +
										"\t\t<lexical-unit>\r\n" +
										"\t\t\t<form\r\n" +
										"\t\t\t\tlang='a'>\r\n" +
										"\t\t\t\t<text>form alpha1</text>\r\n" +
										"\t\t\t</form>\r\n" +
										"\t\t</lexical-unit>\r\n" +
										"\t</entry>\r\n" +
										"</lift>").Replace('\'', '\"');

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation)
									{EventListener = listener};
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
											"header",
											"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Console.WriteLine(result);
				Assert.AreEqual(expectedResult, result);
			}
		}

		[Test]
		public void GetMergedLift_LiftIsUnchanged_IndentingIsCorrect()
		{
			const string alpha = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa6-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string beta = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa6-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='alpha' guid='c1ed1fa6-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='a'>
									<text>form alpha</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			string expectedResult =    ("<?xml version='1.0' encoding='utf-8'?>\r\n" +
										"<lift\r\n" +
										"\tproducer='WeSay 1.0.0.0'\r\n" +
										"\tversion='0.10'>\r\n" +
										"\t<entry\r\n" +
										"\t\tid='alpha'\r\n" +
										"\t\tguid='c1ed1fa6-e382-11de-8a39-0800200c9a66'>\r\n" +
										"\t\t<lexical-unit>\r\n" +
										"\t\t\t<form\r\n" +
										"\t\t\t\tlang='a'>\r\n" +
										"\t\t\t\t<text>form alpha</text>\r\n" +
										"\t\t\t</form>\r\n" +
										"\t\t</lexical-unit>\r\n" +
										"\t</entry>\r\n" +
										"</lift>").Replace('\'','\"');

			using (var oursTemp = new TempFile(alpha))
			using (var theirsTemp = new TempFile(beta))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
											"header",
											"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				Console.WriteLine(result);
				Assert.AreEqual(expectedResult, result);
			}

		}

		/*			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='ours' guid='aaed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
							<note><form lang='es'>hola</form></note>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='theirs' guid='bbed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
							<note><form lang='en'>hello</form></note>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
						</entry>
					</lift>";*/

		/// <summary>
		/// this is a regression, from http://jira.palaso.org/issues/browse/CHR-10
		/// We would expect to either get an exception, or have the system do its best.
		/// At the time of this error (Mar 2012), it seemed to do the worst of both: it quitely didn't merge,
		/// thus giving us "data loss"
		/// </summary>
		[Test]
		public void DuplicateGuids_StillMergesWhatComesNext()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'/>
						<entry id='newGuy' guid='aaed1f95-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
					   <entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'/>
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />

						<!-- everthing above this line was being merged, but not this -->
						<entry id='lostBoy' guid='bbed1f95-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'/>
					</lift>";
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new PoorMansMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);

				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//entry[@id='newGuy']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//entry[@id='lostBoy']");
				Assert.AreEqual(1, listener.Warnings.Count);
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[0].GetType());
			}
		}

		[Test]
		public void BothEditedFoo_WithEditVsDeleteOfBar_AndNoChangesToDull_ProducesTwoConflictRreports()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='dull' guid='C1EDBBDE-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>dull</text>
								</form>
							</lexical-unit>
						</entry>
						<entry id='foo' guid='C1EDBBDF-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>foo</text>
								</form>
							</lexical-unit>
						</entry>
						<entry id='bar' guid='C1EDBBE0-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>bar</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='dull' guid='C1EDBBDE-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>dull</text>
								</form>
							</lexical-unit>
						</entry>
						<entry id='foo' guid='C1EDBBDF-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>ourfoo</text>
								</form>
							</lexical-unit>
						</entry>
						<entry id='bar' guid='C1EDBBE0-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>mybar</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='dull' guid='C1EDBBDE-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>dull</text>
								</form>
							</lexical-unit>
						</entry>
						<entry id='foo' guid='C1EDBBDF-E382-11DE-8A39-0800200C9A66'>
							<lexical-unit>
								<form lang='en'>
									<text>theirfoo</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";

			// This tests: http://jira.palaso.org/issues/browse/CHR-13
			// In a test where all I did was an edited vs. delete test, the resulting conflict note listed the element as "unknown".
			// In a test where all I both a) had both parties edit the same field on record A and b) had parties edit vs. delete record B, the all resulting conflict notes were attached to A.
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				// 'We' (O) are set to win.
				// Both edited foo: O should win.
				// O edited bar, T deleted it. O should win.
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation
									{
										AlphaUserId = "O",
										BetaUserId = "T"
									};
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='dull']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='foo']/lexical-unit/form/text[text()='ourfoo']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='bar']");
				Assert.AreEqual(2, listener.Conflicts.Count);
				var firstConflict = listener.Conflicts[0];
				var secondConflict = listener.Conflicts[1];
				Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), firstConflict.GetType());
				Assert.AreEqual(typeof(EditedVsRemovedElementConflict), secondConflict.GetType());

				// Doesn't work with ListenerForUnitTests, as ListenerForUnitTests doesn't set the Context on the conflict, as does the Chorus notes listener.
				//var annotationXml = XmlTestHelper.WriteConflictAnnotation(firstConflict);
				//var annotationXml = XmlTestHelper.WriteConflictAnnotation(secondConflict);
			}

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				// 'They' (T) are set to win.
				// Both edited foo: 'T' should win
				// O edited bar, T deleted it. O should win.
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituationTheyWin
					{
						AlphaUserId = "O",
						BetaUserId = "T"
					};
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='dull']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='foo']/lexical-unit/form/text[text()='theirfoo']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='bar']");
				Assert.AreEqual(2, listener.Conflicts.Count);
				var firstConflict = listener.Conflicts[0];
				var secondConflict = listener.Conflicts[1];
				Assert.AreEqual(typeof(XmlTextBothEditedTextConflict), firstConflict.GetType());
				Assert.AreEqual(typeof(RemovedVsEditedElementConflict), secondConflict.GetType());

				// Doesn't work with ListenerForUnitTests, as ListenerForUnitTests doesn't set the Context on the conflict, as does the Chorus notes listener.
				//var annotationXml = XmlTestHelper.WriteConflictAnnotation(firstConflict);
				//var annotationXml = XmlTestHelper.WriteConflictAnnotation(secondConflict);
			}
		}

		[Test]
		public void NewWSAddedToNote_Merged()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
							<note><form lang='es'>hola</form></note>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
					   <entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
							<note>
								<form lang='en'>hello</form>
							</note>
							<note><form lang='es'>hola</form></note>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>

						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'>
							<note><form lang='es'>hola</form></note>
						</entry>
					</lift>";
			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new PoorMansMergeStrategy(),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//note/form[@lang='es']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "//note/form[@lang='en']");
			}
		}
	}
}
