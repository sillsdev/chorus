using Autofac;
using Chorus.annotations;
using Chorus.UI.Notes;
using Chorus.UI.Notes.Bar;
using Chorus.UI.Notes.Browser;
using Chorus.Utilities;

namespace Chorus
{
	public class ChorusNotesSystem
	{
		public delegate ChorusNotesSystem Factory(AnnotationRepository repository, string pathToFileBeingAnnotated, IProgress progress);//autofac uses this

		public delegate string IdGeneratorFunction(object targetOfAnnotation);

		/// <summary>
		/// Note, the key will be "escaped" (made safe for going in a url) for you, so don't make
		/// your UrlGeneratorFunction do that.
		/// </summary>
		/// <example>(escapedId) => string.Format("myimages://image?id={0}&amp;type=jpg",escapedId)</example>
		/// <example>(ignoreIt) => string.Format("myimages://image?id={0}&amp;type={1}",_currentImage.Guid, _currentImage.FileType)</example>
		public delegate string UrlGeneratorFunction(string escapedId);
		internal static UrlGeneratorFunction DefaultUrlGenerator = (id) => string.Format("chorus://object?id={0}", id);

		/// <summary>
		/// The Id is what normally comes after the "id=" in the url
		/// </summary>
		public IdGeneratorFunction IdGenerator { get; set; }

		public static IdGeneratorFunction DefaultIdGeneratorUsingObjectToStringAsId = (target) => target.ToString();

		private readonly Autofac.IContainer _container;
		private readonly AnnotationRepository _repository;

		public ChorusNotesSystem(IContainer parentContainer, AnnotationRepository repository,string pathToFileBeingAnnotated, IProgress progress)
		{
			_container = parentContainer;
			_repository = repository;
		   // UrlGenerator = DefaultUrlGenerator;
			IdGenerator = DefaultIdGeneratorUsingObjectToStringAsId;
		}

		/// <summary>
		/// Get a control which will show existing annotations related to your application's current context,
		/// and allow users to create new ones (possibly limitted by the settings of the current IChorusUser).
		/// </summary>
		/// <param name="functionToMakeUrlForAnnotation">Given what you app is displaying, where the user's cursor is,
		/// whatever, this function should create a url for that location, to be added
		/// to a newly created annotation.  It will be called whenever a new annotation is going to be created.</param>
		/// <returns></returns>
		public NotesBarView CreateNotesBarView(UrlGeneratorFunction functionToMakeUrlForAnnotation)
		{
			var model = _container.Resolve<NotesBarModel.Factory>()(_repository, functionToMakeUrlForAnnotation);
			model.IdGenerator = IdGenerator;
			return new NotesBarView(model, _container.Resolve<AnnotationEditorModel.Factory>());
		}

		 /// <summary>
		/// Get a control which will show existing annotations related to your application's current context,
		/// and allow users to create new ones (possibly limitted by the settings of the current IChorusUser).
		/// This version will create a simple default url using a "chorus://" scheme.
		/// </summary>
		public NotesBarView CreateNotesBarView()
		 {
			 return CreateNotesBarView(DefaultUrlGenerator);
		 }

		public NotesBrowserPage CreateNotesBrowserPage()
		{
			return _container.Resolve<NotesBrowserPage.Factory>()();
		}
	}
}