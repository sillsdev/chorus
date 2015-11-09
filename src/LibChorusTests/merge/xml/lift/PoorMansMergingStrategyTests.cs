using System.IO;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;

namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class PoorMansMergingStrategyTests
	{
		[Test]
		public void Conflict_TheirsAppearsInCollisionNote()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission' guid='c1ed1fa7-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='x'>
									<text>ours</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission' guid='c1ed1fa7-e382-11de-8a39-0800200c9a66' >
							<lexical-unit>
								<form lang='x'>
									<text>theirs</text>
								</form>
							</lexical-unit>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='lexicalformcollission' guid='c1ed1fa7-e382-11de-8a39-0800200c9a66' />
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
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='lexicalformcollission']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry");//just one
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/field[@type='mergeConflict']/trait[@name = 'looserData']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/field[@type='mergeConflict' and @dateCreated]");
			}
		}

		[Test]
		public void EachHasNewSense_BothSensesCoveyed()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test' guid='c1ed1fa8-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test' guid='c1ed1fa8-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test' guid='c1ed1fa8-e382-11de-8a39-0800200c9a66' />
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
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
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense/gloss/text='ourSense']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense/gloss/text='theirSense']");
			}
		}
	}
}
