using System;
using System.Collections.Generic;
using System.IO;
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
		private readonly string _folderPath;
		private readonly IChorusUser _user;
		private readonly IContainer _container;
		internal readonly Dictionary<string, AnnotationRepository> _annotationRepositories = new Dictionary<string, AnnotationRepository>();
		private bool _searchedForAllExistingNotesFiles;


		public ChorusSystem(string folderPath)
		{
			_folderPath = folderPath;
			var hgrepo = HgRepository.CreateOrLocate(folderPath, new NullProgress());
			var builder = new Autofac.Builder.ContainerBuilder();

			builder.Register<ProjectFolderConfiguration>(c => new ProjectFolderConfiguration(folderPath));

			ChorusUIComponentsInjector.Inject(builder, folderPath);

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

			public NotesBarView CreateNotesBar(string pathToAnnotatedFile, NotesToRecordMapping mapping, IProgress progress)
			{
				var repo = _parent.GetNotesRepository(pathToAnnotatedFile, progress);
				var model = _container.Resolve<NotesBarModel.Factory>()(repo, mapping);
				return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
			}

			public NotesBrowserPage CreateNotesBrowser()
			{
				_parent.EnsureAllNotesRepositoriesLoaded();
				return _container.Resolve<NotesBrowserPage.Factory>()(_parent._annotationRepositories.Values);
			}


			public HistoryPage CreateHistoryPage()
			{
				return _container.Resolve<HistoryPage>();
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
				foreach (var repo in AnnotationRepository.CreateRepositoriesFromFolder(_folderPath, progress))
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



	public class NotesToRecordMapping
	{

		static internal NotesToRecordMapping SimpleForTest()
		{
			var m = new NotesToRecordMapping();
			m.FunctionToGoFromObjectToItsId = DefaultIdGeneratorUsingObjectToStringAsId;
			m.FunctionToGetCurrentUrlForNewNotes = DefaultUrlGenerator;
			return m;
		}

		 public delegate string UrlGeneratorFunction(object target, string escapedId);

		public delegate string IdGeneratorFunction(object targetOfAnnotation);


		public static IdGeneratorFunction DefaultIdGeneratorUsingObjectToStringAsId = (target) => target.ToString();
		internal static UrlGeneratorFunction DefaultUrlGenerator = (unused, id) => string.Format("chorus://object?id={0}", id);

	   /// <summary>
		/// Used to figure out which existing notes to show
		/// The Id is what normally comes after the "id=" in the url
		/// </summary>
		public IdGeneratorFunction FunctionToGoFromObjectToItsId = o => { throw new ArgumentNullException("FunctionToGoFromObjectToItsId", "You need to supply a function for FunctionToGoFromObjectToItsId."); };

		/// <summary>
		/// Used to make new annotations with a url refelctign the current object/insertion-point/whatever
		/// Note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGeneratorFunction do that.
		/// <example>(escapedId) => string.Format("myimages://image?id={0}&amp;type=jpg",escapedId)</example>
		/// <example>(ignoreIt) => string.Format("myimages://image?id={0}&amp;type={1}",_currentImage.Guid, _currentImage.FileType)</example>
		public UrlGeneratorFunction FunctionToGetCurrentUrlForNewNotes = DefaultUrlGenerator;
	}

}