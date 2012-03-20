using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;

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
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation)
									{EventListener = listener};
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
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
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(situation),
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				var conflict = listener.Conflicts[0];
				AssertConflictType<XmlTextBothEditedTextConflict>(conflict);
				const string expectedContext = "lift://unknown?type=entry&id=F169EB3D-16F2-4eb0-91AA-FDB91636F8F6";
				Assert.AreEqual(expectedContext, listener.Contexts[0].PathToUserUnderstandableElement,
								"the listener wasn't give the expected context");
			}
		}

		/// <summary>
		/// regression http://jira.palaso.org/issues/browse/CHR-103
		/// </summary>
		[Test]
		public void GetMergedLift_MysteryDroppedGloss()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
					   <entry
		id='nɪntɔnnʊ_49327c35-759e-4db3-984a-b7dc5af1b1b0'
		dateCreated='2012-01-30T10:45:04Z'
		dateModified='2012-02-23T08:52:25Z'
		guid='49327c35-759e-4db3-984a-b7dc5af1b1b0'>
		<lexical-unit>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́nnʊ̄</text>
			</form>
		</lexical-unit>
		<field
			type='pl'>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́ntʊ́</text>
			</form>
		</field>
		<field
			type='tn'>
			<form
				lang='fr'>
				<text>HHM</text>
			</form>
		</field>
		<field
			type='tnpl'>
			<form
				lang='fr'>
				<text>HHH</text>
			</form>
		</field>
		<sense
			id='lip_39e4b942-0bf6-4494-aa9b-7f5163feb2bc'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>lip</text>
			</gloss>
			<gloss
				lang='es'>
				<text>labio</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>lèvre</text>
			</gloss>
			<gloss
				lang='ha'>
				<text>bā̀kī</text>
			</gloss>
			<gloss
				lang='nku-fonipa-x-etic'>
				<text>nwɔ́gbɛ́ɟɛ̀</text>
			</gloss>
			<gloss
				lang='swh'>
				<text>mdomo / midomo</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>lip</text>
				</form>
				<form
					lang='es'>
					<text>labio</text>
				</form>
				<form
					lang='fr'>
					<text>lèvre</text>
				</form>
				<form
					lang='ha'>
					<text>bā̀kī</text>
				</form>
				<form
					lang='id'>
					<text>bibir</text>
				</form>
				<form
					lang='pt'>
					<text>lábio</text>
				</form>
				<form
					lang='swh'>
					<text>mdomo / midomo</text>
				</form>
			</definition>
			<note
				type='source'>
				<form
					lang='x-unk'>
					<text>
			00<span
							lang='en'>16</span></text>
				</form>
			</note>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0016</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.4 Mouth' />
		</sense>
	</entry>

					</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry
		id='nɪntɔnnʊ_49327c35-759e-4db3-984a-b7dc5af1b1b0'
		dateCreated='2012-01-30T10:45:04Z'
		dateModified='2012-01-30T10:59:31Z'
		guid='49327c35-759e-4db3-984a-b7dc5af1b1b0'>
		<lexical-unit>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́nnʊ̄</text>
			</form>
		</lexical-unit>

		<field
			type='pl'>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́ntʊ́</text>
			</form>
		</field>
		<note>
			<form
				lang='fr'>
				<text>Homonyme avec l'ethnie lobi sg pl. A faire.</text>
			</form>
		</note>

		<field
			type='tn'>
			<form
				lang='fr'>
				<text>HHM</text>
			</form>
		</field>
		<field
			type='tnpl'>
			<form
				lang='fr'>
				<text>HHH</text>
			</form>
		</field>
		<sense
			id='lip_39e4b942-0bf6-4494-aa9b-7f5163feb2bc'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>lip</text>
			</gloss>
			<gloss
				lang='es'>
				<text>labio</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>lèvre</text>
			</gloss>
			<gloss
				lang='ha'>
				<text>bā̀kī</text>
			</gloss>
			<gloss
				lang='swh'>
				<text>mdomo / midomo</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>lip</text>
				</form>
				<form
					lang='es'>
					<text>labio</text>
				</form>
				<form
					lang='fr'>
					<text>lèvre</text>
				</form>
				<form
					lang='ha'>
					<text>bā̀kī</text>
				</form>
				<form
					lang='id'>
					<text>bibir</text>
				</form>
				<form
					lang='pt'>
					<text>lábio</text>
				</form>
				<form
					lang='swh'>
					<text>mdomo / midomo</text>
				</form>
			</definition>
			<note
				type='source'>
				<form
					lang='x-unk'>
					<text>
			00<span
							lang='en'>16</span></text>
				</form>
			</note>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0016</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.4 Mouth' />
		</sense>
	</entry>

					</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<entry
		id='nɪntɔnnʊ_49327c35-759e-4db3-984a-b7dc5af1b1b0'
		dateCreated='2012-01-30T10:45:04Z'
		dateModified='2012-01-30T10:59:31Z'
		guid='49327c35-759e-4db3-984a-b7dc5af1b1b0'>
		<lexical-unit>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́nnʊ̄</text>
			</form>
		</lexical-unit>
		<annotation
			name='sorted-index'
			value='10' />
		<field
			type='pl'>
			<form
				lang='vag-fonipa-x-etic'>
				<text>nɪ́ntɔ́ntʊ́</text>
			</form>
		</field>
		<field
			type='tn'>
			<form
				lang='fr'>
				<text>HHM</text>
			</form>
		</field>
		<field
			type='tnpl'>
			<form
				lang='fr'>
				<text>HHH</text>
			</form>
		</field>
		<sense
			id='lip_39e4b942-0bf6-4494-aa9b-7f5163feb2bc'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>lip</text>
			</gloss>
			<gloss
				lang='es'>
				<text>labio</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>lèvre</text>
			</gloss>
			<gloss
				lang='ha'>
				<text>bā̀kī</text>
			</gloss>
			<gloss
				lang='swh'>
				<text>mdomo / midomo</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>lip</text>
				</form>
				<form
					lang='es'>
					<text>labio</text>
				</form>
				<form
					lang='fr'>
					<text>lèvre</text>
				</form>
				<form
					lang='ha'>
					<text>bā̀kī</text>
				</form>
				<form
					lang='id'>
					<text>bibir</text>
				</form>
				<form
					lang='pt'>
					<text>lábio</text>
				</form>
				<form
					lang='swh'>
					<text>mdomo / midomo</text>
				</form>
			</definition>
			<note
				type='source'>
				<form
					lang='x-unk'>
					<text>
			00<span
							lang='en'>16</span></text>
				</form>
			</note>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0016</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.4 Mouth' />
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
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//each of these are things we saw go wrong in the user's failed merge. we combined all the missing data
				//into a single entry.  This never did demonstrate the problem, but it ruled out that the problem
				//was a simple one of failure to merge entries properly!


				//glosses like this were being lost
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/sense/gloss[@lang='nku-fonipa-x-etic']");

				//there was a case where the <annotation> was in the base, but not the piers, yet it showed up in the merge!
				XmlTestHelper.AssertXPathIsNull(result, "lift/entry/annotation");

				//notes were being lost
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/note/form[@lang='fr']");
			}
		}


		private void AssertConflictType<TConflictType>(IConflict conflict)
		{
				Assert.AreEqual(typeof(TConflictType), conflict.GetType(), conflict.ToString());
		}
	}
}