// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Chorus.notes;
using Chorus.Review;
using Chorus.sync;
using Chorus.VcsDrivers.Mercurial;
using Palaso.Code;
using Palaso.Progress;
using IContainer = Autofac.IContainer;

namespace Chorus
{
	/// <summary>
	/// A ChorusSystem object hides a lot of the complexity of Chorus from the client programmer.
	/// It offers up the most common controls and services of Chorus. See the SampleApp for
	/// examples of using it.
	/// This simple class is UI independent.
	/// </summary>
	public class ChorusSystemSimple: IChorusSystem, IDisposable
	{
		/// <summary>
		/// The data folder path.
		/// </summary>
		protected string _dataFolderPath;
		/// <summary>
		/// The Chorus user.
		/// </summary>
		protected IChorusUser _user;
		/// <summary>
		/// The container.
		/// </summary>
		protected IContainer _container;
		/// <summary>
		/// The annotation repositories.
		/// </summary>
		internal protected readonly Dictionary<string, AnnotationRepository> _annotationRepositories =
			new Dictionary<string, AnnotationRepository>();
		/// <summary>
		/// <c>true</c> if we already searched for all existing notes files
		/// </summary>
		protected bool _searchedForAllExistingNotesFiles;

		/// <summary>
		/// Constructor. Need to Init after this.
		/// </summary>
		public ChorusSystemSimple()
		{
			DisplaySettings = new ChorusNotesSettings();
		}

		/// <summary>
		/// Constructor. Need to Init after this
		/// </summary>
		/// <param name="dataFolderPath">The root of the project</param>
		public ChorusSystemSimple(string dataFolderPath): this()
		{
			_dataFolderPath = dataFolderPath;
		}

		/// <summary>
		/// Gets the container.
		/// </summary>
		public IContainer Container
		{
			get { return _container; }
		}

		/// <summary>
		/// This is a special init used for functions (such as setting up a NotesBar) which do not
		/// actually require Mercurial. Crashes are likely if you use this and then try functions
		/// like Send/Receive which DO need Hg. This version must be passed a reasonable
		/// userNameForHistoryAndNotes, since there is no way to obtain a default one.
		/// </summary>
		/// <param name="userNameForHistoryAndNotes"></param>
		public void InitWithoutHg(string userNameForHistoryAndNotes)
		{
			Require.That(!string.IsNullOrWhiteSpace(userNameForHistoryAndNotes),
				"Must have a user name to init Chorus without a repo");
			var builder = InitContainerBuilder();
			FinishInit(userNameForHistoryAndNotes, builder);
		}

		/// <summary>
		/// Initialize system with user's name.
		/// </summary>
		/// <param name="userNameForHistoryAndNotes">This is not the same name as that used for
		/// any given network repository credentials. Rather, it's the name which will show in
		/// the history, and besides Notes that this user makes.
		///</param>
		public void Init(string userNameForHistoryAndNotes)
		{
			Require.That(!string.IsNullOrEmpty(_dataFolderPath));
			Init(_dataFolderPath, userNameForHistoryAndNotes);
		}

		/// <summary>
		/// Initialize system with user's name.
		/// </summary>
		/// <param name="dataFolderPath">The root of the project</param>
		/// <param name="userNameForHistoryAndNotes">This is not the same name as that used for
		/// any given network repository credentials. Rather, it's the name which will show in
		/// the history, and besides Notes that this user makes.
		///</param>
		public virtual void Init(string dataFolderPath, string userNameForHistoryAndNotes)
		{
			_dataFolderPath = dataFolderPath;
			Repository = HgRepository.CreateOrUseExisting(_dataFolderPath, new NullProgress());
			var builder = InitContainerBuilder();

			if (String.IsNullOrEmpty(userNameForHistoryAndNotes))
			{
				userNameForHistoryAndNotes = Repository.GetUserIdInUse();
			}
			FinishInit(userNameForHistoryAndNotes, builder);
		}

		private void FinishInit(string userNameForHistoryAndNotes, ContainerBuilder builder)
		{
			_user = new ChorusUser(userNameForHistoryAndNotes);
			builder.RegisterInstance(_user).As<IChorusUser>();
//            builder.RegisterGeneratedFactory<NotesInProjectView.Factory>().ContainerScoped();
//            builder.RegisterGeneratedFactory<NotesInProjectViewModel.Factory>().ContainerScoped();
//            builder.RegisterGeneratedFactory<NotesBrowserPage.Factory>().ContainerScoped();

			// builder.Register(new NullProgress());//TODO
			_container = builder.Build();

			//add the container itself
			var builder2 = new Autofac.ContainerBuilder();
			builder2.RegisterInstance(_container).As<IContainer>();
			builder2.Update(_container);
			DidLoadUpCorrectly = true;
		}

		/// <summary>
		/// Inits the container builder.
		/// </summary>
		protected virtual ContainerBuilder InitContainerBuilder()
		{
			var builder = new Autofac.ContainerBuilder();

			builder.Register<ChorusNotesSettings>(c => DisplaySettings);

			// This might be replaced in a derived class, but it gives us a sensible default
			builder.RegisterType<SyncControlModelSimple>().InstancePerLifetimeScope();

			return builder;
		}

		/// <summary>
		/// <c>true</c> if loaded correctly
		/// </summary>
		public bool DidLoadUpCorrectly { get; private set; }

		/// <summary>
		/// The display settings.
		/// </summary>
		public ChorusNotesSettings DisplaySettings { get; private set; }

		/// <summary>
		/// Gets the NavigateToRecord event.
		/// </summary>
		public NavigateToRecordEvent NavigateToRecordEvent
		{
			get { return _container.Resolve<NavigateToRecordEvent>(); }
		}

		/// <summary>
		/// Use this to set things like what file types to include/exclude
		/// </summary>
		public ProjectFolderConfiguration ProjectFolderConfiguration
		{
			get { return _container.Resolve<ProjectFolderConfiguration>(); }
		}

		/// <summary>
		/// Gets the Mercurial repository.
		/// </summary>
		public HgRepository Repository
		{
			get; private set;
		}

		/// <summary>
		/// Gets the user name for history and notes.
		/// </summary>
		public string UserNameForHistoryAndNotes
		{
			get
			{
				return _user.Name;
			}
//  it's too late to set it, the name is already in the DI container
//            set
//            {
//                Repository.SetUserNameInIni(value, new NullProgress());
//            }
		}

		/// <summary>
		/// Ensures all notes repositories loaded.
		/// </summary>
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

		/// <summary>
		/// Gets the notes repository.
		/// </summary>
		public IAnnotationRepository GetNotesRepository(string pathToFileBeingAnnotated, IProgress progress)
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

		/// <summary>
		/// Gets the notes repository.
		/// </summary>
		public IAnnotationRepository GetNotesRepository(string pathToPrimaryFile,
			IEnumerable<string> pathsToOtherFiles, string idAttrForOtherFiles, IProgress progress)
		{
			Require.That(File.Exists(pathToPrimaryFile));
			foreach (var path in pathsToOtherFiles)
				Require.That(File.Exists(path));

			var pathToPrimaryAnnotationFile = pathToPrimaryFile + "."+AnnotationRepository.FileExtension;
			AnnotationRepository primary;
			if (!_annotationRepositories.TryGetValue(pathToPrimaryAnnotationFile, out primary))
			{
				primary = AddAnnotationRepository(pathToPrimaryAnnotationFile, progress);
			}
			var others = new List<IAnnotationRepository>();
			foreach (var otherPath in pathsToOtherFiles)
			{
				var pathToOtherAnnotationFile = otherPath + "." + AnnotationRepository.FileExtension;
				AnnotationRepository otherRepo;
				if (!_annotationRepositories.TryGetValue(pathToOtherAnnotationFile, out otherRepo))
				{
					otherRepo = AddAnnotationRepository(pathToOtherAnnotationFile, progress, idAttrForOtherFiles);
				}
				others.Add(otherRepo);
			}
			return new MultiSourceAnnotationRepository(primary, others);
		}

		private AnnotationRepository AddAnnotationRepository(string pathToFileBeingAnnotated,
			IProgress progress, string idAttr = "id")
		{
			AnnotationRepository repo;
			repo = AnnotationRepository.FromFile(idAttr, pathToFileBeingAnnotated, progress);
			_annotationRepositories.Add(pathToFileBeingAnnotated, repo);
			return repo;
		}

		/// <summary>
		/// Check in, to the local disk repository, any changes to this point.
		/// </summary>
		/// <param name="checkinDescription">A description of what work was done that you're
		/// wanting to checkin. E.g. "Delete a Book"</param>
		/// <param name="callbackWhenFinished">Code to call when the task finishes</param>
		public void AsyncLocalCheckIn(string checkinDescription, Action<SyncResults> callbackWhenFinished)
		{
			var model = _container.Resolve<SyncControlModelSimple>();
			model.AsyncLocalCheckIn(checkinDescription,callbackWhenFinished);
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Releases all resource used by the <see cref="Chorus.ChorusSystemSimple"/> object.
		/// </summary>
		public void Dispose()
		{
			if (!DidLoadUpCorrectly)
				return;

			foreach (AnnotationRepository repository in _annotationRepositories.Values)
			{
				repository.Dispose();
			}
			_annotationRepositories.Clear();
			_container.Dispose();
		}

		#endregion
	}
}
