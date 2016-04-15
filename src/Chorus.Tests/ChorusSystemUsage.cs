using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Chorus.notes;
using NUnit.Framework;
using Palaso.IO;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Chorus.Tests
{
	/// <summary>
	/// These are not really tests!  They are documented, compilable, runable set of samples to
	/// help you get started incorporating Chorus into your application.
	/// </summary>
	[TestFixture, RequiresSTA]
	public class ChorusSystemUsage
	{
		private ChorusSystem _chorusSystem;

		#region Scaffolding
		private TemporaryFolder _tempFolder;
		private string _dataFolderRoot;
		private string _someDataFilePath;
		private Character _currentCharacter = null;
		private IProgress _progress = new NullProgress();
		private TempFile _someDataFile;

		[SetUp]
		public void Setup()
		{
			_tempFolder = new TemporaryFolder("ChorusSystemUsage");
			_dataFolderRoot = _tempFolder.Path;
			_someDataFile = new TempFileFromFolder(_tempFolder, "test.txt", "hello");
			_someDataFilePath = _someDataFile.Path;

			_chorusSystem = new ChorusSystem(_dataFolderRoot);
			_chorusSystem.Init("john");
		}

		[TearDown]
		public void TearDown()
		{
			_chorusSystem.Dispose();
			_someDataFile.Dispose();
			_tempFolder.Dispose();
		}
		#endregion

		#region Setting Up A Project

		[Test]
		public void CreateARepositoryIfOneDoesntAlreadyExist()
		{
			var cs = new ChorusSystem(_dataFolderRoot);
			cs.Init(string.Empty);
			//before your application closes, call:
			cs.Dispose();
		}

 /*   not yet
		[Test]
		public void ShowSettingsDialog()
		{
			using (var dlg = _chorusSystem.WinForms.CreateSettingDialog())
			{
				dlg.ShowDialog();
			}
		}
		*/
		#endregion

		#region Send/Receive
		[Test, Ignore("Sample Code")]
		public void ShowSynchronizationDialogWhichGivesUsersChoices()
		{
			using (var dlg = _chorusSystem.WinForms.CreateSynchronizationDialog())
			{
				dlg.ShowDialog();
			}
		}

		[Test, Ignore("Sample Code")]
		public void QuitelyMilestoneSomeRecentWork()
		{
			_chorusSystem.AsyncLocalCheckIn("Made a new book called 'surrounded by bitterness'", null);

			//OR, Better:

			_chorusSystem.AsyncLocalCheckIn("Made a new book called 'surrounded by bitterness'",
				(result) =>
					{
						if (result.ErrorEncountered!=null)
						{
							Control yourCurrentUIControl=null;
							yourCurrentUIControl.BeginInvoke(new Action(()=>
								Palaso.Reporting.ErrorReport.NotifyUserOfProblem(result.ErrorEncountered,
										  "Error while checking in your work to the local repository")))
							;
						 }
					});
		}
		#endregion

		#region History
		[Test, Ignore("Sample Code")]
		public void CreateHistoryPage()
		{
			var historyControl = _chorusSystem.WinForms.CreateHistoryPage();
			//Next, we'd add it to a parent control, for example, a tab Page in our UI
		}
		#endregion

		#region Notes UI
		/// <summary>
		/// The NotesBar is a useful GUI component for record-based applications to quickly
		/// get group annotation ability with just a few lines of code.
		/// </summary>
		[Test]
		public void CreateNotesBar()
		{
			//Tell Chorus how to map between our records and the url system we want to use in notes
			var mapping = new NotesToRecordMapping();
			mapping.FunctionToGoFromObjectToItsId = who => ((Character)who).Guid;
			mapping.FunctionToGetCurrentUrlForNewNotes = (unusedTarget, unusedId) => string.Format("comics://character?id={0}?label={1}", _currentCharacter.Guid, _currentCharacter.Name);
			//or
			mapping.FunctionToGetCurrentUrlForNewNotes = (unusedTarget, unusedId) => _currentCharacter.GetUrl();

			var barControl = _chorusSystem.WinForms.CreateNotesBar(_someDataFilePath, mapping, _progress);

			//Next, we'd add it to a parent control, for example, at the top of the screen showing the current record
			//myView.Controls.Add(barControl);

			//Then each time we change record, we let it know, and it will look up an display any notes targeted at this record:
			barControl.SetTargetObject(_currentCharacter);

			barControl.Dispose(); //normally, your parent control will do this for you
		}

/* is this needed?        [Test]
		public void CreateNotesBarUsingFactory()
		{
			//rather than making the chorusSystem available everywhere and calling it directly, it may suit your architecture better
			//to pass factories around, or to inject them into your IOC container.  In this example, we're using autofac, but you
			//could use any IOC system:

			var builder = new Autofac.Builder.ContainerBuilder();
			builder.Register<NotesBarView.Factory>(c => _chorusSystem.NotesBarFactory);
			var container = builder.Build();

			//then make NotesBarView.Factory a parameter in the constructor of the control in which you will use it, and create it there

			//public RecordView(NotesBarView.Factory notesBarFactory)
			//{
			//      _notesBarView = notesBarFactory(_someDataFilePath, mapping);
		}
*/
		/// <summary>
		/// A NotesBrowser grabs up all the notes in all the .ChorusNotes files in the system, and gives the users
		/// tools to search and filter them.
		/// </summary>
		[Test]
	[Category("KnownMonoIssue")] //running CreateNotesBrowser twice in a mono test session causes a crash
	[Platform(Exclude="Mono")]
		public void CreateNotesBrowser()
		{
			var browser = _chorusSystem.WinForms.CreateNotesBrowser();

			//Next, we'd add it to a parent control, for example, a tab Page in our UI
			//myView.Controls.Add(browser);
		}

		#endregion

		#region Notes Low-Level

		[Test]
		public void GetNotesRepository()
		{
			var notes = _chorusSystem.GetNotesRepository(_someDataFilePath, _progress) as AnnotationRepository;

			//We can add
			var annotation = new Annotation("question", "blah://blah", _someDataFilePath);
			notes.AddAnnotation(annotation);

			//We can remove
			notes.Remove(annotation);

			//We can retrieve
			notes.GetAllAnnotations();
			notes.GetByCurrentStatus("closed");
			notes.GetMatchesByPrimaryRefKey("1abc3");
			notes.GetMatches(note => note.Messages.Count() > 0);

			//for large repositories, you can also create indexes and add them
			//to the repository, to speed things up.
			var index = new IndexByDate();
			notes.AddObserver(index, _progress);
			var todaysNotes = index.GetByDate(DateTime.Now);

			//Note, index is just one kind of observer, you can add others too.
		}

		#endregion


	}

	public class IndexByDate : IAnnotationRepositoryObserver
	{
		#region Implementation of IAnnotationRepositoryObserver

		public void Initialize(Func<IEnumerable<Annotation>> allAnnotationsFunction, IProgress progress)
		{
		}

		public void NotifyOfAddition(Annotation annotation)
		{
		}

		public void NotifyOfModification(Annotation annotation)
		{
		}

		public void NotifyOfDeletion(Annotation annotation)
		{
		}

		#endregion

		public IEnumerable<Annotation> GetByDate(DateTime time)
		{
			return null;
		}
	}


	public class Character
	{

		public Character()
		{
			Guid = System.Guid.NewGuid().ToString();
		}
		public string Guid
		{
			get;
			set;
		}

		public string Name{get;set;}

		public string GetUrl()
		{
			return string.Format("comics://character?id={0}?label={1}", Guid, Name);
		}
	}
}
