using System.Collections.Generic;
using System.Linq;
using Chorus.notes;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Browser;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace Chorus.Tests.notes
{
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
				var m = new NotesInProjectViewModel(TheUser, new List<AnnotationRepository>(), new MessageSelectedEvent(), new ConsoleProgress());
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
				var m = new NotesInProjectViewModel(TheUser, repos, new MessageSelectedEvent(), new ConsoleProgress());
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
					var m = new NotesInProjectViewModel(TheUser, AnnotationRepository.CreateRepositoriesFromFolder(folder.Path, _progress), new MessageSelectedEvent(), new ConsoleProgress());
					m.SearchTextChanged("john");
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
					var m = new NotesInProjectViewModel(TheUser, repos, new MessageSelectedEvent(), new ConsoleProgress());
					 Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
				   m.SearchTextChanged("ques");
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
					var m = new NotesInProjectViewModel(TheUser, repos, new MessageSelectedEvent(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
					m.SearchTextChanged("MesSAGE");//es is lower case
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
					var m = new NotesInProjectViewModel(TheUser, repos, new MessageSelectedEvent(), new ConsoleProgress());
					Assert.AreEqual(2, m.GetMessages().Count(), "should get 2 annotations when search box is empty");
					m.SearchTextChanged("Ques");
					Assert.AreEqual(1, m.GetMessages().Count());
					Assert.AreEqual("john", m.GetMessages().First().Message.Author);

				}
			}
		}
	}

}

