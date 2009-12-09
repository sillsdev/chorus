using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Builder;
using Chorus.annotations;
using Chorus.sync;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;

namespace Chorus
{
	public class ChorusSystem
	{
		private readonly IChorusUser _user;
		private readonly IContainer _container;
		private readonly Dictionary<string, AnnotationRepository> _annotationRepositories = new Dictionary<string, AnnotationRepository>();

		static public ChorusSystem CreateAndGuessUserName(string folderPath)
		{
				var hgrepo = HgRepository.CreateOrLocate(folderPath, new NullProgress());
				var user  =new ChorusUser(hgrepo.GetUserIdInUse());
				return new ChorusSystem(folderPath, user);
		}

		public ChorusSystem(string folderPath, IChorusUser user)
		{
			_user = user;
			var builder = new Autofac.Builder.ContainerBuilder();

			builder.Register<ProjectFolderConfiguration>(c => new ProjectFolderConfiguration(folderPath));

			ChorusUIComponentsInjector.InjectNotesUI(builder);

			builder.Register<ChorusNotesSystem>().ContainerScoped();
			builder.RegisterGeneratedFactory<ChorusNotesSystem.Factory>().ContainerScoped();
			builder.Register<IChorusUser>(_user);
			builder.RegisterGeneratedFactory<NotesInProjectView.Factory>().ContainerScoped();
			builder.RegisterGeneratedFactory<NotesInProjectViewModel.Factory>().ContainerScoped();
			builder.RegisterGeneratedFactory<NotesBrowserPage.Factory>().ContainerScoped();

		   // builder.Register(new NullProgress());//TODO
			_container = builder.Build();

			//add the container itself
			var builder2 = new Autofac.Builder.ContainerBuilder();
			builder2.Register<IContainer>(_container);
			builder2.Build(_container);
		}


		public ChorusNotesSystem GetNotesSystem(string pathToFileBeingAnnotated, IProgress progress)
		{
			Require.That(File.Exists(pathToFileBeingAnnotated));
			var pathToAnnotationFile = pathToFileBeingAnnotated + "."+AnnotationRepository.FileExtension;
			AnnotationRepository repo;
			if (!_annotationRepositories.TryGetValue(pathToAnnotationFile, out repo))
			{
				repo = AddAnnotationRepository(pathToAnnotationFile, progress);
			}

			return _container.Resolve<ChorusNotesSystem.Factory>()(repo, pathToAnnotationFile, progress);
		}

		private AnnotationRepository AddAnnotationRepository(string pathToFileBeingAnnotated, IProgress progress)
		{
			AnnotationRepository repo;
			repo = AnnotationRepository.FromFile("id", pathToFileBeingAnnotated, progress);
			_annotationRepositories.Add(pathToFileBeingAnnotated, repo);
//            var builder = new Autofac.Builder.ContainerBuilder();
//            builder.Register<AnnotationRepository>(repo);
//            builder.Build(_container);

			return repo;
		}
	}



	public class ChorusNotesSystem
	{
		public delegate ChorusNotesSystem Factory(AnnotationRepository repository, string pathToFileBeingAnnotated, IProgress progress);//autofac uses this

		public delegate string UrlGenerator(string key);

		/// <summary>
		/// set this if you want something other than a default, chorus-generated URL for your objects
		/// note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGenerator do that.
		/// </summary>
		/// <example>(key) => string.Format("myimages://image?id={0}&amp;type=jpg", key)</example>
		public UrlGenerator UrlGenerater { get; set; }

		 public static UrlGenerator DefaultGenerator = (key) => string.Format("chorus://object?id={0}", key);

		private readonly Autofac.IContainer _container;
		private readonly AnnotationRepository _repository;

		public ChorusNotesSystem(IContainer parentContainer, AnnotationRepository repository,string pathToFileBeingAnnotated, IProgress progress)
		{
			_container = parentContainer;
			_repository = repository;
			UrlGenerater = DefaultGenerator;
		}

		public NotesBarView CreateNotesBarView()
		{
			var model = _container.Resolve<NotesBarModel.Factory>()(_repository);
			model.UrlGenerater = UrlGenerater;
			return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
		}

		public NotesBrowserPage CreateNotesBrowserPage()
		{
			return _container.Resolve<NotesBrowserPage.Factory>()();
		}
	}
}