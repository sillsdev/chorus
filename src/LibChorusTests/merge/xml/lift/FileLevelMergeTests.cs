using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

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
						<entry id='usOnly'/>
						<entry id='doomedByOther'/>
						<entry id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>us</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			_theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='themOnly'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs'/>

						<entry id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";
			_ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='doomedByOther'/>
						<entry id='doomedByUs'/>
						<entry id='brewingConflict'>
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='usOnly']");
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='themOnly']");
			}
		}

		[Test]
		public void UnchangedEntryInBoth_NotDuplicated()
		{
			const string all = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='sameInBoth'>
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[not(entry/@id='doomedByUs')]");
			}
		}

		[Test]
		public void OnlyModificationDateChanged_NoConflictOrRecordedChange()
		{
			const string template = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='blah' guid='blah' dateModified='theDate'/>
					</lift>";

			using (var oursTemp = new TempFile(template.Replace("theDate", "2009-07-08T01:47:02Z")))
			using (var theirsTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:03Z")))
			using (var ancestorTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:04Z")))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				Assert.AreEqual(0, listener.Conflicts.Count);
				Assert.AreEqual(0, listener.Changes.Count);
			}
		}

		[Test]
		public void Merge_RealConflictPlusModDateConflict_ModDateNotReportedAsConflict()
		{
			const string template = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='blah' guid='blah' dateModified='theDate'>
						   <lexical-unit>
								<form lang='a'>
									<text>theForm</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";

			using (var oursTemp = new TempFile(template.Replace("theDate", "2009-07-08T01:47:02Z").Replace("theForm", "1")))
			using (var theirsTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:03Z").Replace("theForm", "2")))
			using (var ancestorTemp = new TempFile(template.Replace("theDate", "2009-07-09T01:47:04Z").Replace("theForm", "3")))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				Assert.AreEqual(1, listener.Conflicts.Count);
				listener.AssertFirstConflictType<BothEditedTextConflict>();
				listener.AssertExpectedConflictCount(1);
				listener.AssertExpectedChangesCount(1);
			}
		}

		[Test]
		public void Empty_Ancestor_Adds_Children_From_Both()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='usOnly'/>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='themOnly'/>
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
					"entry", "id", LiftFileHandler.WritePreliminaryInformation);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				// REVIEW JohnT(RandyR): Should new entries from 'loser' register an addition change?
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[entry/@id='usOnly']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[entry/@id='themOnly']");
			}
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
	}
}