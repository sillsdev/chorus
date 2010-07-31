using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml;
using NUnit.Framework;

namespace LiftIO.Tests.Merging
{
	public class PoorMansMergingStrategyTests
	{
		[Test]
		public void Conflict_TheirsAppearsInCollisionNote()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission'>
							<lexical-unit>
								<form lang='x'>
									<text>ours</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission'>
							<lexical-unit>
								<form lang='x'>
									<text>theirs</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission'/>
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation);
				var merger = new LiftMerger(mergeOrder, oursTemp.Path, theirsTemp.Path,
					new PoorMansMergeStrategy(),
					ancestorTemp.Path, mergeOrder.MergeSituation.AlphaUserId);
				//since we gave it null for the merger, it will die if tries to merge at all
				merger.DoMerge(mergeOrder.pathToOurs);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='lexicalformcollission']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry");//just one
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/field[@type='mergeConflict']/trait[@name = 'looserData']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/field[@type='mergeConflict' and @dateCreated]");
			}
		}

		[Test, Ignore("Not implemented")]
		public void EachHasNewSense_BothSensesCoveyed()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'>
							<sense>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'>
							<sense>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'/>
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation);
				var merger = new LiftMerger(mergeOrder, oursTemp.Path, theirsTemp.Path,
					new PoorMansMergeStrategy(),
					ancestorTemp.Path, mergeOrder.MergeSituation.AlphaUserId);
				merger.DoMerge(mergeOrder.pathToOurs);
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense/gloss/text='ourSense']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense/gloss/text='theirSense']");
			}
		}
	}
}
