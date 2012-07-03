using System;
using System.Collections.Generic;
using System.IO;
using Chorus.FileTypeHanders.lift;
using Chorus.FileTypeHanders.xml;
using Chorus.merge;
using Chorus.merge.xml.generic;
using LibChorus.TestUtilities;
using NUnit.Framework;
using Palaso.IO;

namespace LibChorus.Tests.merge.xml.generic
{
	/// <summary>
	/// Test the XmlMergeService class to make sure it produces the correct conflict/change reports.
	/// </summary>
	[TestFixture]
	public class XmlMergeServiceTests
	{
		private void DoMerge(string ancestorXml, string ourXml, string theirXml,
			MergeSituation mergeSituation,
			NullMergeStrategy nullMergeStrategy,
			IEnumerable<string> xpathQueriesThatMatchExactlyOneNode,
			IEnumerable<string> xpathQueriesThatReturnNull,
			int expectedConflictCount, List<Type> expectedConflictTypes,
			int expectedChangesCount, List<Type> expectedChangeTypes)
		{
			var listener = new ListenerForUnitTests();
			string result;
			using (var oursTemp = new TempFile(ourXml))
			using (var theirsTemp = new TempFile(theirXml))
			using (var ancestorTemp = new TempFile(ancestorXml))
			{
				var mergeOrder = new MergeOrder(oursTemp.Path, ancestorTemp.Path, theirsTemp.Path, mergeSituation)
					{
						EventListener = listener
					};
				XmlMergeService.Do3WayMerge(mergeOrder, nullMergeStrategy,
					"header",
					"entry", "guid", LiftFileHandler.WritePreliminaryInformation);
				result = File.ReadAllText(mergeOrder.pathToOurs);
			}

			XmlTestHelper.CheckMergeResults(result, listener,
				xpathQueriesThatMatchExactlyOneNode, xpathQueriesThatReturnNull,
				expectedConflictCount, expectedConflictTypes,
				expectedChangesCount, expectedChangeTypes);
		}

		[Test]
		public void BothAddedNewFileWithConflictingDataHasConflict()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift
	version='0.10'
	producer='WeSay 1.0.0.0'>
						<entry id='addedByBoth' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>our gloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='addedByBoth' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>their gloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(null, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='our gloss']" }, new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='their gloss']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				0, null);

			// They win merge situation.
			DoMerge(null, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='their gloss']" }, new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='our gloss']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				0, null);
		}

		[Test]
		public void BothAddedNewFileWithNonConflictingDataHasChangeReports()
		{
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift
	version='0.10'
	producer='WeSay 1.0.0.0'>
						<entry id='addedByUs' guid='c1edbbe7-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>our gloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='addedByThem' guid='c1edbbe8-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>their gloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(null, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='addedByUs']/sense/gloss/text[text()='our gloss']", "lift/entry[@id='addedByUs']/sense/gloss/text[text()='our gloss']" }, new string[0],
				0, null,
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });

			// They win merge situation.
			DoMerge(null, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='addedByUs']/sense/gloss/text[text()='our gloss']", "lift/entry[@id='addedByUs']/sense/gloss/text[text()='our gloss']" }, new string[0],
				0, null,
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void OldStyleMainItemRemovedByUsEditedByThemHasCorrectRemovedEditConflicts()
		{
			// Old Style means the deleted entry was just marked as deleted with the dateDeleted attr.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUsEditedByThem' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByThem</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='doomedByUsEditedByThem']/sense/gloss/text[text()='editedByThem']" }, new[] { "lift/entry[@id='doomedByUsEditedByThem' and @dateDeleted='2011-03-15T12:15:05Z']" },
				1, new List<Type> {typeof (RemovedVsEditedElementConflict)},
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='doomedByUsEditedByThem']/sense/gloss/text[text()='editedByThem']" }, new[] { "lift/entry[@id='doomedByUsEditedByThem' and @dateDeleted='2011-03-15T12:15:05Z']" },
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);
		}

		[Test]
		public void OldStyleMainItemEditedByUsRemovedByThemHasCorrectRemovedEditConflicts()
		{
			// Old Style means the deleted entry was just marked as deleted with the dateDeleted attr.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByThemEditedByUs' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByThemEditedByUs' guid='c1ed1f98-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByUs</text>
								 </gloss>
							</sense>
						</entry>

					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByThemEditedByUs' guid='c1ed1f98-e382-11de-8a39-0800200c9a66'  dateDeleted='2011-03-15T12:15:05Z' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='doomedByThemEditedByUs']/sense/gloss/text[text()='editedByUs']" }, new[] { "lift/entry[@id='doomedByThemEditedByUs' and @dateDeleted='2011-03-15T12:15:05Z']" },
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='doomedByThemEditedByUs']/sense/gloss/text[text()='editedByUs']" }, new[] { "lift/entry[@id='doomedByThemEditedByUs' and @dateDeleted='2011-03-15T12:15:05Z']" },
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, null);
		}

		[Test]
		public void NewStyleMainItemRemovedByThemEditedByUsHasHasCorrectRemovedEditConflicts()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByThemEditedByUs' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByUs</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='doomedByThemEditedByUs']/sense/gloss/text[text()='editedByUs']" }, null,
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='doomedByThemEditedByUs']/sense/gloss/text[text()='editedByUs']" }, null,
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, null);
		}

		[Test]
		public void NewStyleMainItemRemovedByUsEditedByThemHasCorrectRemovedEditConflicts()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUsEditedByThem' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='doomedByUsEditedByThem' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByThem</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='doomedByUsEditedByThem']/sense/gloss/text[text()='editedByThem']" }, null,
				1, new List<Type> { typeof(RemovedVsEditedElementConflict) },
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='doomedByUsEditedByThem']/sense/gloss/text[text()='editedByThem']" }, null,
				1, new List<Type> { typeof(EditedVsRemovedElementConflict) },
				0, null);
		}

		[Test]
		public void EachAddedMainItemWithdifferentContentHasTwoChangeReports()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByUs' guid='c1ed94d7-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>addedByUs</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByThem' guid='c1edbbd0-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>addedByThem</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='addedByUs']/sense/gloss/text[text()='addedByUs']", "lift/entry[@id='addedByThem']/sense/gloss/text[text()='addedByThem']" }, null,
				0, null,
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='addedByUs']/sense/gloss/text[text()='addedByUs']", "lift/entry[@id='addedByThem']/sense/gloss/text[text()='addedByThem']" }, null,
				0, null,
				2, new List<Type> { typeof(XmlAdditionChangeReport), typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void BothAddedMainItemWithSameContentHasOneChangeReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByBoth' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByBoth</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByBoth' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByBoth</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByBoth']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByBoth']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void BothAddedMainItemButWithDifferentContentHasOneConflictReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByBoth' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByUs</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='addedByBoth' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>editedByThem</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByUs']" }, new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByThem']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByThem']" }, new[] { "lift/entry[@id='addedByBoth']/sense/gloss/text[text()='editedByUs']" },
				1, new List<Type> { typeof(BothAddedMainElementButWithDifferentContentConflict) },
				0, null);
		}

		[Test]
		public void BothDeletedMainItemHasOneChangeReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='bothDeleted' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/entry[@id='bothDeleted']" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/entry[@id='bothDeleted']" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void OnlyOneDeletedMainItemHasOneChangeReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='onlyOneDeleted' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='onlyOneDeleted' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/entry[@id='onlyOneDeleted']" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/entry[@id='onlyOneDeleted']" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void NobodyTouchedExtantOptionalFirstElement()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = ancestor;
			const string theirs = ancestor;

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				0, null);
		}

		[Test]
		public void BothDeletedExtantOptionalFirstElement()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void EachDeletedExtantOptionalFirstElement()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void BothAddedOptionalFirstElement()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlBothAddedSameChangeReport) });
		}

		[Test]
		public void EachAddedOptionalFirstElement()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header", "lift/entry[@id='noChangesInEither']" }, null,
				0, null,
				1, new List<Type> { typeof(XmlAdditionChangeReport) });
		}

		[Test]
		public void OnlyOneDeletedOptionalFirstElementHasOneChangeReport()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header" },
				0, null,
				1, new List<Type> { typeof(XmlDeletionChangeReport) });
		}

		[Test]
		public void BothAddedOptionalFirstElementButWithDifferentContentHasNoReportsWithNullMergeStrategy()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='ourNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='theirNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='theirNewHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, null, // Wrong, becasue of using brain-dead NullMergeStrategy
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);
		}

		[Test]
		public void BothEditedOptionalFirstElementInDifferentWaysHasNoReportsWithNullMergeStrategy()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='ourNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='theirNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']", "lift/header[@id='theirNewHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='theirNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']", "lift/header[@id='ourNewHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);
		}

		[Test]
		public void EachEditedOptionalFirstElementHasNoReportsWithNullMergeStrategy()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='ourNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='theirNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']", "lift/header[@id='theirNewHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='theirNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']", "lift/header[@id='ourNewHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);
		}

		[Test]
		public void WeEditedOptionalFirstElementHasNoReportsWithNullMergeStrategy()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='ourNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='ourNewHeader']", "lift/entry[@id='noChangesInEither']" }, new[] { "lift/header[@id='originalHeader']" },
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);
		}

		[Test]
		public void TheyEditedOptionalFirstElementHasNoReportsWithNullMergeStrategy()
		{
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='originalHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<header id='theirNewHeader'/>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='theirNewHeader']", "lift/entry[@id='noChangesInEither']" }, null,
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/header[@id='theirNewHeader']", "lift/entry[@id='noChangesInEither']" }, null,
				0, null, // Since we use NullMergeStrategy, there are no reports, but we know we called the MakeMergedEntry
				0, null);
		}

		[Test]
		public void BothEditedMainItemSenseGlossButInDifferentWaysHasNoReportsWithNullMergeStrategy()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='bothEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='bothEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>ourNewGloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='bothEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>theirNewGloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='bothEdited']/sense/gloss/text[text()='ourNewGloss']" }, null,
				0, null,
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='bothEdited']/sense/gloss/text[text()='theirNewGloss']" }, null,
				0, null,
				0, null);
		}

		[Test]
		public void WeEditedMainItemSenseGlossTheyDidNothingHasNoReportsWithNullMergeStrategy()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>ourNewGloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='ourNewGloss']" }, new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='original']" },
				0, null,
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='ourNewGloss']" }, new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='original']" },
				0, null,
				0, null);
		}

		[Test]
		public void TheyEditedMainItemSenseGlossWeDidNothingHasNoReportsWithNullMergeStrategy()
		{
			// New Style means the deleted entry was really removed from the file, not just marked as deleted.
			const string ancestor = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string ours = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>original</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";
			const string theirs = @"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
						<entry id='noChangesInEither' guid='c1ed1f9d-e382-11de-8a39-0800200c9a66' />
						<entry id='oneEdited' guid='c1ed1f9e-e382-11de-8a39-0800200c9a66' >
							<sense>
								 <gloss lang='a'>
									<text>theirNewGloss</text>
								 </gloss>
							</sense>
						</entry>
					</lift>";

			// We win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituation(),
				new NullMergeStrategy(true),
				new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='theirNewGloss']" }, new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='original']" },
				0, null,
				0, null);

			// They win merge situation.
			DoMerge(ancestor, ours, theirs,
				new NullMergeSituationTheyWin(),
				new NullMergeStrategy(false),
				new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='theirNewGloss']" }, new[] { "lift/entry[@id='oneEdited']/sense/gloss/text[text()='original']" },
				0, null,
				0, null);
		}
	}
}