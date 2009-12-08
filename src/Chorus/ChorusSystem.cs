using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Builder;
using Chorus.annotations;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.Utilities;
using Chorus.Utilities.code;

namespace Chorus
{
	public class ChorusSystem
	{
		private readonly IContainer _container;
		private readonly Dictionary<string, AnnotationRepository> _annotationRepositories = new Dictionary<string, AnnotationRepository>();

		public ChorusSystem(string folderPath)
		{
			var builder = new Autofac.Builder.ContainerBuilder();
			ChorusUIComponentsInjector.InjectNotesUI(builder);

			builder.Register<ChorusNotesSystem>().ContainerScoped();
			builder.RegisterGeneratedFactory<ChorusNotesSystem.Factory>().ContainerScoped();

			builder.Register<ChorusNotesUser>(c => new ChorusNotesUser("testGuy"));//TODO

		   // builder.Register(new NullProgress());//TODO
			_container = builder.Build();

			//add the container itself
			var builder2 = new Autofac.Builder.ContainerBuilder();
			builder2.Register<IContainer>(_container);
			builder2.Build(_container);
		}

//        public NotesBarView CreateNotesBarView(string pathToFileToBeAnnotated)
//        {
//            return _container.Resolve<NotesBarView.Factory>()();
//        }

		public ChorusNotesSystem GetNotesSystem(string pathToFileBeingAnnotated, IProgress progress)
		{
			Require.That(File.Exists(pathToFileBeingAnnotated));
			var pathToAnnotationFile = pathToFileBeingAnnotated + AnnotationRepository.FileExtension;
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

		private Autofac.IContainer _container;
		private readonly AnnotationRepository _repository;

		public ChorusNotesSystem(IContainer parentContainer, AnnotationRepository repository,string pathToFileBeingAnnotated, IProgress progress)
		{
			_container = parentContainer;
			_repository = repository;
			UrlGenerater = DefaultGenerator;

			// _container = parentContainer.CreateInnerContainer();
//            var builder = new Autofac.Builder.ContainerBuilder();
//            builder.Register(repository);
//            builder.Build(_container);
		   // var model = _container.Resolve<NotesBarModel>();
//            builder.Register(model);
//            builder.Build(_container);

			//var x = _container.Resolve<NotesBarModel.Factory>()(repository);
		}

		public NotesBarView CreateNotesBarView()
		{
			var model = _container.Resolve<NotesBarModel.Factory>()(_repository);
			model.UrlGenerater = UrlGenerater;
			return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
		}
	}
}