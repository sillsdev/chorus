using System;
using System.Xml;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
using Chorus.Tests.merge.xml;
using NUnit.Framework;

namespace Chorus.Tests.merge.xml.lift
{
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

						<entry id='newSensesCollision'>
							<sense>
								 <gloss lang='a'>
									<text></text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";
			this._ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='doomedByOther'/>
						<entry id='doomedByUs'/>
						<entry id='newSensesCollision'>
							<sense>
								 <gloss lang='a'>
									<text></text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
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