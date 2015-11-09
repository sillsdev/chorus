using System.IO;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;
using SIL.TestUtilities;

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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
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

		/// <summary>
		/// This is a regression test for FW LT-13958. It also tests the path in XmlMerger.MergeInner where the element strategy is atomic
		/// and AllowAtomicTextMerge is true, but the children do not allow a text merge.
		/// </summary>
		[Test]
		public void ClassSpansMerge()
		{
			var ancestor =
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.13' producer='FLEx 7.2.4'>
				<entry id='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256' guid='f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<lexical-unit>
				<form lang='fr'><text>lovely</text></form>
				</lexical-unit>
				<trait name='morph-type' value='stem'/>
				<sense id='de53a9a1-5b70-49e5-8dbe-171bff37d624'>
				<example>
				<form lang='fr'>
				<text>This is <span
					class='Strong'>an</span> example.</text>
				</form>
				<translation
				  type='Free translation'>
				  <form
					lang='en'>
					<text>A translation of <span
						class='Strong'>the</span> example</text>
				  </form>
				</translation>
				</example>
				</sense>
				</entry>
				</lift>";
			var ours = // add 'sentence' to example
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.13' producer='FLEx 7.2.4'>
				<entry id='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256' guid='f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<lexical-unit>
				<form lang='fr'><text>lovely</text></form>
				</lexical-unit>
				<trait name='morph-type' value='stem'/>
				<sense id='de53a9a1-5b70-49e5-8dbe-171bff37d624'>
				<example>
				<form lang='fr'>
				<text>This is <span
					class='Strong'>an</span> example sentence.</text>
				</form>
				<translation
				  type='Free translation'>
				  <form
					lang='en'>
					<text>A translation of <span
						class='Strong'>the</span> example</text>
				  </form>
				</translation>
				</example>
				</sense>
				</entry>
				</lift>";
			var theirs = // add 'sentence' to TRANSLATION of example
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.13' producer='FLEx 7.2.4'>
				<entry id='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256' guid='f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<lexical-unit>
				<form lang='fr'><text>lovely</text></form>
				</lexical-unit>
				<trait name='morph-type' value='stem'/>
				<sense id='de53a9a1-5b70-49e5-8dbe-171bff37d624'>
				<example>
				<form lang='fr'>
				<text>This is <span
					class='Strong'>an</span> example.</text>
				</form>
				<translation
				  type='Free translation'>
				  <form
					lang='en'>
					<text>A translation of <span
						class='Strong'>the</span> example sentence</text>
				  </form>
				</translation>
				</example>
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"/lift/entry/sense/example/form/text/span", 1);
				AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"/lift/entry/sense/example/translation/form/text/span", 1);
			}
		}


		[Test]
		public void RelationsMergeWithoutThrowing()
		{
			var ancestor =
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.13' producer='FLEx 7.2.4'><entry id='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256' guid='f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<lexical-unit>
				<form lang='fr'><text>lovely</text></form>
				</lexical-unit>
				<trait name='morph-type' value='stem'/>
				<sense id='de53a9a1-5b70-49e5-8dbe-171bff37d624'>
				<example>
				<form lang='fr'><text>This is example sentence one</text></form>
				</example>
				<example>
				<form lang='fr'><text>This is example sentence two.</text></form>
				<translation type='Free translation'>
				<form lang='en'><text>This is a translation of two</text></form>
				</translation>
				</example>
				</sense>
				</entry>
				<entry id='love_92b74399-ce6e-4f24-8412-784e0227d3eb' guid='92b74399-ce6e-4f24-8412-784e0227d3eb'>
				<lexical-unit>
				<form lang='fr'><text>love</text></form>
				</lexical-unit>
				<trait  name='morph-type' value='stem'/>
				<etymology type='proto' source='French'>
				<form lang='fr'><text>It came from </text></form>
				<form lang='en'><text>the deep</text></form>
				</etymology>
				<relation type='_component-lexeme' ref='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<trait name='is-primary' value='true'/>
				<trait name='complex-form-type' value=''/>
				</relation>
				<sense id='3185e889-eec3-4148-8df9-8f446bc498b7'>
				</sense>
				</entry></lift>";
			var ours = ancestor.Replace(@"It came from", @"May have come from");
			var theirs = ancestor.Replace(@"</entry></lift>",
										  @"</entry><entry id='newentry' guid='fffffff-ffff-ffff-ffff-ffffffff'><lexical-unit><form lang='fr'><text>loverly</text></form></lexical-unit></entry></lift>");
			theirs = theirs.Replace(@"</relation>",
									@"</relation><relation type='_component-lexeme' ref='newentry'><trait name='is-primary' value='true'/><trait name='complex-form-type' value=''/></relation>");

			Assert.DoesNotThrow(() =>
			{
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
					AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"/lift/entry/relation", 2);
				}
			});
		}

		[Test]
		public void Merge_DuplicateKeyInGloss_NoThrow()
		{
			var ancestor =
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.13' producer='FLEx 7.2.4'><entry id='lovely_f1c5a4c8-a24f-4351-8551-2b70d53a9256' guid='f1c5a4c8-a24f-4351-8551-2b70d53a9256'>
				<lexical-unit>
				<form lang='fr'><text>lovely</text></form>
				</lexical-unit>
				<trait name='morph-type' value='stem'/>
				<sense id='fist_0e0fc867-e56a-4df5-861a-1cb24d861037'>
				<grammatical-info
					value='Noun' />
				<gloss
					lang='en'>
					<text>base</text>
				</gloss>
				<gloss
					lang='swh'>
					<text>ngumi / mangumi</text>
				</gloss>
				<gloss
					lang='swh'>
					<text>konde / makonde</text>
				</gloss>
				</sense>
				</entry></lift>";
			var ours = ancestor.Replace("base", "ours");
			var theirs = ancestor.Replace("base", "theirs");

			Assert.DoesNotThrow(() =>
			{
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
					//var result = File.ReadAllText(mergeOrder.pathToOurs);
					//AssertThatXmlIn.String(result).HasSpecifiedNumberOfMatchesForXpath(@"/lift/entry/relation", 2);
				}
			});
		}

		private void AssertConflictType<TConflictType>(IConflict conflict)
		{
				Assert.AreEqual(typeof(TConflictType), conflict.GetType(), conflict.ToString());
		}
	}
}