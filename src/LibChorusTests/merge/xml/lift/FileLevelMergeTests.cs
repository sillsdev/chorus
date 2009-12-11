using System;
using System.Xml;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using LibChorus.Tests.merge.xml;
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
			this._ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='usOnly'/>
						<entry id='sameInBoth'>
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByOther'/>
						<entry id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>us</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			this._theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
					   <entry id='sameInBoth'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
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
			this._ancestor = @"<?xml version='1.0' encoding='utf-8'?>
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
			LiftMerger merger = new LiftMerger(this._ours, this._theirs, this._ancestor,
											   new DropTheirsMergeStrategy());
			string result = merger.GetMergedLift();
			Assert.IsTrue(result.ToLower().Contains("utf-8"));
		}

		[Test]
		public void NewEntryFromUs_Conveyed()
		{
			LiftMerger merger = new LiftMerger(this._ours, this._theirs, this._ancestor,
											   new DropTheirsMergeStrategy());
			string result = merger.GetMergedLift();
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='usOnly']");
		}

		[Test]
		public void NewEntryFromThem_Conveyed()
		{
			LiftMerger merger = new LiftMerger(this._ours, this._theirs, this._ancestor,
											   new DropTheirsMergeStrategy());
			string result = merger.GetMergedLift();
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='themOnly']");
		}
		[Test]
		public void UnchangedEntryInBoth_NotDuplicated()
		{
			string all = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='sameInBoth'>
							<lexical-unit>
								<form lang='a'>
									<text>form a</text>
								</form>
							</lexical-unit>
						 </entry>
					</lift>";

			LiftMerger merger = new LiftMerger(all, all, all, null);
			//since we gave it null for the merger, it will die if tries to merge at all
			string result = merger.GetMergedLift();
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='sameInBoth']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form/text");
		}

		[Test]
		public void EntryRemovedByOther_Removed()
		{
			LiftMerger merger = new LiftMerger(this._ours, this._theirs, this._ancestor,
											   new DropTheirsMergeStrategy());
			string result = merger.GetMergedLift();
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[not(entry/@id='doomedByOther')]");
		}

		[Test]
		public void EntryRemovedByUs_Removed()
		{
			LiftMerger merger = new LiftMerger(this._ours, this._theirs, this._ancestor,
											   new PoorMansMergeStrategy()); // maybe shouldn't trust "dropTheirs" on this?
			string result = merger.GetMergedLift();
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift[not(entry/@id='doomedByUs')]");
		}

		[Test]
		public void OnlyModificationDateChanged_NoConflictOrRecordedChange()
		{
			string template = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='blah' guid='blah' dateModified='theDate'/>
					</lift>";

			LiftMerger merger = new LiftMerger(template.Replace("theDate", "2009-07-08T01:47:02Z"),
				template.Replace("theDate", "2009-07-09T01:47:03Z"),
				template.Replace("theDate", "2009-07-09T01:47:04Z"),
				new LiftEntryMergingStrategy(new NullMergeSituation()));

			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			string result = merger.GetMergedLift();
			Assert.AreEqual(0, listener.Conflicts.Count);
			Assert.AreEqual(0, listener.Changes.Count);
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

			LiftMerger merger = new LiftMerger(
				template.Replace("theDate", "2009-07-08T01:47:02Z").Replace("theForm", "1"),
				template.Replace("theDate", "2009-07-09T01:47:03Z").Replace("theForm", "2"),
				template.Replace("theDate", "2009-07-09T01:47:04Z").Replace("theForm", "3"),
				new LiftEntryMergingStrategy(new NullMergeSituation()));

			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;

			string result = merger.GetMergedLift();
			Assert.AreEqual(1, listener.Conflicts.Count);
			listener.AssertFirstConflictType<BothEditedTextConflict>();
			listener.AssertExpectedConflictCount(1);
			listener.AssertExpectedChangesCount(1);
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