using System;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using Chorus.merge.xml.lift;
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
			string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='456'>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6' />
					</lift>";
			LiftMerger merger = new LiftMerger(ours, theirs, ancestor, new LiftEntryMergingStrategy(new NullMergeSituation()));
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			string result = merger.GetMergedLift();
			//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
			Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());

			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense[@id='123']/gloss/text='ourSense']");
			XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@id='test' and sense[@id='456']/gloss/text='theirSense']");
		}

		[Test]
		public void GetMergedLift_ConflictingGlosses_ListenerIsNotifiedOfBothEdittedConflict()
		{
			string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>ourSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";

			string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>theirSense</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='test'  guid='F169EB3D-16F2-4eb0-91AA-FDB91636F8F6'>
							<sense id='123'>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							 </sense>
						</entry>
					</lift>";
			LiftMerger merger = new LiftMerger(ours, theirs, ancestor, new LiftEntryMergingStrategy(new NullMergeSituation()));
			var listener = new ListenerForUnitTests();
			merger.EventListener = listener;
			string result = merger.GetMergedLift();
			Assert.AreEqual(1, listener.Conflicts.Count);
			var conflict = listener.Conflicts[0];
			AssertConflictType<BothEdittedTextConflict>(conflict);
			var expectedContext = "lift://unknown?type=entry&guid=F169EB3D-16F2-4eb0-91AA-FDB91636F8F6&id=test";
			Assert.AreEqual(expectedContext, listener.Contexts[0].PathToUserUnderstandableElement, "the listener wasn't give the expected context");
		}

		private void AssertConflictType<TConflictType>(IConflict conflict)
		{
				Assert.AreEqual(typeof(TConflictType), conflict.GetType(), conflict.ToString());
		}
	}
}