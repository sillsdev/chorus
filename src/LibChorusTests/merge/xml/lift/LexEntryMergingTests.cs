using System.IO;
using System.Xml;
using Chorus.FileTypeHandlers.lift;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using SIL.IO;


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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				// Check that the audio made it into the merge.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-fonipa-x-emic']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-Zxxx-x-audio']");
			}

		}

		[Test]
		public void Merge_EditAndDeleteEntry_GeneratesConflictWithContext()
		{
			const string pattern = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2012-05-18T08:31:54Z'
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<text>{0}</text>
							</form>
						</lexical-unit>

					</entry>
				</lift>";
			// We edited the text of the form slightly.
			string ours = string.Format(pattern, "asaten");
			string ancestor = string.Format(pattern, "asat");

			// they deleted the whole entry
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
				</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				var mergeStrategy = new LiftEntryMergingStrategy(mergeOrder);
				var strategies = mergeStrategy.GetStrategies();
				var entryStrategy = strategies.ElementStrategies["entry"];
				entryStrategy.ContextDescriptorGenerator = new EnhancedEntrycontextGenerator();
				XmlMergeService.Do3WayMerge(mergeOrder, mergeStrategy,
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				var conflict = listener.Conflicts[0];
				Assert.That(conflict, Is.InstanceOf<EditedVsRemovedElementConflict>());
				Assert.That(conflict.HtmlDetails, Does.Contain("my silly context"), "merger should have used the context generator to make the html details");
				Assert.That(conflict.HtmlDetails.IndexOf("my silly context"),
					Is.EqualTo(conflict.HtmlDetails.LastIndexOf("my silly context")),
					"since one change is a delete, the details should only be present once");
				var context = conflict.Context;

				Assert.That(context, Is.Not.Null, "the merge should supply a context for the conflict");
				Assert.That(context.PathToUserUnderstandableElement, Is.Not.Null);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-fonipa-x-emic']/text[text()='asaten']");
			}
		}
		[Test]
		public void Merge_DeleteAndEditEntry_GeneratesConflictWithContext()
		{
			const string pattern = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						dateCreated='2011-03-09T17:08:44Z'
						dateModified='2012-05-18T08:31:54Z'
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='ldb-fonipa-x-emic'>
								<text>{0}</text>
							</form>
						</lexical-unit>

					</entry>
				</lift>";
			// We edited the text of the form slightly.
			string theirs = string.Format(pattern, "asaten");
			string ancestor = string.Format(pattern, "asat");

			// they deleted the whole entry
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
				</lift>";

			using (var oursTemp = new TempFile(ours))
			using (var theirsTemp = new TempFile(theirs))
			using (var ancestorTemp = new TempFile(ancestor))
			{
				var listener = new ListenerForUnitTests();
				var situation = new NullMergeSituation();
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, situation) { EventListener = listener };
				var mergeStrategy = new LiftEntryMergingStrategy(mergeOrder);
				var strategies = mergeStrategy.GetStrategies();
				var entryStrategy = strategies.ElementStrategies["entry"];
				entryStrategy.ContextDescriptorGenerator = new EnhancedEntrycontextGenerator();
				XmlMergeService.Do3WayMerge(mergeOrder, mergeStrategy,
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				var conflict = listener.Conflicts[0];
				Assert.That(conflict, Is.InstanceOf<RemovedVsEditedElementConflict>());
				Assert.That(conflict.HtmlDetails, Does.Contain("my silly context"), "merger should have used the context generator to make the html details");
				Assert.That(conflict.HtmlDetails.IndexOf("my silly context"),
					Is.EqualTo(conflict.HtmlDetails.LastIndexOf("my silly context")),
					"since one change is a delete, the details should only be present once");
				var context = conflict.Context;

				Assert.That(context, Is.Not.Null, "the merge should supply a context for the conflict");
				Assert.That(context.PathToUserUnderstandableElement, Is.Not.Null);
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='ldb-fonipa-x-emic']/text[text()='asaten']");
			}
		}

		class EnhancedEntrycontextGenerator : LexEntryContextGenerator, IGenerateHtmlContext
		{
			public string HtmlContext(XmlNode mergeElement)
			{
				return "my silly context";
			}

			public string HtmlContextStyles(XmlNode mergeElement)
			{
				return "";
			}
		}

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_DuplicateGuidsInEntry_ResultHasWarningReport()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
					</entry>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>goner form</text>
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				// Check that there is only one entry in the merged file.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry[@guid='00853b73-fda2-4b12-8a89-6957cc7e7e79']");
				XmlTestHelper.AssertXPathIsNull(result, "lift/entry[@guid='00853b73-fda2-4b12-8a89-6957cc7e7e79']/lexical-unit");
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[0].GetType());
			}
		}

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_DuplicateFormsInEntry_ResultHasWarningReport()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>my form</text>
							</form>
						</lexical-unit>
					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>their form</text>
							</form>
							<form
								lang='en'>
								<text>their dup form</text>
							</form>
						</lexical-unit>
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>common form</text>
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				// Check that there is only one entry in the merged file.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form[@lang='en']");
				XmlTestHelper.AssertXPathIsNull(result, "lift/entry/lexical-unit/form[@lang='en']/text[text()='my dup form']");
				Assert.AreEqual(1, listener.Warnings.Count);
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[0].GetType());
			}
		}

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_DuplicateRelationsInEntry_ResultHasWarningReport()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<relation type='t1' ref='r1' />
						<relation type='t1' ref='r1' />
					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<relation type='t1' ref='r1' />
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<relation type='t1' ref='r1' />
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
				// Check that there is only one entry in the merged file.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/relation");
				Assert.AreEqual(1, listener.Warnings.Count);
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[0].GetType());
			}
		}

		[Test] // See http://jira.palaso.org/issues/browse/CHR-18
		public void Merge_MultiTextInFormInEntry_ResultHasWarningReport()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>common form</text>
								<text>our extra text</text>
							</form>
						</lexical-unit>
					</entry>
				</lift>";

			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>common form</text>
								<text>their extra text</text>
							</form>
						</lexical-unit>
					</entry>
				</lift>";

			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='WeSay 1.0.0.0'>
					<entry
						id='00853b73-fda2-4b12-8a89-6957cc7e7e79'
						guid='00853b73-fda2-4b12-8a89-6957cc7e7e79'>
						<lexical-unit>
							<form
								lang='en'>
								<text>common form</text>
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				// Check that there is only one entry in the merged file.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/lexical-unit/form/text");
				XmlTestHelper.AssertXPathIsNull(result, "lift/entry/lexical-unit/form/text[text()='extra text']");
				Assert.AreEqual(2, listener.Warnings.Count);
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[0].GetType());
				Assert.AreEqual(typeof(MergeWarning), listener.Warnings[1].GetType());
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				// Check that the audio made it into the merge.
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/sense/definition/form[@lang='a']");
				XmlTestHelper.AssertXPathMatchesExactlyOne(result, "lift/entry/sense/definition/form[@lang='b']");
			}

		}

		[Test]
		public void Merge_TheirDuplicateRelationDoesNotResultInEmptyRelationElement()
		{
			const string ours =
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='Whoever'>
					<entry
						dateCreated='2012-04-16T07:27:11Z'
						dateModified='2012-08-12T09:46:54Z'
						id='आप्‍चो_27fcb6ac-b509-4463-aa12-36427ac9b427'
						guid='27fcb6ac-b509-4463-aa12-36427ac9b427'>
						<lexical-unit>
							<form
								lang='bhj'>
								<text>आप्‍चो</text>
							</form>
						</lexical-unit>
						<relation
							type='Compare'
							ref='आम्‌मे_1cc3b8eb-cc46-4ee9-9a53-9195a30cb6b4' />
						<sense
							id='d4c1b46b-554a-4fc6-846b-b136b118817b'
							order='1'>
							<definition>
								<form
									lang='en'>
									<text>shoot</text>
								</form>
								<form
									lang='ne'>
									<text>हिर्काउ</text>
								</form>
							</definition>
						</sense>
					</entry>
				</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='Whoever'>
					<entry
						dateCreated='2012-04-16T07:27:11Z'
						dateModified='2012-06-21T01:37:26Z'
						id='आप्‍चो_27fcb6ac-b509-4463-aa12-36427ac9b427'
						guid='27fcb6ac-b509-4463-aa12-36427ac9b427'>
						<lexical-unit>
							<form
								lang='bhj'>
								<text>आप्‍चो</text>
							</form>
						</lexical-unit>
						<relation
							type='Compare'
							ref='आम्‌मे_1cc3b8eb-cc46-4ee9-9a53-9195a30cb6b4' />
						<relation
							type='Compare'
							ref='आम्‌मे_1cc3b8eb-cc46-4ee9-9a53-9195a30cb6b4' />
						<sense
							id='d4c1b46b-554a-4fc6-846b-b136b118817b'
							order='1'>
							<definition>
								<form
									lang='en'>
									<text>shoot</text>
								</form>
								<form
									lang='ne'>
									<text>हीर्काउँ</text>
								</form>
							</definition>
						</sense>
					</entry>
				</lift>";
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
				<lift version='0.10' producer='Whoever'>
					<entry
						dateCreated='2012-04-16T07:27:11Z'
						dateModified='2012-06-21T01:37:26Z'
						id='आप्‍चो_27fcb6ac-b509-4463-aa12-36427ac9b427'
						guid='27fcb6ac-b509-4463-aa12-36427ac9b427'>
						<lexical-unit>
							<form
								lang='bhj'>
								<text>आप्‍चो</text>
							</form>
						</lexical-unit>
						<relation
							type='Compare'
							ref='आम्‌मे_1cc3b8eb-cc46-4ee9-9a53-9195a30cb6b4' />
						<sense
							id='d4c1b46b-554a-4fc6-846b-b136b118817b'
							order='1'>
							<definition>
								<form
									lang='en'>
									<text>shoot</text>
								</form>
								<form
									lang='ne'>
									<text>हीर्काउँ</text>
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
				XmlMergeService.Do3WayMerge(mergeOrder, new LiftEntryMergingStrategy(mergeOrder),
					false,
					"header",
					"entry", "guid");
				//this doesn't seem particular relevant, but senses are, in fact, ordered, so there is some ambiguity here
				var result = File.ReadAllText(mergeOrder.pathToOurs);
				//Assert.AreEqual(typeof(AmbiguousInsertConflict), listener.Conflicts[0].GetType());
				// Check that the audio made it into the merge.
				XmlTestHelper.AssertXPathIsNull(result, "//relation[not(@type)]");
			}
		}
	}
}
