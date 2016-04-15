using System;
using System.Collections.Generic;
using System.Linq;
using Chorus.notes;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests.notes
{ // TODO pH 2013.08: look for new, untested methods
	[TestFixture]
	public class NotesInProjectModelTests
	{
		private IProgress _progress = new ConsoleProgress();

		[SetUp]
		public void Setup()
		{
			TheUser = new ChorusUser("joe");
		}

		[Test]
		public void GetMessages_NoNotesFiles_GivesZeroMessages()
		{
				var m = new NotesInProjectViewModel(TheUser, new List<AnnotationRepository>(), new ChorusNotesSettings(), new ConsoleProgress());
				Assert.AreEqual(0, m.GetMessages().Count());
		}

		protected ChorusUser TheUser
		{
			get;
			set;
		}

		[Test]
		public void GetMessages_FilesInSubDirs_GetsThemAll()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			using (var subfolder = new TemporaryFolder(folder, "Sub"))
			using (new TempFileFromFolder(folder, "one." + AnnotationRepository.FileExtension, "<notes version='0'><annotation><message/></annotation></notes>"))
			using (new TempFileFromFolder(subfolder, "two." + AnnotationRepository.FileExtension, "<notes  version='0'><annotation><message/></annotation></notes>"))
			{
				var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
				var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
				Assert.AreEqual(2, m.GetMessages().Count());
			}
		}

		private TempFile CreateNotesFile(TemporaryFolder folder, string contents)
		{
			return new TempFileFromFolder(folder, "one." + AnnotationRepository.FileExtension, "<notes version='0'>" + contents + "</notes>");
		}

		[Test]
		public void GetMessages_SearchContainsAuthor_FindsMatches()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = "<annotation><message author='john'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var m = new NotesInProjectViewModel(TheUser, AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress), new ChorusNotesSettings(), new ConsoleProgress());
					m.SearchText = "john";
					Assert.AreEqual(1, m.GetMessages().Count());
				}
			}
		}




		[Test]
		public void GetMessages_SearchContainsClass_FindsMatches()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'><message author='john'></message></annotation>
				<annotation class='note'><message author='bob'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
					m.SearchText = "ques";
					Assert.AreEqual(1, m.GetMessages().Count());
					Assert.AreEqual("john",m.GetMessages().First().Message.Author);

				}
			}
		}

		[Test]
		public void GetMessages_SearchContainsWordInMessageInUpperCase_FindsMatches()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'><message author='john'></message></annotation>
				<annotation class='note'><message author='bob'>my mESsage contents</message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
					m.SearchText = "MesSAGE";//es is lower case
					Assert.AreEqual(1, m.GetMessages().Count());
					Assert.AreEqual("bob", m.GetMessages().First().Message.Author);

				}
			}
		}

		[Test]
		public void GetMessages_SearchContainsClassInWrongUpperCase_FindsMatches()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'><message author='john'></message></annotation>
				<annotation class='note'><message author='bob'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
					m.SearchText = "Ques";
					Assert.AreEqual(1, m.GetMessages().Count());
					Assert.AreEqual("john", m.GetMessages().First().Message.Author);

				}
			}
		}

		[Test]
		public void GetMessages_ShowQuestionsFalse_HidesQuestions()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'><message author='john'></message></annotation>
				<annotation class='note'><message author='bob'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations by default");
					m.ShowQuestions = false;
					Assert.AreEqual(1, m.GetMessages().Count());
					Assert.AreEqual("bob", m.GetMessages().First().Message.Author, "question should not be shown");
				}
			}
		}

		[Test]
		public void FilterStateMessage_HideAll_HideNone()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'><message author='john'></message></annotation>
				<annotation class='note'><message author='bob'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					m.ShowConflicts = false;
					m.ShowNotifications = false;
					m.ShowQuestions = false;
					Assert.AreEqual("Nothing selected to display", m.FilterStateMessage,
						"Wrong filter state message when nothing can be displayed");
					m.ShowQuestions = true;
					m.ShowConflicts = true;
					m.ShowNotifications = true;
					m.ShowClosedNotes = true;
					Assert.AreEqual("All", m.FilterStateMessage,
						"Wrong filter state message when all annotations should be displayed");
				}
			}
		}

		[Test]
		public void GetMessages_HideNotificationsAndConflicts_HidesCorrectItems()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question' date='2013-01-17T20:37:30Z'><message author='john'></message></annotation>
					<annotation
		class='notification'
		ref='unknown'
		guid='1bed8a50-faaa-4814-bcc4-3f6958d0b25e'>
		<message
			author='merger'
			status='open'
			guid='5274ae0b-01b2-4472-bbd0-207c112f57d1'
			date='2013-01-17T20:37:30Z'>unknown: cunninghamd deleted this element, while Jen edited it. The automated merger kept the change made by Jen.<![CDATA[<conflict
	typeGuid='B77C0D86-2368-4380-B2E4-7943F3E7553C'
	class='Chorus.merge.xml.generic.AmbiguousInsertConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon_01.lexdb'
	type='Ambiguous Insertion Conflict'
	guid='5274ae0b-01b2-4472-bbd0-207c112f57d1'
	date='2013-01-18T20:37:30Z'
	whoWon='Jen'
	contextPath='unknown'
	contextDataLabel='unknown'>
	<MergeSituation
		alphaUserId='cunninghamd'
		betaUserId='Jen'
		alphaUserRevision='024ab0827278'
		betaUserRevision='5416cd65b8ad'
		path='Linguistics\Lexicon\Lexicon_01.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
	<message author='jill' date='2013-01-22T20:37:30Z'></message>
	</annotation><annotation
		class='mergeConflict'
		ref='unknown'
		guid='1bed8a50-faaa-4814-bcc4-3f6958d0b25e'>
		<message
			author='merger'
			status='open'
			guid='AE0EF57E-DBBE-4BAA-B530-ADD1E1F29873'
			date='2013-01-20T20:37:30Z'>unknown: cunninghamd and Jen both edited the text of this element. The automated merger kept the change made by Jen.<![CDATA[<conflict
	typeGuid='c1ed6dbe-e382-11de-8a39-0800200c9a66'
	class='Chorus.merge.xml.generic.XmlTextBothEditedTextConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon_01.lexdb'
	type='Ambiguous Insertion Conflict'
	guid='5274ae0b-01b2-4472-bbd0-207c112f57d1'
	date='2013-01-20T20:37:30Z'
	whoWon='Jen'
	contextPath='unknown'
	contextDataLabel='unknown'>
	<MergeSituation
		alphaUserId='cunninghamd'
		betaUserId='Jen'
		alphaUserRevision='024ab0827278'
		betaUserRevision='5416cd65b8ad'
		path='Linguistics\Lexicon\Lexicon_01.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
	<message author='fred' date='2013-01-22T20:37:30Z'></message>
	</annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(), new ConsoleProgress());
					Assert.AreEqual(3, m.GetMessages().Count(), "should get 1 from each of 3 annotations by default");
					m.ShowNotifications = false;
					Assert.AreEqual(2, m.GetMessages().Count(), "one from the notification should not be shown, leaving 2");
					// They are sorted by date in descending order
					Assert.AreEqual("fred", m.GetMessages().First().Message.Author, "should get only most recent message from annotation");
					Assert.AreEqual("john", m.GetMessages().ToArray()[1].Message.Author);

					m.ShowConflicts = false;
					Assert.AreEqual(1, m.GetMessages().Count(), "conflict and notification should not be shown");
					Assert.AreEqual("john", m.GetMessages().First().Message.Author);

					m.ShowNotifications = true;
					Assert.AreEqual(2, m.GetMessages().Count(), "conflict should not be shown");
					Assert.AreEqual("jill", m.GetMessages().First().Message.Author);
					Assert.AreEqual("john", m.GetMessages().ToArray()[1].Message.Author);
				}
			}
		}

		[Test]
		public void GetMessages_ReturnsMostRecentMessagePassingFilterInAnnotation()
		{
			using (var folder = new TemporaryFolder("NotesModelTests"))
			{
				string contents = @"<annotation class='question'>
					<message author='fred' date='2013-01-22T20:37:30Z'/>
					<message author='john' date='2013-01-22T20:37:36Z'/>
					<message author='john' date='2013-01-22T20:37:35Z'/>
					<message author='bill' date='2013-01-22T20:37:39Z'/>
				</annotation>
				<annotation class='note'><message author='bob' date='2013-01-22T20:37:20Z'></message></annotation>";
				using (CreateNotesFile(folder, contents))
				{
					var repos = AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress);
					var m = new NotesInProjectViewModel(TheUser, repos, new ChorusNotesSettings(),
						new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "Should get one from each annotation by default");
					Assert.That(m.GetMessages().First().Message.Author, Is.EqualTo("bill"), "bill's is the newest message");
					Assert.That(m.GetMessages().Last().Message.Author, Is.EqualTo("bob"), "bob's is the oldest message");
					m.SearchText = "john";
					Assert.AreEqual(1, m.GetMessages().Count(), "Should only get a message from the annotation where one passes filter");
					Assert.That(m.GetMessages().First().Message.Author, Is.EqualTo("john"), "Should get one of John's messages (that pass filter)");
					Assert.That(m.GetMessages().First().Message.Date.Second, Is.EqualTo(36), "Should the newest message passing the filter");
				}
			}
		}
	}

}

