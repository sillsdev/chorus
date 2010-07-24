using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Autofac.Builder;
using Chorus.notes;
using Chorus.sync;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.UI.Review;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	public class ChorusSystem :IDisposable
	{
		private readonly string _dataFolderPath;
		private readonly IChorusUser _user;
		private readonly IContainer _container;
		internal readonly Dictionary<string, AnnotationRepository> _annotationRepositories = new Dictionary<string, AnnotationRepository>();
		private bool _searchedForAllExistingNotesFiles;


		public ChorusSystem(string dataFolderPath)
		{
			WritingSystems = new List<IWritingSystem>{new EnglishWritingSystem(), new ThaiWritingSystem()};

			_dataFolderPath = dataFolderPath;
			var hgrepo = HgRepository.CreateOrLocate(dataFolderPath, new NullProgress());
			var builder = new Autofac.Builder.ContainerBuilder();

			builder.Register<ProjectFolderConfiguration>(c => new ProjectFolderConfiguration(dataFolderPath));
			builder.Register<IEnumerable<IWritingSystem>>(c=>WritingSystems);

			ChorusUIComponentsInjector.Inject(builder, dataFolderPath);

			_user  =new ChorusUser(hgrepo.GetUserIdInUse());
			builder.Register<IChorusUser>(_user);
//            builder.RegisterGeneratedFactory<NotesInProjectView.Factory>().ContainerScoped();
//            builder.RegisterGeneratedFactory<NotesInProjectViewModel.Factory>().ContainerScoped();
//            builder.RegisterGeneratedFactory<NotesBrowserPage.Factory>().ContainerScoped();

		   // builder.Register(new NullProgress());//TODO
			_container = builder.Build();


			//add the container itself
			var builder2 = new Autofac.Builder.ContainerBuilder();
			builder2.Register<IContainer>(_container);
			builder2.Build(_container);
		}

		/// <summary>
		/// Set this if you want something other than English
		/// </summary>
		public IEnumerable<IWritingSystem> WritingSystems
		{
			get;
			set;
		}

		public NavigateToRecordEvent NavigateToRecordEvent
		{
			get { return _container.Resolve<NavigateToRecordEvent>(); }
		}
		public WinFormsFactory WinForms
		{
			get { return new WinFormsFactory(this, _container); }
		}

		/// <summary>
		/// This class is exists only to organize all WindowForms UI component factories together,
		/// so the programmer can write, for example:
		/// _chorusSystem.WinForms.CreateSynchronizationDialog()
		/// </summary>
		public class WinFormsFactory
		{
			private readonly ChorusSystem _parent;
			private readonly IContainer _container;

			public WinFormsFactory(ChorusSystem parent, IContainer container)
			{
				_parent = parent;
				_container = container;
			}

			public Chorus.UI.Sync.SyncDialog CreateSynchronizationDialog()
			{
				return _container.Resolve<Chorus.UI.Sync.SyncDialog>();
			}

			/// <summary>
			/// Get a UI control designed to live near some data (e.g., a lexical entry);
			/// it provides buttons
			/// to let users see and open and existing notes attached to that data,
			/// or create new notes related to the data.
			/// </summary>
			public NotesBarView CreateNotesBar(string pathToAnnotatedFile, NotesToRecordMapping mapping, IProgress progress)
			{
				var repo = _parent.GetNotesRepository(pathToAnnotatedFile, progress);
				var model = _container.Resolve<NotesBarModel.Factory>()(repo, mapping);
				return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
			}

			/// <summary>
			/// Get a UI control which shows all notes in the project (including conflicts), and
			/// lets the user filter them and interact with them.
			/// </summary>
			public NotesBrowserPage CreateNotesBrowser()
			{
				_parent.EnsureAllNotesRepositoriesLoaded();
				return _container.Resolve<NotesBrowserPage.Factory>()(_parent._annotationRepositories.Values);
			}

			/// <summary>
			/// Get a UI control which shows all the revisions in the repository, and
			/// lets the user select one to see what changed.
			/// </summary>
			public HistoryPage CreateHistoryPage()
			{
				return _container.Resolve<HistoryPage.Factory>()(new HistoryPageOptions());
			}


			/// <summary>
			/// Get a UI control which shows all the revisions in the repository, and
			/// lets the user select one to see what changed.
			/// </summary>
			public HistoryPage CreateHistoryPage(HistoryPageOptions options)
			{
				return _container.Resolve<HistoryPage.Factory>()(options);
			}

			public Form CreateSettingDialog()
			{
				throw new NotImplementedException();
			}
		}

		public void EnsureAllNotesRepositoriesLoaded()
		{
			if(!_searchedForAllExistingNotesFiles)
			{
				var progress = new NullProgress();
				foreach (var repo in AnnotationRepository.CreateRepositoriesFromFolder(_dataFolderPath, progress))
				{
					if (!_annotationRepositories.ContainsKey(repo.AnnotationFilePath))
					{
						_annotationRepositories.Add(repo.AnnotationFilePath, repo);
					}
				}
				_searchedForAllExistingNotesFiles=true;
			}
		}

		public AnnotationRepository GetNotesRepository(string pathToFileBeingAnnotated, IProgress progress)
		{
			Require.That(File.Exists(pathToFileBeingAnnotated));
			var pathToAnnotationFile = pathToFileBeingAnnotated + "."+AnnotationRepository.FileExtension;
			AnnotationRepository repo;
			if (!_annotationRepositories.TryGetValue(pathToAnnotationFile, out repo))
			{
				repo = AddAnnotationRepository(pathToAnnotationFile, progress);
			}
			return repo;
		}

		private AnnotationRepository AddAnnotationRepository(string pathToFileBeingAnnotated, IProgress progress)
		{
			AnnotationRepository repo;
			repo = AnnotationRepository.FromFile("id", pathToFileBeingAnnotated, progress);
			_annotationRepositories.Add(pathToFileBeingAnnotated, repo);
			return repo;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_container.Dispose();
		}

		#endregion


	}
}