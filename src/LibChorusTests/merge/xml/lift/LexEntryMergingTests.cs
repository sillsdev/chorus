using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;


namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class LexEntryMergingTests
	{

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_AncestorAndOursSame_ResultHasTheirsAlso()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2012-05-18T08:31:54Z'
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<text>asatɛn</text>
							</form>
						</lexical-unit>

					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						dateCreated='2011-03-09T05:08:44Z'
						dateModified='2012-05-14T02:38:00Z'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<text>asatɛn</text>
							</form>
							<form
								lang='ldb-Zxxx-x-audio'>
								<text>asatɛn-63472603074018.wav</text>
							</form>
						</lexical-unit>
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2011-04-08T16:53:45Z'
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<text>asatɛn</text>
							</form>
						</lexical-unit>

					</entry>
				</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				// Check that the audio made it into the merge.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-fonipa-x-emic']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-Zxxx-x-audio']");
			}

		}

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_DefinitionAncestorAndOursSameTheirsHasAdditionalForm_ResultHasAdditionalFormAlso()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2012-05-18T08:31:54Z'
						id='???_00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<sense id='123'>
							<definition>
								<form lang='a'>
									<text>aSense</text>
								</form>
							</definition>
						 </sense>
					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='???_00853b73-fda2-4b12-8a89-6957cc7e7e79'
						dateCreated='2011-03-09T05:08:44Z'
						dateModified='2012-05-14T02:38:00Z'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<sense id='123'>
							<definition>
								<form lang='a'>
									<text>aSense</text>
								</form>
								<form lang='b'>
									<text>bSense</text>
								</form>
							</definition>
						 </sense>
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2011-04-08T16:53:45Z'
						id='???_00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<sense id='123'>
							<definition>
								<form lang='a'>
									<text>aSense</text>
								</form>
							</definition>
						 </sense>
					</entry>
				</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				// Check that the audio made it into the merge.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/sense/definition/form[@lang='a']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/sense/definition/form[@lang='b']");
			}

		}
	}
}
