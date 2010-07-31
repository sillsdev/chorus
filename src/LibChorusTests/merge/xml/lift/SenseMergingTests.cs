using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.Tests.merge.xml.generic;
using NUnit.Framework;

namespace LibChorus.Tests.merge.xml.lift
{
	[TestFixture]
	public class SenseMergingTests
	{
		[Test]
		public void EachHasNewSense_BothSensesCoveyed()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='456'>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6' />
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation);
				var merger = new LiftMerger(mergeOrder, oursTemp.Path, theirsTemp.Path, new LiftEntryMergingStrategy(situation), ancestorTemp.Path, mergeOrder.MergeSituation.AlphaUserId);
				var listener = new ListenerForUnitTests();
				merger.EventListener = listener;
				merger.DoMerge(mergeOrder.pathToOurs);
				//string result = merger.GetMergedLift();
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense[@id='123']/gloss/text='ourSense']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense[@id='456']/gloss/text='theirSense']");
			}
		}

		[Test]
		public void GetMergedLift_ConflictingGlosses_ListenerIsNotifiedOfBothEditedConflict()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation);
				var merger = new LiftMerger(mergeOrder, oursTemp.Path, theirsTemp.Path, new LiftEntryMergingStrategy(situation), ancestorTemp.Path, mergeOrder.MergeSituation.AlphaUserId);
				var listener = new ListenerForUnitTests();
				merger.EventListener = listener;
				merger.DoMerge(mergeOrder.pathToOurs);
				var conflict = listener.Conflicts[0];
				AssertConflictType<BothEditedTextConflict>(conflict);
				var expectedContext = "lift://unknown?type=entry&id=F169EB3D-16F2-4eb0-91AA-FDB91636F8F6";
				Assert.AreEqual(expectedContext, listener.Contexts[0].PathToUserUnderstandableElement,
								"the listener wasn't give the expected context");
			}
		}

		private void AssertConflictType<TConflictType>(IConflict conflict)
		{
				Assert.AreEqual(typeof(TConflictType), conflict.GetType(), conflict.ToString());
		}
	}
}